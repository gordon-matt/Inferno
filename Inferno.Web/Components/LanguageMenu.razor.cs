using System.Globalization;
using Localize;
using Localize.Languages;
using Localize.Models;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.Components;

/// <summary>
/// A translate icon in the shell top bar. The menu lists the enabled languages, and choosing one
/// follows the culture endpoint so the cookie is written on a full page load.
/// </summary>
public partial class LanguageMenu
{
    private IReadOnlyList<LanguageModel> languages = [];

    [Inject]
    private ILanguageService Languages { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        var listed = await Languages.ListEnabledAsync();
        if (listed.IsSuccess)
        {
            languages = listed.Value;
        }
    }

    private bool IsCurrent(string culture) =>
        string.Equals(CultureInfo.CurrentUICulture.Name, culture, StringComparison.OrdinalIgnoreCase);

    private string Href(string culture)
    {
        string relative = Navigation.ToBaseRelativePath(Navigation.Uri);
        string path = string.IsNullOrEmpty(relative) ? "/" : "/" + relative;
        return $"{LocalizeRoutes.Culture}/{Uri.EscapeDataString(culture)}?returnUrl={Uri.EscapeDataString(path)}";
    }
}
