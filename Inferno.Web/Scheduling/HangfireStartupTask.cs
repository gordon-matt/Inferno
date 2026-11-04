using Dependo;
using Hangfire;
using Hangfire.Storage;
using Inferno.Tasks;
using Inferno.Tasks.Scheduling;
using Inferno.Web.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace Inferno.Web.Scheduling;

/// <summary>
/// Registers one Hangfire recurring job for each <see cref="ITask"/>. A job that is already there keeps
/// its schedule. A job whose task no longer exists is removed.
/// </summary>
public class HangfireStartupTask : IStartupTask
{
    public int Order => 100;

    public Task ExecuteAsync()
    {
        var services = DependoResolver.Instance.Resolve<IServiceProvider>();
        var manager = services.GetRequiredService<IRecurringJobManager>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger<HangfireStartupTask>();

        var tasks = ScheduledTaskCatalog.Discover(services, logger);
        var currentIds = new HashSet<string>(StringComparer.Ordinal);

        var existing = new HashSet<string>(StringComparer.Ordinal);
        using (var connection = JobStorage.Current.GetConnection())
        {
            foreach (var job in connection.GetRecurringJobs())
            {
                existing.Add(job.Id);
            }
        }

        foreach (var task in tasks)
        {
            currentIds.Add(task.Id);

            if (existing.Contains(task.Id))
            {
                continue;
            }

            string cron = RecurringSchedule.FromSeconds(task.DefaultIntervalSeconds);
            string typeKey = task.TypeKey;
            manager.AddOrUpdate<ScheduledTaskRunner>(task.Id, runner => runner.Run(typeKey, JobCancellationToken.Null), cron);
            logger.LogInformation("Registered scheduled task {TaskName} ({TaskType}) with cron {Cron}.", task.Name, task.Type.FullName, cron);
        }

        foreach (string id in existing.Where(id => id.StartsWith(ScheduledJobs.JobIdPrefix, StringComparison.Ordinal) && !currentIds.Contains(id)))
        {
            manager.RemoveIfExists(id);
            logger.LogInformation("Removed scheduled task {JobId} because its task is no longer installed.", id);
        }

        return Task.CompletedTask;
    }
}
