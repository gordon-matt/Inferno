using Ardalis.Result;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.Components;

/// <summary>A field's message from a service's validation result, shown under the input.</summary>
public partial class ErrorText
{
    [Parameter]
    public IReadOnlyDictionary<string, string> Errors { get; set; }

    [Parameter, EditorRequired]
    public string Key { get; set; }

    /// <summary>The first message per identifier of an invalid result.</summary>
    public static IReadOnlyDictionary<string, string> From(IResult result) =>
        (result.ValidationErrors ?? [])
            .Where(error => !string.IsNullOrEmpty(error.Identifier))
            .GroupBy(error => error.Identifier, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First().ErrorMessage, StringComparer.Ordinal);
}
