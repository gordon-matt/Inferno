// https://github.com/aspnet/Mvc/blob/master/src/Microsoft.AspNetCore.Mvc.Core/Controllers/IControllerPropertyActivatorFactory.cs

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Inferno.Web.Mvc.Controllers;

/// <summary>Sets controller properties that are marked for activation.</summary>
internal interface IControllerPropertyActivator
{
    /// <summary>Sets the activated properties on <paramref name="controller"/>.</summary>
    /// <param name="context">The current controller context, also the value assigned to context properties.</param>
    /// <param name="controller">The controller instance.</param>
    void Activate(ControllerContext context, object controller);

    /// <summary>A delegate that sets the activated properties for <paramref name="actionDescriptor"/>'s controller type.</summary>
    /// <param name="actionDescriptor">The action whose controller type is inspected.</param>
    /// <returns>The activator delegate.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="actionDescriptor"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><see cref="ControllerActionDescriptor.ControllerTypeInfo"/> is <see langword="null"/>.</exception>
    Action<ControllerContext, object> GetActivatorDelegate(ControllerActionDescriptor actionDescriptor);
}