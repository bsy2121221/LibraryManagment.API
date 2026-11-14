/*
Takes up to @BatchSize messages that are ready to send and marks them Processing, so no other worker
(or a second API instance) picks the same rows (READPAST skips rows another worker has locked).
Rows left in Processing after their lock ended (e.g. the API stopped mid-send) are picked up again.
*/
CREATE PROCEDURE dbo.usp_Notification_ClaimBatch
    @BatchSize   INT,
    @LockSeconds INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();

    WITH next AS
    (
        SELECT TOP (@BatchSize) *
        FROM dbo.Notifications WITH (ROWLOCK, UPDLOCK, READPAST)
        WHERE (Status = 1 AND NextAttemptAt <= @Now)
           OR (Status = 2 AND LockedUntil < @Now)
        ORDER BY NextAttemptAt, Id
    )
    UPDATE next
    SET Status = 2,
        Attempts = Attempts + 1,
        LockedUntil = DATEADD(SECOND, @LockSeconds, @Now),
        UpdatedAt = @Now
    OUTPUT inserted.Id, inserted.UserId, inserted.EventType, inserted.Channel, inserted.Recipient,
           inserted.Subject, inserted.Body, inserted.DataJson, inserted.Attempts, inserted.MaxAttempts;
END
