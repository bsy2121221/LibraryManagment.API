-- Creates (@Id NULL) or updates a notice and returns it. Errors: 50404
CREATE PROCEDURE dbo.usp_SiteNotice_Save
    @Id          INT = NULL,
    @Message     NVARCHAR(300),
    @StartDate   DATE = NULL,
    @EndDate     DATE = NULL,
    @IsActive    BIT,
    @SortOrder   INT,
    @AdminUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @Id IS NULL
    BEGIN
        INSERT INTO dbo.SiteNotices (Message, StartDate, EndDate, IsActive, SortOrder, CreatedByUserId)
        VALUES (@Message, @StartDate, @EndDate, @IsActive, @SortOrder, @AdminUserId);

        SET @Id = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.SiteNotices
        SET Message = @Message, StartDate = @StartDate, EndDate = @EndDate,
            IsActive = @IsActive, SortOrder = @SortOrder, UpdatedAt = SYSUTCDATETIME()
        WHERE Id = @Id;

        IF @@ROWCOUNT = 0
            THROW 50404, N'Notice not found.', 1;
    END

    SELECT Id, Message, StartDate, EndDate, IsActive, SortOrder, CreatedAt, CreatedByUserId, UpdatedAt
    FROM dbo.SiteNotices
    WHERE Id = @Id;
END
