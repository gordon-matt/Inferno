using Dependo;
using Inferno.Data.Entity;
using Inferno.Tasks;
using Inferno.Web.Infrastructure;

[assembly: DependoExports(
    typeof(IStartupTask),
    typeof(ITask),
    typeof(IRouterAssemblyMarker),
    typeof(IInfernoEntityTypeConfiguration))]
