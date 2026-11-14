-- Admin removes a student from their seat. The seat becomes free, unpaid invoices are cancelled
-- and the "seat released" messages are queued.
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
    DECLARE @Status TINYINT, @UserId INT, @SeatId INT, @Data NVARCHAR(MAX);

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

    -- Only students who had a seat (not just an unpaid hold) are told
    IF @Status = 2
    BEGIN
        SET @Data = (SELECT st.SeatLabel AS Seat, sec.Name AS Section, @Reason AS Reason
                     FROM dbo.Seats st JOIN dbo.Sections sec ON sec.Id = st.SectionId
                     WHERE st.Id = @SeatId
                     FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
        EXEC dbo.usp_Notification_EnqueueEvent @UserId = @UserId, @EventType = 'SeatReleased', @DataJson = @Data;
    END

    COMMIT TRANSACTION;
END
