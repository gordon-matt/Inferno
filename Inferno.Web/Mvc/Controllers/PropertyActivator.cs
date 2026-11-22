// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace Inferno.Web.Mvc.Controllers;

/// <summary>Assigns one property from a value read off a context object.</summary>
/// <typeparam name="TContext">The context passed to the value accessor.</typeparam>
internal class PropertyActivator<TContext>
{
    private readonly Func<TContext, object> _valueAccessor;
    private readonly Action<object, object> _fastPropertySetter;

    /// <summary>Creates an activator that writes <paramref name="propertyInfo"/>.</summary>
    /// <param name="propertyInfo">The property to set.</param>
    /// <param name="valueAccessor">Reads the value to assign from the context.</param>
    /// <exception cref="ArgumentNullException"><paramref name="propertyInfo"/> or <paramref name="valueAccessor"/> is <see langword="null"/>.</exception>
    public PropertyActivator(
        PropertyInfo propertyInfo,
        Func<TContext, object> valueAccessor)
    {
        PropertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
        _valueAccessor = valueAccessor ?? throw new ArgumentNullException(nameof(valueAccessor));
        _fastPropertySetter = PropertyHelper.MakeFastPropertySetter(propertyInfo);
    }

    /// <summary>The property this activator sets.</summary>
    public PropertyInfo PropertyInfo { get; private set; }

    /// <summary>Sets the property on <paramref name="instance"/> from <paramref name="context"/>.</summary>
    /// <param name="instance">The object whose property is set.</param>
    /// <param name="context">Passed to the value accessor.</param>
    /// <returns>The value that was assigned.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="instance"/> is <see langword="null"/>.</exception>
    public object Activate(object instance, TContext context)
    {
        if (instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        object value = _valueAccessor(context);
        _fastPropertySetter(instance, value);
        return value;
    }

    /// <summary>Finds public, settable, non-indexed properties on <paramref name="type"/> marked with <paramref name="activateAttributeType"/>.</summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="activateAttributeType">The attribute that marks a property.</param>
    /// <param name="createActivateInfo">Builds an activator for each matching property.</param>
    /// <returns>The activators.</returns>
    /// <exception cref="ArgumentNullException">Any argument is <see langword="null"/>.</exception>
    public static PropertyActivator<TContext>[] GetPropertiesToActivate(
        Type type,
        Type activateAttributeType,
        Func<PropertyInfo, PropertyActivator<TContext>> createActivateInfo) => type == null
            ? throw new ArgumentNullException(nameof(type))
            : activateAttributeType == null
            ? throw new ArgumentNullException(nameof(activateAttributeType))
            : createActivateInfo == null
            ? throw new ArgumentNullException(nameof(createActivateInfo))
            : GetPropertiesToActivate(type, activateAttributeType, createActivateInfo, includeNonPublic: false);

    /// <summary>Finds settable, non-indexed properties on <paramref name="type"/> marked with <paramref name="activateAttributeType"/>.</summary>
    /// <param name="type">The type to inspect.</param>
    /// <param name="activateAttributeType">The attribute that marks a property.</param>
    /// <param name="createActivateInfo">Builds an activator for each matching property.</param>
    /// <param name="includeNonPublic"><see langword="true"/> includes properties whose setter is not public. Static setters are still omitted.</param>
    /// <returns>The activators.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/>, <paramref name="activateAttributeType"/>, or <paramref name="createActivateInfo"/> is <see langword="null"/>.</exception>
    public static PropertyActivator<TContext>[] GetPropertiesToActivate(
        Type type,
        Type activateAttributeType,
        Func<PropertyInfo, PropertyActivator<TContext>> createActivateInfo,
        bool includeNonPublic)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (activateAttributeType == null)
        {
            throw new ArgumentNullException(nameof(activateAttributeType));
        }

        if (createActivateInfo == null)
        {
            throw new ArgumentNullException(nameof(createActivateInfo));
        }

        var properties = type.GetRuntimeProperties()
            .Where((property) => property.IsDefined(activateAttributeType) &&
                    property.GetIndexParameters().Length == 0 &&
                    property.SetMethod != null &&
                    !property.SetMethod.IsStatic);

        if (!includeNonPublic)
        {
            properties = properties.Where(property => property.SetMethod.IsPublic);
        }

        return properties.Select(createActivateInfo).ToArray();
    }
}