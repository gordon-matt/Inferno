using Localize;

namespace InfernoCMS.Infrastructure;

/// <inheritdoc cref="ILanguagePack" />
public class LanguagePackInvariant : ILanguagePack
{
    #region ILanguagePack Members

    /// <inheritdoc />
    public string CultureCode => null;

    /// <inheritdoc />
    public IDictionary<string, string> LocalizedStrings => new Dictionary<string, string>
    {
        { LocalizableStrings.Dashboard.Administration, "Administration" },
        { LocalizableStrings.Dashboard.Frontend, "Frontend" },
        { LocalizableStrings.Dashboard.Title, "Dashboard" },
        { LocalizableStrings.Dashboard.ToggleNavigation, "Toggle Navigation" },
    };

    #endregion ILanguagePack Members
}