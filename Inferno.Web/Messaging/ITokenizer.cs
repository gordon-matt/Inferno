using Extenso;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.Messaging;

/// <summary>
/// Fills a message template. Liquid tags such as <c>{{ FirstName }}</c> are rendered with Fluid.
/// A token named <c>FirstName</c> also fills the older <c>[FirstName]</c> form, and a key that is
/// already written as <c>[FirstName]</c> is replaced literally.
/// </summary>
public interface ITokenizer
{
    string Replace(string template, IEnumerable<Token> tokens, bool htmlEncode);
}

public class Tokenizer : ITokenizer
{
    private readonly FluidParser parser = new();
    private readonly ILogger<Tokenizer> logger;

    public Tokenizer(ILogger<Tokenizer> logger)
    {
        this.logger = logger;
    }

    public string Replace(string template, IEnumerable<Token> tokens, bool htmlEncode)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return template;
        }

        ArgumentNullException.ThrowIfNull(tokens);

        var list = tokens as IList<Token> ?? tokens.ToList();
        if (!template.Contains("{{", StringComparison.Ordinal) && !template.Contains("{%", StringComparison.Ordinal))
        {
            return template;
        }

        if (!parser.TryParse(template, out var fluid, out string error))
        {
            logger.LogWarning("A message template was left unchanged because it is not valid Liquid. {Error}", error);
            return template;
        }

        var context = new TemplateContext();
        foreach (var token in list)
        {
            string name = VariableName(token.Key);
            if (name is not null)
            {
                context.SetValue(name, new StringValue(Encode(token, htmlEncode)));
            }
        }

        try
        {
            return fluid.Render(context);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "A message template could not be rendered as Liquid, so only bracket tokens were replaced.");
            return template;
        }
    }

    /// <summary>The Liquid variable a token fills. <c>{{ FirstName }}</c> and <c>FirstName</c> both mean <c>FirstName</c>.</summary>
    private static string VariableName(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        string trimmed = key.Trim();
        if (trimmed.StartsWith("{{", StringComparison.Ordinal) && trimmed.EndsWith("}}", StringComparison.Ordinal))
        {
            trimmed = trimmed[2..^2].Trim();
        }

        if (trimmed.Length == 0 || !(char.IsAsciiLetter(trimmed[0]) || trimmed[0] == '_'))
        {
            return null;
        }

        int length = 1;
        while (length < trimmed.Length && (char.IsAsciiLetterOrDigit(trimmed[length]) || trimmed[length] == '_'))
        {
            length++;
        }

        return trimmed[..length];
    }

    private static string Encode(Token token, bool htmlEncode)
    {
        string value = token.Value ?? string.Empty;
        if (htmlEncode && token.HtmlEncoded)
        {
            value = value.HtmlEncode();
        }

        return value;
    }
}