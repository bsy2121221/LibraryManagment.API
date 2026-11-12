-- Errors: 50404 not found
CREATE PROCEDURE dbo.usp_User_UpdatePassword
    @Id           INT,
    @PasswordHash VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Users
    SET PasswordHash = @PasswordHash, UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @Id;

    IF @@ROWCOUNT = 0
        THROW 50404, N'User not found.', 1;
END
