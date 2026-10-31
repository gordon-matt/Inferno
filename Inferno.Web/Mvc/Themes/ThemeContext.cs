using Dependo;
using Inferno.Security.Membership;
using Inferno.Threading;
using Inferno.Web.Configuration;

namespace Inferno.Web.Mvc.Themes;

public class ThemeContext : IThemeContext
{
    private readonly IWorkContext workContext;
    private readonly IThemeProvider themeProvider;
    private readonly SiteSettings siteSettings;

    private bool isDesktopThemeCached;
    private string cachedDesktopThemeName;

    public ThemeContext(
        IWorkContext workContext,
        IThemeProvider themeProvider,
        SiteSettings siteSettings)
    {
        this.workContext = workContext;
        this.themeProvider = themeProvider;
        this.siteSettings = siteSettings;
    }

    public string WorkingTheme
    {
        get
        {
            if (isDesktopThemeCached)
            {
                return cachedDesktopThemeName;
            }

            string theme = string.Empty;

            if (siteSettings.AllowUserToSelectTheme && workContext.CurrentUser != null)
            {
                var membershipService = DependoResolver.Instance.Resolve<IMembershipService>();
                string userTheme = AsyncHelper.RunSync(() => membershipService.GetProfileEntryAsync(
                    workContext.CurrentUser.Id,
                    ThemeUserProfileProvider.Fields.PreferredTheme));

                if (!string.IsNullOrEmpty(userTheme))
                {
                    theme = userTheme;
                }
            }

            if (string.IsNullOrEmpty(theme))
            {
                theme = siteSettings.DefaultTheme ?? "Default";
            }

            if (!themeProvider.ThemeConfigurationExists(theme))
            {
                var themeInstance = themeProvider.GetThemeConfigurations().FirstOrDefault();
                if (themeInstance == null)
                {
                    theme = "Default";
                }
                else
                {
                    theme = themeInstance.ThemeName;
                }
            }

            cachedDesktopThemeName = theme;
            isDesktopThemeCached = true;
            return theme;
        }
        set
        {
            if (!siteSettings.AllowUserToSelectTheme || workContext.CurrentUser == null)
            {
                return;
            }

            var membershipService = DependoResolver.Instance.Resolve<IMembershipService>();
            AsyncHelper.RunSync(() => membershipService.SaveProfileEntryAsync(
                workContext.CurrentUser.Id,
                ThemeUserProfileProvider.Fields.PreferredTheme,
                value));

            isDesktopThemeCached = false;
        }
    }
}
