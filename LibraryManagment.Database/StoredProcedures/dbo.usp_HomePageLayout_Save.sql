/*
Saves the home page design as a new version (every save is kept, so older designs can be restored).
@Publish = 1 makes it the live home page straight away. The JSON content is validated by the API.
Returns the new version. Errors: 50400 invalid JSON
*/
CREATE PROCEDURE dbo.usp_HomePageLayout_Save
    @LayoutJson      NVARCHAR(MAX),
    @Publish         BIT,
    @CreatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @LayoutJson IS NULL OR ISJSON(@LayoutJson) = 0
        THROW 50400, N'The page design is not valid JSON.', 1;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();
    DECLARE @Version INT;

    BEGIN TRANSACTION;

    SELECT @Version = ISNULL(MAX(Version), 0) + 1
    FROM dbo.HomePageLayouts WITH (UPDLOCK, HOLDLOCK);

    INSERT INTO dbo.HomePageLayouts (Version, LayoutJson, Status, CreatedByUserId, CreatedAt, PublishedAt)
    VALUES (@Version, @LayoutJson,
            CASE WHEN @Publish = 1 THEN 2 ELSE 1 END,
            @CreatedByUserId, @Now,
            CASE WHEN @Publish = 1 THEN @Now END);

    COMMIT TRANSACTION;

    SELECT l.Id, l.Version, l.LayoutJson, l.Status, l.CreatedByUserId, u.FullName AS CreatedByName, l.CreatedAt, l.PublishedAt
    FROM dbo.HomePageLayouts l
    JOIN dbo.Users u ON u.Id = l.CreatedByUserId
    WHERE l.Version = @Version;
END
