CREATE PROCEDURE dbo.usp_User_GetByMobile
    @Mobile VARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, FullName, Mobile, Email, PasswordHash, Gender, Address, Role,
           IsMobileVerified, IsActive, CreatedAt, UpdatedAt
    FROM dbo.Users
    WHERE Mobile = @Mobile;
END
