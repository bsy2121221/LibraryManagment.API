-- Status: 1 = Created, 2 = Success, 3 = Failed, 4 = Refunded
CREATE TABLE dbo.Payments
(
    Id               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Payments PRIMARY KEY,
    UserId           INT               NOT NULL CONSTRAINT FK_Payments_Users REFERENCES dbo.Users (Id),
    BookingId        INT               NULL CONSTRAINT FK_Payments_Bookings REFERENCES dbo.Bookings (Id),
    FeeInvoiceId     INT               NULL CONSTRAINT FK_Payments_FeeInvoices REFERENCES dbo.FeeInvoices (Id),
    Gateway          VARCHAR(20)       NOT NULL,
    GatewayOrderId   VARCHAR(100)      NOT NULL,
    GatewayPaymentId VARCHAR(100)      NULL,
    Amount           DECIMAL(10,2)     NOT NULL,
    Currency         CHAR(3)           NOT NULL,
    Status           TINYINT           NOT NULL,
    FailureReason    NVARCHAR(500)     NULL,
    RawResponse      NVARCHAR(MAX)     NULL,
    CreatedAt        DATETIME2(0)      NOT NULL CONSTRAINT DF_Payments_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt        DATETIME2(0)      NULL,
    CONSTRAINT CK_Payments_Status CHECK (Status IN (1, 2, 3, 4))
);
GO

CREATE UNIQUE INDEX UX_Payments_GatewayOrderId ON dbo.Payments (GatewayOrderId);
GO

CREATE INDEX IX_Payments_UserId ON dbo.Payments (UserId);
GO
