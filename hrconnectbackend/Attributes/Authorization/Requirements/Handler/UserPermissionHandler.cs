using Microsoft.AspNetCore.Authorization;

namespace hrconnectbackend.Attributes.Authorization.Requirements.Handler;

public class UserPermissionHandler(IHttpContextAccessor httpContextAccessor, ILogger<UserPermissionHandler> logger)
    : AuthorizationHandler<UserPermissionAttribute>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserPermissionAttribute requirement)
    {
        logger.LogInformation("Handling user permission");
        
        var user = httpContextAccessor.HttpContext!.User;

        // Example: Check if the user has the required permission claim
        if (user.HasClaim(c => c.Type == "Permission" && c.Value == requirement.RequiredPermission))
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