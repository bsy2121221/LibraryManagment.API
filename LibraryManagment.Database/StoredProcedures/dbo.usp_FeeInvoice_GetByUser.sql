-- A student's monthly fees, newest period first
CREATE PROCEDURE dbo.usp_FeeInvoice_GetByUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (120)
           Id, UserId, BookingId, PeriodStart, PeriodEnd, Amount, DueDate, Status, PaidAt, CreatedAt,
           SeatLabel, SectionName, StudentName, StudentMobile, PaidVia
    FROM dbo.vw_FeeInvoiceDetails
    WHERE UserId = @UserId
    ORDER BY PeriodStart DESC, Id DESC;
END
