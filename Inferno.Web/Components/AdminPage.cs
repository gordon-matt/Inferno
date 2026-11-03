using Heroicsoft.NetForge.Components.Dialogs;
using Heroicsoft.NetForge.Components.Toasts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;

namespace Inferno.Web.Components;

/// <summary>
/// Base for admin screens that call services directly. The page's <c>[Authorize]</c> attribute
/// covers reading; writes are checked again here because a circuit outlives the request that
/// was authorized, and so the screen can hide commands the user cannot run.
/// </summary>
public abstract class AdminPage : ComponentBase
{
    [Inject]
    protected IWorkContext WorkContext { get; set; }

    [Inject]
    protected ToastService Toasts { get; set; }

    [Inject]
    protected DialogService Dialogs { get; set; }

    [Inject]
    private IStringLocalizer Localizer { get; set; }

    [Inject]
    private IAuthorizationService AuthorizationService { get; set; }

    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    protected int TenantId => WorkContext.CurrentTenant.Id;

    protected async Task<bool> HasPolicyAsync(string policy)
    {
        var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var result = await AuthorizationService.AuthorizeAsync(state.User, policy);
        return result.Succeeded;
    }

    /// <summary>Checks <paramref name="policy"/> before a write and tells the user when it fails.</summary>
    protected async Task<bool> EnsurePolicyAsync(string policy)
    {
        if (await HasPolicyAsync(policy))
        {
            return true;
        }

        Toasts.Error(Localizer[InfernoWebLocalizableStrings.Admin.NoPermission]);
        return false;
    }
}
