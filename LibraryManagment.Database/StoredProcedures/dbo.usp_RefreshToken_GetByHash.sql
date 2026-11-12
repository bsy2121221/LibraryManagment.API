CREATE PROCEDURE dbo.usp_RefreshToken_GetByHash
    @TokenHash CHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, UserId, TokenHash, ExpiresAt, CreatedAt, RevokedAt, ReplacedByTokenHash
    FROM dbo.RefreshTokens
    WHERE TokenHash = @TokenHash;
END
