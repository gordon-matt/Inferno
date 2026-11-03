using Heroicsoft.NetForge.Results;
using Inferno.Web.Areas.Admin.Configuration.Themes.Services;
using Inferno.Web.Security;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.Areas.Admin.Configuration.Themes.Pages;

public partial class Index
{
    private IReadOnlyList<ThemeSummary> themes;
    private bool canWrite;
    private bool busy;

    /// <summary>Themes whose manifest names a preview image that failed to load; these show the placeholder instead.</summary>
    private readonly HashSet<string> brokenPreviews = [];

    [Inject]
    private IThemeAdminService ThemeAdminService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        canWrite = await HasPolicyAsync(InfernoWebPolicies.ThemesWrite);
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var result = await ThemeAdminService.ListAsync(TenantId);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            themes = [];
            return;
        }

        themes = result.Value;
    }

    private async Task SetDefaultAsync(ThemeSummary theme)
    {
        if (busy || !await EnsurePolicyAsync(InfernoWebPolicies.ThemesWrite))
        {
            return;
        }

        busy = true;
        try
        {
            var result = await ThemeAdminService.SetDefaultAsync(TenantId, theme.Name);
            if (!result.IsSuccess)
            {
                Toasts.ShowFailure(result);
                return;
            }

            Toasts.Success(T[InfernoWebLocalizableStrings.Themes.SetDefaultSuccessFormat, theme.Title]);
            await LoadAsync();
        }
        finally
        {
            busy = false;
        }
    }
}
