using Heroicsoft.NetForge.Results;
using Inferno.Security;
using Inferno.Tasks.Scheduling;
using Inferno.Web.Scheduling;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.Areas.Admin.Configuration.Pages;

public partial class ScheduledTasks
{
    private IReadOnlyList<ScheduledJobState> jobs;
    private ScheduleEdit editing;
    private bool saving;

    [Inject]
    private IScheduledJobService ScheduledJobService { get; set; }

    private string DrawerTitle => editing is null
        ? null
        : T[InfernoWebLocalizableStrings.General.EditFormat, editing.Name].Value;

    protected override Task OnInitializedAsync() => LoadAsync();

    private async Task LoadAsync()
    {
        var result = await ScheduledJobService.ListAsync();
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            jobs = [];
            return;
        }

        jobs = result.Value.OrderBy(job => job.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private string IntervalText(ScheduledJobState job)
    {
        if (!job.IsScheduled)
        {
            return T[InfernoWebLocalizableStrings.ScheduledTasks.NotScheduled].Value;
        }

        if (job.CronIsCustom)
        {
            return job.Cron;
        }

        return T[InfernoWebLocalizableStrings.ScheduledTasks.EveryMinutes, job.Minutes].Value;
    }

    private string EditStatus(ScheduleEdit job)
    {
        if (!job.IsScheduled)
        {
            return T[InfernoWebLocalizableStrings.ScheduledTasks.NotScheduled].Value;
        }

        if (job.CronIsCustom)
        {
            return T[InfernoWebLocalizableStrings.ScheduledTasks.ReplacesCron, job.Cron].Value;
        }

        return job.NextExecutionUtc is DateTime next
            ? T[InfernoWebLocalizableStrings.ScheduledTasks.NextRun, next.ToLocalTime().ToString("g")].Value
            : null;
    }

    private string ScheduleHint(int minutes)
    {
        if (minutes < 1)
        {
            return T[InfernoWebLocalizableStrings.ScheduledTasks.AtLeastOne].Value;
        }

        int? effective = RecurringSchedule.TryToMinutes(RecurringSchedule.FromSeconds(minutes * 60));
        return effective is null || effective == minutes
            ? T[InfernoWebLocalizableStrings.ScheduledTasks.EveryMinutes, effective ?? minutes].Value
            : T[InfernoWebLocalizableStrings.ScheduledTasks.AdjustedTo, effective].Value;
    }

    private void Edit(ScheduledJobState job) => editing = ToEdit(job);

    private void Close() => editing = null;

    private async Task SaveAsync()
    {
        if (saving || editing is null || editing.Minutes < 1 || !await EnsurePolicyAsync(StandardPolicies.FullAccess))
        {
            return;
        }

        saving = true;
        try
        {
            var result = await ScheduledJobService.UpdateAsync(editing.Id, editing.Minutes);
            if (!result.IsSuccess)
            {
                Toasts.ShowFailure(result);
                return;
            }

            Toasts.Success(T[InfernoWebLocalizableStrings.Admin.Saved]);
            editing = null;
            await LoadAsync();
        }
        finally
        {
            saving = false;
        }
    }

    private async Task RunNowAsync()
    {
        if (saving || editing is null || editing.Minutes < 1 || !await EnsurePolicyAsync(StandardPolicies.FullAccess))
        {
            return;
        }

        saving = true;
        try
        {
            string id = editing.Id;
            var updated = await ScheduledJobService.UpdateAsync(id, editing.Minutes);
            if (!updated.IsSuccess)
            {
                Toasts.ShowFailure(updated);
                return;
            }

            var ran = await ScheduledJobService.RunNowAsync(id);
            if (!ran.IsSuccess)
            {
                Toasts.ShowFailure(ran);
                await LoadAsync();
                Reselect(id);
                return;
            }

            Toasts.Success(T[InfernoWebLocalizableStrings.ScheduledTasks.Started]);
            await LoadAsync();
            Reselect(id);
        }
        finally
        {
            saving = false;
        }
    }

    private void Reselect(string id)
    {
        var match = jobs.FirstOrDefault(job => job.Id == id);
        editing = match is null ? null : ToEdit(match);
    }

    private static ScheduleEdit ToEdit(ScheduledJobState state) => new()
    {
        Id = state.Id,
        Name = state.Name,
        Minutes = state.Minutes,
        IsScheduled = state.IsScheduled,
        CronIsCustom = state.CronIsCustom,
        Cron = state.Cron,
        NextExecutionUtc = state.NextExecutionUtc
    };

    private sealed class ScheduleEdit
    {
        public string Id { get; init; }

        public string Name { get; init; }

        public int Minutes { get; set; }

        public bool IsScheduled { get; init; }

        public bool CronIsCustom { get; init; }

        public string Cron { get; init; }

        public DateTime? NextExecutionUtc { get; init; }
    }
}
