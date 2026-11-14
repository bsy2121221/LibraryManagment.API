-- Number of messages per status, for the admin dashboard
CREATE PROCEDURE dbo.usp_Notification_GetSummary
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Status, COUNT(*) AS [Count]
    FROM dbo.Notifications
    GROUP BY Status;
END
