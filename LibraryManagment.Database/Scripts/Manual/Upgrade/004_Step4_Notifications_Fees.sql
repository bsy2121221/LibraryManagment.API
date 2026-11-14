/*
ONE-TIME upgrade of an existing database to step 4 (notifications queue, fees and reminders).
Not needed for a new database: 00_CreateAll.sql already creates everything.

SSMS: Query menu -> SQLCMD Mode, check DbProjectPath below, then Execute (F5).

What it does:
  1. Drops dbo.NotificationLogs (never used; replaced by dbo.Notifications). Stops if it has rows.
  2. Creates dbo.NotificationEventChannels and dbo.Notifications.
  3. Recreates all views and all procedures.
  4. Runs the seed script (adds the default messages and reminder rules; safe to re-run).
*/

:setvar DbProjectPath "C:\Users\bakumar\Documents\Project\LibraryManagment\LibraryManagment_API\LibraryManagment.Database"
:on error exit

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Notifications', N'U') IS NOT NULL
    THROW 50000, N'dbo.Notifications already exists: this upgrade has already been run.', 1;

IF OBJECT_ID(N'dbo.NotificationLogs', N'U') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.NotificationLogs)
        THROW 50000, N'dbo.NotificationLogs has rows. Check them before running this upgrade.', 1;
    DROP TABLE dbo.NotificationLogs;
END
GO

PRINT 'Creating tables...';
:r $(DbProjectPath)\Tables\dbo.NotificationEventChannels.sql
:r $(DbProjectPath)\Tables\dbo.Notifications.sql

PRINT 'Creating views...';
:r $(DbProjectPath)\Scripts\Manual\02_CreateAllViews.sql

PRINT 'Creating stored procedures...';
:r $(DbProjectPath)\Scripts\Manual\01_CreateAllProcedures.sql

PRINT 'Seeding data...';
:r $(DbProjectPath)\Scripts\PostDeployment\Script.PostDeployment.sql
GO

PRINT 'Step 4 upgrade done.';
