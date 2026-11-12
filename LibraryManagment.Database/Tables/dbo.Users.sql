-- Role: 1 = Admin, 2 = Student | Gender: 1 = Male, 2 = Female
CREATE TABLE dbo.Users
(
    Id               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    FullName         NVARCHAR(150)     NOT NULL,
    Mobile           VARCHAR(15)       NOT NULL,
    Email            NVARCHAR(256)     NULL,
    PasswordHash     VARCHAR(100)      NOT NULL,
    Gender           TINYINT           NULL,
    Address          NVARCHAR(500)     NULL,
    Role             TINYINT           NOT NULL,
    IsMobileVerified BIT               NOT NULL CONSTRAINT DF_Users_IsMobileVerified DEFAULT (0),
    IsActive         BIT               NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    CreatedAt        DATETIME2(0)      NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt        DATETIME2(0)      NULL,
    CONSTRAINT CK_Users_Role CHECK (Role IN (1, 2)),
    CONSTRAINT CK_Users_Gender CHECK (Gender IS NULL OR Gender IN (1, 2)),
    -- Students must have a gender so they can be restricted to their section
    CONSTRAINT CK_Users_StudentGender CHECK (Role = 1 OR Gender IS NOT NULL)
);
GO

CREATE UNIQUE INDEX UX_Users_Mobile ON dbo.Users (Mobile);
GO
