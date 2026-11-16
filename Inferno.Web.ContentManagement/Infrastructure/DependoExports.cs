using Dependo;
using Inferno.Data.Entity;
using Inferno.Web.Infrastructure;

[assembly: DependoExports(typeof(IStartupTask), typeof(IRouterAssemblyMarker), typeof(IInfernoEntityTypeConfiguration))]
