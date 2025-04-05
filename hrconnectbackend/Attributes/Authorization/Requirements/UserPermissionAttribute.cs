using Microsoft.AspNetCore.Authorization;

namespace hrconnectbackend.Attributes.Authorization.Requirements;

public class UserPermissionAttribute(string permission) : AuthorizeAttribute, IAuthorizationRequirement
{
    public string RequiredPermission { get; set; } = permission;
}