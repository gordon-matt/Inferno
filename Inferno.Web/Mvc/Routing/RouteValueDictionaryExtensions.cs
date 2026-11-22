using System.Collections.Specialized;
using Microsoft.AspNetCore.Routing;

namespace Inferno.Web.Mvc.Routing;

/// <summary>Copies route values and overlays another set.</summary>
public static class RouteValueDictionaryExtensions
{
    /// <summary>
    /// Copies <paramref name="obj"/> and overlays <paramref name="values"/>. Underscores in an overlaid key are replaced with hyphens.
    /// Keys in <paramref name="removeKeys"/> are then removed when they are present.
    /// </summary>
    /// <param name="obj">The values to copy.</param>
    /// <param name="values">Values written over the copy. A property's route value is used.</param>
    /// <param name="removeKeys">Keys removed after the overlay. <see langword="null"/> or empty removes nothing.</param>
    /// <returns>The merged dictionary.</returns>
    public static RouteValueDictionary Merge(this RouteValueDictionary obj, object values, params string[] removeKeys)
    {
        var mergeValues = new RouteValueDictionary(values);

        var result = new RouteValueDictionary(obj);
        foreach (var value in mergeValues)
        {
            result[value.Key.Replace("_", "-")] = value.Value;
        }

        if (removeKeys != null && removeKeys.Length > 0)
        {
            foreach (string key in removeKeys.Where(result.ContainsKey))
            {
                result.Remove(key);
            }
        }

        return result;
    }

    /// <summary>
    /// Copies <paramref name="obj"/> and overlays <paramref name="values"/>. Keys are copied as they are.
    /// Keys in <paramref name="removeKeys"/> are then removed when they are present.
    /// </summary>
    /// <param name="obj">The values to copy.</param>
    /// <param name="values">Values written over the copy.</param>
    /// <param name="removeKeys">Keys removed after the overlay. <see langword="null"/> or empty removes nothing.</param>
    /// <returns>The merged dictionary.</returns>
    public static RouteValueDictionary Merge(this RouteValueDictionary obj, NameValueCollection values, params string[] removeKeys)
    {
        var result = new RouteValueDictionary(obj);
        foreach (string key in values.AllKeys)
        {
            result[key] = values[key];
        }

        if (removeKeys != null && removeKeys.Length > 0)
        {
            foreach (string key in removeKeys.Where(result.ContainsKey))
            {
                result.Remove(key);
            }
        }

        return result;
    }
}