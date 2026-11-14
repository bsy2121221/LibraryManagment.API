/*
Queues fee reminders according to the active dbo.NotificationRules. Run every minute by the API's billing job.
A rule fires for an unpaid invoice of an active booking when:
  BeforeDue (1): from OffsetDays before the due date until the due date (invoice still Due)
  OnDue     (2): from OffsetDays after the due date while the invoice is still Due (not yet overdue)
  Overdue   (3): from OffsetDays after the invoice became overdue (due date + @GraceDays + 1)
It sends once, or - with RepeatEveryMinutes - again after that many minutes, at most MaxAttempts times per channel.
Nothing is queued during the rule's quiet hours (India time); it is queued on the first run after they end.
Returns the number of messages queued.
*/
CREATE PROCEDURE dbo.usp_Notification_EnqueueFeeReminders
    @GraceDays INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();
    DECLARE @NowIst DATETIME2(0) = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'India Standard Time' AS DATETIME2(0));
    DECLARE @Today DATE = CAST(@NowIst AS DATE);
    DECLARE @Time TIME(0) = CAST(@NowIst AS TIME(0));

    INSERT INTO dbo.Notifications (UserId, EventType, Channel, Recipient, Subject, Body, DataJson, Status, RuleId, FeeInvoiceId)
    SELECT i.UserId,
           'FeeReminder',
           rc.Channel,
           IIF(rc.Channel = 3, u.Email, u.Mobile),
           t.Subject,
           t.Body,
           (SELECT u.FullName                               AS Name,
                   FORMAT(i.Amount, '0.##')                 AS Amount,
                   CONVERT(VARCHAR(11), i.DueDate, 106)     AS DueDate,
                   CONVERT(VARCHAR(11), i.PeriodStart, 106) AS PeriodStart,
                   CONVERT(VARCHAR(11), i.PeriodEnd, 106)   AS PeriodEnd,
                   GREATEST(DATEDIFF(DAY, i.DueDate, @Today), 0) AS DaysOverdue,
                   d.SeatLabel                              AS Seat,
                   d.SectionName                            AS Section
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           1,
           r.Id,
           i.Id
    FROM dbo.FeeInvoices i
    JOIN dbo.Users u                    ON u.Id = i.UserId
    JOIN dbo.vw_BookingDetails d        ON d.Id = i.BookingId AND d.Status = 2
    JOIN dbo.NotificationRules r        ON r.IsActive = 1
    JOIN dbo.NotificationRuleChannels rc ON rc.RuleId = r.Id
    JOIN dbo.NotificationTemplates t    ON t.Id = rc.TemplateId AND t.IsActive = 1
    OUTER APPLY (SELECT COUNT(*) AS SentCount, MAX(n.CreatedAt) AS LastAt
                 FROM dbo.Notifications n
                 WHERE n.FeeInvoiceId = i.Id AND n.RuleId = r.Id AND n.Channel = rc.Channel AND n.Status <> 6) h
    WHERE i.Status IN (1, 3)
      AND NULLIF(LTRIM(IIF(rc.Channel = 3, u.Email, u.Mobile)), N'') IS NOT NULL
      AND (   (r.[Trigger] = 1 AND i.Status = 1 AND @Today >= DATEADD(DAY, -r.OffsetDays, i.DueDate) AND @Today < i.DueDate)
           OR (r.[Trigger] = 2 AND i.Status = 1 AND @Today >= DATEADD(DAY, r.OffsetDays, i.DueDate))
           OR (r.[Trigger] = 3 AND i.Status = 3 AND @Today >= DATEADD(DAY, @GraceDays + 1 + r.OffsetDays, i.DueDate)))
      AND NOT (r.QuietHoursStart IS NOT NULL AND r.QuietHoursEnd IS NOT NULL
               AND (   (r.QuietHoursStart <= r.QuietHoursEnd AND @Time >= r.QuietHoursStart AND @Time < r.QuietHoursEnd)
                    OR (r.QuietHoursStart > r.QuietHoursEnd AND (@Time >= r.QuietHoursStart OR @Time < r.QuietHoursEnd))))
      AND (   h.SentCount = 0
           OR (r.RepeatEveryMinutes IS NOT NULL
               AND h.SentCount < r.MaxAttempts
               AND h.LastAt <= DATEADD(MINUTE, -r.RepeatEveryMinutes, @Now)));

    SELECT @@ROWCOUNT AS Enqueued;
END
