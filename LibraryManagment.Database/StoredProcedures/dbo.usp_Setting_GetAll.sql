CREATE PROCEDURE dbo.usp_Setting_GetAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Key], [Value], DataType, Description, UpdatedAt, UpdatedByUserId
    FROM dbo.Settings
    ORDER BY [Key];
END
