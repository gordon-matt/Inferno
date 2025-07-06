using Dependo;
using Inferno.Tasks.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Tasks.Infrastructure;

public class DependencyRegistrar : IDependencyRegistrar
{
    #region IDependencyRegistrar Members

    public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration) =>
        builder.Register<IScheduledTaskService, ScheduledTaskService>(ServiceLifetime.Transient);

    public int Order => 0;

    #endregion IDependencyRegistrar Members
}