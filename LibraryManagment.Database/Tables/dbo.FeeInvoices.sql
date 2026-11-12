-- Status: 1 = Due, 2 = Paid, 3 = Overdue, 4 = Cancelled
CREATE TABLE dbo.FeeInvoices
(
    Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FeeInvoices PRIMARY KEY,
    UserId      INT               NOT NULL CONSTRAINT FK_FeeInvoices_Users REFERENCES dbo.Users (Id),
    BookingId   INT               NOT NULL CONSTRAINT FK_FeeInvoices_Bookings REFERENCES dbo.Bookings (Id),
    PeriodStart DATE              NOT NULL,
    PeriodEnd   DATE              NOT NULL,
    Amount      DECIMAL(10,2)     NOT NULL,
    DueDate     DATE              NOT NULL,
    Status      TINYINT           NOT NULL,
    PaidAt      DATETIME2(0)      NULL,
    CreatedAt   DATETIME2(0)      NOT NULL CONSTRAINT DF_FeeInvoices_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt   DATETIME2(0)      NULL,
    CONSTRAINT CK_FeeInvoices_Status CHECK (Status IN (1, 2, 3, 4))
);
GO

CREATE UNIQUE INDEX UX_FeeInvoices_BookingPeriod ON dbo.FeeInvoices (BookingId, PeriodStart);
GO

CREATE INDEX IX_FeeInvoices_Status_DueDate ON dbo.FeeInvoices (Status, DueDate);
GO
