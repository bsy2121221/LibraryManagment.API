-- Gender: 1 = Male (Boys section), 2 = Female (Girls section)
CREATE TABLE dbo.Sections
(
    Id                 INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Sections PRIMARY KEY,
    Name               NVARCHAR(100)     NOT NULL,
    Gender             TINYINT           NOT NULL CONSTRAINT CK_Sections_Gender CHECK (Gender IN (1, 2)),
    LayoutWidth        INT               NOT NULL CONSTRAINT DF_Sections_LayoutWidth DEFAULT (1000),
    LayoutHeight       INT               NOT NULL CONSTRAINT DF_Sections_LayoutHeight DEFAULT (600),
    BackgroundImageUrl NVARCHAR(500)     NULL,
    DisplayOrder       INT               NOT NULL CONSTRAINT DF_Sections_DisplayOrder DEFAULT (0),
    IsActive           BIT               NOT NULL CONSTRAINT DF_Sections_IsActive DEFAULT (1),
    CreatedAt          DATETIME2(0)      NOT NULL CONSTRAINT DF_Sections_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt          DATETIME2(0)      NULL
);
GO

CREATE UNIQUE INDEX UX_Sections_Name ON dbo.Sections (Name);
GO
