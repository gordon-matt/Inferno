using Ardalis.Result;
using Localize.Languages;
using Localize.Models;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.Areas.Admin.Configuration.Components;

/// <summary>Edits site settings. The default language is chosen from the languages stored for this site.</summary>
public partial class SiteSettingsEditor
{
    private IReadOnlyList<LanguageOption> languages = [];

    [Inject]
    private ILanguageService LanguageService { get; set; } = default!;

    /// <summary>A saved culture that is no longer in the language list. It stays selectable so a save does not drop it.</summary>
    private bool HasUnlistedLanguage =>
        !string.IsNullOrWhiteSpace(Model?.DefaultLanguage)
        && !languages.Any(language => string.Equals(language.CultureCode, Model.DefaultLanguage, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        var listed = await LanguageService.ListAsync();
        if (!listed.IsSuccess)
        {
            return;
        }

        languages = listed.Value
            .Select(language => new LanguageOption(language.CultureCode, Label(language)))
            .ToArray();

        // The select matches option values exactly, so "en-us" would not select "en-US".
        var match = languages.FirstOrDefault(language =>
            string.Equals(language.CultureCode, Model.DefaultLanguage, StringComparison.OrdinalIgnoreCase));
        if (match is not null)
        {
            Model.DefaultLanguage = match.CultureCode;
        }
    }

    private void LanguageChanged(ChangeEventArgs args) =>
        Model.DefaultLanguage = args.Value?.ToString() ?? string.Empty;

    private static string Label(LanguageModel language) => $"{language.Name} ({language.CultureCode})";

    private sealed record LanguageOption(string CultureCode, string Label);
}
