namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.Components;

public partial class TagCloudBlockEditor
{
    // InputNumber has no byte support, so the stored bytes are edited through ints.
    private int Steps
    {
        get => Model.Steps;
        set => Model.Steps = (byte)Math.Clamp(value, 1, byte.MaxValue);
    }

    private int? Delay
    {
        get => Model.Delay;
        set => Model.Delay = value.HasValue ? (byte)Math.Clamp(value.Value, byte.MinValue, byte.MaxValue) : null;
    }
}
