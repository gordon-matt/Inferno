using System.Text.Json;
using System.Text.Json.Serialization;
using PageKrafter.Blocks;
using PageKrafter.Documents;
using PageKrafter.Templates;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages;

/// <summary>The JSON stored in <see cref="Entities.PageVersion.Fields"/> for a standard page.</summary>
public sealed class StandardPageFields
{
    /// <summary>The title used in the document head.</summary>
    public string MetaTitle { get; set; }

    /// <summary>The meta keywords.</summary>
    public string MetaKeywords { get; set; }

    /// <summary>The meta description.</summary>
    public string MetaDescription { get; set; }

    /// <summary>HTML shown to visitors. Generated from <see cref="Document"/>.</summary>
    public string BodyContent { get; set; }

    /// <summary>The page builder document. This is what the content editor reopens.</summary>
    public string Document { get; set; }
}

/// <summary>Reads and writes <see cref="StandardPageFields"/> JSON.</summary>
public static class StandardPageContent
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Reads fields from JSON.</summary>
    /// <param name="json">The stored JSON. Whitespace, or JSON that cannot be read, returns empty fields.</param>
    /// <returns>The fields. Never <see langword="null"/>.</returns>
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

    /// <summary>Writes <paramref name="fields"/> as JSON, omitting null properties.</summary>
    /// <param name="fields">The fields to store.</param>
    /// <returns>The JSON.</returns>
    public static string Serialize(StandardPageFields fields) => JsonSerializer.Serialize(fields, JsonOptions);

    /// <summary>
    /// Gets the document to open in the editor. Pages saved before the content editor existed come back
    /// as one HTML block, so their content is kept and can be rebuilt from there.
    /// </summary>
    /// <param name="fields">The stored fields.</param>
    /// <returns>The stored document, a one-block document built from <see cref="StandardPageFields.BodyContent"/>, or <see langword="null"/> when both are empty.</returns>
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
