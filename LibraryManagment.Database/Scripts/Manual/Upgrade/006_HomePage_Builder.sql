/*
ONE-TIME upgrade of an existing database: home page builder, media library and website footer details.
Run after 005. Not needed for a new database (00_CreateAll.sql has everything).

SSMS: Query menu -> SQLCMD Mode, check DbProjectPath below, then Execute (F5).
No table changes (dbo.HomePageLayouts and dbo.MediaAssets already exist): recreates all procedures
and adds the footer settings (address, phone, opening hours, links). Safe to run again.
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

PRINT 'Home page builder upgrade done.';
