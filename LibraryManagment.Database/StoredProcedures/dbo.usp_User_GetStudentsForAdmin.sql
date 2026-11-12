-- Admin list of students with their current seat (Active booking or unexpired hold), if any.
-- @Search matches name, mobile, email or seat label. @IsActive / @HasSeat NULL = no filter.
CREATE PROCEDURE dbo.usp_User_GetStudentsForAdmin
    @Search   NVARCHAR(100) = NULL,
    @IsActive BIT           = NULL,
    @HasSeat  BIT           = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();
    DECLARE @Pattern NVARCHAR(102) = IIF(NULLIF(LTRIM(RTRIM(@Search)), N'') IS NULL, NULL, CONCAT(N'%', LTRIM(RTRIM(@Search)), N'%'));

    SELECT TOP (500)
           u.Id, u.FullName, u.Mobile, u.Email, u.Gender, u.Address, u.IsActive, u.CreatedAt, u.UpdatedAt,
           b.Id          AS BookingId,
           b.Status      AS BookingStatus,
           b.SeatLabel,
           b.SectionName,
           b.StartDate,
           b.NextDueDate
    FROM dbo.Users u
    OUTER APPLY (SELECT TOP (1) d.Id, d.Status, d.SeatLabel, d.SectionName, d.StartDate, d.NextDueDate
                 FROM dbo.vw_BookingDetails d
                 WHERE d.UserId = u.Id
                   AND (d.Status = 2 OR (d.Status = 1 AND d.HoldExpiresAt > @Now))) b
    WHERE u.Role = 2
      AND (@IsActive IS NULL OR u.IsActive = @IsActive)
      AND (@HasSeat IS NULL OR (@HasSeat = 1 AND b.Id IS NOT NULL) OR (@HasSeat = 0 AND b.Id IS NULL))
      AND (@Pattern IS NULL OR u.FullName LIKE @Pattern OR u.Mobile LIKE @Pattern
           OR u.Email LIKE @Pattern OR b.SeatLabel LIKE @Pattern)
    ORDER BY u.FullName, u.Id;
END
