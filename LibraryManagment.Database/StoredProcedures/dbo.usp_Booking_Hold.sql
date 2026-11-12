/*
Holds a seat for a student for @HoldMinutes while they pay. Returns the booking (usp_Booking_GetById shape).
  - Holding the seat you already hold returns that hold unchanged.
  - Holding a different seat gives up your previous hold.
  - Holds whose time has run out are expired here, so the seat is free even before the expiry job runs.
Errors: 50400 not allowed, 50404 seat not available, 50409 seat taken / student already has a seat
*/
CREATE PROCEDURE dbo.usp_Booking_Hold
    @UserId      INT,
    @SeatId      INT,
    @HoldMinutes INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();
    DECLARE @UserGender TINYINT, @SectionGender TINYINT, @Label NVARCHAR(20), @Message NVARCHAR(2048);
    DECLARE @BookingId INT, @ExistingStatus TINYINT, @ExistingSeatId INT;

    SELECT @UserGender = Gender FROM dbo.Users WHERE Id = @UserId AND Role = 2 AND IsActive = 1;
    IF @UserGender IS NULL
        THROW 50400, N'Only active student accounts can book a seat.', 1;

    BEGIN TRANSACTION;

    SELECT @SectionGender = sec.Gender, @Label = st.SeatLabel
    FROM dbo.Seats st WITH (UPDLOCK, HOLDLOCK)
    JOIN dbo.Sections sec ON sec.Id = st.SectionId
    WHERE st.Id = @SeatId AND st.IsDeleted = 0 AND st.Status = 1 AND sec.IsActive = 1;

    IF @SectionGender IS NULL
        THROW 50404, N'This seat is not available for booking.', 1;

    IF @SectionGender <> @UserGender
    BEGIN
        SET @Message = CONCAT(N'You can only book seats in the ', IIF(@UserGender = 2, N'Girls', N'Boys'), N' section.');
        THROW 50400, @Message, 1;
    END

    -- Clear holds that ran out, on this seat or by this student
    UPDATE dbo.Bookings
    SET Status = 4, UpdatedAt = @Now
    WHERE Status = 1 AND HoldExpiresAt <= @Now AND (SeatId = @SeatId OR UserId = @UserId);

    SELECT @BookingId = Id, @ExistingStatus = Status, @ExistingSeatId = SeatId
    FROM dbo.Bookings WITH (UPDLOCK, HOLDLOCK)
    WHERE UserId = @UserId AND Status IN (1, 2);

    IF @ExistingStatus = 2
        THROW 50409, N'You already have a booked seat. Ask the library to move you if you want a different seat.', 1;

    IF @ExistingStatus = 1 AND @ExistingSeatId <> @SeatId
    BEGIN
        -- Switching seats: give up the previous hold
        UPDATE dbo.Bookings SET Status = 4, UpdatedAt = @Now WHERE Id = @BookingId;
        SET @BookingId = NULL;
    END

    IF @BookingId IS NULL
    BEGIN
        IF EXISTS (SELECT 1 FROM dbo.Bookings WHERE SeatId = @SeatId AND Status IN (1, 2))
        BEGIN
            SET @Message = CONCAT(N'Sorry, seat ', @Label, N' was just taken. Please choose another seat.');
            THROW 50409, @Message, 1;
        END

        INSERT INTO dbo.Bookings (UserId, SeatId, Status, HoldExpiresAt)
        VALUES (@UserId, @SeatId, 1, DATEADD(MINUTE, @HoldMinutes, @Now));

        SET @BookingId = SCOPE_IDENTITY();
    END

    COMMIT TRANSACTION;

    EXEC dbo.usp_Booking_GetById @Id = @BookingId;
END
