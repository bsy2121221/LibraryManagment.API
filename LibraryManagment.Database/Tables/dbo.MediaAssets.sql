CREATE TABLE dbo.MediaAssets
(
    Id               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MediaAssets PRIMARY KEY,
    FileName         NVARCHAR(255)     NOT NULL,
    StoredFileName   NVARCHAR(255)     NOT NULL,
    Url              NVARCHAR(500)     NOT NULL,
    ContentType      VARCHAR(100)      NOT NULL,
    SizeBytes        BIGINT            NOT NULL,
    UploadedByUserId INT               NOT NULL CONSTRAINT FK_MediaAssets_Users REFERENCES dbo.Users (Id),
    CreatedAt        DATETIME2(0)      NOT NULL CONSTRAINT DF_MediaAssets_CreatedAt DEFAULT (SYSUTCDATETIME())
);
GO
