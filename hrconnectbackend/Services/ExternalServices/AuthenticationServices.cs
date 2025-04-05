using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace hrconnectbackend.Helper
{
    public class AuthenticationServices : ControllerBase
    {
        private readonly ILogger<AuthenticationServices> _logger;

        public AuthenticationServices(ILogger<AuthenticationServices> logger)
        {
            _logger = logger;
        }

        private bool TryGetCurrentUser(ClaimsPrincipal user, out int empId)
        {
            empId = 0;
            var currentUser = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentUser == null)
            {
                _logger.LogWarning("User not authenticated");
                return false;
            }

            if (!int.TryParse(currentUser, out empId))
            {
                _logger.LogWarning("Invalid user ID format");
                return false;
            }

            return true;
        }

        public IActionResult ValidateUser(ClaimsPrincipal user, int employeeId, IEnumerable<string>? requiredRoles = null)
        {
            const string UNAUTHORIZED_MESSAGE = "Authentication required. Please sign in to access this resource.";
            const string FORBIDDEN_MESSAGE = "Access denied. You don't have permission to access this resource.";
            
            if (!TryGetCurrentUser(user, out var empId))
            {
                return Unauthorized(new ApiResponse(false, UNAUTHORIZED_MESSAGE));
            }
            

            if (requiredRoles == null){
                if (employeeId != empId)
                {
                    _logger.LogWarning($"User {empId} attempted to access data for employee {employeeId}: Access denied");
                    return Forbid();
                }
            }
            else{
                if (employeeId != empId && !IsAdmin(user, requiredRoles))
                {
                    _logger.LogWarning($"User {empId} attempted to access data for employee {employeeId}: Access denied");
                    return Forbid();
                }
            }
            
            
            return null;
        }
        private bool IsAdmin(ClaimsPrincipal user, IEnumerable<string> requiredRoles)
        {
            var roles = requiredRoles?.ToList() ?? new List<string>();
            if (!roles.Any())
            {
                throw new ArgumentException("At least one required role must be specified");
            }
            
            var userRoles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            
            if (!roles.Any(role => userRoles.Contains(role))){
                return false;
            }

            return true;
        }
    }
}