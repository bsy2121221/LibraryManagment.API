CREATE TABLE dbo.SeatChangeHistory
(
    Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SeatChangeHistory PRIMARY KEY,
    UserId          INT               NOT NULL CONSTRAINT FK_SeatChangeHistory_Users REFERENCES dbo.Users (Id),
    FromSeatId      INT               NULL CONSTRAINT FK_SeatChangeHistory_FromSeat REFERENCES dbo.Seats (Id),
    ToSeatId        INT               NULL CONSTRAINT FK_SeatChangeHistory_ToSeat REFERENCES dbo.Seats (Id),
    ChangedByUserId INT               NOT NULL CONSTRAINT FK_SeatChangeHistory_ChangedBy REFERENCES dbo.Users (Id),
    Reason          NVARCHAR(500)     NULL,
    ChangedAt       DATETIME2(0)      NOT NULL CONSTRAINT DF_SeatChangeHistory_ChangedAt DEFAULT (SYSUTCDATETIME())
);
GO

CREATE INDEX IX_SeatChangeHistory_UserId ON dbo.SeatChangeHistory (UserId);
GO
