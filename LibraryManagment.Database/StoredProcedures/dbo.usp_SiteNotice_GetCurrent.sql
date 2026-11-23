-- Public: active notices whose date range includes @Today (the library's local date, passed by the API)
CREATE PROCEDURE dbo.usp_SiteNotice_GetCurrent
    @Today DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Message, StartDate, EndDate, IsActive, SortOrder, CreatedAt, CreatedByUserId, UpdatedAt
    FROM dbo.SiteNotices
    WHERE IsActive = 1
      AND (StartDate IS NULL OR StartDate <= @Today)
      AND (EndDate IS NULL OR EndDate >= @Today)
    ORDER BY SortOrder, Id DESC;
END
