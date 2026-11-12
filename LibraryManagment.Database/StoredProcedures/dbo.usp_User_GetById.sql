CREATE PROCEDURE dbo.usp_User_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, FullName, Mobile, Email, PasswordHash, Gender, Address, Role,
           IsMobileVerified, IsActive, CreatedAt, UpdatedAt
    FROM dbo.Users
    WHERE Id = @Id;
END
