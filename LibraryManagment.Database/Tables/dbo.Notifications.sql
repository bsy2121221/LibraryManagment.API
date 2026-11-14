/*
Outbox and history of every message sent to a student. Rows are written in the same transaction as the
change that caused them (seat booked, profile edited, fee reminder...) with Status = Ready; the API's
background job picks Ready rows every minute and sends them, so no request ever waits for SMS/email.

Subject/Body hold the template text with {Placeholders} until sent, then the final text that was sent.
DataJson holds the placeholder values, e.g. {"Name":"Ravi","Seat":"B12"}.

Status:  1 = Ready, 2 = Processing, 3 = Sent, 4 = Failed, 5 = Skipped, 6 = Cancelled
Channel: 1 = Sms, 2 = WhatsApp, 3 = Email, 4 = Call
*/
CREATE TABLE dbo.Notifications
(
    Id                BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Notifications PRIMARY KEY,
    UserId            INT                  NOT NULL CONSTRAINT FK_Notifications_Users REFERENCES dbo.Users (Id),
    EventType         VARCHAR(50)          NOT NULL,
    Channel           TINYINT              NOT NULL CONSTRAINT CK_Notifications_Channel CHECK (Channel IN (1, 2, 3, 4)),
    Recipient         NVARCHAR(256)        NOT NULL,
    Subject           NVARCHAR(200)        NULL,
    Body              NVARCHAR(MAX)        NOT NULL,
    DataJson          NVARCHAR(MAX)        NULL CONSTRAINT CK_Notifications_DataJson CHECK (DataJson IS NULL OR ISJSON(DataJson) = 1),
    Status            TINYINT              NOT NULL CONSTRAINT CK_Notifications_Status CHECK (Status BETWEEN 1 AND 6),
    Attempts          INT                  NOT NULL CONSTRAINT DF_Notifications_Attempts DEFAULT (0),
    MaxAttempts       INT                  NOT NULL CONSTRAINT DF_Notifications_MaxAttempts DEFAULT (3),
    NextAttemptAt     DATETIME2(0)         NOT NULL CONSTRAINT DF_Notifications_NextAttemptAt DEFAULT (SYSUTCDATETIME()),
    LockedUntil       DATETIME2(0)         NULL,
    Provider          VARCHAR(30)          NULL,
    ProviderMessageId VARCHAR(100)         NULL,
    LastError         NVARCHAR(1000)       NULL,
    RuleId            INT                  NULL CONSTRAINT FK_Notifications_Rules REFERENCES dbo.NotificationRules (Id),
    FeeInvoiceId      INT                  NULL CONSTRAINT FK_Notifications_FeeInvoices REFERENCES dbo.FeeInvoices (Id),
    CreatedAt         DATETIME2(0)         NOT NULL CONSTRAINT DF_Notifications_CreatedAt DEFAULT (SYSUTCDATETIME()),
    SentAt            DATETIME2(0)         NULL,
    UpdatedAt         DATETIME2(0)         NULL
);
GO
-- What the dispatcher picks up
CREATE INDEX IX_Notifications_Ready ON dbo.Notifications (NextAttemptAt, Id) WHERE Status = 1;
GO
CREATE INDEX IX_Notifications_Processing ON dbo.Notifications (LockedUntil) WHERE Status = 2;
GO
-- How many reminders a rule already sent for an invoice
CREATE INDEX IX_Notifications_Reminders ON dbo.Notifications (FeeInvoiceId, RuleId, Channel) INCLUDE (CreatedAt, Status) WHERE RuleId IS NOT NULL;
GO
CREATE INDEX IX_Notifications_UserId ON dbo.Notifications (UserId, Id);
GO
