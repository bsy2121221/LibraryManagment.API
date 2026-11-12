/*
Creates the complete database on an EMPTY database, in the correct dependency order.

How to run in SSMS:
  1. Create the database first:  CREATE DATABASE LibraryManagement;
  2. Open this file and select the LibraryManagement database
  3. Query menu -> enable "SQLCMD Mode"
  4. Check DbProjectPath below points to the LibraryManagment.Database folder
  5. Execute (F5)

Or from a terminal:
  sqlcmd -S <server> -E -C -d LibraryManagement -i 00_CreateAll.sql -v DbProjectPath="<path to LibraryManagment.Database>"
*/

:setvar DbProjectPath "C:\Users\bakumar\Documents\Project\LibraryManagment\LibraryManagment_API\LibraryManagment.Database"
:on error exit

-- Required for filtered indexes; command-line sqlcmd defaults QUOTED_IDENTIFIER to OFF
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

PRINT 'Creating tables...';
:r $(DbProjectPath)\Tables\dbo.Users.sql
:r $(DbProjectPath)\Tables\dbo.RefreshTokens.sql
:r $(DbProjectPath)\Tables\dbo.Sections.sql
:r $(DbProjectPath)\Tables\dbo.Seats.sql
:r $(DbProjectPath)\Tables\dbo.Bookings.sql
:r $(DbProjectPath)\Tables\dbo.FeeInvoices.sql
:r $(DbProjectPath)\Tables\dbo.Payments.sql
:r $(DbProjectPath)\Tables\dbo.SeatChangeHistory.sql
:r $(DbProjectPath)\Tables\dbo.Settings.sql
:r $(DbProjectPath)\Tables\dbo.NotificationTemplates.sql
:r $(DbProjectPath)\Tables\dbo.NotificationRules.sql
:r $(DbProjectPath)\Tables\dbo.NotificationRuleChannels.sql
:r $(DbProjectPath)\Tables\dbo.NotificationLogs.sql
:r $(DbProjectPath)\Tables\dbo.MediaAssets.sql
:r $(DbProjectPath)\Tables\dbo.HomePageLayouts.sql

PRINT 'Creating views...';
:r $(DbProjectPath)\Scripts\Manual\02_CreateAllViews.sql

PRINT 'Creating stored procedures...';
:r $(DbProjectPath)\Scripts\Manual\01_CreateAllProcedures.sql

PRINT 'Seeding data...';
:r $(DbProjectPath)\Scripts\PostDeployment\Script.PostDeployment.sql
GO

PRINT 'Done.';
