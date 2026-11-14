-- Fee reminder rules. Result sets: 1. rules  2. their channels with message text
CREATE PROCEDURE dbo.usp_NotificationRule_GetAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Name, [Trigger], OffsetDays, RepeatEveryMinutes, MaxAttempts, QuietHoursStart, QuietHoursEnd,
           IsActive, CreatedAt, UpdatedAt
    FROM dbo.NotificationRules
    ORDER BY [Trigger], OffsetDays, Id;

    SELECT rc.RuleId, rc.Channel, t.Subject, t.Body
    FROM dbo.NotificationRuleChannels rc
    JOIN dbo.NotificationTemplates t ON t.Id = rc.TemplateId
    ORDER BY rc.RuleId, rc.Channel;
END
