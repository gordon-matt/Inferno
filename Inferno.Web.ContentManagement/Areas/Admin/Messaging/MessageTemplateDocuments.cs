using PageKrafter.Blocks;
using PageKrafter.Documents;
using PageKrafter.Rendering;
using PageKrafter.Templates;

namespace Inferno.Web.ContentManagement.Areas.Admin.Messaging;

/// <summary>
/// Message templates are PageKrafter email documents. Older rows stored raw HTML, and those still send as HTML
/// until someone saves them in the editor.
/// </summary>
public static class MessageTemplateDocuments
{
    /// <summary>Whether <paramref name="data"/> looks like a PageKrafter document rather than raw HTML.</summary>
    /// <param name="data">The stored template body.</param>
    /// <returns><see langword="true"/> when the text starts with an object that contains a root property.</returns>
    public static bool IsDocument(string data) =>
        !string.IsNullOrWhiteSpace(data)
        && data.TrimStart().StartsWith('{')
        && data.Contains("\"root\"", StringComparison.Ordinal);

    /// <summary>Returns a PageKrafter email document the editor can open. Raw HTML becomes one HTML block.</summary>
    /// <param name="data">The stored body. A document that cannot be read is also wrapped as one HTML block.</param>
    /// <returns>The serialized email document.</returns>
    public static string ForEditor(string data)
    {
        if (IsDocument(data))
        {
            var parsed = PageKrafterJson.Deserialize(data, DocumentMode.Email);
            if (parsed.IsSuccess)
            {
                parsed.Value.Mode = DocumentMode.Email;
                return PageKrafterJson.Serialize(parsed.Value);
            }
        }

        var document = PageDocument.Create(DocumentMode.Email);
        if (!string.IsNullOrWhiteSpace(data))
        {
            document.Root.Add(Nodes.Create(BlockTypes.Html, ("html", data)));
        }

        return PageKrafterJson.Serialize(document);
    }

    /// <summary>Builds an email document whose only block is the given HTML.</summary>
    /// <param name="html">The HTML placed in the text block.</param>
    /// <returns>The serialized email document.</returns>
    public static string Email(string html)
    {
        var document = PageDocument.Create(DocumentMode.Email);
        document.Root.Add(Nodes.Create(BlockTypes.Text, ("content", html)));
        return PageKrafterJson.Serialize(document);
    }
}

/// <summary>Renders a PageKrafter email document to HTML. Raw HTML is left for the caller.</summary>
/// <param name="renderer">Renders the parsed document.</param>
public sealed class PageKrafterMessageTemplateRenderer(PageRenderer renderer) : Inferno.Web.Messaging.IMessageTemplateRenderer
{
    /// <summary>Renders a stored email document.</summary>
    /// <param name="data">The stored body. Anything that is not a readable document returns <see langword="null"/>.</param>
    /// <param name="subject">The subject passed to the renderer. <see langword="null"/> is passed as empty.</param>
    /// <returns>The rendered HTML, or <see langword="null"/> when <paramref name="data"/> is not a readable document.</returns>
    public string ToHtml(string data, string subject)
    {
        if (!MessageTemplateDocuments.IsDocument(data))
        {
            return null;
        }

        var parsed = PageKrafterJson.Deserialize(data, DocumentMode.Email);
        if (!parsed.IsSuccess)
        {
            return null;
        }

        return renderer.RenderDocumentAsync(parsed.Value, subject ?? string.Empty).GetAwaiter().GetResult();
    }
}
