namespace Inferno.Web.Messaging;

public sealed class Token
{
    public Token(string key, string value)
        : this(key, value, htmlEncoded: true)
    {
    }

    public Token(string key, string value, bool htmlEncoded)
    {
        Key = key;
        Value = value;
        HtmlEncoded = htmlEncoded;
    }

    public string Key { get; }

    public string Value { get; }

    /// <summary>
    /// When true, the value is HTML-encoded before it is written into an HTML body.
    /// </summary>
    public bool HtmlEncoded { get; }
}
