CREATE PROCEDURE dbo.usp_Booking_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, UserId, SeatId, Status, HoldExpiresAt, StartDate, NextDueDate, HasOverdueFee, ReleasedAt, ReleaseReason, CreatedAt,
           SeatLabel, SectionId, SectionName, SectionGender, StudentName, StudentMobile, StudentEmail
    FROM dbo.vw_BookingDetails
    WHERE Id = @Id;
END
