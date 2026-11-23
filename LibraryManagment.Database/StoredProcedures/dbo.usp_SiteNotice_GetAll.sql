-- Admin: every notice, including inactive and expired ones
CREATE PROCEDURE dbo.usp_SiteNotice_GetAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Message, StartDate, EndDate, IsActive, SortOrder, CreatedAt, CreatedByUserId, UpdatedAt
    FROM dbo.SiteNotices
    ORDER BY SortOrder, Id DESC;
END
