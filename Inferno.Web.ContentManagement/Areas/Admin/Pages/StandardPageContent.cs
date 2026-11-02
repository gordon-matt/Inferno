using System.Text.Json;
using System.Text.Json.Serialization;
using PageKrafter.Blocks;
using PageKrafter.Documents;
using PageKrafter.Templates;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages;

/// <summary>The JSON stored in <see cref="Entities.PageVersion.Fields"/> for a standard page.</summary>
public sealed class StandardPageFields
{
    public string MetaTitle { get; set; }

    public string MetaKeywords { get; set; }

    public string MetaDescription { get; set; }

    /// <summary>HTML shown to visitors. Generated from <see cref="Document"/>.</summary>
    public string BodyContent { get; set; }

    /// <summary>The page builder document. This is what the content editor reopens.</summary>
    public string Document { get; set; }
}

public static class StandardPageContent
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static StandardPageFields Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new StandardPageFields();
        }

        try
        {
            return JsonSerializer.Deserialize<StandardPageFields>(json, JsonOptions) ?? new StandardPageFields();
        }
        catch (JsonException)
        {
            return new StandardPageFields();
        }
    }

    public static string Serialize(StandardPageFields fields) => JsonSerializer.Serialize(fields, JsonOptions);

    /// <summary>
    /// The document to open in the editor. Pages saved before the content editor existed come back
    /// as one HTML block, so their content is kept and can be rebuilt from there.
    /// </summary>
    public static string ForEditor(StandardPageFields fields)
    {
        if (!string.IsNullOrWhiteSpace(fields.Document))
        {
            return fields.Document;
        }

        if (string.IsNullOrWhiteSpace(fields.BodyContent))
        {
            return null;
        }

        var document = PageDocument.Create();
        document.Root.Add(Nodes.Create(BlockTypes.Html, ("html", fields.BodyContent)));
        return PageKrafterJson.Serialize(document);
    }
}
