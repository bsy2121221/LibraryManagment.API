-- Status: 1 = Held, 2 = Active, 3 = Released, 4 = Expired
CREATE TABLE dbo.Bookings
(
    Id               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Bookings PRIMARY KEY,
    UserId           INT               NOT NULL CONSTRAINT FK_Bookings_Users REFERENCES dbo.Users (Id),
    SeatId           INT               NOT NULL CONSTRAINT FK_Bookings_Seats REFERENCES dbo.Seats (Id),
    Status           TINYINT           NOT NULL,
    HoldExpiresAt    DATETIME2(0)      NULL,
    StartDate        DATE              NULL,
    NextDueDate      DATE              NULL,
    ReleasedAt       DATETIME2(0)      NULL,
    ReleasedByUserId INT               NULL CONSTRAINT FK_Bookings_ReleasedBy REFERENCES dbo.Users (Id),
    ReleaseReason    NVARCHAR(500)     NULL,
    CreatedAt        DATETIME2(0)      NOT NULL CONSTRAINT DF_Bookings_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt        DATETIME2(0)      NULL,
    CONSTRAINT CK_Bookings_Status CHECK (Status IN (1, 2, 3, 4))
);
GO

-- One live (Held/Active) booking per seat, and per student.
-- When shifts are introduced, extend the seat index to (SeatId, ShiftId).
CREATE UNIQUE INDEX UX_Bookings_LiveSeat ON dbo.Bookings (SeatId) WHERE Status IN (1, 2);
GO

CREATE UNIQUE INDEX UX_Bookings_LiveUser ON dbo.Bookings (UserId) WHERE Status IN (1, 2);
GO

CREATE INDEX IX_Bookings_NextDueDate ON dbo.Bookings (NextDueDate) WHERE Status = 2;
GO
