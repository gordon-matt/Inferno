namespace Inferno.Web.Security.Permissions;

/// <summary>
/// Declares the permissions a module defines. Register implementations in a dependency registrar;
/// every registered provider is read once, when the catalog is first needed.
/// </summary>
public interface IPermissionProvider
{
    /// <summary>The permissions this module defines.</summary>
    /// <returns>The permissions to add to the catalog.</returns>
    IEnumerable<Permission> GetPermissions();
}
