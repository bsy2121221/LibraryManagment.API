-- Status: 1 = Draft, 2 = Published. Every save creates a new version.
CREATE TABLE dbo.HomePageLayouts
(
    Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HomePageLayouts PRIMARY KEY,
    Version         INT               NOT NULL,
    LayoutJson      NVARCHAR(MAX)     NOT NULL CONSTRAINT CK_HomePageLayouts_Json CHECK (ISJSON(LayoutJson) = 1),
    Status          TINYINT           NOT NULL CONSTRAINT CK_HomePageLayouts_Status CHECK (Status IN (1, 2)),
    CreatedByUserId INT               NOT NULL CONSTRAINT FK_HomePageLayouts_Users REFERENCES dbo.Users (Id),
    CreatedAt       DATETIME2(0)      NOT NULL CONSTRAINT DF_HomePageLayouts_CreatedAt DEFAULT (SYSUTCDATETIME()),
    PublishedAt     DATETIME2(0)      NULL
);
GO

CREATE UNIQUE INDEX UX_HomePageLayouts_Version ON dbo.HomePageLayouts (Version);
GO
