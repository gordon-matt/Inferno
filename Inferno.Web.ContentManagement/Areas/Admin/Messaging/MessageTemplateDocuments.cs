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
    public static bool IsDocument(string data) =>
        !string.IsNullOrWhiteSpace(data)
        && data.TrimStart().StartsWith('{')
        && data.Contains("\"root\"", StringComparison.Ordinal);

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

    public static string Email(string html)
    {
        var document = PageDocument.Create(DocumentMode.Email);
        document.Root.Add(Nodes.Create(BlockTypes.Text, ("content", html)));
        return PageKrafterJson.Serialize(document);
    }
}

public sealed class PageKrafterMessageTemplateRenderer(PageRenderer renderer) : Inferno.Web.Messaging.IMessageTemplateRenderer
{
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
