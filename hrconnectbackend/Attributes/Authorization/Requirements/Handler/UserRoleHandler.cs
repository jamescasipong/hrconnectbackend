using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace hrconnectbackend.Attributes.Authorization.Requirements.Handler;

public class UserRolehandler(IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<UserRoleAttribute>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserRoleAttribute requirement)
    {
        
        var user = httpContextAccessor.HttpContext!.User;

        // Example: Check if the user has the required permission claim
        if (user.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == requirement.RequiredPermission))
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