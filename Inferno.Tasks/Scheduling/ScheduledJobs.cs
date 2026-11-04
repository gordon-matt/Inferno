namespace Inferno.Tasks.Scheduling;

public static class ScheduledJobs
{
    /// <summary>Where the Hangfire dashboard is mounted. The admin menu opens it in a new tab.</summary>
    public const string DashboardPath = "/hangfire";

    /// <summary>Marks recurring jobs that Inferno registered, so a removed task can be dropped without touching other jobs.</summary>
    public const string JobIdPrefix = "inferno:";

    public static string JobId(Type taskType) => JobIdPrefix + taskType.FullName;

    /// <summary>Version-less, so a rebuilt assembly still resolves. The assembly has to be loaded.</summary>
    public static string TypeKey(Type taskType) => taskType.FullName + ", " + taskType.Assembly.GetName().Name;
}
