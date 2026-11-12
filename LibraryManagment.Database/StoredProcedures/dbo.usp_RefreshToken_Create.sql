CREATE PROCEDURE dbo.usp_RefreshToken_Create
    @UserId    INT,
    @TokenHash CHAR(64),
    @ExpiresAt DATETIME2(0)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.RefreshTokens (UserId, TokenHash, ExpiresAt)
    VALUES (@UserId, @TokenHash, @ExpiresAt);
END
