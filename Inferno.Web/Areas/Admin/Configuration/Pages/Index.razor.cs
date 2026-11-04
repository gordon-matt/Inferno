using Heroicsoft.NetForge.Results;
using Inferno.Security;
using Inferno.Tasks.Scheduling;
using Inferno.Web.Areas.Admin.Configuration.Services;
using Inferno.Web.Components;
using Inferno.Web.Configuration.Entities;
using Inferno.Web.Scheduling;
using Inferno.Web.Security;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.Areas.Admin.Configuration.Pages;

public partial class Index
{
    private IReadOnlyList<SettingsGroup> groups;
    private bool canWrite;
    private bool showSchedules;

    private Setting editing;
    private Type editorType;
    private DynamicComponent editor;
    private bool editingSchedules;
    private List<ScheduleEdit> scheduleEdits = [];
    private bool saving;

    [Inject]
    private ISettingsAdminService SettingsAdminService { get; set; }

    [Inject]
    private IScheduledJobService ScheduledJobService { get; set; }

    private string DrawerTitle => editingSchedules
        ? T[InfernoWebLocalizableStrings.ScheduledTasks.Title].Value
        : editing is null
            ? null
            : T[InfernoWebLocalizableStrings.General.EditFormat, editing.Name].Value;

    protected override async Task OnInitializedAsync()
    {
        canWrite = await HasPolicyAsync(InfernoWebPolicies.SettingsWrite);
        showSchedules = await HasPolicyAsync(StandardPolicies.FullAccess);
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var result = await SettingsAdminService.ListAsync(TenantId);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            groups = [];
            return;
        }

        groups = result.Value;
    }

    private static string IconFor(SettingsGroup group)
    {
        string name = group.Type;
        return name.Contains("Smtp", StringComparison.OrdinalIgnoreCase) ? "mail"
            : name.Contains("Membership", StringComparison.OrdinalIgnoreCase) ? "users"
            : name.Contains("DateTime", StringComparison.OrdinalIgnoreCase) ? "clock"
            : name.Contains("Page", StringComparison.OrdinalIgnoreCase) ? "file-earmark-text"
            : name.Contains("Site", StringComparison.OrdinalIgnoreCase) ? "globe"
            : "sliders";
    }

    private async Task EditAsync(SettingsGroup group)
    {
        var result = await SettingsAdminService.GetAsync(TenantId, group.Type);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        editingSchedules = false;
        editorType = group.EditorType;
        editing = result.Value;
    }

    private async Task EditSchedulesAsync()
    {
        if (!await EnsurePolicyAsync(StandardPolicies.FullAccess))
        {
            return;
        }

        var result = await ScheduledJobService.ListAsync();
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        scheduleEdits = result.Value.Select(ToEdit).ToList();
        editing = null;
        editorType = null;
        editingSchedules = true;
    }

    private void Close()
    {
        editing = null;
        editorType = null;
        editingSchedules = false;
        scheduleEdits = [];
    }

    private string ScheduleStatus(ScheduleEdit job)
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

    private async Task SaveAsync()
    {
        if (saving || editor?.Instance is not ISettingsEditor settingsEditor)
        {
            return;
        }

        if (!await EnsurePolicyAsync(InfernoWebPolicies.SettingsWrite))
        {
            return;
        }

        saving = true;
        try
        {
            var result = await SettingsAdminService.SaveAsync(TenantId, editing.Type, settingsEditor.Save());
            if (!result.IsSuccess)
            {
                Toasts.ShowFailure(result);
                return;
            }

            Toasts.Success(T[InfernoWebLocalizableStrings.Admin.Saved]);
            Close();
            await LoadAsync();
        }
        finally
        {
            saving = false;
        }
    }

    private async Task SaveSchedulesAsync()
    {
        if (saving || !await EnsurePolicyAsync(StandardPolicies.FullAccess))
        {
            return;
        }

        saving = true;
        try
        {
            foreach (var job in scheduleEdits.Where(job => job.NeedsSave))
            {
                var result = await ScheduledJobService.UpdateAsync(job.Id, job.Minutes);
                if (!result.IsSuccess)
                {
                    Toasts.ShowFailure(result);
                    return;
                }
            }

            Toasts.Success(T[InfernoWebLocalizableStrings.Admin.Saved]);
            Close();
        }
        finally
        {
            saving = false;
        }
    }

    private async Task RunNowAsync(ScheduleEdit job)
    {
        if (saving || !await EnsurePolicyAsync(StandardPolicies.FullAccess))
        {
            return;
        }

        saving = true;
        try
        {
            var updated = await ScheduledJobService.UpdateAsync(job.Id, job.Minutes);
            if (!updated.IsSuccess)
            {
                Toasts.ShowFailure(updated);
                return;
            }

            var ran = await ScheduledJobService.RunNowAsync(job.Id);
            if (!ran.IsSuccess)
            {
                Toasts.ShowFailure(ran);
                return;
            }

            Toasts.Success(T[InfernoWebLocalizableStrings.ScheduledTasks.Started]);
            var listed = await ScheduledJobService.ListAsync();
            if (listed.IsSuccess)
            {
                scheduleEdits = listed.Value.Select(ToEdit).ToList();
            }
        }
        finally
        {
            saving = false;
        }
    }

    private static string ScheduleFieldId(ScheduleEdit job) =>
        "schedule-" + job.Id.Replace(':', '-').Replace('.', '-');

    private static ScheduleEdit ToEdit(ScheduledJobState state) => new()
    {
        Id = state.Id,
        Name = state.Name,
        Minutes = state.Minutes,
        StoredMinutes = state.StoredMinutes,
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

        public int? StoredMinutes { get; init; }

        public bool IsScheduled { get; init; }

        public bool CronIsCustom { get; init; }

        public string Cron { get; init; }

        public DateTime? NextExecutionUtc { get; init; }

        public bool NeedsSave => !IsScheduled || CronIsCustom || StoredMinutes != Minutes;
    }
}
