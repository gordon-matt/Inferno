namespace Inferno.Web.Messaging;

/// <summary>
/// Turns stored message-template data into the HTML that is actually emailed.
/// Return null when the data is already HTML or this renderer does not understand it.
/// </summary>
public interface IMessageTemplateRenderer
{
    string ToHtml(string data, string subject);
}
