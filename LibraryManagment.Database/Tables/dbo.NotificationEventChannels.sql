-- Which channels each event (SeatBooked, SeatMoved, ProfileUpdated...) is sent on, and the template for each.
-- Channel: 1 = Sms, 2 = WhatsApp, 3 = Email, 4 = Call
CREATE TABLE dbo.NotificationEventChannels
(
    EventType  VARCHAR(50)  NOT NULL,
    Channel    TINYINT      NOT NULL CONSTRAINT CK_NotificationEventChannels_Channel CHECK (Channel IN (1, 2, 3, 4)),
    TemplateId INT          NOT NULL CONSTRAINT FK_NotificationEventChannels_Templates REFERENCES dbo.NotificationTemplates (Id),
    IsActive   BIT          NOT NULL CONSTRAINT DF_NotificationEventChannels_IsActive DEFAULT (1),
    UpdatedAt  DATETIME2(0) NULL,
    CONSTRAINT PK_NotificationEventChannels PRIMARY KEY (EventType, Channel)
);
GO
