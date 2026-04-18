using Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Models;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Pages
{
    public partial class XmlSitemap
    {
        private RadzenDataGrid<SitemapConfigModel> DataGrid { get; set; }

        private IEnumerable<SitemapConfigModel> Records { get; set; } = Enumerable.Empty<SitemapConfigModel>();

        private bool IsLoading { get; set; }

        private IEnumerable<ChangeFrequencyOption> ChangeFrequencies { get; set; }

        protected override async Task OnInitializedAsync()
        {
            BuildChangeFrequencyOptions();
            await LoadAsync();
        }

        private void BuildChangeFrequencyOptions()
        {
            ChangeFrequencies = Enum.GetValues<ChangeFrequency>()
                .Select(value => new ChangeFrequencyOption
                {
                    Value = value,
                    Name = T[GetChangeFrequencyLocalizationKey(value)].Value
                })
                .ToList();
        }

        private static string GetChangeFrequencyLocalizationKey(ChangeFrequency frequency) => frequency switch
        {
            ChangeFrequency.Always => InfernoCmsLocalizableStrings.Sitemap.Model.ChangeFrequencies.Always,
            ChangeFrequency.Hourly => InfernoCmsLocalizableStrings.Sitemap.Model.ChangeFrequencies.Hourly,
            ChangeFrequency.Daily => InfernoCmsLocalizableStrings.Sitemap.Model.ChangeFrequencies.Daily,
            ChangeFrequency.Weekly => InfernoCmsLocalizableStrings.Sitemap.Model.ChangeFrequencies.Weekly,
            ChangeFrequency.Monthly => InfernoCmsLocalizableStrings.Sitemap.Model.ChangeFrequencies.Monthly,
            ChangeFrequency.Yearly => InfernoCmsLocalizableStrings.Sitemap.Model.ChangeFrequencies.Yearly,
            ChangeFrequency.Never => InfernoCmsLocalizableStrings.Sitemap.Model.ChangeFrequencies.Never,
            _ => frequency.ToString()
        };

        private async Task LoadAsync()
        {
            IsLoading = true;
            var response = await SitemapService.GetConfigAsync();
            if (response.Succeeded)
            {
                Records = response.Data?.Value?.ToList() ?? Enumerable.Empty<SitemapConfigModel>();
            }
            else
            {
                Records = Enumerable.Empty<SitemapConfigModel>();
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Unable to load sitemap configuration.");
            }
            IsLoading = false;
        }

        private async Task OnChangeFrequencyChangedAsync(SitemapConfigModel record, ChangeFrequency newValue)
        {
            record.ChangeFrequency = newValue;
            await SaveRecordAsync(record);
        }

        private async Task OnPriorityChangedAsync(SitemapConfigModel record, float? newValue)
        {
            record.Priority = newValue ?? 0f;
            await SaveRecordAsync(record);
        }

        private async Task SaveRecordAsync(SitemapConfigModel record)
        {
            var response = await SitemapService.SetConfigAsync(record.Id, record.ChangeFrequency, record.Priority);
            if (!response.Succeeded)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Unable to save configuration.");
            }
        }

        private async Task GenerateAsync()
        {
            if (await DialogService.Confirm(T[InfernoCmsLocalizableStrings.Sitemap.ConfirmGenerateFile].Value) != true)
            {
                return;
            }

            var response = await SitemapService.GenerateAsync();
            if (response.Succeeded)
            {
                NotificationService.Notify(
                    NotificationSeverity.Success,
                    "Success",
                    T[InfernoCmsLocalizableStrings.Sitemap.GenerateFileSuccess].Value);
            }
            else
            {
                NotificationService.Notify(
                    NotificationSeverity.Error,
                    "Error",
                    T[InfernoCmsLocalizableStrings.Sitemap.GenerateFileError].Value);
            }
        }

        private sealed class ChangeFrequencyOption
        {
            public ChangeFrequency Value { get; set; }

            public string Name { get; set; }
        }
    }
}
