using Dependo;
using Microsoft.Extensions.Localization;

namespace Inferno.Localization.ComponentModel;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class LocalizedHelpTextAttribute : Attribute
{
    private static IStringLocalizer T => field ??= DependoResolver.Instance.Resolve<IStringLocalizer>();

    public LocalizedHelpTextAttribute(string resourceKey)
    {
        ResourceKey = resourceKey;
    }

    public string ResourceKey { get; set; }

    public string HelpText => T[ResourceKey];
}