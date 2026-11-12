-- Admin list of bookings. @Status NULL = live bookings (Active + unexpired Held).
-- @Search matches student name, mobile or seat label.
CREATE PROCEDURE dbo.usp_Booking_GetAllForAdmin
    @Status TINYINT       = NULL,
    @Search NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();
    DECLARE @Pattern NVARCHAR(102) = IIF(NULLIF(LTRIM(RTRIM(@Search)), N'') IS NULL, NULL, CONCAT(N'%', LTRIM(RTRIM(@Search)), N'%'));

    SELECT TOP (500)
           d.Id, d.UserId, d.SeatId, d.Status, d.HoldExpiresAt, d.StartDate, d.NextDueDate, d.ReleasedAt, d.ReleaseReason, d.CreatedAt,
           d.SeatLabel, d.SectionId, d.SectionName, d.SectionGender, d.StudentName, d.StudentMobile, d.StudentEmail
    FROM dbo.vw_BookingDetails d
    JOIN dbo.Sections sec ON sec.Id = d.SectionId
    WHERE ((@Status IS NULL AND (d.Status = 2 OR (d.Status = 1 AND d.HoldExpiresAt > @Now)))
           OR d.Status = @Status)
      AND (@Pattern IS NULL OR d.StudentName LIKE @Pattern OR d.StudentMobile LIKE @Pattern OR d.SeatLabel LIKE @Pattern)
    ORDER BY sec.DisplayOrder, d.SectionName, LEN(d.SeatLabel), d.SeatLabel, d.Id DESC;
END
