-- Business settings editable by the admin. Infrastructure config (API keys, providers) lives in appsettings.json.
CREATE TABLE dbo.Settings
(
    [Key]           VARCHAR(100)  NOT NULL CONSTRAINT PK_Settings PRIMARY KEY,
    [Value]         NVARCHAR(MAX) NOT NULL,
    DataType        VARCHAR(20)   NOT NULL,
    Description     NVARCHAR(300) NULL,
    UpdatedAt       DATETIME2(0)  NULL,
    UpdatedByUserId INT           NULL CONSTRAINT FK_Settings_Users REFERENCES dbo.Users (Id)
);
GO
