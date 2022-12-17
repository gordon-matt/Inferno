using Autofac;
using Dependo.Autofac;
using Inferno.Web.OData;
using InfernoCMS.Data.Entities;

namespace InfernoCMS.Services
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => 999;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<PersonODataService>().As<IRadzenODataService<Person, int>>().SingleInstance();
        }
    }
}