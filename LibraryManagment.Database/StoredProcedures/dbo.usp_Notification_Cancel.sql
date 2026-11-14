-- Admin stops a message that has not been sent yet. Errors: 50409 already being sent or done
CREATE PROCEDURE dbo.usp_Notification_Cancel
    @Id BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Notifications
    SET Status = 6, UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @Id AND Status = 1;

    IF @@ROWCOUNT = 0
        THROW 50409, N'Only messages waiting to be sent can be cancelled.', 1;
END
