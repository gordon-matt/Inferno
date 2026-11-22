using Dependo;
using Inferno.Tasks;
using Inferno.Tasks.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.Scheduling;

/// <summary>An installed background job and the interval it uses the first time it is registered.</summary>
/// <param name="Name">The name shown for the job.</param>
/// <param name="Type">The <see cref="ITask"/> implementation.</param>
/// <param name="DefaultIntervalSeconds">The interval used the first time the job is registered, in seconds.</param>
public sealed record ScheduledTaskDefinition(string Name, Type Type, int DefaultIntervalSeconds)
{
    /// <summary>The Hangfire job id for <see cref="Type"/>.</summary>
    public string Id => ScheduledJobs.JobId(Type);

    /// <summary>The type key Hangfire stores for <see cref="Type"/>.</summary>
    public string TypeKey => ScheduledJobs.TypeKey(Type);
}

/// <summary>The same task list startup registers and the settings page edits.</summary>
public static class ScheduledTaskCatalog
{
    /// <summary>
    /// Creates each <see cref="ITask"/>, using the one already registered when the container has it.
    /// A type that cannot be created is logged and skipped.
    /// </summary>
    /// <param name="services">The host provider. A scope is opened to resolve registered tasks.</param>
    /// <param name="logger">Receives the exception when a task cannot be created.</param>
    /// <returns>The tasks that were created.</returns>
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
