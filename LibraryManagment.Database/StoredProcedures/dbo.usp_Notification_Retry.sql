-- Admin sends a failed, skipped or cancelled message again (on the next job run). Errors: 50409 wrong status
CREATE PROCEDURE dbo.usp_Notification_Retry
    @Id BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();

    UPDATE dbo.Notifications
    SET Status = 1, Attempts = 0, NextAttemptAt = @Now, LastError = NULL, UpdatedAt = @Now
    WHERE Id = @Id AND Status IN (4, 5, 6);

    IF @@ROWCOUNT = 0
        THROW 50409, N'Only failed, skipped or cancelled messages can be sent again.', 1;
END
