-- Status: 1 = Sent, 2 = Failed, 3 = Mocked, 4 = Skipped
CREATE TABLE dbo.NotificationLogs
(
    Id                BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NotificationLogs PRIMARY KEY,
    UserId            INT                  NOT NULL CONSTRAINT FK_NotificationLogs_Users REFERENCES dbo.Users (Id),
    RuleId            INT                  NULL CONSTRAINT FK_NotificationLogs_Rules REFERENCES dbo.NotificationRules (Id),
    FeeInvoiceId      INT                  NULL CONSTRAINT FK_NotificationLogs_FeeInvoices REFERENCES dbo.FeeInvoices (Id),
    Channel           TINYINT              NOT NULL,
    Recipient         NVARCHAR(256)        NOT NULL,
    Message           NVARCHAR(MAX)        NOT NULL,
    Status            TINYINT              NOT NULL,
    Provider          VARCHAR(30)          NOT NULL,
    ProviderMessageId VARCHAR(100)         NULL,
    Error             NVARCHAR(1000)       NULL,
    CreatedAt         DATETIME2(0)         NOT NULL CONSTRAINT DF_NotificationLogs_CreatedAt DEFAULT (SYSUTCDATETIME())
);
GO

CREATE INDEX IX_NotificationLogs_Dedupe ON dbo.NotificationLogs (FeeInvoiceId, RuleId, Channel, CreatedAt);
GO

CREATE INDEX IX_NotificationLogs_UserId ON dbo.NotificationLogs (UserId);
GO
