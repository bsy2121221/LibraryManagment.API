/*
ONE-TIME upgrade of an existing database: overdue fee popup shown to students after login.
Run after 006. Not needed for a new database (the post-deployment seed adds these settings).

SSMS: Query menu -> SQLCMD Mode, check DbProjectPath below, then Execute (F5).
No table or procedure changes: only adds the OverdueNoticeTitle / OverdueNoticeMessage settings
(admin edits them under Settings -> Student notices). Safe to run again.
*/

:setvar DbProjectPath "C:\Users\bakumar\Documents\Project\LibraryManagment\LibraryManagment_API\LibraryManagment.Database"
:on error exit

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

PRINT 'Seeding data...';
:r $(DbProjectPath)\Scripts\PostDeployment\Script.PostDeployment.sql
GO

PRINT 'Overdue notice upgrade done.';
