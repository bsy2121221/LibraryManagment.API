-- Deletes an image record and returns its StoredFileName so the API can remove the file.
-- Refused while the image is used by a section, the live home page or the latest saved home page version.
-- Errors: 50404 not found, 50409 still in use
CREATE PROCEDURE dbo.usp_MediaAsset_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Url NVARCHAR(500), @StoredFileName NVARCHAR(255);

    BEGIN TRANSACTION;

    SELECT @Url = Url, @StoredFileName = StoredFileName
    FROM dbo.MediaAssets WITH (UPDLOCK, HOLDLOCK)
    WHERE Id = @Id;

    IF @Url IS NULL
        THROW 50404, N'Image not found.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Sections WHERE BackgroundImageUrl = @Url)
        THROW 50409, N'This image is the background of a section. Remove it there first.', 1;

    -- Live version = most recently published; latest version = what the builder opens
    IF EXISTS (SELECT 1
               FROM (SELECT TOP (1) LayoutJson FROM dbo.HomePageLayouts WHERE Status = 2 ORDER BY PublishedAt DESC, Version DESC) AS live
               WHERE CHARINDEX(@Url, live.LayoutJson) > 0)
       OR EXISTS (SELECT 1
                  FROM (SELECT TOP (1) LayoutJson FROM dbo.HomePageLayouts ORDER BY Version DESC) AS latest
                  WHERE CHARINDEX(@Url, latest.LayoutJson) > 0)
        THROW 50409, N'This image is used on the home page. Remove it from the page and save first.', 1;

    DELETE FROM dbo.MediaAssets WHERE Id = @Id;

    COMMIT TRANSACTION;

    SELECT @StoredFileName AS StoredFileName;
END
