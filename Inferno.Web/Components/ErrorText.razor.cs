using Ardalis.Result;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.Components;

/// <summary>A field's message from a service's validation result, shown under the input.</summary>
public partial class ErrorText
{
    /// <summary>Validation messages keyed by field. The component shows <see cref="Key"/>.</summary>
    [Parameter]
    public IReadOnlyDictionary<string, string> Errors { get; set; }

    /// <summary>The field identifier to display.</summary>
    [Parameter, EditorRequired]
    public string Key { get; set; }

    /// <summary>The first message per identifier of an invalid result.</summary>
    /// <param name="result">A service result. A result with no validation errors yields an empty dictionary.</param>
    /// <returns>The first message for each non-empty identifier.</returns>
    public static IReadOnlyDictionary<string, string> From(IResult result) =>
        (result.ValidationErrors ?? [])
            .Where(error => !string.IsNullOrEmpty(error.Identifier))
            .GroupBy(error => error.Identifier, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First().ErrorMessage, StringComparer.Ordinal);
}
