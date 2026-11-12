-- Admin removes a student from their seat. The seat becomes free and unpaid invoices are cancelled.
-- Errors: 50404 not found, 50409 already closed
CREATE PROCEDURE dbo.usp_Booking_Release
    @BookingId   INT,
    @AdminUserId INT,
    @Reason      NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();
    DECLARE @Status TINYINT, @UserId INT, @SeatId INT;

    BEGIN TRANSACTION;

    SELECT @Status = Status, @UserId = UserId, @SeatId = SeatId
    FROM dbo.Bookings WITH (UPDLOCK, HOLDLOCK)
    WHERE Id = @BookingId;

    IF @Status IS NULL
        THROW 50404, N'Booking not found.', 1;

    IF @Status NOT IN (1, 2)
        THROW 50409, N'This booking is already closed.', 1;

    UPDATE dbo.Bookings
    SET Status = 3,
        ReleasedAt = @Now,
        ReleasedByUserId = @AdminUserId,
        ReleaseReason = @Reason,
        UpdatedAt = @Now
    WHERE Id = @BookingId;

    UPDATE dbo.FeeInvoices
    SET Status = 4, UpdatedAt = @Now
    WHERE BookingId = @BookingId AND Status IN (1, 3);

    INSERT INTO dbo.SeatChangeHistory (UserId, FromSeatId, ToSeatId, ChangedByUserId, Reason)
    VALUES (@UserId, @SeatId, NULL, @AdminUserId, @Reason);

    COMMIT TRANSACTION;
END
