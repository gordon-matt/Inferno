using Extenso.Data.Entity;
using Inferno.Plugins.GoogleAnalytics;
using Inferno.Web.Components;
using Inferno.Web.ContentManagement;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;
using Microsoft.AspNetCore.Components;

namespace Inferno.Plugins.GoogleAnalytics.Pages;

public partial class Settings : AdminPage
{
    [Inject]
    private IGoogleAnalyticsSettingsStore Store { get; set; } = default!;

    [Inject]
    private IZoneService Zones { get; set; } = default!;

    private readonly SettingsForm form = new();
    private bool loaded;
    private bool saving;
    private bool hasKey;

    protected override async Task OnInitializedAsync()
    {
        await EnsureScriptsZoneAsync();
        var settings = Store.Get();
        form.MeasurementId = settings.MeasurementId;
        form.PropertyId = settings.PropertyId;
        hasKey = ServiceAccountKey.TryParse(settings.ServiceAccountJson, out _, out _);
        loaded = true;
    }

    private async Task SaveAsync()
    {
        if (saving || !await EnsurePolicyAsync(GoogleAnalyticsPolicies.Manage))
        {
            return;
        }

        string measurementId = GoogleAnalyticsIds.NormalizeMeasurementId(form.MeasurementId);
        if (measurementId.Length > 0 && !GoogleAnalyticsIds.IsMeasurementId(measurementId))
        {
            Toasts.Error("The measurement ID should look like G-AB12CD34.");
            return;
        }

        string propertyId = form.PropertyId?.Trim() ?? string.Empty;
        if (propertyId.Length > 0 && GoogleAnalyticsIds.PropertyId(propertyId).Length == 0)
        {
            Toasts.Error("The property ID should be the number from the GA4 property, or properties/ followed by that number.");
            return;
        }

        var existing = Store.Get();
        string keyJson = string.IsNullOrWhiteSpace(form.ServiceAccountJson) ? existing.ServiceAccountJson : form.ServiceAccountJson;
        if (!string.IsNullOrWhiteSpace(keyJson) && !ServiceAccountKey.TryParse(keyJson, out _, out string? error))
        {
            Toasts.Error(error ?? "The service account key could not be read.");
            return;
        }

        saving = true;
        try
        {
            Store.Save(new GoogleAnalyticsSettings
            {
                MeasurementId = measurementId,
                PropertyId = GoogleAnalyticsIds.PropertyId(propertyId),
                ServiceAccountJson = keyJson ?? string.Empty,
            });
            form.ServiceAccountJson = string.Empty;
            hasKey = ServiceAccountKey.TryParse(keyJson, out _, out _);
            Toasts.Success("Google Analytics settings saved.");
        }
        finally
        {
            saving = false;
        }
    }

    private async Task EnsureScriptsZoneAsync()
    {
        var existing = Zones.FindOne(new SearchOptions<Zone>
        {
            Query = zone => zone.TenantId == TenantId && zone.Name == CmsConstants.LayoutZones.Scripts,
        });

        if (existing is not null)
        {
            return;
        }

        await Zones.InsertAsync(new Zone
        {
            Id = Guid.NewGuid(),
            TenantId = TenantId,
            Name = CmsConstants.LayoutZones.Scripts,
        });
    }

    private sealed class SettingsForm
    {
        public string? MeasurementId { get; set; }

        public string? PropertyId { get; set; }

        public string? ServiceAccountJson { get; set; }
    }
}
