namespace Inferno.Tasks;

/// <summary>
/// A background job. Hangfire runs one recurring job per implementation and calls <see cref="Execute"/>
/// inside a dependency-injection scope. The job is registered once, for the whole site: an implementation
/// that needs each tenant, such as queued mail, walks the tenants itself.
/// </summary>
public partial interface ITask
{
    string Name { get; }

    /// <summary>
    /// How often the job should run, in seconds, the first time it is registered.
    /// Hangfire's smallest step is one minute, so a smaller value runs every minute.
    /// The host administrator can change it later on the settings page, and that change is kept.
    /// </summary>
    int DefaultInterval { get; }

    void Execute();
}
