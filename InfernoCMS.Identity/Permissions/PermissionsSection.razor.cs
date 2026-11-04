using System.Security.Claims;
using Ardalis.Result;
using IdentityNexus.Extensibility;
using IdentityNexus.Services.Extensibility;
using Inferno.Security;
using Inferno.Web.Security;
using Inferno.Web.Security.Permissions;
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
