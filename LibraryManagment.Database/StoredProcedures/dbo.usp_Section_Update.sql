-- Errors: 50404 not found, 50409 change not allowed while students are seated, 50400 seats outside the new size
CREATE PROCEDURE dbo.usp_Section_Update
    @Id                 INT,
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
    SET XACT_ABORT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();
    DECLARE @CurrentGender TINYINT, @CurrentIsActive BIT;

    BEGIN TRANSACTION;

    SELECT @CurrentGender = Gender, @CurrentIsActive = IsActive
    FROM dbo.Sections WITH (UPDLOCK, HOLDLOCK)
    WHERE Id = @Id;

    IF @CurrentGender IS NULL
        THROW 50404, N'Section not found.', 1;

    -- Changing gender or hiding the section would strand the students seated in it
    IF (@Gender <> @CurrentGender OR (@CurrentIsActive = 1 AND @IsActive = 0))
       AND EXISTS (SELECT 1 FROM dbo.Seats st
                     JOIN dbo.Bookings b ON b.SeatId = st.Id
                    WHERE st.SectionId = @Id AND st.IsDeleted = 0
                      AND (b.Status = 2 OR (b.Status = 1 AND b.HoldExpiresAt > @Now)))
        THROW 50409, N'Students are seated in this section. Release or move them before changing its gender or deactivating it.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Seats
                WHERE SectionId = @Id AND IsDeleted = 0
                  AND (PosX > @LayoutWidth OR PosY > @LayoutHeight))
        THROW 50400, N'Some seats are outside the new layout size. Move them inside first.', 1;

    UPDATE dbo.Sections
    SET Name = @Name,
        Gender = @Gender,
        LayoutWidth = @LayoutWidth,
        LayoutHeight = @LayoutHeight,
        BackgroundImageUrl = @BackgroundImageUrl,
        DisplayOrder = @DisplayOrder,
        IsActive = @IsActive,
        UpdatedAt = @Now
    WHERE Id = @Id;

    COMMIT TRANSACTION;
END
