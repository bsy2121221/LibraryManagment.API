/*
A send attempt failed.
  @Skip = 1       -> Skipped (nothing to retry, e.g. channel has no sender configured)
  @Permanent = 1  -> Failed now (e.g. invalid number)
  otherwise       -> Ready again after @RetryDelaySeconds, or Failed once Attempts reached MaxAttempts
*/
CREATE PROCEDURE dbo.usp_Notification_MarkFailed
    @Id                BIGINT,
    @Provider          VARCHAR(30),
    @Error             NVARCHAR(1000),
    @RetryDelaySeconds INT,
    @Permanent         BIT,
    @Skip              BIT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();

    UPDATE dbo.Notifications
    SET Status = CASE WHEN @Skip = 1 THEN 5
                      WHEN @Permanent = 1 OR Attempts >= MaxAttempts THEN 4
                      ELSE 1 END,
        NextAttemptAt = DATEADD(SECOND, @RetryDelaySeconds, @Now),
        Provider = @Provider,
        LastError = LEFT(@Error, 1000),
        LockedUntil = NULL,
        UpdatedAt = @Now
    WHERE Id = @Id AND Status = 2;
END
