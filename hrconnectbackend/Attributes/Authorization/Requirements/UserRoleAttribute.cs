using Microsoft.AspNetCore.Authorization;

namespace hrconnectbackend.Attributes.Authorization.Requirements;

public class UserRoleAttribute(string permission) : AuthorizeAttribute, IAuthorizationRequirement, IAuthorizationRequirementData
{
    public string RequiredPermission { get; set; } = permission;
    
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return this;
    }
}