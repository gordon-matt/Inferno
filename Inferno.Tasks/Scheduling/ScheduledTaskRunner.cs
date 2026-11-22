using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Inferno.Tasks.Scheduling;

/// <summary>
/// Hangfire calls this. The task itself is created in a fresh scope, so a scoped dependency it asks
/// the container for is disposed when the run ends. Hangfire waits up to one hour for a previous run of the same job to finish.
/// </summary>
/// <param name="scopes">Creates the scope the task is resolved in.</param>
/// <param name="logger">Receives the start line, and a warning when the type cannot be run.</param>
public sealed class ScheduledTaskRunner(IServiceScopeFactory scopes, ILogger<ScheduledTaskRunner> logger)
{
    private const int LockTimeoutSeconds = 60 * 60;

    /// <summary>
    /// Runs one scheduled task. A blank type, a missing type, or a type that is not an <see cref="ITask"/> is skipped.
    /// A registered service of that exact type is used; otherwise the type is constructed from the scope.
    /// </summary>
    /// <param name="taskType">The <see cref="ScheduledJobs.TypeKey"/> stored on the job. A blank value is skipped.</param>
    /// <param name="cancellationToken">Observed before the task is resolved. A cancelled job throws and does not run.</param>
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
