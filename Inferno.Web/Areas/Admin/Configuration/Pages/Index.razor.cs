using Heroicsoft.NetForge.Results;
using Inferno.Web.Areas.Admin.Configuration.Services;
using Inferno.Web.Components;
using Inferno.Web.Configuration.Entities;
using Inferno.Web.Security;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.Areas.Admin.Configuration.Pages;

public partial class Index
{
    private IReadOnlyList<SettingsGroup> groups;
    private bool canWrite;

    private Setting editing;
    private Type editorType;
    private DynamicComponent editor;
    private bool saving;

    [Inject]
    private ISettingsAdminService SettingsAdminService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        canWrite = await HasPolicyAsync(InfernoWebPolicies.SettingsWrite);
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var result = await SettingsAdminService.ListAsync(TenantId);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            groups = [];
            return;
        }

        groups = result.Value;
    }

    private static string IconFor(SettingsGroup group)
    {
        string name = group.Type;
        return name.Contains("Smtp", StringComparison.OrdinalIgnoreCase) ? "mail"
            : name.Contains("Membership", StringComparison.OrdinalIgnoreCase) ? "users"
            : name.Contains("DateTime", StringComparison.OrdinalIgnoreCase) ? "clock"
            : name.Contains("Page", StringComparison.OrdinalIgnoreCase) ? "file-earmark-text"
            : name.Contains("Site", StringComparison.OrdinalIgnoreCase) ? "globe"
            : "sliders";
    }

    private async Task EditAsync(SettingsGroup group)
    {
        var result = await SettingsAdminService.GetAsync(TenantId, group.Type);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        editorType = group.EditorType;
        editing = result.Value;
    }

    private void Close()
    {
        editing = null;
        editorType = null;
    }

    private async Task SaveAsync()
    {
        if (saving || editor?.Instance is not ISettingsEditor settingsEditor)
        {
            return;
        }

        if (!await EnsurePolicyAsync(InfernoWebPolicies.SettingsWrite))
        {
            return;
        }

        saving = true;
        try
        {
            var result = await SettingsAdminService.SaveAsync(TenantId, editing.Type, settingsEditor.Save());
            if (!result.IsSuccess)
            {
                Toasts.ShowFailure(result);
                return;
            }

            Toasts.Success(T[InfernoWebLocalizableStrings.Admin.Saved]);
            Close();
            await LoadAsync();
        }
        finally
        {
            saving = false;
        }
    }
}
