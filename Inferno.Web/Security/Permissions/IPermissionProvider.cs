namespace Inferno.Web.Security.Permissions;

/// <summary>
/// Declares the permissions a module defines. Register implementations in a dependency registrar;
/// every registered provider is read once, when the catalog is first needed.
/// </summary>
public interface IPermissionProvider
{
    IEnumerable<Permission> GetPermissions();
}
