-- Saved versions for the builder's history list (without the JSON), newest first.
-- IsLive marks the version visitors currently see.
CREATE PROCEDURE dbo.usp_HomePageLayout_GetHistory
    @Top INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @LiveId INT =
        (SELECT TOP (1) Id FROM dbo.HomePageLayouts WHERE Status = 2 ORDER BY PublishedAt DESC, Version DESC);

    SELECT TOP (@Top) l.Id, l.Version, l.Status, l.CreatedByUserId, u.FullName AS CreatedByName, l.CreatedAt, l.PublishedAt,
           CAST(CASE WHEN l.Id = @LiveId THEN 1 ELSE 0 END AS BIT) AS IsLive
    FROM dbo.HomePageLayouts l
    JOIN dbo.Users u ON u.Id = l.CreatedByUserId
    ORDER BY l.Version DESC;
END
