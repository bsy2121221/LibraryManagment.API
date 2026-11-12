/*
Public seat map. Returns two result sets:
  1. active sections
  2. their seats with Availability: 1 = Available, 2 = Occupied (held or booked), 3 = Disabled
@UserId (optional) marks the caller's own held/booked seat with IsMine = 1. Student identities are never returned.
*/
CREATE PROCEDURE dbo.usp_SeatMap_Get
    @UserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();

    SELECT s.Id, s.Name, s.Gender, s.LayoutWidth, s.LayoutHeight, s.BackgroundImageUrl, s.DisplayOrder
    FROM dbo.Sections s
    WHERE s.IsActive = 1
    ORDER BY s.DisplayOrder, s.Name;

    SELECT st.Id, st.SectionId, st.SeatLabel, st.PosX, st.PosY, st.Rotation,
           CAST(CASE WHEN st.Status = 2 THEN 3
                     WHEN b.Id IS NOT NULL THEN 2
                     ELSE 1 END AS TINYINT) AS Availability,
           CAST(CASE WHEN @UserId IS NOT NULL AND b.UserId = @UserId THEN 1 ELSE 0 END AS BIT) AS IsMine
    FROM dbo.Seats st
    JOIN dbo.Sections s ON s.Id = st.SectionId AND s.IsActive = 1
    LEFT JOIN dbo.Bookings b
           ON b.SeatId = st.Id
          AND (b.Status = 2 OR (b.Status = 1 AND b.HoldExpiresAt > @Now))
    WHERE st.IsDeleted = 0
    ORDER BY st.SectionId, st.PosY, st.PosX;
END
