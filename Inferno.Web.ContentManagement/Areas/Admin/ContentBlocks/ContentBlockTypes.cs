namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;

/// <summary>
/// Resolves a content block type from the name stored on the row. Rows keep the type's full name,
/// which <see cref="Type.GetType(string)"/> cannot see once the type lives in another assembly —
/// a plugin's, for example.
/// </summary>
public static class ContentBlockTypes
{
    public static Type Resolve(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return null;
        }

        var direct = Type.GetType(typeName, throwOnError: false);
        if (direct is not null)
        {
            return direct;
        }

        string fullName = typeName;
        string assemblyName = null;
        int comma = typeName.IndexOf(',');
        if (comma >= 0)
        {
            fullName = typeName[..comma].Trim();
            assemblyName = typeName[(comma + 1)..].Trim();
            int extra = assemblyName.IndexOf(',');
            if (extra >= 0)
            {
                assemblyName = assemblyName[..extra].Trim();
            }
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic)
            {
                continue;
            }

            if (assemblyName is not null && !string.Equals(assembly.GetName().Name, assemblyName, StringComparison.Ordinal))
            {
                continue;
            }

            var type = assembly.GetType(fullName, throwOnError: false);
            if (type is not null)
            {
                return type;
            }
        }

        return null;
    }
}
