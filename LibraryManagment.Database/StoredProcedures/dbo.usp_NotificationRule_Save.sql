/*
Creates (@Id NULL) or updates a fee reminder rule and replaces its channels.
@ChannelsJson: [{ "Channel": 1, "Subject": null, "Body": "Dear {Name}..." }, ...]
Each rule channel owns its template, named "Rule <Id> - <Channel>". Returns the rule Id.
Errors: 50400 invalid channels, 50404 rule not found
*/
CREATE PROCEDURE dbo.usp_NotificationRule_Save
    @Id                 INT,
    @Name               NVARCHAR(100),
    @Trigger            TINYINT,
    @OffsetDays         INT,
    @RepeatEveryMinutes INT,
    @MaxAttempts        INT,
    @QuietHoursStart    TIME(0),
    @QuietHoursEnd      TIME(0),
    @IsActive           BIT,
    @ChannelsJson       NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();

    IF @ChannelsJson IS NULL OR ISJSON(@ChannelsJson) = 0
        THROW 50400, N'Channels are not valid JSON.', 1;

    DECLARE @Channels TABLE (Channel TINYINT NOT NULL PRIMARY KEY, Subject NVARCHAR(200) NULL, Body NVARCHAR(MAX) NOT NULL);
    INSERT INTO @Channels (Channel, Subject, Body)
    SELECT Channel, Subject, Body
    FROM OPENJSON(@ChannelsJson) WITH (Channel TINYINT '$.Channel', Subject NVARCHAR(200) '$.Subject', Body NVARCHAR(MAX) '$.Body')
    WHERE Channel BETWEEN 1 AND 4 AND Body IS NOT NULL;

    IF NOT EXISTS (SELECT 1 FROM @Channels)
        THROW 50400, N'Choose at least one channel with a message.', 1;

    BEGIN TRANSACTION;

    IF @Id IS NULL
    BEGIN
        INSERT INTO dbo.NotificationRules (Name, [Trigger], OffsetDays, RepeatEveryMinutes, MaxAttempts, QuietHoursStart, QuietHoursEnd, IsActive)
        VALUES (@Name, @Trigger, @OffsetDays, @RepeatEveryMinutes, @MaxAttempts, @QuietHoursStart, @QuietHoursEnd, @IsActive);
        SET @Id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.NotificationRules
        SET Name = @Name, [Trigger] = @Trigger, OffsetDays = @OffsetDays, RepeatEveryMinutes = @RepeatEveryMinutes,
            MaxAttempts = @MaxAttempts, QuietHoursStart = @QuietHoursStart, QuietHoursEnd = @QuietHoursEnd,
            IsActive = @IsActive, UpdatedAt = @Now
        WHERE Id = @Id;

        IF @@ROWCOUNT = 0
            THROW 50404, N'Reminder rule not found.', 1;
    END

    -- Channels no longer used: remove them and their templates
    DECLARE @Removed TABLE (TemplateId INT);
    DELETE rc
    OUTPUT deleted.TemplateId INTO @Removed
    FROM dbo.NotificationRuleChannels rc
    WHERE rc.RuleId = @Id AND NOT EXISTS (SELECT 1 FROM @Channels c WHERE c.Channel = rc.Channel);

    DELETE t
    FROM dbo.NotificationTemplates t
    JOIN @Removed r ON r.TemplateId = t.Id
    WHERE NOT EXISTS (SELECT 1 FROM dbo.NotificationRuleChannels x WHERE x.TemplateId = t.Id)
      AND NOT EXISTS (SELECT 1 FROM dbo.NotificationEventChannels x WHERE x.TemplateId = t.Id);

    -- Existing channels: update their message
    UPDATE t
    SET Subject = c.Subject, Body = c.Body, IsActive = 1, UpdatedAt = @Now
    FROM dbo.NotificationTemplates t
    JOIN dbo.NotificationRuleChannels rc ON rc.TemplateId = t.Id AND rc.RuleId = @Id
    JOIN @Channels c ON c.Channel = rc.Channel;

    -- New channels: create a template for each
    DECLARE @Channel TINYINT, @TemplateId INT, @TemplateName NVARCHAR(100);
    DECLARE @NewChannels TABLE (Channel TINYINT PRIMARY KEY);
    INSERT INTO @NewChannels (Channel)
    SELECT c.Channel FROM @Channels c
    WHERE NOT EXISTS (SELECT 1 FROM dbo.NotificationRuleChannels rc WHERE rc.RuleId = @Id AND rc.Channel = c.Channel);

    WHILE EXISTS (SELECT 1 FROM @NewChannels)
    BEGIN
        SELECT TOP (1) @Channel = Channel FROM @NewChannels ORDER BY Channel;
        SET @TemplateName = CONCAT(N'Rule ', @Id, N' - ', CHOOSE(@Channel, N'SMS', N'WhatsApp', N'Email', N'Call'));
        SET @TemplateId = (SELECT Id FROM dbo.NotificationTemplates WHERE Name = @TemplateName);

        IF @TemplateId IS NULL
        BEGIN
            INSERT INTO dbo.NotificationTemplates (Name, Channel, Subject, Body)
            SELECT @TemplateName, Channel, Subject, Body FROM @Channels WHERE Channel = @Channel;
            SET @TemplateId = SCOPE_IDENTITY();
        END
        ELSE
            UPDATE t SET Subject = c.Subject, Body = c.Body, IsActive = 1, UpdatedAt = @Now
            FROM dbo.NotificationTemplates t JOIN @Channels c ON c.Channel = @Channel
            WHERE t.Id = @TemplateId;

        INSERT INTO dbo.NotificationRuleChannels (RuleId, Channel, TemplateId) VALUES (@Id, @Channel, @TemplateId);
        DELETE FROM @NewChannels WHERE Channel = @Channel;
    END

    COMMIT TRANSACTION;

    SELECT @Id AS Id;
END
