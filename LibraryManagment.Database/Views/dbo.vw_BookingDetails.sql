-- A booking with its seat, section and student. Used by every booking read procedure.
-- NextDueDate = the earliest unpaid fee's due date, or (when everything is paid) the start of the next
-- period that has no invoice yet (dbo.Bookings.NextDueDate).
CREATE VIEW dbo.vw_BookingDetails
AS
SELECT b.Id,
       b.UserId,
       b.SeatId,
       b.Status,
       b.HoldExpiresAt,
       b.StartDate,
       COALESCE(unpaid.FirstDueDate, b.NextDueDate) AS NextDueDate,
       CAST(IIF(unpaid.OverdueCount > 0, 1, 0) AS BIT) AS HasOverdueFee,
       b.ReleasedAt,
       b.ReleaseReason,
       b.CreatedAt,
       st.SeatLabel,
       st.SectionId,
       sec.Name     AS SectionName,
       sec.Gender   AS SectionGender,
       u.FullName   AS StudentName,
       u.Mobile     AS StudentMobile,
       u.Email      AS StudentEmail
FROM dbo.Bookings b
JOIN dbo.Seats st     ON st.Id = b.SeatId
JOIN dbo.Sections sec ON sec.Id = st.SectionId
JOIN dbo.Users u      ON u.Id = b.UserId
OUTER APPLY (SELECT MIN(i.DueDate)                AS FirstDueDate,
                    SUM(IIF(i.Status = 3, 1, 0))  AS OverdueCount
             FROM dbo.FeeInvoices i
             WHERE i.BookingId = b.Id AND i.Status IN (1, 3)) unpaid;
