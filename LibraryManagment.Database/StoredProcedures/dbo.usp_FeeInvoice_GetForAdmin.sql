-- Admin fee list. @Status NULL = unpaid (Due + Overdue). @Search matches student name, mobile or seat.
CREATE PROCEDURE dbo.usp_FeeInvoice_GetForAdmin
    @Status TINYINT       = NULL,
    @Search NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Pattern NVARCHAR(102) = IIF(NULLIF(LTRIM(RTRIM(@Search)), N'') IS NULL, NULL, CONCAT(N'%', LTRIM(RTRIM(@Search)), N'%'));

    SELECT TOP (500)
           Id, UserId, BookingId, PeriodStart, PeriodEnd, Amount, DueDate, Status, PaidAt, CreatedAt,
           SeatLabel, SectionName, StudentName, StudentMobile, PaidVia
    FROM dbo.vw_FeeInvoiceDetails
    WHERE ((@Status IS NULL AND Status IN (1, 3)) OR Status = @Status)
      AND (@Pattern IS NULL OR StudentName LIKE @Pattern OR StudentMobile LIKE @Pattern OR SeatLabel LIKE @Pattern)
    ORDER BY IIF(Status IN (1, 3), 0, 1), DueDate, Id;
END
