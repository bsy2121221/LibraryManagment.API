CREATE PROCEDURE dbo.usp_Section_GetById
    @Id INT
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
    WHERE s.Id = @Id;
END
