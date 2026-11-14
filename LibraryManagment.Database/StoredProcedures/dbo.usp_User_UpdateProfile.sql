-- Admin edits a student's details.
-- A student who holds or occupies a seat cannot change gender (the section is for one gender) or be deactivated;
-- release or move the seat first. A duplicate mobile raises the unique index error 2601.
-- Queues "profile updated" (listing what changed) and "account deactivated / reactivated" messages.
-- Errors: 50404 not found, 50409 seated
CREATE PROCEDURE dbo.usp_User_UpdateProfile
    @Id       INT,
    @FullName NVARCHAR(150),
    @Mobile   VARCHAR(15),
    @Email    NVARCHAR(256),
    @Gender   TINYINT,
    @Address  NVARCHAR(500),
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();
    DECLARE @CurrentGender TINYINT, @CurrentIsActive BIT;
    DECLARE @CurFullName NVARCHAR(150), @CurMobile VARCHAR(15), @CurEmail NVARCHAR(256), @CurAddress NVARCHAR(500);
    DECLARE @Changes NVARCHAR(400), @Data NVARCHAR(MAX);

    BEGIN TRANSACTION;

    SELECT @CurrentGender = Gender, @CurrentIsActive = IsActive,
           @CurFullName = FullName, @CurMobile = Mobile, @CurEmail = Email, @CurAddress = Address
    FROM dbo.Users WITH (UPDLOCK, HOLDLOCK)
    WHERE Id = @Id AND Role = 2;

    IF @CurrentIsActive IS NULL
        THROW 50404, N'Student not found.', 1;

    IF (@Gender <> @CurrentGender OR (@CurrentIsActive = 1 AND @IsActive = 0))
       AND EXISTS (SELECT 1 FROM dbo.Bookings
                    WHERE UserId = @Id AND (Status = 2 OR (Status = 1 AND HoldExpiresAt > @Now)))
        THROW 50409, N'This student has a seat. Release the seat before changing their gender or deactivating the account.', 1;

    UPDATE dbo.Users
    SET FullName = @FullName,
        Mobile = @Mobile,
        Email = @Email,
        Gender = @Gender,
        Address = @Address,
        IsActive = @IsActive,
        UpdatedAt = @Now
    WHERE Id = @Id;

    -- Case-sensitive compare so "ravi" -> "Ravi" also counts as a change
    SET @Changes = CONCAT_WS(N', ',
        IIF(@FullName COLLATE Latin1_General_CS_AS <> @CurFullName, N'name', NULL),
        IIF(@Mobile <> @CurMobile, N'mobile number', NULL),
        IIF(ISNULL(@Email, N'') COLLATE Latin1_General_CS_AS <> ISNULL(@CurEmail, N''), N'email', NULL),
        IIF(@Gender <> @CurrentGender, N'gender', NULL),
        IIF(ISNULL(@Address, N'') COLLATE Latin1_General_CS_AS <> ISNULL(@CurAddress, N''), N'address', NULL));

    IF @Changes <> N''
    BEGIN
        SET @Data = (SELECT @Changes AS Changes FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
        EXEC dbo.usp_Notification_EnqueueEvent @UserId = @Id, @EventType = 'ProfileUpdated', @DataJson = @Data;
    END

    IF @CurrentIsActive = 1 AND @IsActive = 0
        EXEC dbo.usp_Notification_EnqueueEvent @UserId = @Id, @EventType = 'AccountDeactivated';
    ELSE IF @CurrentIsActive = 0 AND @IsActive = 1
        EXEC dbo.usp_Notification_EnqueueEvent @UserId = @Id, @EventType = 'AccountReactivated';

    COMMIT TRANSACTION;
END
