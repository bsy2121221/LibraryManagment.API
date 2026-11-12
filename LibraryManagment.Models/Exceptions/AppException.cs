namespace LibraryManagement.Models.Exceptions
{
    // Base for expected business errors; the API maps StatusCode to a ProblemDetails response.
    public abstract class AppException : Exception
    {
        protected AppException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public int StatusCode { get; }
    }

    public class BadRequestException(string message) : AppException(message, 400);

    public class UnauthorizedException(string message) : AppException(message, 401);

    public class ForbiddenException(string message) : AppException(message, 403);

    public class NotFoundException(string message) : AppException(message, 404);

    public class ConflictException(string message) : AppException(message, 409);
}
