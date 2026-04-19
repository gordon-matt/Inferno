using Extenso;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components;

public abstract class ContentBlockEditor<T> : ComponentBase, IContentBlockEditor
    where T : IContentBlock, new()
{
    [Parameter]
    public ContentBlock Data { get; set; }

    public T Model { get; set; }

    public string Save() => Model.JsonSerialize();

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Model = !string.IsNullOrEmpty(Data?.BlockValues) ? Data.BlockValues.JsonDeserialize<T>() : new T();
    }
}