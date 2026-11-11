using Heroicsoft.NetForge.Results;
using Inferno.Web.Areas.Admin.Configuration.Services;
using Inferno.Web.Components;
using Inferno.Web.Configuration;
using Inferno.Web.Configuration.Entities;
using Inferno.Web.Security;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.Areas.Admin.Configuration.Pages;

public partial class Index
{
    private IReadOnlyList<SettingsGroup> groups;
    private IReadOnlyList<ISettingsSection> visibleSections = [];
    private IReadOnlyList<SettingsTile> tiles;
    private bool canWrite;

    private Setting editing;
    private Type editorType;
    private DynamicComponent editor;
    private bool saving;

    [Inject]
    private ISettingsAdminService SettingsAdminService { get; set; }

    [Inject]
    private IEnumerable<ISettingsSection> SettingsSections { get; set; }

    private string DrawerTitle => editing is null
        ? null
        : T[InfernoWebLocalizableStrings.General.EditFormat, editing.Name].Value;

    protected override async Task OnInitializedAsync()
    {
        canWrite = await HasPolicyAsync(InfernoWebPolicies.SettingsWrite);
        visibleSections = await VisibleSectionsAsync();
        await LoadAsync();
    }

    private async Task<IReadOnlyList<ISettingsSection>> VisibleSectionsAsync()
    {
        var visible = new List<ISettingsSection>();
        foreach (var section in SettingsSections)
        {
            if (string.IsNullOrEmpty(section.Permission) || await HasPolicyAsync(section.Permission))
            {
                visible.Add(section);
            }
        }

        return visible;
    }

    private async Task LoadAsync()
    {
        var result = await SettingsAdminService.ListAsync(TenantId);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            groups = [];
        }
        else
        {
            groups = result.Value;
        }

        BuildTiles();
    }

    private void BuildTiles()
    {
        var documentTiles = groups.Select(group => new SettingsTile(
            group.Type,
            group.Name,
            IconFor(group),
            null,
            group.Stored is null,
            null,
            group));

        var sectionTiles = visibleSections.Select(section => new SettingsTile(
            section.Url,
            section.Title,
            string.IsNullOrEmpty(section.Icon) ? "sliders" : section.Icon,
            section.Description,
            false,
            section.Url,
            null));

        tiles = documentTiles
            .Concat(sectionTiles)
            .OrderBy(tile => tile.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(tile => tile.Key, StringComparer.Ordinal)
            .ToList();
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

    private sealed record SettingsTile(
        string Key,
        string Title,
        string Icon,
        string Meta,
        bool ShowUnspecified,
        string Url,
        SettingsGroup Group);
}
