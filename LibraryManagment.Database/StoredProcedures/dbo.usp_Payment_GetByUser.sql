CREATE PROCEDURE dbo.usp_Payment_GetByUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (200)
           Id, UserId, BookingId, FeeInvoiceId, Gateway, GatewayOrderId, GatewayPaymentId, Amount, Currency,
           Status, FailureReason, CreatedAt, UpdatedAt
    FROM dbo.Payments
    WHERE UserId = @UserId
    ORDER BY CreatedAt DESC, Id DESC;
END
