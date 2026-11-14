/*
Queues the messages for one event (e.g. SeatBooked) for one student: one Ready row per channel that is switched on
for the event in dbo.NotificationEventChannels. Meant to be called INSIDE the transaction of the change itself,
so the messages are saved if and only if the change is. Nothing is sent here; the background job sends them.

@DataJson: placeholder values, e.g. {"Seat":"B12","Section":"Boys"}. Name is added automatically.
Channels without a recipient (no email address) are not queued.
*/
CREATE PROCEDURE dbo.usp_Notification_EnqueueEvent
    @UserId       INT,
    @EventType    VARCHAR(50),
    @DataJson     NVARCHAR(MAX) = NULL,
    @FeeInvoiceId INT           = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Notifications (UserId, EventType, Channel, Recipient, Subject, Body, DataJson, Status, FeeInvoiceId)
    SELECT u.Id,
           @EventType,
           ec.Channel,
           IIF(ec.Channel = 3, u.Email, u.Mobile),
           t.Subject,
           t.Body,
           JSON_MODIFY(ISNULL(@DataJson, N'{}'), '$.Name', u.FullName),
           1,
           @FeeInvoiceId
    FROM dbo.NotificationEventChannels ec
    JOIN dbo.NotificationTemplates t ON t.Id = ec.TemplateId AND t.IsActive = 1
    JOIN dbo.Users u ON u.Id = @UserId
    WHERE ec.EventType = @EventType
      AND ec.IsActive = 1
      AND NULLIF(LTRIM(IIF(ec.Channel = 3, u.Email, u.Mobile)), N'') IS NOT NULL;
END
