-- Returns the new section Id
CREATE PROCEDURE dbo.usp_Section_Create
    @Name               NVARCHAR(100),
    @Gender             TINYINT,
    @LayoutWidth        INT,
    @LayoutHeight       INT,
    @BackgroundImageUrl NVARCHAR(500),
    @DisplayOrder       INT,
    @IsActive           BIT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Sections (Name, Gender, LayoutWidth, LayoutHeight, BackgroundImageUrl, DisplayOrder, IsActive)
    OUTPUT INSERTED.Id
    VALUES (@Name, @Gender, @LayoutWidth, @LayoutHeight, @BackgroundImageUrl, @DisplayOrder, @IsActive);
END
