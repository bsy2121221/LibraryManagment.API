-- One version with its JSON. @Id NULL = the latest saved version (what the builder opens).
CREATE PROCEDURE dbo.usp_HomePageLayout_GetById
    @Id INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1) l.Id, l.Version, l.LayoutJson, l.Status, l.CreatedByUserId, u.FullName AS CreatedByName, l.CreatedAt, l.PublishedAt
    FROM dbo.HomePageLayouts l
    JOIN dbo.Users u ON u.Id = l.CreatedByUserId
    WHERE @Id IS NULL OR l.Id = @Id
    ORDER BY l.Version DESC;
END
