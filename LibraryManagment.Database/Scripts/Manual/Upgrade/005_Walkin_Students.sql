/*
ONE-TIME upgrade of an existing database: admin registers walk-in students, assigns seats and adds fees.
Run after 004. Not needed for a new database (00_CreateAll.sql has everything).

SSMS: Query menu -> SQLCMD Mode, check DbProjectPath below, then Execute (F5).
No table changes: recreates all procedures and adds the new default messages
(AccountCreated, SeatAssigned, FeeCreated). Safe to run again.
*/

:setvar DbProjectPath "C:\Users\bakumar\Documents\Project\LibraryManagment\LibraryManagment_API\LibraryManagment.Database"
:on error exit

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

PRINT 'Creating stored procedures...';
:r $(DbProjectPath)\Scripts\Manual\01_CreateAllProcedures.sql

PRINT 'Seeding data...';
:r $(DbProjectPath)\Scripts\PostDeployment\Script.PostDeployment.sql
GO

PRINT 'Walk-in students upgrade done.';
