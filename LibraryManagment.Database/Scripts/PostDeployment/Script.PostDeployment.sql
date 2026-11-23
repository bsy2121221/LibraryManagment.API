/*
Seed data. Safe to run any number of times: rows are only inserted when missing,
so values the admin has changed are never overwritten.

Runs automatically after a Visual Studio "Publish", or execute it manually after the tables exist.
*/

SET NOCOUNT ON;

-- Settings
INSERT INTO dbo.Settings ([Key], [Value], DataType, Description)
SELECT s.[Key], s.[Value], s.DataType, s.Description
FROM (VALUES
    ('LibraryName',                 N'My Library', 'string',  N'Library name shown on the website and in notifications'),
    ('MonthlyFee',                  N'1000',       'decimal', N'Flat monthly fee in INR'),
    ('SeatHoldMinutes',             N'7',          'int',     N'Minutes a seat stays held while the student pays'),
    ('InvoiceGenerationDaysBefore', N'5',          'int',     N'Days before the due date to generate the next invoice'),
    ('GraceDays',                   N'3',          'int',     N'Days after the due date before an invoice becomes overdue'),
    ('AutoReleaseAfterOverdueDays', N'0',          'int',     N'Release the seat automatically after this many overdue days (0 = never)')
) AS s ([Key], [Value], DataType, Description)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Settings x WHERE x.[Key] = s.[Key]);

-- Website footer details. The footer design is fixed; the admin only fills in these values (empty = hidden).
INSERT INTO dbo.Settings ([Key], [Value], DataType, Description)
SELECT s.[Key], s.[Value], s.DataType, s.Description
FROM (VALUES
    ('FooterAbout',       N'A calm, comfortable study library with your own fixed seat.', 'string', N'Short line about the library shown in the footer'),
    ('LibraryAddress',    N'', 'string', N'Full address shown in the footer'),
    ('ContactPhone',      N'', 'string', N'Phone number shown in the footer (click to call)'),
    ('ContactEmail',      N'', 'string', N'Email address shown in the footer'),
    ('WhatsAppNumber',    N'', 'string', N'WhatsApp number with country code, e.g. 919876543210'),
    ('OpeningHours',      N'', 'string', N'e.g. Mon-Sun: 6:00 AM - 11:00 PM'),
    ('GoogleMapsUrl',     N'', 'url',    N'Link to the library on Google Maps'),
    ('InstagramUrl',      N'', 'url',    N'Instagram page link'),
    ('FacebookUrl',       N'', 'url',    N'Facebook page link'),
    ('YouTubeUrl',        N'', 'url',    N'YouTube channel link')
) AS s ([Key], [Value], DataType, Description)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Settings x WHERE x.[Key] = s.[Key]);

-- Important popup shown to a student with an overdue fee right after login (with a Pay now button).
-- Placeholders: {Name} {Amount} (total overdue) {DueDate} (oldest) {DaysOverdue} {Count} {Seat} {Section} {LibraryName}
-- Empty message = no popup.
INSERT INTO dbo.Settings ([Key], [Value], DataType, Description)
SELECT s.[Key], s.[Value], s.DataType, s.Description
FROM (VALUES
    ('OverdueNoticeTitle',   N'Important: fee overdue', 'string', N'Title of the popup. Placeholders: {Name} {Amount} {DueDate} {DaysOverdue} {Count} {Seat} {Section} {LibraryName}'),
    ('OverdueNoticeMessage', N'Dear {Name}, your fee of Rs.{Amount} was due on {DueDate} and is {DaysOverdue} days overdue. Please pay now to keep your seat {Seat}.', 'string',
                             N'Shown to students with an overdue fee after login. Same placeholders as the title. Leave empty to turn the popup off.')
) AS s ([Key], [Value], DataType, Description)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Settings x WHERE x.[Key] = s.[Key]);

