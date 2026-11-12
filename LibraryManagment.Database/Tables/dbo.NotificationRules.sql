-- Trigger: 1 = BeforeDue, 2 = OnDue, 3 = Overdue
-- RepeatEveryMinutes: NULL = send once, 60 = hourly, 1440 = daily
CREATE TABLE dbo.NotificationRules
(
    Id                 INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NotificationRules PRIMARY KEY,
    Name               NVARCHAR(100)     NOT NULL,
    [Trigger]          TINYINT           NOT NULL CONSTRAINT CK_NotificationRules_Trigger CHECK ([Trigger] IN (1, 2, 3)),
    OffsetDays         INT               NOT NULL CONSTRAINT DF_NotificationRules_OffsetDays DEFAULT (0),
    RepeatEveryMinutes INT               NULL,
    MaxAttempts        INT               NOT NULL CONSTRAINT DF_NotificationRules_MaxAttempts DEFAULT (1),
    QuietHoursStart    TIME(0)           NULL,
    QuietHoursEnd      TIME(0)           NULL,
    IsActive           BIT               NOT NULL CONSTRAINT DF_NotificationRules_IsActive DEFAULT (1),
    CreatedAt          DATETIME2(0)      NOT NULL CONSTRAINT DF_NotificationRules_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt          DATETIME2(0)      NULL
);
GO
