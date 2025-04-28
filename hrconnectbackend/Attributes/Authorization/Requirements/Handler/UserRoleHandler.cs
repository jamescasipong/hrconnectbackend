using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace hrconnectbackend.Attributes.Authorization.Requirements.Handler;

public class UserRolehandler(IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<UserRoleAttribute>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserRoleAttribute requirement)
    {
        var requiredPermissions = requirement.RequiredPermission?.Trim();

        if (string.IsNullOrEmpty(requiredPermissions))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // Split only if the required permissions contain multiple roles
        var requirementRoles = requiredPermissions.Split(',')
            .Select(role => role.Trim())
            .ToList();

        var user = httpContextAccessor.HttpContext?.User;

        // Check if the user has the required permission claim
        if (user != null && user.HasClaim(c => c.Type == ClaimTypes.Role && requirementRoles.Contains(c.Value)))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(); // The user is not authorized
        }

        return Task.CompletedTask;
    }
}