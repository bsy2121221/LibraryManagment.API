-- Returns the new user Id
CREATE PROCEDURE dbo.usp_User_Create
    @FullName         NVARCHAR(150),
    @Mobile           VARCHAR(15),
    @Email            NVARCHAR(256),
    @PasswordHash     VARCHAR(100),
    @Gender           TINYINT,
    @Address          NVARCHAR(500),
    @Role             TINYINT,
    @IsMobileVerified BIT,
    @IsActive         BIT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Users (FullName, Mobile, Email, PasswordHash, Gender, Address, Role, IsMobileVerified, IsActive)
    OUTPUT INSERTED.Id
    VALUES (@FullName, @Mobile, @Email, @PasswordHash, @Gender, @Address, @Role, @IsMobileVerified, @IsActive);
END
