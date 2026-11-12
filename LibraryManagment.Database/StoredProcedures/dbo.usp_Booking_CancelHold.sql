-- Student gives up their own seat hold before paying. Errors: 50404 no such hold
CREATE PROCEDURE dbo.usp_Booking_CancelHold
    @BookingId INT,
    @UserId    INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Bookings
    SET Status = 4, UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @BookingId AND UserId = @UserId AND Status = 1;

    IF @@ROWCOUNT = 0
        THROW 50404, N'There is no seat hold to cancel.', 1;
END
