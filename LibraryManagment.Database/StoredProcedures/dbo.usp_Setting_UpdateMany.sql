/*
Updates several settings at once. Values are validated by the API before this is called.
@SettingsJson: [{ "Key": "MonthlyFee", "Value": "1200" }, ...]
Errors: 50400 invalid JSON / unknown key
*/
CREATE PROCEDURE dbo.usp_Setting_UpdateMany
    @SettingsJson    NVARCHAR(MAX),
    @UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();
    DECLARE @UnknownKey VARCHAR(100), @Message NVARCHAR(2048);

    IF @SettingsJson IS NULL OR ISJSON(@SettingsJson) = 0
        THROW 50400, N'Settings are not valid JSON.', 1;

    DECLARE @Items TABLE ([Key] VARCHAR(100) NOT NULL PRIMARY KEY, [Value] NVARCHAR(MAX) NOT NULL);

    INSERT INTO @Items ([Key], [Value])
    SELECT [Key], [Value]
    FROM OPENJSON(@SettingsJson)
    WITH ([Key] VARCHAR(100) '$.Key', [Value] NVARCHAR(MAX) '$.Value')
    WHERE [Key] IS NOT NULL AND [Value] IS NOT NULL;

    SELECT TOP (1) @UnknownKey = i.[Key]
    FROM @Items i
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Settings s WHERE s.[Key] = i.[Key]);

    IF @UnknownKey IS NOT NULL
    BEGIN
        SET @Message = CONCAT(N'Unknown setting "', @UnknownKey, N'".');
        THROW 50400, @Message, 1;
    END

    BEGIN TRANSACTION;

    UPDATE s
    SET [Value] = i.[Value], UpdatedAt = @Now, UpdatedByUserId = @UpdatedByUserId
    FROM dbo.Settings s
    JOIN @Items i ON i.[Key] = s.[Key]
    WHERE s.[Value] <> i.[Value];

    COMMIT TRANSACTION;
END
