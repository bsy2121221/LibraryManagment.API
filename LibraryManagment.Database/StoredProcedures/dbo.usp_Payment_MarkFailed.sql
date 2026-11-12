-- A payment attempt failed or was cancelled. Never overwrites a successful payment.
-- The seat hold is kept, so the student can retry until it runs out.
CREATE PROCEDURE dbo.usp_Payment_MarkFailed
    @GatewayOrderId   VARCHAR(100),
    @GatewayPaymentId VARCHAR(100),
    @FailureReason    NVARCHAR(500),
    @RawResponse      NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Payments
    SET Status = 3,
        GatewayPaymentId = ISNULL(@GatewayPaymentId, GatewayPaymentId),
        FailureReason = @FailureReason,
        RawResponse = ISNULL(@RawResponse, RawResponse),
        UpdatedAt = SYSUTCDATETIME()
    WHERE GatewayOrderId = @GatewayOrderId AND Status IN (1, 3);
END
