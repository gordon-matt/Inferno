namespace Inferno.Threading;

// Taken from:
// http://www.symbolsource.org/MyGet/Metadata/aspnetwebstacknightly/Project/Microsoft.AspNet.Identity.Core/2.0.0-beta1-140203/Release/Default/Microsoft.AspNet.Identity.Core/Microsoft.AspNet.Identity.Core/AsyncHelper.cs?ImageName=Microsoft.AspNet.Identity.Core
/// <summary>
/// Runs an asynchronous method to completion on the default task scheduler.
/// The call blocks the current thread. Exceptions from the task propagate to the caller.
/// </summary>
public static class AsyncHelper
{
    private static readonly TaskFactory taskFactory = new(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

    /// <summary>Runs <paramref name="func"/> to completion and returns its result.</summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="func">The asynchronous method. Must not be <see langword="null"/>.</param>
    /// <returns>The task's result.</returns>
    public static TResult RunSync<TResult>(Func<Task<TResult>> func) => taskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();

    /// <summary>Runs <paramref name="func"/> to completion.</summary>
    /// <param name="func">The asynchronous method. Must not be <see langword="null"/>.</param>
    public static void RunSync(Func<Task> func) => taskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();
}