-- Notification templates. Placeholders: {Name}, {Amount}, {DueDate}, {LibraryName}
INSERT INTO dbo.NotificationTemplates (Name, Channel, Subject, Body)
SELECT t.Name, t.Channel, t.Subject, t.Body
FROM (VALUES
    (N'Fee due reminder - SMS', CAST(1 AS TINYINT), CAST(NULL AS NVARCHAR(200)), N'Dear {Name}, your {LibraryName} fee of Rs.{Amount} is due on {DueDate}. Please pay on time.'),
    (N'Fee overdue - SMS',      1, NULL, N'Dear {Name}, your {LibraryName} fee of Rs.{Amount} was due on {DueDate} and is now overdue. Please pay immediately.'),
    (N'Fee overdue - WhatsApp', 2, NULL, N'Hello {Name}, your {LibraryName} fee of Rs.{Amount} (due {DueDate}) is overdue. Please pay to keep your seat.'),
    (N'Fee overdue - Email',    3, N'{LibraryName}: Fee overdue', N'Dear {Name},<br/><br/>Your monthly fee of Rs.{Amount} was due on {DueDate} and is now overdue. Please pay at the earliest to keep your seat.<br/><br/>Regards,<br/>{LibraryName}'),
    (N'Fee overdue - Call',     4, NULL, N'Hello {Name}. This is a reminder from {LibraryName}. Your monthly fee of {Amount} rupees is overdue. Please pay as soon as possible.')
) AS t (Name, Channel, Subject, Body)
WHERE NOT EXISTS (SELECT 1 FROM dbo.NotificationTemplates x WHERE x.Name = t.Name);

-- More fee reminder templates (used by the default rules below)
INSERT INTO dbo.NotificationTemplates (Name, Channel, Subject, Body)
SELECT t.Name, t.Channel, t.Subject, t.Body
FROM (VALUES
    (N'Fee due reminder - WhatsApp', CAST(2 AS TINYINT), CAST(NULL AS NVARCHAR(200)), N'Hello {Name}, a reminder from {LibraryName}: your fee of Rs.{Amount} for seat {Seat} is due on {DueDate}.'),
    (N'Fee due reminder - Email',    3, N'{LibraryName}: Fee due on {DueDate}', N'Dear {Name},<br/><br/>Your monthly fee of Rs.{Amount} for seat {Seat} ({PeriodStart} to {PeriodEnd}) is due on {DueDate}. You can pay online from the My seat page.<br/><br/>Regards,<br/>{LibraryName}')
) AS t (Name, Channel, Subject, Body)
WHERE NOT EXISTS (SELECT 1 FROM dbo.NotificationTemplates x WHERE x.Name = t.Name);

