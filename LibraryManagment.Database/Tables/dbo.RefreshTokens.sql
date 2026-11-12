CREATE TABLE dbo.RefreshTokens
(
    Id                  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RefreshTokens PRIMARY KEY,
    UserId              INT               NOT NULL CONSTRAINT FK_RefreshTokens_Users REFERENCES dbo.Users (Id),
    TokenHash           CHAR(64)          NOT NULL,
    ExpiresAt           DATETIME2(0)      NOT NULL,
    CreatedAt           DATETIME2(0)      NOT NULL CONSTRAINT DF_RefreshTokens_CreatedAt DEFAULT (SYSUTCDATETIME()),
    RevokedAt           DATETIME2(0)      NULL,
    ReplacedByTokenHash CHAR(64)          NULL
);
GO

CREATE UNIQUE INDEX UX_RefreshTokens_TokenHash ON dbo.RefreshTokens (TokenHash);
GO

CREATE INDEX IX_RefreshTokens_UserId ON dbo.RefreshTokens (UserId);
GO
