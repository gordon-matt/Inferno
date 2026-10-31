using Extenso;

namespace Inferno.Web.Messaging;

public interface ITokenizer
{
    string Replace(string template, IEnumerable<Token> tokens, bool htmlEncode);
}

public class Tokenizer : ITokenizer
{
    public string Replace(string template, IEnumerable<Token> tokens, bool htmlEncode)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return template;
        }

        ArgumentNullException.ThrowIfNull(tokens);

        foreach (var token in tokens)
        {
            string tokenValue = token.Value ?? string.Empty;
            if (htmlEncode && token.HtmlEncoded)
            {
                tokenValue = tokenValue.HtmlEncode();
            }

            template = template.Replace(token.Key, tokenValue);
        }

        return template;
    }
}
