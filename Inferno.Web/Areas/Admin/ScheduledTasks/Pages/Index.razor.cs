using Ardalis.Result;
using Heroicsoft.NetForge.Results;
using Inferno.Tasks.Entities;
using Inferno.Web.Areas.Admin.ScheduledTasks.Services;
using Inferno.Web.Components;
using Inferno.Web.Security;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.Areas.Admin.ScheduledTasks.Pages;

public partial class Index
{
    private IReadOnlyList<ScheduledTask> tasks;
    private readonly HashSet<int> running = [];
    private string search;
    private bool canWrite;

    private ScheduledTask editing;
    private ScheduleForm form = new();
    private IReadOnlyDictionary<string, string> errors;

    [Inject]
    private IScheduledTaskAdminService ScheduledTaskAdminService { get; set; }

    private IEnumerable<ScheduledTask> VisibleTasks => string.IsNullOrWhiteSpace(search)
        ? tasks
        : tasks.Where(task => task.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase));

    protected override async Task OnInitializedAsync()
    {
        canWrite = await HasPolicyAsync(InfernoWebPolicies.ScheduledTasksWrite);
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var result = await ScheduledTaskAdminService.ListAsync();
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            tasks = [];
            return;
        }

        tasks = result.Value;
    }

    /// <summary>A run that started after the last success and has finished.</summary>
    private static bool LastRunFailed(ScheduledTask task) =>
        task.LastEndUtc.HasValue && task.LastStartUtc.HasValue &&
        (!task.LastSuccessUtc.HasValue || task.LastSuccessUtc < task.LastStartUtc);

    private string FormatTime(DateTime? utc) =>
        utc.HasValue ? utc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") : T[InfernoWebLocalizableStrings.Admin.Never];

    private static string FormatInterval(int seconds) => seconds switch
    {
        <= 0 => "-",
        _ when seconds % 86400 == 0 => $"{seconds / 86400} d",
        _ when seconds % 3600 == 0 => $"{seconds / 3600} h",
        _ when seconds % 60 == 0 => $"{seconds / 60} min",
        _ => $"{seconds} s",
    };

    private void Edit(ScheduledTask task)
    {
        errors = null;
        form = new ScheduleForm { Seconds = task.Seconds, Enabled = task.Enabled, StopOnError = task.StopOnError };
        editing = task;
    }

    private void Close() => editing = null;

    private async Task SaveAsync()
    {
        if (!await EnsurePolicyAsync(InfernoWebPolicies.ScheduledTasksWrite))
        {
            return;
        }

        var result = await ScheduledTaskAdminService.UpdateScheduleAsync(editing.Id, form.Seconds, form.Enabled, form.StopOnError);
        if (result.Status == ResultStatus.Invalid)
        {
            errors = ErrorText.From(result);
            return;
        }

        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        Toasts.Success(T[InfernoWebLocalizableStrings.Admin.Saved]);
        editing = null;
        await LoadAsync();
    }

    private async Task RunNowAsync(ScheduledTask task)
    {
        if (!running.Add(task.Id))
        {
            return;
        }

        try
        {
            if (!await EnsurePolicyAsync(InfernoWebPolicies.ScheduledTasksWrite))
            {
                return;
            }

            var result = await ScheduledTaskAdminService.RunNowAsync(task.Id);
            if (result.IsSuccess)
            {
                Toasts.Success(T[InfernoWebLocalizableStrings.ScheduledTasks.ExecutedTaskSuccess]);
            }
            else
            {
                Toasts.Error($"{T[InfernoWebLocalizableStrings.ScheduledTasks.ExecutedTaskError]} {result.GetDisplayMessage()}");
            }
        }
        finally
        {
            running.Remove(task.Id);
        }

        await LoadAsync();
        if (editing?.Id == task.Id)
        {
            editing = tasks.FirstOrDefault(candidate => candidate.Id == task.Id);
        }
    }

    private sealed class ScheduleForm
    {
        public int Seconds { get; set; }

        public bool Enabled { get; set; }

        public bool StopOnError { get; set; }
    }
}
