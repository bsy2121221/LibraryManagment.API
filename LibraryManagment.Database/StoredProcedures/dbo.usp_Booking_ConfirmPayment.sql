/*
Called after the gateway confirms a payment (from the browser callback or the webhook; safe to call twice).
  - Seat booking payment (Payments.FeeInvoiceId is NULL): turns the held booking into an Active booking,
    records the first month as a paid invoice and queues the "seat booked" messages.
  - Monthly fee payment (Payments.FeeInvoiceId set): marks that invoice paid and queues the "fee paid" messages.

Returns one row: Outcome, BookingId
  1 = Confirmed
  2 = AlreadyProcessed  payment was already confirmed earlier (nothing changed)
  3 = SeatLost          paid, but the hold had ended and the seat went to someone else -> refund needed
  4 = InvoiceClosed     paid, but the fee was already paid or was cancelled -> refund needed

Dates are the library's local date (India Standard Time).
Errors: 50404 unknown order, 50409 payment was refunded
*/
CREATE PROCEDURE dbo.usp_Booking_ConfirmPayment
    @GatewayOrderId   VARCHAR(100),
    @GatewayPaymentId VARCHAR(100),
    @RawResponse      NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();
    DECLARE @Today DATE = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'India Standard Time' AS DATE);
    DECLARE @PaymentId INT, @PaymentStatus TINYINT, @BookingId INT, @UserId INT, @Amount DECIMAL(10,2), @PaidInvoiceId INT;
    DECLARE @BookingStatus TINYINT, @SeatId INT, @InvoiceId INT, @InvoiceStatus TINYINT, @Outcome TINYINT, @Data NVARCHAR(MAX);

    BEGIN TRANSACTION;

    SELECT @PaymentId = Id, @PaymentStatus = Status, @BookingId = BookingId, @UserId = UserId, @Amount = Amount,
           @PaidInvoiceId = FeeInvoiceId
    FROM dbo.Payments WITH (UPDLOCK, HOLDLOCK)
    WHERE GatewayOrderId = @GatewayOrderId;

    IF @PaymentId IS NULL
        THROW 50404, N'Payment order not found.', 1;

    IF @PaymentStatus = 2
    BEGIN
        COMMIT TRANSACTION;
        SELECT CAST(2 AS TINYINT) AS Outcome, @BookingId AS BookingId;
        RETURN;
    END

    IF @PaymentStatus = 4
        THROW 50409, N'This payment has been refunded.', 1;

    -- ---------------------------------------------------------------- monthly fee payment
    IF @PaidInvoiceId IS NOT NULL
    BEGIN
        SELECT @InvoiceStatus = Status FROM dbo.FeeInvoices WITH (UPDLOCK, HOLDLOCK) WHERE Id = @PaidInvoiceId;

        IF @InvoiceStatus IN (1, 3)
        BEGIN
            UPDATE dbo.FeeInvoices SET Status = 2, PaidAt = @Now, UpdatedAt = @Now WHERE Id = @PaidInvoiceId;

            UPDATE dbo.Payments
            SET Status = 2, GatewayPaymentId = @GatewayPaymentId, FailureReason = NULL, RawResponse = @RawResponse, UpdatedAt = @Now
            WHERE Id = @PaymentId;

            SET @Data = (SELECT FORMAT(i.Amount, '0.##')                AS Amount,
                                CONVERT(VARCHAR(11), i.PeriodStart, 106) AS PeriodStart,
                                CONVERT(VARCHAR(11), i.PeriodEnd, 106)   AS PeriodEnd,
                                CONVERT(VARCHAR(11), d.NextDueDate, 106) AS NextDueDate,
                                d.SeatLabel                             AS Seat,
                                d.SectionName                           AS Section
                         FROM dbo.FeeInvoices i
                         JOIN dbo.vw_BookingDetails d ON d.Id = i.BookingId
                         WHERE i.Id = @PaidInvoiceId
                         FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
            EXEC dbo.usp_Notification_EnqueueEvent @UserId = @UserId, @EventType = 'FeePaid', @DataJson = @Data, @FeeInvoiceId = @PaidInvoiceId;

            SET @Outcome = 1;
        END
        ELSE
        BEGIN
            UPDATE dbo.Payments
            SET Status = 2, GatewayPaymentId = @GatewayPaymentId, RawResponse = @RawResponse, UpdatedAt = @Now,
                FailureReason = N'Paid a fee that was already paid or cancelled. Refund required.'
            WHERE Id = @PaymentId;

            SET @Outcome = 4;
        END

        COMMIT TRANSACTION;
        SELECT @Outcome AS Outcome, @BookingId AS BookingId;
        RETURN;
    END

    -- ---------------------------------------------------------------- seat booking payment
    SELECT @BookingStatus = Status, @SeatId = SeatId
    FROM dbo.Bookings WITH (UPDLOCK, HOLDLOCK)
    WHERE Id = @BookingId;

    -- Still held (even if the time just ran out, nobody else could take it while Status = 1),
    -- or expired but the seat is still free and the student has no other seat
    IF @BookingStatus = 1
       OR (@BookingStatus = 4
           AND NOT EXISTS (SELECT 1 FROM dbo.Bookings WHERE SeatId = @SeatId AND Status IN (1, 2))
           AND NOT EXISTS (SELECT 1 FROM dbo.Bookings WHERE UserId = @UserId AND Status IN (1, 2)))
    BEGIN
        UPDATE dbo.Bookings
        SET Status = 2,
            HoldExpiresAt = NULL,
            StartDate = @Today,
            NextDueDate = DATEADD(MONTH, 1, @Today),
            UpdatedAt = @Now
        WHERE Id = @BookingId;

        INSERT INTO dbo.FeeInvoices (UserId, BookingId, PeriodStart, PeriodEnd, Amount, DueDate, Status, PaidAt)
        VALUES (@UserId, @BookingId, @Today, DATEADD(DAY, -1, DATEADD(MONTH, 1, @Today)), @Amount, @Today, 2, @Now);
        SET @InvoiceId = SCOPE_IDENTITY();

        UPDATE dbo.Payments
        SET Status = 2,
            GatewayPaymentId = @GatewayPaymentId,
            FeeInvoiceId = @InvoiceId,
            FailureReason = NULL,
            RawResponse = @RawResponse,
            UpdatedAt = @Now
        WHERE Id = @PaymentId;

        INSERT INTO dbo.SeatChangeHistory (UserId, FromSeatId, ToSeatId, ChangedByUserId, Reason)
        VALUES (@UserId, NULL, @SeatId, @UserId, N'Booked online');

        SET @Data = (SELECT d.SeatLabel                              AS Seat,
                            d.SectionName                            AS Section,
                            FORMAT(@Amount, '0.##')                  AS Amount,
                            CONVERT(VARCHAR(11), @Today, 106)        AS StartDate,
                            CONVERT(VARCHAR(11), d.NextDueDate, 106) AS NextDueDate
                     FROM dbo.vw_BookingDetails d
                     WHERE d.Id = @BookingId
                     FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
        EXEC dbo.usp_Notification_EnqueueEvent @UserId = @UserId, @EventType = 'SeatBooked', @DataJson = @Data, @FeeInvoiceId = @InvoiceId;

        SET @Outcome = 1;
    END
    ELSE
    BEGIN
        UPDATE dbo.Payments
        SET Status = 2,
            GatewayPaymentId = @GatewayPaymentId,
            FailureReason = N'Paid after the seat hold ended and the seat was no longer available. Refund required.',
            RawResponse = @RawResponse,
            UpdatedAt = @Now
        WHERE Id = @PaymentId;

        SET @Outcome = 3;
    END

    COMMIT TRANSACTION;

    SELECT @Outcome AS Outcome, @BookingId AS BookingId;
END
