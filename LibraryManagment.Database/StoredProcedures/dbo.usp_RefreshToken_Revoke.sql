CREATE PROCEDURE dbo.usp_RefreshToken_Revoke
    @Id                  INT,
    @ReplacedByTokenHash CHAR(64) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.RefreshTokens
    SET RevokedAt = SYSUTCDATETIME(),
        ReplacedByTokenHash = @ReplacedByTokenHash
    WHERE Id = @Id
      AND RevokedAt IS NULL;
END
