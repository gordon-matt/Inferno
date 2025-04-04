using Microsoft.AspNetCore.Authorization;

namespace Inferno.Identity.Permissions;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; init; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}