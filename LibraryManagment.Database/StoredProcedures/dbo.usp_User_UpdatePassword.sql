-- Admin sets a new password. Queues the "password reset" messages (the password itself is never sent).
-- Errors: 50404 not found
CREATE PROCEDURE dbo.usp_User_UpdatePassword
    @Id           INT,
    @PasswordHash VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    UPDATE dbo.Users
    SET PasswordHash = @PasswordHash, UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @Id;

    IF @@ROWCOUNT = 0
        THROW 50404, N'User not found.', 1;

    EXEC dbo.usp_Notification_EnqueueEvent @UserId = @Id, @EventType = 'PasswordReset';

    COMMIT TRANSACTION;
END
