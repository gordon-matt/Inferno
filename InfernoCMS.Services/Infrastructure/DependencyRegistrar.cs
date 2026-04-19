using Dependo;
using Inferno.Web.OData;
using InfernoCMS.Data.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InfernoCMS.Services;

public class DependencyRegistrar : IDependencyRegistrar
{
    public int Order => 999;

    public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration) =>
        builder.Register<IRadzenODataService<Person, int>, PersonODataService>(ServiceLifetime.Singleton);
}