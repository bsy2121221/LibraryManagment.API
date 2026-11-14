-- Admin registers a walk-in student. Queues the "account created" welcome messages (never the password).
-- A duplicate mobile raises the unique index error 2601. Returns the new user Id.
CREATE PROCEDURE dbo.usp_User_CreateByAdmin
    @FullName     NVARCHAR(150),
    @Mobile       VARCHAR(15),
    @Email        NVARCHAR(256),
    @PasswordHash VARCHAR(100),
    @Gender       TINYINT,
    @Address      NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Id INT, @Data NVARCHAR(MAX);

    BEGIN TRANSACTION;

    INSERT INTO dbo.Users (FullName, Mobile, Email, PasswordHash, Gender, Address, Role, IsMobileVerified, IsActive)
    VALUES (@FullName, @Mobile, @Email, @PasswordHash, @Gender, @Address, 2, 0, 1);
    SET @Id = SCOPE_IDENTITY();

    SET @Data = (SELECT @Mobile AS Mobile FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
    EXEC dbo.usp_Notification_EnqueueEvent @UserId = @Id, @EventType = 'AccountCreated', @DataJson = @Data;

    COMMIT TRANSACTION;

    SELECT @Id AS Id;
END
