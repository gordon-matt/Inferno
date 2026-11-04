using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Inferno.Tasks.Scheduling;

/// <summary>
/// Hangfire calls this. The task itself is created in a fresh scope, so a scoped dependency it asks
/// the container for is disposed when the run ends. The same task does not run twice at once.
/// </summary>
public sealed class ScheduledTaskRunner(IServiceScopeFactory scopes, ILogger<ScheduledTaskRunner> logger)
{
    private const int LockTimeoutSeconds = 60 * 60;

    [DisableConcurrentExecution(LockTimeoutSeconds)]
    public void Run(string taskType, IJobCancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var type = ResolveType(taskType);
        if (type is null || !typeof(ITask).IsAssignableFrom(type))
        {
            logger.LogWarning("Scheduled task {TaskType} is no longer loaded, so this run was skipped.", taskType);
            return;
        }

        using var scope = scopes.CreateScope();
        var registered = scope.ServiceProvider.GetServices<ITask>().FirstOrDefault(candidate => candidate.GetType() == type);
        var task = registered ?? (ITask)ActivatorUtilities.CreateInstance(scope.ServiceProvider, type);

        logger.LogInformation("Running scheduled task {TaskName}.", task.Name);
        task.Execute();
    }

    private static Type ResolveType(string taskType)
    {
        if (string.IsNullOrWhiteSpace(taskType))
        {
            return null;
        }

        var resolved = Type.GetType(taskType, throwOnError: false);
        if (resolved is not null)
        {
            return resolved;
        }

        string fullName = taskType.Split(',')[0].Trim();
        return AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(fullName, throwOnError: false))
            .FirstOrDefault(candidate => candidate is not null);
    }
}
