/*
Saves the full seat layout of one section, as drawn in the admin designer.

@SeatsJson is an array of: { "Id": int|null, "SeatLabel": string, "PosX": number, "PosY": number,
                             "Rotation": int, "Status": 1|2 }
  - Id null      -> new seat
  - Id present   -> existing seat is updated
  - existing seats missing from the array are removed (soft delete, so booking history stays valid)

Seats that a student holds or occupies cannot be removed or disabled.
Errors: 50400 invalid input, 50404 section/seat not found, 50409 seat in use
*/
CREATE PROCEDURE dbo.usp_Seat_SaveLayout
    @SectionId INT,
    @SeatsJson NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();
    DECLARE @Message NVARCHAR(2048), @Label NVARCHAR(20);

    IF @SeatsJson IS NULL OR ISJSON(@SeatsJson) = 0
        THROW 50400, N'Seat layout is not valid JSON.', 1;

    DECLARE @Items TABLE
    (
        Id        INT           NULL,
        SeatLabel NVARCHAR(20)  NULL,
        PosX      DECIMAL(9,2)  NULL,
        PosY      DECIMAL(9,2)  NULL,
        Rotation  SMALLINT      NULL,
        Status    TINYINT       NULL
    );

    INSERT INTO @Items (Id, SeatLabel, PosX, PosY, Rotation, Status)
    SELECT Id, LTRIM(RTRIM(SeatLabel)), PosX, PosY, ISNULL(Rotation, 0), ISNULL(Status, 1)
    FROM OPENJSON(@SeatsJson)
    WITH
    (
        Id        INT           '$.Id',
        SeatLabel NVARCHAR(20)  '$.SeatLabel',
        PosX      DECIMAL(9,2)  '$.PosX',
        PosY      DECIMAL(9,2)  '$.PosY',
        Rotation  SMALLINT      '$.Rotation',
        Status    TINYINT       '$.Status'
    );

    IF EXISTS (SELECT 1 FROM @Items
                WHERE SeatLabel IS NULL OR SeatLabel = N'' OR PosX IS NULL OR PosY IS NULL
                   OR Status NOT IN (1, 2))
        THROW 50400, N'Every seat needs a label, a position and a valid status.', 1;

    SELECT TOP (1) @Label = SeatLabel FROM @Items GROUP BY SeatLabel HAVING COUNT(*) > 1;
    IF @Label IS NOT NULL
    BEGIN
        SET @Message = CONCAT(N'Seat label "', @Label, N'" is used more than once.');
        THROW 50400, @Message, 1;
    END

    IF EXISTS (SELECT Id FROM @Items WHERE Id IS NOT NULL GROUP BY Id HAVING COUNT(*) > 1)
        THROW 50400, N'The same seat appears more than once.', 1;

    BEGIN TRANSACTION;

    -- Lock the section so two admins saving at once cannot interleave
    IF NOT EXISTS (SELECT 1 FROM dbo.Sections WITH (UPDLOCK, HOLDLOCK) WHERE Id = @SectionId)
        THROW 50404, N'Section not found.', 1;

    IF EXISTS (SELECT 1 FROM @Items i
                WHERE i.Id IS NOT NULL
                  AND NOT EXISTS (SELECT 1 FROM dbo.Seats st
                                   WHERE st.Id = i.Id AND st.SectionId = @SectionId AND st.IsDeleted = 0))
        THROW 50404, N'One or more seats were not found in this section. Reload the layout and try again.', 1;

    -- Seats being removed or disabled must not have a student on them
    SET @Label = NULL;
    SELECT TOP (1) @Label = st.SeatLabel
    FROM dbo.Seats st
    JOIN dbo.Bookings b
      ON b.SeatId = st.Id
     AND (b.Status = 2 OR (b.Status = 1 AND b.HoldExpiresAt > @Now))
    LEFT JOIN @Items i ON i.Id = st.Id
    WHERE st.SectionId = @SectionId AND st.IsDeleted = 0
      AND (i.Id IS NULL OR i.Status = 2);

    IF @Label IS NOT NULL
    BEGIN
        SET @Message = CONCAT(N'Seat "', @Label, N'" has a student. Release or move the student before removing or disabling it.');
        THROW 50409, @Message, 1;
    END

    -- Remove first, so a new seat can reuse the label of a removed one
    UPDATE st
    SET IsDeleted = 1, UpdatedAt = @Now
    FROM dbo.Seats st
    WHERE st.SectionId = @SectionId AND st.IsDeleted = 0
      AND NOT EXISTS (SELECT 1 FROM @Items i WHERE i.Id = st.Id);

    UPDATE st
    SET SeatLabel = i.SeatLabel,
        PosX = i.PosX,
        PosY = i.PosY,
        Rotation = i.Rotation,
        Status = i.Status,
        UpdatedAt = @Now
    FROM dbo.Seats st
    JOIN @Items i ON i.Id = st.Id
    WHERE st.SeatLabel <> i.SeatLabel OR st.PosX <> i.PosX OR st.PosY <> i.PosY
       OR st.Rotation <> i.Rotation OR st.Status <> i.Status;

    INSERT INTO dbo.Seats (SectionId, SeatLabel, PosX, PosY, Rotation, Status)
    SELECT @SectionId, SeatLabel, PosX, PosY, Rotation, Status
    FROM @Items
    WHERE Id IS NULL;

    COMMIT TRANSACTION;
END
