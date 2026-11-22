namespace Inferno.Tasks.Scheduling;

/// <summary>Identifiers for the recurring jobs Inferno registers with Hangfire.</summary>
public static class ScheduledJobs
{
    /// <summary>Where the Hangfire dashboard is mounted. The admin menu opens it in a new tab.</summary>
    public const string DashboardPath = "/hangfire";

    /// <summary>Marks recurring jobs that Inferno registered, so a removed task can be dropped without touching other jobs.</summary>
    public const string JobIdPrefix = "inferno:";

    /// <summary>The Hangfire job id for <paramref name="taskType"/>.</summary>
    /// <param name="taskType">The <see cref="ITask"/> implementation. Must not be <see langword="null"/>.</param>
    /// <returns><see cref="JobIdPrefix"/> followed by the type's full name.</returns>
    public static string JobId(Type taskType) => JobIdPrefix + taskType.FullName;

    /// <summary>Version-less, so a rebuilt assembly still resolves. The assembly has to be loaded.</summary>
    /// <param name="taskType">The <see cref="ITask"/> implementation. Must not be <see langword="null"/>.</param>
    /// <returns>The type's full name, a comma, and the assembly's simple name.</returns>
    public static string TypeKey(Type taskType) => taskType.FullName + ", " + taskType.Assembly.GetName().Name;
}
