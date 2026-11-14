/*
Admin gives a seat to a student directly (walk-in), without the online hold and payment.
  - Creates an Active booking from @StartDate; the next period starts one month later.
  - Creates the first month's invoice: Paid (recorded as a desk payment) when @PaidAtDesk = 1, otherwise Due on @StartDate.
  - Queues the "seat assigned" messages.
Returns the booking Id.
Errors: 50400 not allowed, 50404 seat/student not found, 50409 seat taken / student already has a seat
*/
CREATE PROCEDURE dbo.usp_Booking_AssignByAdmin
    @UserId      INT,
    @SeatId      INT,
    @StartDate   DATE,
    @Amount      DECIMAL(10,2),
    @PaidAtDesk  BIT,
    @Note        NVARCHAR(500),
    @AdminUserId INT,
    @Currency    CHAR(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();
    DECLARE @UserGender TINYINT, @UserActive BIT, @SectionGender TINYINT, @Label NVARCHAR(20), @Message NVARCHAR(2048);
    DECLARE @BookingId INT, @InvoiceId INT, @NextPeriodStart DATE = DATEADD(MONTH, 1, @StartDate), @Data NVARCHAR(MAX), @FeeNote NVARCHAR(200);

    SELECT @UserGender = Gender, @UserActive = IsActive FROM dbo.Users WHERE Id = @UserId AND Role = 2;
    IF @UserGender IS NULL
        THROW 50404, N'Student not found.', 1;
    IF @UserActive = 0
        THROW 50400, N'This account is deactivated. Activate it before assigning a seat.', 1;

    BEGIN TRANSACTION;

    SELECT @SectionGender = sec.Gender, @Label = st.SeatLabel
    FROM dbo.Seats st WITH (UPDLOCK, HOLDLOCK)
    JOIN dbo.Sections sec ON sec.Id = st.SectionId
    WHERE st.Id = @SeatId AND st.IsDeleted = 0 AND st.Status = 1;

    IF @SectionGender IS NULL
        THROW 50404, N'The seat was not found or is disabled.', 1;

    IF @SectionGender <> @UserGender
    BEGIN
        SET @Message = CONCAT(N'Seat ', @Label, N' is in a ', IIF(@SectionGender = 2, N'Girls', N'Boys'), N' section.');
        THROW 50400, @Message, 1;
    END

    -- Holds that ran out do not count
    UPDATE dbo.Bookings
    SET Status = 4, UpdatedAt = @Now
    WHERE Status = 1 AND HoldExpiresAt <= @Now AND (SeatId = @SeatId OR UserId = @UserId);

    IF EXISTS (SELECT 1 FROM dbo.Bookings WITH (UPDLOCK, HOLDLOCK) WHERE UserId = @UserId AND Status IN (1, 2))
        THROW 50409, N'This student already has a seat (or is paying for one online). Move or release it instead.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Bookings WHERE SeatId = @SeatId AND Status IN (1, 2))
    BEGIN
        SET @Message = CONCAT(N'Seat ', @Label, N' is already taken.');
        THROW 50409, @Message, 1;
    END

    INSERT INTO dbo.Bookings (UserId, SeatId, Status, StartDate, NextDueDate)
    VALUES (@UserId, @SeatId, 2, @StartDate, @NextPeriodStart);
    SET @BookingId = SCOPE_IDENTITY();

    INSERT INTO dbo.FeeInvoices (UserId, BookingId, PeriodStart, PeriodEnd, Amount, DueDate, Status, PaidAt)
    VALUES (@UserId, @BookingId, @StartDate, DATEADD(DAY, -1, @NextPeriodStart), @Amount, @StartDate,
            IIF(@PaidAtDesk = 1, 2, 1), IIF(@PaidAtDesk = 1, @Now, NULL));
    SET @InvoiceId = SCOPE_IDENTITY();

    IF @PaidAtDesk = 1
        INSERT INTO dbo.Payments (UserId, BookingId, FeeInvoiceId, Gateway, GatewayOrderId, Amount, Currency, Status, RawResponse)
        VALUES (@UserId, @BookingId, @InvoiceId, 'Cash', CONCAT('cash_', @InvoiceId, '_', FORMAT(@Now, 'yyyyMMddHHmmss')), @Amount, @Currency, 2,
                (SELECT @AdminUserId AS recordedByUserId, @Note AS note FOR JSON PATH, WITHOUT_ARRAY_WRAPPER));

    INSERT INTO dbo.SeatChangeHistory (UserId, FromSeatId, ToSeatId, ChangedByUserId, Reason)
    VALUES (@UserId, NULL, @SeatId, @AdminUserId, ISNULL(NULLIF(@Note, N''), N'Assigned by admin'));

    SET @FeeNote = IIF(@PaidAtDesk = 1,
                       CONCAT(N'Fee of Rs.', FORMAT(@Amount, '0.##'), N' received.'),
                       CONCAT(N'Fee of Rs.', FORMAT(@Amount, '0.##'), N' is due on ', CONVERT(VARCHAR(11), @StartDate, 106), N'.'));
    SET @Data = (SELECT d.SeatLabel                              AS Seat,
                        d.SectionName                            AS Section,
                        FORMAT(@Amount, '0.##')                  AS Amount,
                        CONVERT(VARCHAR(11), @StartDate, 106)    AS StartDate,
                        CONVERT(VARCHAR(11), @NextPeriodStart, 106) AS NextDueDate,
                        @FeeNote                                 AS FeeNote
                 FROM dbo.vw_BookingDetails d WHERE d.Id = @BookingId
                 FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
    EXEC dbo.usp_Notification_EnqueueEvent @UserId = @UserId, @EventType = 'SeatAssigned', @DataJson = @Data, @FeeInvoiceId = @InvoiceId;

    COMMIT TRANSACTION;

    SELECT @BookingId AS Id;
END
