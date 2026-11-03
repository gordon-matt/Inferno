using Ardalis.Result;
using Inferno.Tasks.Entities;
using Inferno.Tasks.Services;
using Microsoft.Extensions.Logging;
using InfernoTask = Inferno.Tasks.Task;

namespace Inferno.Web.Areas.Admin.ScheduledTasks.Services;

public interface IScheduledTaskAdminService
{
    Task<Result<IReadOnlyList<ScheduledTask>>> ListAsync();

    /// <summary>
    /// Changes how a task runs. Name and type come from task discovery at startup, so they are
    /// not editable.
    /// </summary>
    Task<Result<ScheduledTask>> UpdateScheduleAsync(int id, int seconds, bool enabled, bool stopOnError);

    /// <summary>Runs the task once, even when it is switched off.</summary>
    Task<Result<ScheduledTask>> RunNowAsync(int id);
}

public class ScheduledTaskAdminService(
    IScheduledTaskService scheduledTaskService,
    ILogger<ScheduledTaskAdminService> logger) : IScheduledTaskAdminService
{
    public Task<Result<IReadOnlyList<ScheduledTask>>> ListAsync()
    {
        IReadOnlyList<ScheduledTask> tasks = scheduledTaskService.GetAllTasks(showHidden: true)
            .OrderBy(task => task.Name)
            .ToList();

        return Task.FromResult(Result.Success(tasks));
    }

    public Task<Result<ScheduledTask>> UpdateScheduleAsync(int id, int seconds, bool enabled, bool stopOnError)
    {
        if (seconds < 1)
        {
            return Task.FromResult(Result<ScheduledTask>.Invalid(new ValidationError
            {
                Identifier = nameof(ScheduledTask.Seconds),
                ErrorMessage = "Enter at least 1 second.",
            }));
        }

        var task = scheduledTaskService.GetTaskById(id);
        if (task is null)
        {
            return Task.FromResult(Result<ScheduledTask>.NotFound());
        }

        task.Seconds = seconds;
        task.Enabled = enabled;
        task.StopOnError = stopOnError;
        scheduledTaskService.UpdateTask(task);

        return Task.FromResult(Result.Success(task));
    }

    public async Task<Result<ScheduledTask>> RunNowAsync(int id)
    {
        var scheduledTask = scheduledTaskService.GetTaskById(id);
        if (scheduledTask is null)
        {
            return Result<ScheduledTask>.NotFound();
        }

        try
        {
            // Execute blocks until the task finishes; keep it off the circuit's thread.
            await Task.Run(() => new InfernoTask(scheduledTask) { Enabled = true }.Execute(throwException: true));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error while executing scheduled task {TaskName}", scheduledTask.Name);
            return Result<ScheduledTask>.Error(exception.Message);
        }

        return Result.Success(scheduledTaskService.GetTaskById(id) ?? scheduledTask);
    }
}
