-- Every configured event channel with its message template
CREATE PROCEDURE dbo.usp_NotificationEvent_GetAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ec.EventType, ec.Channel, ec.IsActive, t.Id AS TemplateId, t.Subject, t.Body, ec.UpdatedAt
    FROM dbo.NotificationEventChannels ec
    JOIN dbo.NotificationTemplates t ON t.Id = ec.TemplateId
    ORDER BY ec.EventType, ec.Channel;
END
