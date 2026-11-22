namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;

/// <summary>Helpers for reading a block's runtime type.</summary>
public static class IContentBlockExtensions
{
    /// <summary>The assembly-qualified name of the block's runtime type.</summary>
    /// <param name="contentBlock">The block whose type is read.</param>
    /// <returns>The full type name, a comma, and the assembly's full name.</returns>
    public static string GetTypeFullName(this IContentBlock contentBlock)
    {
        var type = contentBlock.GetType();
        return $"{type.FullName}, {type.Assembly.FullName}";
    }
}