/*
Messages sent when something happens to a student. Placeholders per event:
  SeatBooked:     {Seat} {Section} {Amount} {StartDate} {NextDueDate}
  SeatMoved:      {FromSeat} {ToSeat} {Section} {Reason}
  SeatReleased:   {Seat} {Section} {Reason}
  FeePaid:        {Amount} {PeriodStart} {PeriodEnd} {NextDueDate} {Seat}
  ProfileUpdated: {Changes}
  Every message:  {Name} {LibraryName}
Only rows that do not exist yet are added, so channels the admin switched off stay off.
*/
DECLARE @EventMessages TABLE (EventType VARCHAR(50), Channel TINYINT, Subject NVARCHAR(200), Body NVARCHAR(MAX));
INSERT INTO @EventMessages (EventType, Channel, Subject, Body) VALUES
    ('SeatBooked', 1, NULL, N'Dear {Name}, your seat {Seat} ({Section}) at {LibraryName} is booked from {StartDate}. Fee paid: Rs.{Amount}. Next fee due {NextDueDate}.'),
    ('SeatBooked', 2, NULL, N'Hello {Name}! Welcome to {LibraryName}. Your seat *{Seat}* in {Section} is booked from {StartDate}. Paid Rs.{Amount}. Next fee due on {NextDueDate}.'),
    ('SeatBooked', 3, N'{LibraryName}: Seat {Seat} booked', N'Dear {Name},<br/><br/>Welcome to {LibraryName}! Your seat <b>{Seat}</b> in {Section} is booked from {StartDate}.<br/>Amount paid: Rs.{Amount}<br/>Next fee due: {NextDueDate}<br/><br/>Regards,<br/>{LibraryName}'),

    ('SeatMoved', 1, NULL, N'Dear {Name}, {LibraryName} has moved you from seat {FromSeat} to seat {ToSeat} ({Section}).'),
    ('SeatMoved', 2, NULL, N'Hello {Name}, your seat at {LibraryName} has changed from {FromSeat} to *{ToSeat}* ({Section}).'),
    ('SeatMoved', 3, N'{LibraryName}: Your seat is now {ToSeat}', N'Dear {Name},<br/><br/>Your seat has been changed from {FromSeat} to <b>{ToSeat}</b> ({Section}).<br/>Reason: {Reason}<br/><br/>Regards,<br/>{LibraryName}'),

    ('SeatReleased', 1, NULL, N'Dear {Name}, your seat {Seat} at {LibraryName} has been released. Reason: {Reason}. Contact the library for help.'),
    ('SeatReleased', 2, NULL, N'Hello {Name}, your seat {Seat} at {LibraryName} has been released. Reason: {Reason}.'),
    ('SeatReleased', 3, N'{LibraryName}: Seat {Seat} released', N'Dear {Name},<br/><br/>Your seat {Seat} ({Section}) has been released.<br/>Reason: {Reason}<br/><br/>Please contact the library if you have questions.<br/><br/>Regards,<br/>{LibraryName}'),

    ('FeePaid', 1, NULL, N'Dear {Name}, we received Rs.{Amount} for {PeriodStart} to {PeriodEnd}. Thank you! - {LibraryName}'),
    ('FeePaid', 2, NULL, N'Hello {Name}, payment of Rs.{Amount} received for {PeriodStart} to {PeriodEnd}. Thank you! - {LibraryName}'),
    ('FeePaid', 3, N'{LibraryName}: Payment received', N'Dear {Name},<br/><br/>We received your payment of Rs.{Amount} for seat {Seat}, period {PeriodStart} to {PeriodEnd}.<br/>Next fee due: {NextDueDate}<br/><br/>Thank you,<br/>{LibraryName}'),

    ('ProfileUpdated', 1, NULL, N'Dear {Name}, {LibraryName} updated your details: {Changes}. Contact the library if this was not expected.'),
    ('ProfileUpdated', 2, NULL, N'Hello {Name}, your details at {LibraryName} were updated: {Changes}.'),
    ('ProfileUpdated', 3, N'{LibraryName}: Your details were updated', N'Dear {Name},<br/><br/>The library updated your details: {Changes}.<br/>If you did not expect this, please contact us.<br/><br/>Regards,<br/>{LibraryName}'),

    ('AccountDeactivated', 1, NULL, N'Dear {Name}, your {LibraryName} account has been deactivated. Contact the library for help.'),
    ('AccountDeactivated', 2, NULL, N'Hello {Name}, your {LibraryName} account has been deactivated. Please contact the library.'),
    ('AccountDeactivated', 3, N'{LibraryName}: Account deactivated', N'Dear {Name},<br/><br/>Your account has been deactivated. Please contact the library if you have questions.<br/><br/>Regards,<br/>{LibraryName}'),

    ('AccountReactivated', 1, NULL, N'Dear {Name}, your {LibraryName} account is active again. You can log in now.'),
    ('AccountReactivated', 2, NULL, N'Hello {Name}, your {LibraryName} account is active again. You can log in now.'),
    ('AccountReactivated', 3, N'{LibraryName}: Account active again', N'Dear {Name},<br/><br/>Your account is active again and you can log in.<br/><br/>Regards,<br/>{LibraryName}'),

    ('PasswordReset', 1, NULL, N'Dear {Name}, your {LibraryName} password was reset by the library. Ask the library for your new password.'),
    ('PasswordReset', 3, N'{LibraryName}: Password reset', N'Dear {Name},<br/><br/>Your password was reset by the library. Please ask the library for your new password.<br/>If you did not request this, contact us.<br/><br/>Regards,<br/>{LibraryName}'),

    -- Walk-in students registered by the admin
    ('AccountCreated', 1, NULL, N'Welcome to {LibraryName}, {Name}! Your account is ready. Log in with mobile {Mobile}; the library will give you your password.'),
    ('AccountCreated', 2, NULL, N'Welcome to {LibraryName}, {Name}! Your account is ready. Log in with your mobile number {Mobile}. Ask the library for your password.'),
    ('AccountCreated', 3, N'Welcome to {LibraryName}', N'Dear {Name},<br/><br/>Your {LibraryName} account is ready. Log in with your mobile number <b>{Mobile}</b>. The library will give you your password.<br/><br/>Regards,<br/>{LibraryName}'),

    ('SeatAssigned', 1, NULL, N'Dear {Name}, seat {Seat} ({Section}) at {LibraryName} is yours from {StartDate}. {FeeNote} Next fee due {NextDueDate}.'),
    ('SeatAssigned', 2, NULL, N'Hello {Name}! Seat *{Seat}* ({Section}) at {LibraryName} is yours from {StartDate}. {FeeNote} Next fee due on {NextDueDate}.'),
    ('SeatAssigned', 3, N'{LibraryName}: Seat {Seat} is yours', N'Dear {Name},<br/><br/>Seat <b>{Seat}</b> in {Section} is yours from {StartDate}.<br/>{FeeNote}<br/>Next fee due: {NextDueDate}<br/><br/>Regards,<br/>{LibraryName}'),

    ('FeeCreated', 1, NULL, N'Dear {Name}, a fee of Rs.{Amount} for {PeriodStart} to {PeriodEnd} has been added. Please pay by {DueDate}. - {LibraryName}'),
    ('FeeCreated', 2, NULL, N'Hello {Name}, a fee of Rs.{Amount} for {PeriodStart} to {PeriodEnd} is due on {DueDate}. - {LibraryName}'),
    ('FeeCreated', 3, N'{LibraryName}: Fee of Rs.{Amount} due on {DueDate}', N'Dear {Name},<br/><br/>A fee of Rs.{Amount} for seat {Seat}, period {PeriodStart} to {PeriodEnd}, has been added. Please pay by {DueDate}, online from the My seat page or at the library.<br/><br/>Regards,<br/>{LibraryName}');

