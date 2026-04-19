using Inferno.Web.Areas.Admin.Configuration.Themes.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace Inferno.Web.Areas.Admin.Configuration.Themes.Pages
{
    public partial class Index
    {
        [Inject]
        private ThemeODataService ThemeODataService { get; set; }

        protected override string GetODataFilter(LoadDataArgs args) => args.Filter;

        protected async Task SetThemeAsync(string themeName)
        {
            var response = await ThemeODataService.SetThemeAsync(themeName);
            if (response.Succeeded)
            {
                NotificationService.Notify(
                    NotificationSeverity.Success,
                    "Success",
                    T[InfernoWebLocalizableStrings.Themes.SetDesktopThemeSuccess]);

                await DataGrid.Reload();
            }
            else
            {
                NotificationService.Notify(
                    NotificationSeverity.Error,
                    "Error",
                    T[InfernoWebLocalizableStrings.Themes.SetDesktopThemeError]);
            }
        }
    }
}
