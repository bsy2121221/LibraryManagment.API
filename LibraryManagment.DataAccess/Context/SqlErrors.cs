using LibraryManagement.Models.Exceptions;
using Microsoft.Data.SqlClient;

namespace LibraryManagement.DataAccess.Context
{
    // Stored procedures report business errors with THROW 504xx, '<message for the user>'.
    // Usage: catch (SqlException ex) when (ex.ToAppException() is { } appEx) { throw appEx; }
    public static class SqlErrors
    {
        public const int BadRequest = 50400;
        public const int NotFound = 50404;
        public const int Conflict = 50409;

        public static bool IsUniqueViolation(this SqlException ex) => ex.Number is 2601 or 2627;

        public static AppException? ToAppException(this SqlException ex) => ex.Number switch
        {
            BadRequest => new BadRequestException(ex.Message),
            NotFound => new NotFoundException(ex.Message),
            Conflict => new ConflictException(ex.Message),
            _ => null
        };
    }
}
