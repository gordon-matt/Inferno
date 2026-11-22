namespace Inferno.Web.Messaging;

/// <summary>One value substituted into a message template.</summary>
public sealed class Token
{
    /// <summary>Creates a token whose value is encoded when the caller asks for HTML encoding.</summary>
    /// <param name="key">The Liquid name, such as <c>FirstName</c> or <c>{{ FirstName }}</c>.</param>
    /// <param name="value">The text to substitute. <see langword="null"/> is written as empty.</param>
    public Token(string key, string value)
        : this(key, value, htmlEncoded: true)
    {
    }

    /// <summary>Creates a token.</summary>
    /// <param name="key">The Liquid name, such as <c>FirstName</c> or <c>{{ FirstName }}</c>.</param>
    /// <param name="value">The text to substitute. <see langword="null"/> is written as empty.</param>
    /// <param name="htmlEncoded">Whether <see cref="ITokenizer.Replace"/> encodes <paramref name="value"/> when HTML encoding is requested.</param>
    public Token(string key, string value, bool htmlEncoded)
    {
        Key = key;
        Value = value;
        HtmlEncoded = htmlEncoded;
    }

    /// <summary>The Liquid variable name. A <c>{{ }}</c> wrapper is stripped, and only the leading identifier is used.</summary>
    public string Key { get; }

    /// <summary>The text substituted for <see cref="Key"/>.</summary>
    public string Value { get; }

    /// <summary>
    /// Whether <see cref="ITokenizer.Replace"/> HTML-encodes <see cref="Value"/> when its caller requests encoding.
    /// </summary>
    public bool HtmlEncoded { get; }
}
