/*
Monthly fee housekeeping, run every minute by the API's billing job (each step only changes rows once).
  1. Creates next month's invoice @InvoiceDaysBefore days before it is due.
     dbo.Bookings.NextDueDate = start of the next period that has no invoice yet; it moves on one month
     each time an invoice is created. Periods follow the booking's start day (31 Jan -> 28 Feb -> 31 Mar).
  2. Marks unpaid invoices Overdue once the due date + @GraceDays has passed.
  3. If @AutoReleaseAfterOverdueDays > 0: releases seats whose fee has been overdue that many days
     (recorded as done by the first admin account; the student is notified like any release).
Dates are the library's local date (India Standard Time).
Returns one row: InvoicesCreated, InvoicesMarkedOverdue, SeatsReleased
*/
CREATE PROCEDURE dbo.usp_Billing_Run
    @MonthlyFee                  DECIMAL(10,2),
    @InvoiceDaysBefore           INT,
    @GraceDays                   INT,
    @AutoReleaseAfterOverdueDays INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Now DATETIME2(0) = SYSUTCDATETIME();
    DECLARE @Today DATE = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'India Standard Time' AS DATE);
    DECLARE @Created INT = 0, @Overdue INT = 0, @Released INT = 0;

    -- 0. Periods that already have an invoice (added by the admin in advance): move past them
    DECLARE @Skips INT = 0;
    WHILE @Skips < 24
    BEGIN
        UPDATE b
        SET NextDueDate = DATEADD(MONTH, DATEDIFF(MONTH, b.StartDate, b.NextDueDate) + 1, b.StartDate),
            UpdatedAt = @Now
        FROM dbo.Bookings b
        WHERE b.Status = 2
          AND b.StartDate IS NOT NULL
          AND b.NextDueDate IS NOT NULL
          AND EXISTS (SELECT 1 FROM dbo.FeeInvoices i WHERE i.BookingId = b.Id AND i.PeriodStart = b.NextDueDate);

        IF @@ROWCOUNT = 0 BREAK;
        SET @Skips += 1;
    END

    -- 1. Next month's invoice
    BEGIN TRANSACTION;

    DECLARE @New TABLE (BookingId INT PRIMARY KEY);

    INSERT INTO dbo.FeeInvoices (UserId, BookingId, PeriodStart, PeriodEnd, Amount, DueDate, Status)
    OUTPUT inserted.BookingId INTO @New
    SELECT b.UserId, b.Id, b.NextDueDate, DATEADD(DAY, -1, p.NextPeriodStart), @MonthlyFee, b.NextDueDate, 1
    FROM dbo.Bookings b WITH (UPDLOCK)
    CROSS APPLY (SELECT DATEADD(MONTH, DATEDIFF(MONTH, b.StartDate, b.NextDueDate) + 1, b.StartDate) AS NextPeriodStart) p
    WHERE b.Status = 2
      AND b.StartDate IS NOT NULL
      AND b.NextDueDate IS NOT NULL
      AND DATEADD(DAY, -@InvoiceDaysBefore, b.NextDueDate) <= @Today
      AND NOT EXISTS (SELECT 1 FROM dbo.FeeInvoices i WHERE i.BookingId = b.Id AND i.PeriodStart = b.NextDueDate);

    SET @Created = @@ROWCOUNT;

    UPDATE b
    SET NextDueDate = DATEADD(MONTH, DATEDIFF(MONTH, b.StartDate, b.NextDueDate) + 1, b.StartDate),
        UpdatedAt = @Now
    FROM dbo.Bookings b
    JOIN @New n ON n.BookingId = b.Id;

    COMMIT TRANSACTION;

    -- 2. Overdue
    UPDATE dbo.FeeInvoices
    SET Status = 3, UpdatedAt = @Now
    WHERE Status = 1 AND DATEADD(DAY, @GraceDays, DueDate) < @Today;

    SET @Overdue = @@ROWCOUNT;

    -- 3. Automatic release
    IF @AutoReleaseAfterOverdueDays > 0
    BEGIN
        DECLARE @SystemUserId INT = (SELECT MIN(Id) FROM dbo.Users WHERE Role = 1);
        DECLARE @BookingId INT, @Reason NVARCHAR(500);

        WHILE @SystemUserId IS NOT NULL
        BEGIN
            SET @BookingId = NULL;

            SELECT TOP (1) @BookingId = b.Id,
                           @Reason = CONCAT(N'Released automatically: fee due on ', CONVERT(VARCHAR(11), i.DueDate, 106), N' was not paid')
            FROM dbo.Bookings b
            JOIN dbo.FeeInvoices i ON i.BookingId = b.Id
            WHERE b.Status = 2
              AND i.Status = 3
              AND DATEADD(DAY, @GraceDays + @AutoReleaseAfterOverdueDays, i.DueDate) <= @Today
            ORDER BY i.DueDate, b.Id;

            IF @BookingId IS NULL BREAK;

            EXEC dbo.usp_Booking_Release @BookingId = @BookingId, @AdminUserId = @SystemUserId, @Reason = @Reason;
            SET @Released += 1;
        END
    END

    SELECT @Created AS InvoicesCreated, @Overdue AS InvoicesMarkedOverdue, @Released AS SeatsReleased;
END
