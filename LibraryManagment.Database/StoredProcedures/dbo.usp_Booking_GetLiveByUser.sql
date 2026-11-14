-- The student's current booking: Active, or Held with time left on the hold
CREATE PROCEDURE dbo.usp_Booking_GetLiveByUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, UserId, SeatId, Status, HoldExpiresAt, StartDate, NextDueDate, HasOverdueFee, ReleasedAt, ReleaseReason, CreatedAt,
           SeatLabel, SectionId, SectionName, SectionGender, StudentName, StudentMobile, StudentEmail
    FROM dbo.vw_BookingDetails
    WHERE UserId = @UserId
      AND (Status = 2 OR (Status = 1 AND HoldExpiresAt > SYSUTCDATETIME()));
END
