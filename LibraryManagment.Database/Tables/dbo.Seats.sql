-- Status is admin-controlled only: 1 = Active, 2 = Disabled.
-- Whether a seat is taken is derived from dbo.Bookings.
CREATE TABLE dbo.Seats
(
    Id        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Seats PRIMARY KEY,
    SectionId INT               NOT NULL CONSTRAINT FK_Seats_Sections REFERENCES dbo.Sections (Id),
    SeatLabel NVARCHAR(20)      NOT NULL,
    PosX      DECIMAL(9,2)      NOT NULL CONSTRAINT DF_Seats_PosX DEFAULT (0),
    PosY      DECIMAL(9,2)      NOT NULL CONSTRAINT DF_Seats_PosY DEFAULT (0),
    Rotation  SMALLINT          NOT NULL CONSTRAINT DF_Seats_Rotation DEFAULT (0),
    Status    TINYINT           NOT NULL CONSTRAINT DF_Seats_Status DEFAULT (1),
    IsDeleted BIT               NOT NULL CONSTRAINT DF_Seats_IsDeleted DEFAULT (0),
    CreatedAt DATETIME2(0)      NOT NULL CONSTRAINT DF_Seats_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt DATETIME2(0)      NULL,
    CONSTRAINT CK_Seats_Status CHECK (Status IN (1, 2))
);
GO

CREATE UNIQUE INDEX UX_Seats_SectionLabel ON dbo.Seats (SectionId, SeatLabel) WHERE IsDeleted = 0;
GO
