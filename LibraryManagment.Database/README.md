# LibraryManagement.Database

All database objects live here. The API does **not** create or change the schema; run these scripts yourself.

```
Tables/                          one file per table (with its indexes)
StoredProcedures/                one file per procedure: dbo.usp_<Entity>_<Action>.sql
Scripts/PostDeployment/          seed data (Settings, NotificationTemplates), safe to re-run
Scripts/Manual/00_CreateAll.sql  creates everything on an empty database, in the right order
Scripts/Manual/01_CreateAllProcedures.sql   drops and recreates every procedure
Scripts/Manual/02_CreateAllViews.sql        drops and recreates every view (run before 01)
Scripts/Manual/Upgrade/                     one-time scripts that bring an existing database up to a new step
```

## New database
1. `CREATE DATABASE LibraryManagement;`
2. Open `Scripts/Manual/00_CreateAll.sql` in SSMS, enable **Query → SQLCMD Mode**, check `DbProjectPath`, press F5.
3. Start the API. It creates the first admin from `AdminSeed` in appsettings (the password must be hashed in code).

## Changing things later
- **Procedure changed / added:** run `01_CreateAllProcedures.sql` (add a `DROP` + `:r` pair for new procedures).
  To run one procedure file directly in SSMS on an existing database, change `CREATE PROCEDURE` to `CREATE OR ALTER PROCEDURE` in the editor before executing. Keep `CREATE PROCEDURE` in the saved file, because the database project only accepts `CREATE`.
- **Table changed:** edit the table file so it stays the source of truth, then either write an `ALTER TABLE` script yourself
  or use Visual Studio **Schema Compare** / **Publish**, which generates the `ALTER` for you.
- **New table:** add the file under `Tables/` and a `:r` line in `00_CreateAll.sql` (after the tables it references).

## Notes
- Always keep `SET QUOTED_IDENTIFIER ON` when creating tables or procedures. Filtered indexes (e.g. on `dbo.Bookings`) require it.
- Building the project (`dotnet build` or Visual Studio) validates every table, column and procedure reference and produces a `.dacpac`.

## Upgrading an existing database
Run the scripts in `Scripts/Manual/Upgrade/` that you have not run yet, in number order (SQLCMD Mode).
Each one refuses to run twice. A new database does not need them: `00_CreateAll.sql` already has everything.

## Messages to students (dbo.Notifications)
Procedures that change something for a student (booking, payment, seat move/release, profile edit, password reset)
add the messages to `dbo.Notifications` with Status = 1 (Ready) inside their own transaction.
The API's background job sends Ready rows every minute and sets Status 3 (Sent) or 4 (Failed, after 3 attempts).
Status: 1 Ready, 2 Processing, 3 Sent, 4 Failed, 5 Skipped, 6 Cancelled.
