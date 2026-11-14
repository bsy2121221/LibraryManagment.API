-- A monthly fee invoice with its seat, student and how it was paid.
-- Status: 1 = Due, 2 = Paid, 3 = Overdue, 4 = Cancelled
CREATE VIEW dbo.vw_FeeInvoiceDetails
AS
SELECT i.Id,
       i.UserId,
       i.BookingId,
       i.PeriodStart,
       i.PeriodEnd,
       i.Amount,
       i.DueDate,
       i.Status,
       i.PaidAt,
       i.CreatedAt,
       st.SeatLabel,
       sec.Name   AS SectionName,
       u.FullName AS StudentName,
       u.Mobile   AS StudentMobile,
       pay.Gateway AS PaidVia
FROM dbo.FeeInvoices i
JOIN dbo.Bookings b   ON b.Id = i.BookingId
JOIN dbo.Seats st     ON st.Id = b.SeatId
JOIN dbo.Sections sec ON sec.Id = st.SectionId
JOIN dbo.Users u      ON u.Id = i.UserId
OUTER APPLY (SELECT TOP (1) p.Gateway
             FROM dbo.Payments p
             WHERE p.FeeInvoiceId = i.Id AND p.Status = 2
             ORDER BY p.Id DESC) pay;
