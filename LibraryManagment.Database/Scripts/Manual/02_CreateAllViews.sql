/*
(Re)creates every view. Safe to run any time. Run this BEFORE 01_CreateAllProcedures.sql,
because procedures read from these views.

SSMS: Query menu -> SQLCMD Mode, check DbProjectPath below, then Execute (F5).

When you add a new view file, add a DROP + :r pair for it here.
*/

:setvar DbProjectPath "C:\Users\bakumar\Documents\Project\LibraryManagment\LibraryManagment_API\LibraryManagment.Database"
:on error exit

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

DROP VIEW IF EXISTS dbo.vw_BookingDetails;
GO
:r $(DbProjectPath)\Views\dbo.vw_BookingDetails.sql
GO
