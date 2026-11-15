-- The live home page: the most recently published version. No rows = nothing published yet.
CREATE PROCEDURE dbo.usp_HomePageLayout_GetPublished
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1) l.Id, l.Version, l.LayoutJson, l.Status, l.CreatedByUserId, u.FullName AS CreatedByName, l.CreatedAt, l.PublishedAt
    FROM dbo.HomePageLayouts l
    JOIN dbo.Users u ON u.Id = l.CreatedByUserId
    WHERE l.Status = 2
    ORDER BY l.PublishedAt DESC, l.Version DESC;
END
