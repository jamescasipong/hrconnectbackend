using hrconnectbackend.Constants;
using hrconnectbackend.Exceptions;
using System.Security.Claims;

namespace hrconnectbackend.Extensions
{
    public static class UserClaimsExtensions
    {

        public static string RetrieveSpecificUser(this ClaimsPrincipal user, string name)
        {
            if (user == null)
            {
                throw new UnauthorizedException(ErrorCodes.Unauthorized, "Unable to retrieve user's session");
            }
            string foundUser = user.FindFirst(name)?.Value ?? string.Empty;
            if (string.IsNullOrEmpty(foundUser))
            {
                throw new UnauthorizedException(ErrorCodes.Unauthorized, "Unable to retrieve user's session");
            }
            return foundUser;
        }

        public static string GetUserSession(this ClaimsPrincipal user)
        {

            if (user == null)
            {
                throw new UnauthorizedException(ErrorCodes.Unauthorized, "Unable to retrieve user's session");
            }

            string foundUser = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(foundUser))
            {
                throw new UnauthorizedException(ErrorCodes.Unauthorized, "Unable to retrieve user's session");
            }

            return foundUser;
        }

        public static string GetEmployeeSession(this ClaimsPrincipal employee)
        {
            if (employee == null)
            {
                throw new UnauthorizedException(ErrorCodes.Unauthorized, "Unable to retrieve employee's session");
            }

            string foundEmployee = employee.FindFirst("EmployeeId")?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(foundEmployee))
            {
                throw new UnauthorizedException(ErrorCodes.Unauthorized, "Unable to retrieve employee's session");
            }

            return foundEmployee;
        }
    }
}
