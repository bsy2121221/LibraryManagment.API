-- Stores the final text that was sent and marks the message Sent
CREATE PROCEDURE dbo.usp_Notification_MarkSent
    @Id                BIGINT,
    @Provider          VARCHAR(30),
    @ProviderMessageId VARCHAR(100),
    @Subject           NVARCHAR(200),
    @Body              NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();

    UPDATE dbo.Notifications
    SET Status = 3,
        Provider = @Provider,
        ProviderMessageId = @ProviderMessageId,
        Subject = @Subject,
        Body = @Body,
        LastError = NULL,
        LockedUntil = NULL,
        SentAt = @Now,
        UpdatedAt = @Now
    WHERE Id = @Id AND Status = 2;
END
