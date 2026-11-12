CREATE PROCEDURE dbo.usp_Payment_GetByOrderId
    @GatewayOrderId VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, UserId, BookingId, FeeInvoiceId, Gateway, GatewayOrderId, GatewayPaymentId, Amount, Currency,
           Status, FailureReason, CreatedAt, UpdatedAt
    FROM dbo.Payments
    WHERE GatewayOrderId = @GatewayOrderId;
END
