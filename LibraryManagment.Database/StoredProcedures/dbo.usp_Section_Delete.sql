-- Deletes a section and its seats. Only allowed when none of its seats was ever booked;
-- otherwise the admin should deactivate it so booking history is kept.
-- Errors: 50404 not found, 50409 has booking history
CREATE PROCEDURE dbo.usp_Section_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    IF NOT EXISTS (SELECT 1 FROM dbo.Sections WITH (UPDLOCK, HOLDLOCK) WHERE Id = @Id)
        THROW 50404, N'Section not found.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Seats st JOIN dbo.Bookings b ON b.SeatId = st.Id WHERE st.SectionId = @Id)
       OR EXISTS (SELECT 1 FROM dbo.Seats st
                    JOIN dbo.SeatChangeHistory h ON h.FromSeatId = st.Id OR h.ToSeatId = st.Id
                   WHERE st.SectionId = @Id)
        THROW 50409, N'This section has booking history and cannot be deleted. Deactivate it instead.', 1;

    DELETE FROM dbo.Seats WHERE SectionId = @Id;
    DELETE FROM dbo.Sections WHERE Id = @Id;

    COMMIT TRANSACTION;
END
