namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.Components;

public partial class CategoriesBlockEditor
{
    // InputNumber has no byte support, so the stored byte is edited through an int.
    private int NumberOfCategories
    {
        get => Model.NumberOfCategories;
        set => Model.NumberOfCategories = (byte)Math.Clamp(value, byte.MinValue, byte.MaxValue);
    }
}
