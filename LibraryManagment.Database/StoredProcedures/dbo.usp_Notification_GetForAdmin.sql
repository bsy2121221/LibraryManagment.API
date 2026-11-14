-- Admin message log, newest first. Every filter is optional.
CREATE PROCEDURE dbo.usp_Notification_GetForAdmin
    @Status    TINYINT       = NULL,
    @Channel   TINYINT       = NULL,
    @EventType VARCHAR(50)   = NULL,
    @UserId    INT           = NULL,
    @Search    NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Pattern NVARCHAR(102) = IIF(NULLIF(LTRIM(RTRIM(@Search)), N'') IS NULL, NULL, CONCAT(N'%', LTRIM(RTRIM(@Search)), N'%'));

    SELECT TOP (300)
           n.Id, n.UserId, u.FullName AS StudentName, n.EventType, n.Channel, n.Recipient, n.Subject, n.Body, n.DataJson,
           n.Status, n.Attempts, n.MaxAttempts, n.NextAttemptAt, n.Provider, n.LastError, n.CreatedAt, n.SentAt,
           r.Name AS RuleName
    FROM dbo.Notifications n
    JOIN dbo.Users u ON u.Id = n.UserId
    LEFT JOIN dbo.NotificationRules r ON r.Id = n.RuleId
    WHERE (@Status IS NULL OR n.Status = @Status)
      AND (@Channel IS NULL OR n.Channel = @Channel)
      AND (@EventType IS NULL OR n.EventType = @EventType)
      AND (@UserId IS NULL OR n.UserId = @UserId)
      AND (@Pattern IS NULL OR u.FullName LIKE @Pattern OR n.Recipient LIKE @Pattern)
    ORDER BY n.Id DESC;
END
