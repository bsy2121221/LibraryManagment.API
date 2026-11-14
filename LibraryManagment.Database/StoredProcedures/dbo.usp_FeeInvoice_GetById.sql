CREATE PROCEDURE dbo.usp_FeeInvoice_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, UserId, BookingId, PeriodStart, PeriodEnd, Amount, DueDate, Status, PaidAt, CreatedAt,
           SeatLabel, SectionName, StudentName, StudentMobile, PaidVia
    FROM dbo.vw_FeeInvoiceDetails
    WHERE Id = @Id;
END
