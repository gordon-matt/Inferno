using Dependo;
using Inferno.Tasks;

namespace Inferno.Caching;

/// <summary>
/// Clears the application cache. The cache is process-wide, so this is a host job rather than a per-tenant one.
/// </summary>
public class ClearCacheTask : ITask
{
    public string Name => "Clear Cache Task";

    public int DefaultInterval => 600;

    public void Execute() => DependoResolver.Instance.Resolve<ICacheManager>().Clear();
}