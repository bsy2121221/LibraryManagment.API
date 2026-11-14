/*
Admin adds a fee for a student who has a seat (a missed month, an advance month, an extra charge...).
  - @PaidAtDesk = 1: created as Paid with a desk payment, queues "fee paid".
  - otherwise: created as Due, queues "fee added" (reminder rules then apply as for any unpaid fee).
If the fee starts the booking's next period, the billing job will not create that month again.
A second fee with the same period start for the booking raises the unique index error 2601.
Returns the invoice Id.
Errors: 50404 booking not found, 50409 booking not active
*/
CREATE PROCEDURE dbo.usp_FeeInvoice_CreateByAdmin
    @BookingId   INT,
    @PeriodStart DATE,
    @PeriodEnd   DATE,
    @Amount      DECIMAL(10,2),
    @DueDate     DATE,
    @PaidAtDesk  BIT,
    @Note        NVARCHAR(500),
    @AdminUserId INT,
    @Currency    CHAR(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();
    DECLARE @Status TINYINT, @UserId INT, @InvoiceId INT, @Data NVARCHAR(MAX);
    DECLARE @EventType VARCHAR(50) = IIF(@PaidAtDesk = 1, 'FeePaid', 'FeeCreated');

    BEGIN TRANSACTION;

    SELECT @Status = Status, @UserId = UserId
    FROM dbo.Bookings WITH (UPDLOCK, HOLDLOCK)
    WHERE Id = @BookingId;

    IF @Status IS NULL
        THROW 50404, N'Booking not found.', 1;

    IF @Status <> 2
        THROW 50409, N'Fees can only be added for a student who has a seat.', 1;

    INSERT INTO dbo.FeeInvoices (UserId, BookingId, PeriodStart, PeriodEnd, Amount, DueDate, Status, PaidAt)
    VALUES (@UserId, @BookingId, @PeriodStart, @PeriodEnd, @Amount, @DueDate,
            IIF(@PaidAtDesk = 1, 2, 1), IIF(@PaidAtDesk = 1, @Now, NULL));
    SET @InvoiceId = SCOPE_IDENTITY();

    IF @PaidAtDesk = 1
        INSERT INTO dbo.Payments (UserId, BookingId, FeeInvoiceId, Gateway, GatewayOrderId, Amount, Currency, Status, RawResponse)
        VALUES (@UserId, @BookingId, @InvoiceId, 'Cash', CONCAT('cash_', @InvoiceId, '_', FORMAT(@Now, 'yyyyMMddHHmmss')), @Amount, @Currency, 2,
                (SELECT @AdminUserId AS recordedByUserId, @Note AS note FOR JSON PATH, WITHOUT_ARRAY_WRAPPER));

    -- This month is now invoiced: move the booking on so the billing job does not create it again
    UPDATE b
    SET NextDueDate = DATEADD(MONTH, DATEDIFF(MONTH, b.StartDate, b.NextDueDate) + 1, b.StartDate), UpdatedAt = @Now
    FROM dbo.Bookings b
    WHERE b.Id = @BookingId AND b.NextDueDate = @PeriodStart;

    SET @Data = (SELECT FORMAT(@Amount, '0.##')                AS Amount,
                        CONVERT(VARCHAR(11), @PeriodStart, 106) AS PeriodStart,
                        CONVERT(VARCHAR(11), @PeriodEnd, 106)   AS PeriodEnd,
                        CONVERT(VARCHAR(11), @DueDate, 106)     AS DueDate,
                        CONVERT(VARCHAR(11), d.NextDueDate, 106) AS NextDueDate,
                        d.SeatLabel                            AS Seat,
                        d.SectionName                          AS Section
                 FROM dbo.vw_BookingDetails d WHERE d.Id = @BookingId
                 FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
    EXEC dbo.usp_Notification_EnqueueEvent @UserId = @UserId,
                                           @EventType = @EventType,
                                           @DataJson = @Data,
                                           @FeeInvoiceId = @InvoiceId;

    COMMIT TRANSACTION;

    SELECT @InvoiceId AS Id;
END
