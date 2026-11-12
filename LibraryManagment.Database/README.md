# LibraryManagement.Database

All database objects live here. The API does **not** create or change the schema; run these scripts yourself.

```
Tables/                          one file per table (with its indexes)
StoredProcedures/                one file per procedure: dbo.usp_<Entity>_<Action>.sql
Scripts/PostDeployment/          seed data (Settings, NotificationTemplates), safe to re-run
Scripts/Manual/00_CreateAll.sql  creates everything on an empty database, in the right order
Scripts/Manual/01_CreateAllProcedures.sql   drops and recreates every procedure
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
