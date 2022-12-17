using Extenso.AspNetCore.OData;
using InfernoCMS.Data.Entities;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;

namespace InfernoCMS.Api.Infrastructure
{
    public class ODataRegistrar : IODataRegistrar
    {
        public void Register(ODataOptions options)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Person>("PersonApi");
            options.AddRouteComponents("odata", builder.GetEdmModel());
        }
    }
}