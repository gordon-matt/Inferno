using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace InfernoCMS.Api.Infrastructure;

/// <summary>
/// Removes the inherited <c>Stream</c> action from every controller deriving from
/// <see cref="ODataController"/>.
///
/// Extenso.AspNetCore.OData 10.x adds a <c>Stream</c> method to its
/// <c>GenericODataController&lt;,&gt;</c> base class decorated with both
/// <c>[HttpGet("stream")]</c> and <c>[Route("~/odata/[controller]/stream")]</c>.
/// The two combined cause two problems:
///   1. Swashbuckle reports "Ambiguous HTTP method" because the <c>[Route]</c>
///      attribute is not constrained to a verb.
///   2. Mixing MVC attribute routing with OData convention routing in a single
///      controller can cause the OData convention routing for the standard CRUD
///      methods (Get/Post/Patch/Delete) on the derived controller to silently
///      fail to wire up, leading to 404s for every entity set.
/// We are not currently using the streaming endpoint, so simply removing the
/// action restores standard OData routing and silences Swagger.
/// </summary>
public sealed class OmitODataStreamActionConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            if (!typeof(ODataController).IsAssignableFrom(controller.ControllerType))
            {
                continue;
            }

            var streamActions = controller.Actions
                .Where(a => a.ActionMethod.Name == "Stream")
                .ToList();

            foreach (var action in streamActions)
            {
                controller.Actions.Remove(action);
            }
        }
    }
}