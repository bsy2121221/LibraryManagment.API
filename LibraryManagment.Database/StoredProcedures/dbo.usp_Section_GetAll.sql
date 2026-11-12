-- Admin list of every section (active or not) with seat counts.
-- A seat is occupied when it has an Active booking or a Held booking whose hold has not expired yet.
CREATE PROCEDURE dbo.usp_Section_GetAll
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();

    SELECT s.Id, s.Name, s.Gender, s.LayoutWidth, s.LayoutHeight, s.BackgroundImageUrl,
           s.DisplayOrder, s.IsActive, s.CreatedAt, s.UpdatedAt,
           (SELECT COUNT(*) FROM dbo.Seats st
             WHERE st.SectionId = s.Id AND st.IsDeleted = 0) AS TotalSeats,
           (SELECT COUNT(*) FROM dbo.Seats st
             WHERE st.SectionId = s.Id AND st.IsDeleted = 0 AND st.Status = 2) AS DisabledSeats,
           (SELECT COUNT(*) FROM dbo.Seats st
              JOIN dbo.Bookings b ON b.SeatId = st.Id
             WHERE st.SectionId = s.Id AND st.IsDeleted = 0
               AND (b.Status = 2 OR (b.Status = 1 AND b.HoldExpiresAt > @Now))) AS OccupiedSeats
    FROM dbo.Sections s
    ORDER BY s.DisplayOrder, s.Name;
END
