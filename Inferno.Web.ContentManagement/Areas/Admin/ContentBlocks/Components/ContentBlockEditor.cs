using Extenso;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components;

/// <summary>Edits a content block of type <typeparamref name="T"/> and serializes it back to JSON.</summary>
/// <typeparam name="T">The block type. It must have a public parameterless constructor.</typeparam>
public abstract class ContentBlockEditor<T> : ComponentBase, IContentBlockEditor
    where T : IContentBlock, new()
{
    /// <inheritdoc />
    [Parameter]
    public ContentBlock Data { get; set; }

    /// <summary>The deserialized block. A missing or empty value starts as a new <typeparamref name="T"/>.</summary>
    public T Model { get; set; }

    /// <inheritdoc />
    public string Save() => Model.JsonSerialize();

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Model = !string.IsNullOrEmpty(Data?.BlockValues) ? Data.BlockValues.JsonDeserialize<T>() : new T();
    }
}