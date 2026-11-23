/*
ONE-TIME upgrade of an existing database: admin notices shown in the rotating bar above the navbar.
Run after 007. Not needed for a new database (00_CreateAll.sql has everything).

SSMS: Query menu -> SQLCMD Mode, check DbProjectPath below, then Execute (F5).
Adds the dbo.SiteNotices table (same definition as Tables/dbo.SiteNotices.sql) and recreates all procedures.
Safe to run again.
*/

:setvar DbProjectPath "C:\Users\bakumar\Documents\Project\LibraryManagment\LibraryManagment_API\LibraryManagment.Database"
:on error exit

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.SiteNotices', N'U') IS NULL
BEGIN
    PRINT 'Creating dbo.SiteNotices...';
    CREATE TABLE dbo.SiteNotices
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SiteNotices PRIMARY KEY,
        Message         NVARCHAR(300)     NOT NULL,
        StartDate       DATE              NULL,
        EndDate         DATE              NULL,
        IsActive        BIT               NOT NULL CONSTRAINT DF_SiteNotices_IsActive DEFAULT (1),
        SortOrder       INT               NOT NULL CONSTRAINT DF_SiteNotices_SortOrder DEFAULT (0),
        CreatedAt       DATETIME2(0)      NOT NULL CONSTRAINT DF_SiteNotices_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedByUserId INT               NOT NULL CONSTRAINT FK_SiteNotices_CreatedBy REFERENCES dbo.Users (Id),
        UpdatedAt       DATETIME2(0)      NULL,
        CONSTRAINT CK_SiteNotices_Dates CHECK (StartDate IS NULL OR EndDate IS NULL OR EndDate >= StartDate)
    );
END
GO

PRINT 'Creating stored procedures...';
:r $(DbProjectPath)\Scripts\Manual\01_CreateAllProcedures.sql
GO

PRINT 'Site notices upgrade done.';
