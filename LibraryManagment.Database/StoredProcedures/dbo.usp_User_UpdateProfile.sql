-- Admin edits a student's details.
-- A student who holds or occupies a seat cannot change gender (the section is for one gender) or be deactivated;
-- release or move the seat first. A duplicate mobile raises the unique index error 2601.
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

    BEGIN TRANSACTION;

    SELECT @CurrentGender = Gender, @CurrentIsActive = IsActive
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

    COMMIT TRANSACTION;
END
