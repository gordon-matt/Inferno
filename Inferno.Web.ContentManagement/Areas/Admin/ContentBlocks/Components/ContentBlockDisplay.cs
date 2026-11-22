using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components;

/// <summary>Renders a content block of type <typeparamref name="T"/>.</summary>
/// <typeparam name="T">The block type. It must have a public parameterless constructor.</typeparam>
public abstract class ContentBlockDisplay<T> : ComponentBase
    where T : IContentBlock, new()
{
    /// <summary>The block to render.</summary>
    [Parameter]
    public T Model { get; set; }
}