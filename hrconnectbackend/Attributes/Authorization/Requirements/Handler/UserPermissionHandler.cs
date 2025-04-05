using Microsoft.AspNetCore.Authorization;

namespace hrconnectbackend.Attributes.Authorization.Requirements.Handler;

public class UserPermissionHandler : AuthorizationHandler<UserPermissionAttribute>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserPermissionHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserPermissionAttribute requirement)
    {
        
        var user = _httpContextAccessor.HttpContext!.User;

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