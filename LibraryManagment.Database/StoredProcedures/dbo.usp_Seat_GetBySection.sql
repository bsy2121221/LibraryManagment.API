-- Admin view of a section's seats, with the student currently holding or occupying each seat.
-- The filtered unique index on dbo.Bookings guarantees at most one live booking per seat.
CREATE PROCEDURE dbo.usp_Seat_GetBySection
    @SectionId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();

    SELECT st.Id, st.SectionId, st.SeatLabel, st.PosX, st.PosY, st.Rotation, st.Status,
           b.Id            AS BookingId,
           b.Status        AS BookingStatus,
           b.HoldExpiresAt,
           b.StartDate,
           COALESCE((SELECT MIN(i.DueDate) FROM dbo.FeeInvoices i
                     WHERE i.BookingId = b.Id AND i.Status IN (1, 3)), b.NextDueDate) AS NextDueDate,
           u.Id            AS StudentId,
           u.FullName      AS StudentName,
           u.Mobile        AS StudentMobile
    FROM dbo.Seats st
    LEFT JOIN dbo.Bookings b
           ON b.SeatId = st.Id
          AND (b.Status = 2 OR (b.Status = 1 AND b.HoldExpiresAt > @Now))
    LEFT JOIN dbo.Users u ON u.Id = b.UserId
    WHERE st.SectionId = @SectionId AND st.IsDeleted = 0
    ORDER BY st.PosY, st.PosX, st.SeatLabel;
END
