-- Run by the API's background job. Marks every hold whose time has run out as Expired; returns how many.
CREATE PROCEDURE dbo.usp_Booking_ExpireHolds
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();

    UPDATE dbo.Bookings
    SET Status = 4, UpdatedAt = @Now
    WHERE Status = 1 AND HoldExpiresAt <= @Now;

    SELECT @@ROWCOUNT AS ExpiredCount;
END
