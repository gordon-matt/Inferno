using Ardalis.Result;
using Hangfire;
using Hangfire.Storage;
using Inferno.Tasks.Scheduling;
using Inferno.Web;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.Scheduling;

/// <summary>One installed job and the schedule Hangfire currently has for it.</summary>
/// <param name="Id">The Hangfire job id.</param>
/// <param name="Name">The task name.</param>
/// <param name="Minutes">The interval shown in the editor: the stored minutes when the cron is one this site writes, otherwise the task default, and at least one.</param>
/// <param name="StoredMinutes">The minutes Hangfire is using, when the cron is one this site writes. <see langword="null"/> when the job is missing or its cron was set by hand.</param>
/// <param name="IsScheduled"><see langword="false"/> when Hangfire has no recurring job for this task.</param>
/// <param name="CronIsCustom"><see langword="true"/> when a job exists and its cron is not a minute interval this site writes.</param>
/// <param name="Cron">The cron Hangfire has stored. <see langword="null"/> when the job is missing.</param>
/// <param name="NextExecutionUtc">The next run Hangfire reports, in UTC. <see langword="null"/> when the job is missing.</param>
public sealed record ScheduledJobState(
    string Id,
    string Name,
    int Minutes,
    int? StoredMinutes,
    bool IsScheduled,
    bool CronIsCustom,
    string Cron,
    DateTime? NextExecutionUtc);

/// <summary>Reads and changes the Hangfire schedule of installed tasks.</summary>
public interface IScheduledJobService
{
    /// <summary>Reads every installed task and its Hangfire job.</summary>
    /// <returns>The jobs, or an error when Hangfire cannot be read.</returns>
    Task<Result<IReadOnlyList<ScheduledJobState>>> ListAsync();

    /// <summary>Creates the job when it is missing, so a deleted job comes back without a restart. Less than one minute is rejected.</summary>
    /// <param name="id">The Hangfire job id.</param>
    /// <param name="minutes">The new interval, in minutes.</param>
    /// <returns>Success, not found when no installed task has <paramref name="id"/>, or invalid when <paramref name="minutes"/> is below 1.</returns>
    Task<Result> UpdateAsync(string id, int minutes);

    /// <summary>Triggers the job now. The saved interval is left as it is.</summary>
    /// <param name="id">The Hangfire job id.</param>
    /// <returns>Success, not found when no installed task has <paramref name="id"/>, or an error when Hangfire refuses the trigger.</returns>
    Task<Result> RunNowAsync(string id);
}

/// <inheritdoc cref="IScheduledJobService" />
public class ScheduledJobService(
    IServiceProvider services,
    IRecurringJobManager jobs,
    IWorkContext workContext,
    ILogger<ScheduledJobService> logger) : IScheduledJobService
{
    /// <inheritdoc />
    public Task<Result<IReadOnlyList<ScheduledJobState>>> ListAsync()
    {
        try
        {
            Dictionary<string, RecurringJobDto> stored;
            using (var connection = JobStorage.Current.GetConnection())
            {
                stored = connection.GetRecurringJobs().ToDictionary(job => job.Id, StringComparer.Ordinal);
            }

            IReadOnlyList<ScheduledJobState> states = Discover().Select(definition =>
            {
                stored.TryGetValue(definition.Id, out var job);
                int? storedMinutes = job is null ? null : RecurringSchedule.TryToMinutes(job.Cron);
                int minutes = storedMinutes ?? DefaultMinutes(definition);
                return new ScheduledJobState(
                    definition.Id,
                    definition.Name,
                    minutes,
                    storedMinutes,
                    job is not null,
                    job is not null && storedMinutes is null,
                    job?.Cron,
                    job?.NextExecution);
            }).ToList();

            return Task.FromResult(Result.Success(states));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Scheduled tasks could not be read.");
            return Task.FromResult(Result<IReadOnlyList<ScheduledJobState>>.Error("Scheduled tasks could not be read."));
        }
    }

    /// <inheritdoc />
    public Task<Result> UpdateAsync(string id, int minutes)
    {
        var definition = Discover().FirstOrDefault(task => task.Id == id);
        if (definition is null)
        {
            return Task.FromResult(Result.NotFound());
        }

        if (minutes < 1)
        {
            return Task.FromResult(Result.Invalid(new ValidationError
            {
                Identifier = "Minutes",
                ErrorMessage = "Enter at least 1 minute."
            }));
        }

        string cron = RecurringSchedule.FromSeconds(minutes * 60);
        string typeKey = definition.TypeKey;
        jobs.AddOrUpdate<ScheduledTaskRunner>(id, runner => runner.Run(typeKey, JobCancellationToken.Null), cron);
        logger.LogInformation(
            "User {User} set scheduled task {TaskName} to cron {Cron}.",
            workContext.CurrentUser?.Email ?? "unknown",
            definition.Name,
            cron);
        return Task.FromResult(Result.Success());
    }

    /// <inheritdoc />
    public Task<Result> RunNowAsync(string id)
    {
        var definition = Discover().FirstOrDefault(task => task.Id == id);
        if (definition is null)
        {
            return Task.FromResult(Result.NotFound());
        }

        try
        {
            jobs.Trigger(id);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Scheduled task {TaskName} could not be started.", definition.Name);
            return Task.FromResult(Result.Error("The task could not be started."));
        }

        logger.LogInformation(
            "User {User} started scheduled task {TaskName}.",
            workContext.CurrentUser?.Email ?? "unknown",
            definition.Name);
        return Task.FromResult(Result.Success());
    }

    private IReadOnlyList<ScheduledTaskDefinition> Discover() => ScheduledTaskCatalog.Discover(services, logger);

    private static int DefaultMinutes(ScheduledTaskDefinition definition) =>
        Math.Max(1, (int)Math.Round(definition.DefaultIntervalSeconds / 60d, MidpointRounding.AwayFromZero));
}
