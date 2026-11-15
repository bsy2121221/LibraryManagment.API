-- Records an uploaded image. The file itself is already stored by the API (Storage:Provider).
CREATE PROCEDURE dbo.usp_MediaAsset_Create
    @FileName         NVARCHAR(255),
    @StoredFileName   NVARCHAR(255),
    @Url              NVARCHAR(500),
    @ContentType      VARCHAR(100),
    @SizeBytes        BIGINT,
    @UploadedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Id INT;

    INSERT INTO dbo.MediaAssets (FileName, StoredFileName, Url, ContentType, SizeBytes, UploadedByUserId)
    VALUES (@FileName, @StoredFileName, @Url, @ContentType, @SizeBytes, @UploadedByUserId);

    SET @Id = SCOPE_IDENTITY();

    SELECT m.Id, m.FileName, m.StoredFileName, m.Url, m.ContentType, m.SizeBytes, m.UploadedByUserId,
           u.FullName AS UploadedByName, m.CreatedAt
    FROM dbo.MediaAssets m
    JOIN dbo.Users u ON u.Id = m.UploadedByUserId
    WHERE m.Id = @Id;
END
