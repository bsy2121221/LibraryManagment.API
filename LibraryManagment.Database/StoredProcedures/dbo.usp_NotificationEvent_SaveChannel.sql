-- Switches one channel of an event on/off and saves its message text (creates the template the first time).
-- Templates for events are named "Event <EventType> - <Channel>".
CREATE PROCEDURE dbo.usp_NotificationEvent_SaveChannel
    @EventType VARCHAR(50),
    @Channel   TINYINT,
    @IsActive  BIT,
    @Subject   NVARCHAR(200),
    @Body      NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();
    DECLARE @TemplateId INT;
    DECLARE @TemplateName NVARCHAR(100) = CONCAT(N'Event ', @EventType, N' - ', CHOOSE(@Channel, N'SMS', N'WhatsApp', N'Email', N'Call'));

    BEGIN TRANSACTION;

    SELECT @TemplateId = TemplateId
    FROM dbo.NotificationEventChannels WITH (UPDLOCK, HOLDLOCK)
    WHERE EventType = @EventType AND Channel = @Channel;

    IF @TemplateId IS NULL
        SELECT @TemplateId = Id FROM dbo.NotificationTemplates WITH (UPDLOCK, HOLDLOCK) WHERE Name = @TemplateName;

    IF @TemplateId IS NULL
    BEGIN
        INSERT INTO dbo.NotificationTemplates (Name, Channel, Subject, Body)
        VALUES (@TemplateName, @Channel, @Subject, @Body);
        SET @TemplateId = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.NotificationTemplates
        SET Subject = @Subject, Body = @Body, IsActive = 1, UpdatedAt = @Now
        WHERE Id = @TemplateId;
    END

    IF EXISTS (SELECT 1 FROM dbo.NotificationEventChannels WHERE EventType = @EventType AND Channel = @Channel)
        UPDATE dbo.NotificationEventChannels
        SET IsActive = @IsActive, TemplateId = @TemplateId, UpdatedAt = @Now
        WHERE EventType = @EventType AND Channel = @Channel;
    ELSE
        INSERT INTO dbo.NotificationEventChannels (EventType, Channel, TemplateId, IsActive, UpdatedAt)
        VALUES (@EventType, @Channel, @TemplateId, @IsActive, @Now);

    COMMIT TRANSACTION;
END
