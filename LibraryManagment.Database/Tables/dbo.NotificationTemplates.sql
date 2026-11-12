-- Channel: 1 = Sms, 2 = WhatsApp, 3 = Email, 4 = Call
-- Body placeholders: {Name}, {Amount}, {DueDate}, {LibraryName}
CREATE TABLE dbo.NotificationTemplates
(
    Id        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NotificationTemplates PRIMARY KEY,
    Name      NVARCHAR(100)     NOT NULL,
    Channel   TINYINT           NOT NULL CONSTRAINT CK_NotificationTemplates_Channel CHECK (Channel IN (1, 2, 3, 4)),
    Subject   NVARCHAR(200)     NULL,
    Body      NVARCHAR(MAX)     NOT NULL,
    IsActive  BIT               NOT NULL CONSTRAINT DF_NotificationTemplates_IsActive DEFAULT (1),
    CreatedAt DATETIME2(0)      NOT NULL CONSTRAINT DF_NotificationTemplates_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt DATETIME2(0)      NULL
);
GO

CREATE UNIQUE INDEX UX_NotificationTemplates_Name ON dbo.NotificationTemplates (Name);
GO
