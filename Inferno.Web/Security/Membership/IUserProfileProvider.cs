namespace Inferno.Web.Security.Membership;

/// <summary>A group of profile fields shown on the user screen.</summary>
public interface IUserProfileProvider
{
    /// <summary>The group name.</summary>
    string Name { get; }

    /// <summary>The display template path.</summary>
    string DisplayTemplatePath { get; }

    /// <summary>The editor template path, or the assembly-qualified name of an editor component.</summary>
    string EditorTemplatePath { get; }

    /// <summary>Sort key. Lower values come first.</summary>
    int Order { get; }

    /// <summary>The profile keys this provider reads and writes.</summary>
    /// <returns>The field names.</returns>
    IEnumerable<string> GetFieldNames();

    /// <summary>Loads this provider's fields for <paramref name="userId"/>.</summary>
    /// <param name="userId">The user whose profile is loaded.</param>
    /// <returns>A task that completes when the fields are populated.</returns>
    Task PopulateFieldsAsync(string userId);
}