-- Which channels a rule sends on, and with which template
CREATE TABLE dbo.NotificationRuleChannels
(
    RuleId     INT     NOT NULL CONSTRAINT FK_NotificationRuleChannels_Rules REFERENCES dbo.NotificationRules (Id) ON DELETE CASCADE,
    Channel    TINYINT NOT NULL,
    TemplateId INT     NOT NULL CONSTRAINT FK_NotificationRuleChannels_Templates REFERENCES dbo.NotificationTemplates (Id),
    CONSTRAINT PK_NotificationRuleChannels PRIMARY KEY (RuleId, Channel)
);
GO
