-- Deletes a reminder rule and its templates. Messages it already queued stay in the log. Errors: 50404
CREATE PROCEDURE dbo.usp_NotificationRule_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    IF NOT EXISTS (SELECT 1 FROM dbo.NotificationRules WITH (UPDLOCK, HOLDLOCK) WHERE Id = @Id)
        THROW 50404, N'Reminder rule not found.', 1;

    UPDATE dbo.Notifications SET RuleId = NULL WHERE RuleId = @Id;

    DECLARE @Templates TABLE (TemplateId INT);
    DELETE FROM dbo.NotificationRuleChannels
    OUTPUT deleted.TemplateId INTO @Templates
    WHERE RuleId = @Id;

    DELETE FROM dbo.NotificationRules WHERE Id = @Id;

    DELETE t
    FROM dbo.NotificationTemplates t
    JOIN @Templates x ON x.TemplateId = t.Id
    WHERE NOT EXISTS (SELECT 1 FROM dbo.NotificationRuleChannels rc WHERE rc.TemplateId = t.Id)
      AND NOT EXISTS (SELECT 1 FROM dbo.NotificationEventChannels ec WHERE ec.TemplateId = t.Id);

    COMMIT TRANSACTION;
END
