-- Records a payment order created at the gateway (Status 1 = Created). Returns the new Id.
CREATE PROCEDURE dbo.usp_Payment_Create
    @UserId         INT,
    @BookingId      INT,
    @FeeInvoiceId   INT,
    @Gateway        VARCHAR(20),
    @GatewayOrderId VARCHAR(100),
    @Amount         DECIMAL(10,2),
    @Currency       CHAR(3)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Payments (UserId, BookingId, FeeInvoiceId, Gateway, GatewayOrderId, Amount, Currency, Status)
    OUTPUT INSERTED.Id
    VALUES (@UserId, @BookingId, @FeeInvoiceId, @Gateway, @GatewayOrderId, @Amount, @Currency, 1);
END
