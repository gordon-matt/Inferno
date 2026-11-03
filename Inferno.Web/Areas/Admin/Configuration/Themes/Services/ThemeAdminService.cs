using Ardalis.Result;
using Inferno.Web.Configuration;
using Inferno.Web.Configuration.Services;
using Inferno.Web.Mvc.Themes;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.Areas.Admin.Configuration.Themes.Services;

public sealed record ThemeSummary(string Name, string Title, string PreviewImageUrl, string PreviewText, bool SupportRtl, bool IsDefault);

public interface IThemeAdminService
{
    Task<Result<IReadOnlyList<ThemeSummary>>> ListAsync(int tenantId);

    /// <summary>Makes <paramref name="themeName"/> the public site's theme, switching to its layout too.</summary>
    Task<Result> SetDefaultAsync(int tenantId, string themeName);
}

public class ThemeAdminService(
    IThemeProvider themeProvider,
    ISettingService settingService,
    ILogger<ThemeAdminService> logger) : IThemeAdminService
{
    private const string FALLBACK_LAYOUT_PATH = "~/Views/Shared/_Layout.cshtml";

    public Task<Result<IReadOnlyList<ThemeSummary>>> ListAsync(int tenantId)
    {
        string current = settingService.GetSettings<SiteSettings>(tenantId).DefaultTheme;

        IReadOnlyList<ThemeSummary> themes = themeProvider.GetThemeConfigurations()
            .Select(theme => new ThemeSummary(
                theme.ThemeName,
                string.IsNullOrWhiteSpace(theme.ThemeTitle) ? theme.ThemeName : theme.ThemeTitle,
                theme.PreviewImageUrl,
                theme.PreviewText,
                theme.SupportRtl,
                string.Equals(theme.ThemeName, current, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(theme => theme.IsDefault)
            .ThenBy(theme => theme.Title)
            .ToList();

        return Task.FromResult(Result.Success(themes));
    }

    public Task<Result> SetDefaultAsync(int tenantId, string themeName)
    {
        var theme = string.IsNullOrWhiteSpace(themeName) ? null : themeProvider.GetThemeConfiguration(themeName);
        if (theme is null)
        {
            return Task.FromResult(Result.NotFound());
        }

        try
        {
            var siteSettings = settingService.GetSettings<SiteSettings>(tenantId);
            siteSettings.DefaultTheme = theme.ThemeName;
            siteSettings.DefaultFrontendLayoutPath = string.IsNullOrEmpty(theme.DefaultLayoutPath) ? FALLBACK_LAYOUT_PATH : theme.DefaultLayoutPath;
            settingService.SaveSettings(siteSettings, tenantId);
            return Task.FromResult(Result.Success());
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not set theme {ThemeName} for tenant {TenantId}", themeName, tenantId);
            return Task.FromResult(Result.Error("The theme could not be changed."));
        }
    }
}
