-- Every seat change of a student, newest first. FromSeat NULL = booked, ToSeat NULL = released.
CREATE PROCEDURE dbo.usp_SeatChangeHistory_GetByUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT h.Id,
           h.ChangedAt,
           h.Reason,
           fs.SeatLabel   AS FromSeatLabel,
           fsec.Name      AS FromSectionName,
           ts.SeatLabel   AS ToSeatLabel,
           tsec.Name      AS ToSectionName,
           cb.FullName    AS ChangedByName
    FROM dbo.SeatChangeHistory h
    LEFT JOIN dbo.Seats fs      ON fs.Id = h.FromSeatId
    LEFT JOIN dbo.Sections fsec ON fsec.Id = fs.SectionId
    LEFT JOIN dbo.Seats ts      ON ts.Id = h.ToSeatId
    LEFT JOIN dbo.Sections tsec ON tsec.Id = ts.SectionId
    JOIN dbo.Users cb           ON cb.Id = h.ChangedByUserId
    WHERE h.UserId = @UserId
    ORDER BY h.ChangedAt DESC, h.Id DESC;
END