INSERT INTO dbo.NotificationTemplates (Name, Channel, Subject, Body)
SELECT CONCAT(N'Event ', m.EventType, N' - ', CHOOSE(m.Channel, N'SMS', N'WhatsApp', N'Email', N'Call')), m.Channel, m.Subject, m.Body
FROM @EventMessages m
WHERE NOT EXISTS (SELECT 1 FROM dbo.NotificationEventChannels ec WHERE ec.EventType = m.EventType AND ec.Channel = m.Channel)
  AND NOT EXISTS (SELECT 1 FROM dbo.NotificationTemplates t
                  WHERE t.Name = CONCAT(N'Event ', m.EventType, N' - ', CHOOSE(m.Channel, N'SMS', N'WhatsApp', N'Email', N'Call')));

INSERT INTO dbo.NotificationEventChannels (EventType, Channel, TemplateId, IsActive)
SELECT m.EventType, m.Channel, t.Id, 1
FROM @EventMessages m
JOIN dbo.NotificationTemplates t ON t.Name = CONCAT(N'Event ', m.EventType, N' - ', CHOOSE(m.Channel, N'SMS', N'WhatsApp', N'Email', N'Call'))
WHERE NOT EXISTS (SELECT 1 FROM dbo.NotificationEventChannels ec WHERE ec.EventType = m.EventType AND ec.Channel = m.Channel);

-- Default fee reminder rules. Only added to an empty rules table, so rules the admin deleted do not come back.
IF NOT EXISTS (SELECT 1 FROM dbo.NotificationRules)
BEGIN
    DECLARE @RuleId INT;

    INSERT INTO dbo.NotificationRules (Name, [Trigger], OffsetDays, RepeatEveryMinutes, MaxAttempts, IsActive)
    VALUES (N'3 days before the due date', 1, 3, NULL, 1, 1);
    SET @RuleId = SCOPE_IDENTITY();
    INSERT INTO dbo.NotificationRuleChannels (RuleId, Channel, TemplateId)
    SELECT @RuleId, t.Channel, t.Id FROM dbo.NotificationTemplates t
    WHERE t.Name IN (N'Fee due reminder - SMS', N'Fee due reminder - WhatsApp', N'Fee due reminder - Email');

    INSERT INTO dbo.NotificationRules (Name, [Trigger], OffsetDays, RepeatEveryMinutes, MaxAttempts, QuietHoursStart, QuietHoursEnd, IsActive)
    VALUES (N'Overdue - daily reminder', 3, 0, 1440, 5, '21:00', '09:00', 1);
    SET @RuleId = SCOPE_IDENTITY();
    INSERT INTO dbo.NotificationRuleChannels (RuleId, Channel, TemplateId)
    SELECT @RuleId, t.Channel, t.Id FROM dbo.NotificationTemplates t
    WHERE t.Name IN (N'Fee overdue - SMS', N'Fee overdue - WhatsApp', N'Fee overdue - Email');

    INSERT INTO dbo.NotificationRules (Name, [Trigger], OffsetDays, RepeatEveryMinutes, MaxAttempts, QuietHoursStart, QuietHoursEnd, IsActive)
    VALUES (N'Overdue - phone call', 3, 2, 1440, 3, '20:00', '10:00', 1);
    SET @RuleId = SCOPE_IDENTITY();
    INSERT INTO dbo.NotificationRuleChannels (RuleId, Channel, TemplateId)
    SELECT @RuleId, t.Channel, t.Id FROM dbo.NotificationTemplates t
    WHERE t.Name = N'Fee overdue - Call';
END
