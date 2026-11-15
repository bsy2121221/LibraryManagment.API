-- Media library for the admin, newest first
CREATE PROCEDURE dbo.usp_MediaAsset_GetAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT m.Id, m.FileName, m.StoredFileName, m.Url, m.ContentType, m.SizeBytes, m.UploadedByUserId,
           u.FullName AS UploadedByName, m.CreatedAt
    FROM dbo.MediaAssets m
    JOIN dbo.Users u ON u.Id = m.UploadedByUserId
    ORDER BY m.CreatedAt DESC, m.Id DESC;
END
