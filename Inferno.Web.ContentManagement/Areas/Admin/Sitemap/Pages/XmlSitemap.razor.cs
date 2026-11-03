using System.Globalization;
using Heroicsoft.NetForge.Results;
using Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Models;
using Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Services;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Pages;

public partial class XmlSitemap
{
    private IReadOnlyList<SitemapConfigModel> entries;
    private string search;
    private bool canWrite;
    private bool generating;

    [Inject]
    private ISitemapAdminService SitemapAdminService { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    private IEnumerable<SitemapConfigModel> Visible =>
        string.IsNullOrWhiteSpace(search)
            ? entries
            : entries.Where(entry => entry.Location?.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) == true);

    protected override async Task OnInitializedAsync()
    {
        canWrite = await HasPolicyAsync(CmsConstants.Policies.SitemapWrite);

        var result = await SitemapAdminService.ListAsync(TenantId);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            entries = [];
            return;
        }

        entries = result.Value;
    }

    private static string FrequencyKey(ChangeFrequency frequency) => frequency switch
    {
        ChangeFrequency.Always => InfernoCmsLocalizableStrings.Sitemap.Model.ChangeFrequencies.Always,
        ChangeFrequency.Hourly => InfernoCmsLocalizableStrings.Sitemap.Model.ChangeFrequencies.Hourly,
        ChangeFrequency.Daily => InfernoCmsLocalizableStrings.Sitemap.Model.ChangeFrequencies.Daily,
        ChangeFrequency.Weekly => InfernoCmsLocalizableStrings.Sitemap.Model.ChangeFrequencies.Weekly,
        ChangeFrequency.Monthly => InfernoCmsLocalizableStrings.Sitemap.Model.ChangeFrequencies.Monthly,
        ChangeFrequency.Yearly => InfernoCmsLocalizableStrings.Sitemap.Model.ChangeFrequencies.Yearly,
        _ => InfernoCmsLocalizableStrings.Sitemap.Model.ChangeFrequencies.Never
    };

    private async Task ChangeFrequencyChangedAsync(SitemapConfigModel entry, ChangeEventArgs args)
    {
        if (Enum.TryParse<ChangeFrequency>(args.Value?.ToString(), out var frequency))
        {
            await SaveAsync(entry, frequency, entry.Priority);
        }
    }

    private async Task PriorityChangedAsync(SitemapConfigModel entry, ChangeEventArgs args)
    {
        if (float.TryParse(args.Value?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out float priority))
        {
            await SaveAsync(entry, entry.ChangeFrequency, priority);
        }
    }

    // Each change is saved as it is made, so there is no save button to forget.
    private async Task SaveAsync(SitemapConfigModel entry, ChangeFrequency frequency, float priority)
    {
        if (!await EnsurePolicyAsync(CmsConstants.Policies.SitemapWrite))
        {
            return;
        }

        var result = await SitemapAdminService.UpdateAsync(TenantId, entry.Id, frequency, priority);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        entry.ChangeFrequency = frequency;
        entry.Priority = (float)Math.Round(priority, 1);
    }

    private async Task GenerateAsync()
    {
        bool confirmed = await Dialogs.ConfirmAsync(
            T[InfernoCmsLocalizableStrings.Sitemap.GenerateFile],
            T[InfernoCmsLocalizableStrings.Sitemap.ConfirmGenerateFile],
            T[InfernoCmsLocalizableStrings.Sitemap.GenerateFile]);

        if (!confirmed || !await EnsurePolicyAsync(CmsConstants.Policies.SitemapWrite))
        {
            return;
        }

        generating = true;
        try
        {
            string requestBaseUrl = new Uri(NavigationManager.BaseUri).GetLeftPart(UriPartial.Authority);
            var result = await SitemapAdminService.GenerateAsync(TenantId, requestBaseUrl);
            if (!result.IsSuccess)
            {
                Toasts.Error(T[InfernoCmsLocalizableStrings.Sitemap.GenerateFileError]);
                return;
            }

            Toasts.Success(T[InfernoCmsLocalizableStrings.Sitemap.GenerateFileSuccess]);
        }
        finally
        {
            generating = false;
        }
    }
}
