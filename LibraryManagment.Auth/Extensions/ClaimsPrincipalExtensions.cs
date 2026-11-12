using System.Security.Claims;

namespace LibraryManagement.Auth.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal principal)
        {
            var value = principal.FindFirstValue(AppClaimTypes.UserId);
            return int.TryParse(value, out var userId)
                ? userId
                : throw new InvalidOperationException("Authenticated user has no valid 'sub' claim.");
        }

        // For [AllowAnonymous] endpoints that behave differently when a valid token is sent
        public static int? GetUserIdOrNull(this ClaimsPrincipal principal)
        {
            if (principal.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            return int.TryParse(principal.FindFirstValue(AppClaimTypes.UserId), out var userId) ? userId : null;
        }
    }
}
