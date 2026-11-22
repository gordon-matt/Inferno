using System.Security.Claims;
using Ardalis.Result;
using IdentityNexus.Extensibility;
using IdentityNexus.Services.Extensibility;
using Inferno.Security;
using Inferno.Web.Security;
using Inferno.Web.Security.Permissions;
using Inferno.Web.Tenants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace InfernoCMS.Identity.Permissions;

/// <summary>The Permissions tab IdentityNexus shows on users and roles.</summary>
public partial class PermissionsSection : IdentityNexusSection
{
    [Inject]
    private IPermissionCatalog Catalog { get; set; } = default!;

    [Inject]
    private IPermissionStore Store { get; set; } = default!;

    [Inject]
    private TenantAdminImpersonation Impersonation { get; set; } = default!;

    [Inject]
    private IAuthorizationService Authorization { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider Authentication { get; set; } = default!;

    private HashSet<string> selected = new(StringComparer.Ordinal);
    private HashSet<string> saved = new(StringComparer.Ordinal);
    private IReadOnlyList<RolePermissions> roles = [];
    private GrantedPermissions mine = GrantedPermissions.None;
    private bool canEdit;
    private bool loaded;

    private bool IsUser => Subject.Kind == IdentitySubjectKind.User;

    private bool IsAdministratorsRole =>
        !IsUser && string.Equals(Subject.Name, InfernoSecurityConstants.Roles.Administrators, StringComparison.Ordinal);

    private bool IsInAdministratorsRole =>
        roles.Any(role => string.Equals(role.RoleName, InfernoSecurityConstants.Roles.Administrators, StringComparison.Ordinal));

    private IEnumerable<IGrouping<string, Permission>> Groups => Catalog.All.GroupBy(permission => permission.Category);

    protected override async Task OnInitializedAsync()
    {
        var state = await Authentication.GetAuthenticationStateAsync();
        canEdit = (await Authorization.AuthorizeAsync(state.User, InfernoWebPolicies.MembershipPermissionsWrite)).Succeeded;
        mine = await Store.GetGrantedAsync(state.User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (Impersonation.IsActive)
        {
            mine = mine.AsTenantAdministrator();
        }

        if (Subject.Id is not null)
        {
            if (IsUser)
            {
                selected = [.. await Store.GetUserPermissionsAsync(Subject.Id)];
                roles = await Store.GetUserRolePermissionsAsync(Subject.Id);
            }
            else
            {
                selected = [.. await Store.GetRolePermissionsAsync(Subject.Id)];
            }
        }

        saved = new HashSet<string>(selected, StringComparer.Ordinal);
        loaded = true;
    }

    /// <summary>
    /// Replaces the user or role grants with the boxes that are ticked.
    /// Succeeds without writing when the caller cannot edit, the role is Administrators, or nothing changed.
    /// </summary>
    /// <param name="id">The user or role id the editor just saved.</param>
    /// <param name="cancellationToken">Observed by the permission save. Not observed when the section has nothing to write.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when nothing is written or the save succeeds.
    /// A forbidden result when a newly ticked permission is one the caller does not hold.
    /// Otherwise the result from <see cref="IPermissionStore"/>.
    /// </returns>
    public override async Task<Result> SaveAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!canEdit || IsAdministratorsRole || selected.SetEquals(saved))
        {
            return Result.Success();
        }

        // Someone who may assign permissions still cannot hand out more than they hold.
        var beyondReach = selected
            .Except(saved)
            .Where(name => !Catalog.IsGranted(name, mine))
            .Select(name => Catalog.Find(name)?.DisplayName ?? name)
            .ToList();

        if (beyondReach.Count > 0)
        {
            return Result.Forbidden($"You can only grant permissions you hold yourself. Not granted: {string.Join(", ", beyondReach)}.");
        }

        var result = IsUser
            ? await Store.SetUserPermissionsAsync(id, selected, cancellationToken)
            : await Store.SetRolePermissionsAsync(id, selected, cancellationToken);

        if (result.IsSuccess)
        {
            saved = new HashSet<string>(selected, StringComparer.Ordinal);
        }

        return result;
    }

    private void Toggle(string name, bool on)
    {
        if (on)
        {
            selected.Add(name);
        }
        else
        {
            selected.Remove(name);
        }
    }

    /// <summary>Why the permission applies even though its own box is clear, or null when it does not.</summary>
    private string Note(Permission permission)
    {
        if (selected.Contains(permission.Name))
        {
            return null;
        }

        var fromRoles = roles
            .Where(role => Catalog.Expand(role.Permissions).Contains(permission.Name))
            .Select(role => role.RoleName)
            .ToList();

        if (fromRoles.Count > 0)
        {
            return $"From {string.Join(", ", fromRoles)}";
        }

        return Catalog.Expand(selected).Contains(permission.Name) ? "Included with another permission" : null;
    }
}
