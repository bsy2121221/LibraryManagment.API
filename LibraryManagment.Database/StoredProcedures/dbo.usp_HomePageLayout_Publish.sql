-- Makes a saved version the live home page (also used to roll back to an older version).
-- PublishedAt is refreshed even if it was published before, so it becomes the live one again.
-- Errors: 50404 not found
CREATE PROCEDURE dbo.usp_HomePageLayout_Publish
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.HomePageLayouts
    SET Status = 2, PublishedAt = SYSUTCDATETIME()
    WHERE Id = @Id;

    IF @@ROWCOUNT = 0
        THROW 50404, N'Home page version not found.', 1;
END
