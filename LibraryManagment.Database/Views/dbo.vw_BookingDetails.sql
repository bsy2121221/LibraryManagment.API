-- A booking with its seat, section and student. Used by every booking read procedure.
CREATE VIEW dbo.vw_BookingDetails
AS
SELECT b.Id,
       b.UserId,
       b.SeatId,
       b.Status,
       b.HoldExpiresAt,
       b.StartDate,
       b.NextDueDate,
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
JOIN dbo.Users u      ON u.Id = b.UserId;
