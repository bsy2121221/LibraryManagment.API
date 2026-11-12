-- Admin moves a student with an Active booking to another free seat in a section for their gender.
-- Errors: 50400 invalid target, 50404 not found, 50409 not movable / seat taken
CREATE PROCEDURE dbo.usp_Booking_MoveSeat
    @BookingId   INT,
    @ToSeatId    INT,
    @AdminUserId INT,
    @Reason      NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();
    DECLARE @Status TINYINT, @UserId INT, @FromSeatId INT, @UserGender TINYINT, @SectionGender TINYINT;
    DECLARE @Label NVARCHAR(20), @Message NVARCHAR(2048);

    BEGIN TRANSACTION;

    SELECT @Status = Status, @UserId = UserId, @FromSeatId = SeatId
    FROM dbo.Bookings WITH (UPDLOCK, HOLDLOCK)
    WHERE Id = @BookingId;

    IF @Status IS NULL
        THROW 50404, N'Booking not found.', 1;

    IF @Status <> 2
        THROW 50409, N'Only an active booking can be moved to another seat.', 1;

    IF @ToSeatId = @FromSeatId
        THROW 50400, N'The student is already on this seat.', 1;

    SELECT @SectionGender = sec.Gender, @Label = st.SeatLabel
    FROM dbo.Seats st WITH (UPDLOCK, HOLDLOCK)
    JOIN dbo.Sections sec ON sec.Id = st.SectionId
    WHERE st.Id = @ToSeatId AND st.IsDeleted = 0 AND st.Status = 1;

    IF @SectionGender IS NULL
        THROW 50404, N'The new seat was not found or is disabled.', 1;

    SELECT @UserGender = Gender FROM dbo.Users WHERE Id = @UserId;
    IF @SectionGender <> @UserGender
    BEGIN
        SET @Message = CONCAT(N'Seat ', @Label, N' is in a ', IIF(@SectionGender = 2, N'Girls', N'Boys'), N' section.');
        THROW 50400, @Message, 1;
    END

    UPDATE dbo.Bookings
    SET Status = 4, UpdatedAt = @Now
    WHERE SeatId = @ToSeatId AND Status = 1 AND HoldExpiresAt <= @Now;

    IF EXISTS (SELECT 1 FROM dbo.Bookings WHERE SeatId = @ToSeatId AND Status IN (1, 2))
    BEGIN
        SET @Message = CONCAT(N'Seat ', @Label, N' is already taken.');
        THROW 50409, @Message, 1;
    END

    UPDATE dbo.Bookings SET SeatId = @ToSeatId, UpdatedAt = @Now WHERE Id = @BookingId;

    INSERT INTO dbo.SeatChangeHistory (UserId, FromSeatId, ToSeatId, ChangedByUserId, Reason)
    VALUES (@UserId, @FromSeatId, @ToSeatId, @AdminUserId, @Reason);

    COMMIT TRANSACTION;
END
