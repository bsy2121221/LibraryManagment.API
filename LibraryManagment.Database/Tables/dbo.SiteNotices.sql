-- Important notices from the admin, shown to every visitor in the rotating bar above the navbar
-- (e.g. "Library closed on 5 Dec for maintenance"). StartDate/EndDate NULL = no limit; dates are library local dates.
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
GO
