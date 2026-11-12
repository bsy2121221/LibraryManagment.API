CREATE PROCEDURE dbo.usp_RefreshToken_RevokeAllForUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.RefreshTokens
    SET RevokedAt = SYSUTCDATETIME()
    WHERE UserId = @UserId
      AND RevokedAt IS NULL;
END
