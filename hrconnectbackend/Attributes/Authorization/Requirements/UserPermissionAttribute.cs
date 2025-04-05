using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

public class UserPermissionAttribute : AuthorizeAttribute, IAuthorizationRequirement
{
    public string RequiredPermission { get; set; }

    // You can pass custom parameters to the attribute via the constructor
    public UserPermissionAttribute(string permission)
    {
        RequiredPermission = permission;
    }
}