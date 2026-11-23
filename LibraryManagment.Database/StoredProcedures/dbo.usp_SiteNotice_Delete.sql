-- Errors: 50404
CREATE PROCEDURE dbo.usp_SiteNotice_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.SiteNotices WHERE Id = @Id;

    IF @@ROWCOUNT = 0
        THROW 50404, N'Notice not found.', 1;
END
