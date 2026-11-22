namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.Components;

/// <summary>Editor for a <see cref="Inferno.Web.ContentManagement.Areas.Admin.Blog.ContentBlocks.LastNPostsBlock"/>.</summary>
public partial class LastNPostsBlockEditor
{
    // InputNumber has no byte support, so the stored byte is edited through an int.
    private int NumberOfEntries
    {
        get => Model.NumberOfEntries;
        set => Model.NumberOfEntries = (byte)Math.Clamp(value, byte.MinValue, byte.MaxValue);
    }
}
