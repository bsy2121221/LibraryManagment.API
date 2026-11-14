-- Admin records a fee paid at the desk (cash / UPI to the library). Queues the "fee paid" messages.
-- Errors: 50404 not found, 50409 already paid or cancelled
CREATE PROCEDURE dbo.usp_FeeInvoice_MarkPaid
    @InvoiceId   INT,
    @AdminUserId INT,
    @Note        NVARCHAR(500),
    @Currency    CHAR(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();
    DECLARE @Status TINYINT, @UserId INT, @BookingId INT, @Amount DECIMAL(10,2), @PeriodStart DATE, @PeriodEnd DATE;
    DECLARE @Data NVARCHAR(MAX);

    BEGIN TRANSACTION;

    SELECT @Status = Status, @UserId = UserId, @BookingId = BookingId, @Amount = Amount, @PeriodStart = PeriodStart, @PeriodEnd = PeriodEnd
    FROM dbo.FeeInvoices WITH (UPDLOCK, HOLDLOCK)
    WHERE Id = @InvoiceId;

    IF @Status IS NULL
        THROW 50404, N'Fee not found.', 1;

    IF @Status NOT IN (1, 3)
        THROW 50409, N'This fee is already paid or cancelled.', 1;

    INSERT INTO dbo.Payments (UserId, BookingId, FeeInvoiceId, Gateway, GatewayOrderId, Amount, Currency, Status, RawResponse)
    VALUES (@UserId, @BookingId, @InvoiceId, 'Cash', CONCAT('cash_', @InvoiceId, '_', FORMAT(@Now, 'yyyyMMddHHmmss')), @Amount, @Currency, 2,
            (SELECT @AdminUserId AS recordedByUserId, @Note AS note FOR JSON PATH, WITHOUT_ARRAY_WRAPPER));

    UPDATE dbo.FeeInvoices SET Status = 2, PaidAt = @Now, UpdatedAt = @Now WHERE Id = @InvoiceId;

    SET @Data = (SELECT FORMAT(@Amount, '0.##')                AS Amount,
                        CONVERT(VARCHAR(11), @PeriodStart, 106) AS PeriodStart,
                        CONVERT(VARCHAR(11), @PeriodEnd, 106)   AS PeriodEnd,
                        CONVERT(VARCHAR(11), d.NextDueDate, 106) AS NextDueDate,
                        d.SeatLabel                            AS Seat,
                        d.SectionName                          AS Section
                 FROM dbo.vw_BookingDetails d WHERE d.Id = @BookingId
                 FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
    EXEC dbo.usp_Notification_EnqueueEvent @UserId = @UserId, @EventType = 'FeePaid', @DataJson = @Data, @FeeInvoiceId = @InvoiceId;

    COMMIT TRANSACTION;
END
