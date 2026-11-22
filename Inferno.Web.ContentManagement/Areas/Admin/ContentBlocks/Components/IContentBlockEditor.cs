using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components;

/// <summary>Edits one stored content block and returns its serialized values.</summary>
public interface IContentBlockEditor
{
    /// <summary>The stored block being edited, including its current values.</summary>
    ContentBlock Data { get; set; }

    /// <summary>Serializes the edited values.</summary>
    /// <returns>The block values as JSON.</returns>
    string Save();
}