using Dependo;
using Inferno.Tasks;
using Inferno.Tasks.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.Scheduling;

/// <summary>An installed background job and the interval it uses the first time it is registered.</summary>
public sealed record ScheduledTaskDefinition(string Name, Type Type, int DefaultIntervalSeconds)
{
    public string Id => ScheduledJobs.JobId(Type);

    public string TypeKey => ScheduledJobs.TypeKey(Type);
}

/// <summary>The same task list startup registers and the settings page edits.</summary>
public static class ScheduledTaskCatalog
{
    public static IReadOnlyList<ScheduledTaskDefinition> Discover(IServiceProvider services, ILogger logger)
    {
        var typeFinder = services.GetRequiredService<ITypeFinder>();
        using var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var registered = scope.ServiceProvider.GetServices<ITask>().ToDictionary(task => task.GetType());
        var tasks = new List<ScheduledTaskDefinition>();

        foreach (var type in typeFinder.FindClassesOfType<ITask>())
        {
            try
            {
                if (!registered.TryGetValue(type, out var task))
                {
                    task = (ITask)ActivatorUtilities.CreateInstance(scope.ServiceProvider, type);
                }

                tasks.Add(new ScheduledTaskDefinition(task.Name, type, task.DefaultInterval));
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Scheduled task {TaskType} could not be created.", type.FullName);
            }
        }

        return tasks;
    }
}
