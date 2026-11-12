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
