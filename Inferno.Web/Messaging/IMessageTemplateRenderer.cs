namespace Inferno.Web.Messaging;

/// <summary>
/// Turns stored message-template data into the HTML that is actually emailed.
/// Return null when the data is already HTML or this renderer does not understand it.
/// </summary>
public interface IMessageTemplateRenderer
{
    /// <summary>Renders <paramref name="data"/> to HTML, or returns <see langword="null"/> to leave it for the next renderer.</summary>
    /// <param name="data">The stored template body.</param>
    /// <param name="subject">The template subject. A renderer may ignore it.</param>
    /// <returns>The HTML, or <see langword="null"/> when <paramref name="data"/> is already HTML or this renderer does not understand it.</returns>
    string ToHtml(string data, string subject);
}
