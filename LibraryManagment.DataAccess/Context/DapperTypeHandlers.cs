using Dapper;
using System.Data;

namespace LibraryManagement.DataAccess.Context
{
    // All DATETIME2 columns hold UTC (SYSUTCDATETIME()). Marking them as UTC makes the API send "...Z",
    // so browsers do not mistake them for local time (important for the seat-hold countdown).
    // DATE columns (StartDate, NextDueDate, DueDate) map to DateOnly and are sent as "yyyy-MM-dd".
    public static class DapperTypeHandlers
    {
        private static int _registered;

        public static void Register()
        {
            if (Interlocked.Exchange(ref _registered, 1) == 1)
            {
                return;
            }

            SqlMapper.AddTypeHandler(new UtcDateTimeHandler());
            SqlMapper.AddTypeHandler(new DateOnlyHandler());
        }

        private sealed class UtcDateTimeHandler : SqlMapper.TypeHandler<DateTime>
        {
            public override void SetValue(IDbDataParameter parameter, DateTime value)
            {
                parameter.DbType = DbType.DateTime2;
                parameter.Value = value;
            }

            public override DateTime Parse(object value) => DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
        }

        private sealed class DateOnlyHandler : SqlMapper.TypeHandler<DateOnly>
        {
            public override void SetValue(IDbDataParameter parameter, DateOnly value)
            {
                parameter.DbType = DbType.Date;
                parameter.Value = value.ToDateTime(TimeOnly.MinValue);
            }

            public override DateOnly Parse(object value) => value switch
            {
                DateOnly date => date,
                DateTime dateTime => DateOnly.FromDateTime(dateTime),
                _ => DateOnly.Parse(value.ToString()!)
            };
        }
    }
}
