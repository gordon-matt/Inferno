using System.Text.Json;
using Ardalis.Result;

namespace Inferno.Web.Messaging;

/// <summary>
/// The placeholder list stored on a message template. Authors insert these strings; Liquid fills them when the email is sent.
/// A bare name such as <c>FirstName</c> is stored as <c>{{ FirstName }}</c>.
/// </summary>
public static class MessageTemplateTokens
{
    /// <summary>The longest canonical token that <see cref="Normalize"/> accepts.</summary>
    public const int MaxLength = 200;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
    };

    /// <summary>Reads a JSON array of token strings. Blank JSON, invalid JSON, and a null array all return an empty list.</summary>
    /// <param name="json">The stored JSON.</param>
    /// <returns>The tokens, or an empty list.</returns>
    public static IReadOnlyList<string> Read(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    /// <summary>Writes <paramref name="tokens"/> as a JSON array. <see langword="null"/> is written as an empty array.</summary>
    /// <param name="tokens">The tokens to store.</param>
    /// <returns>The JSON.</returns>
    public static string Write(IReadOnlyList<string> tokens) =>
        JsonSerializer.Serialize(tokens ?? [], JsonOptions);

    /// <summary>
    /// Trims each token, drops blanks, and stores a bare name or a <c>{{ }}</c> tag as <c>{{ name }}</c>.
    /// A filter after the name, such as <c>{{ FirstName | upcase }}</c>, is kept inside the tag.
    /// Duplicates, tokens longer than <see cref="MaxLength"/>, and anything that is not a Liquid identifier are invalid.
    /// </summary>
    /// <param name="tokens">The tokens an author entered. <see langword="null"/> normalizes to an empty list.</param>
    /// <returns>The canonical tokens, or invalid with the token identifier.</returns>
    public static Result<IReadOnlyList<string>> Normalize(IEnumerable<string> tokens)
    {
        var normalized = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (string raw in tokens ?? [])
        {
            string text = raw?.Trim() ?? string.Empty;
            if (text.Length == 0)
            {
                continue;
            }

            if (!TryCanonical(text, out string canonical))
            {
                return Result<IReadOnlyList<string>>.Invalid(new ValidationError
                {
                    Identifier = "Tokens",
                    ErrorMessage = "\"" + text + "\" is not a Liquid token. Use a name such as {{ FirstName }}.",
                });
            }

            if (canonical.Length > MaxLength)
            {
                return Result<IReadOnlyList<string>>.Invalid(new ValidationError
                {
                    Identifier = "Tokens",
                    ErrorMessage = "\"" + text + "\" is too long.",
                });
            }

            if (!seen.Add(canonical))
            {
                return Result<IReadOnlyList<string>>.Invalid(new ValidationError
                {
                    Identifier = "Tokens",
                    ErrorMessage = canonical + " is already in the list.",
                });
            }

            normalized.Add(canonical);
        }

        return Result<IReadOnlyList<string>>.Success(normalized);
    }

    private static bool TryCanonical(string text, out string canonical)
    {
        canonical = null;
        string inner;
        if (text.StartsWith("{{", StringComparison.Ordinal) && text.EndsWith("}}", StringComparison.Ordinal) && text.Length >= 4)
        {
            inner = text[2..^2].Trim();
        }
        else if (IsIdentifier(text))
        {
            inner = text;
        }
        else
        {
            return false;
        }

        if (inner.Length == 0 || inner.IndexOfAny(['{', '}', '\r', '\n']) >= 0 || !StartsWithIdentifier(inner))
        {
            return false;
        }

        canonical = "{{ " + inner + " }}";
        return true;
    }

    private static bool StartsWithIdentifier(string text)
    {
        int length = IdentifierLength(text);
        if (length == 0)
        {
            return false;
        }

        if (length == text.Length)
        {
            return true;
        }

        char next = text[length];
        return char.IsWhiteSpace(next) || next == '|';
    }

    private static bool IsIdentifier(string text) => IdentifierLength(text) == text.Length;

    private static int IdentifierLength(string text)
    {
        if (text.Length == 0 || !(char.IsAsciiLetter(text[0]) || text[0] == '_'))
        {
            return 0;
        }

        int length = 1;
        while (length < text.Length && (char.IsAsciiLetterOrDigit(text[length]) || text[length] == '_'))
        {
            length++;
        }

        return length;
    }
}