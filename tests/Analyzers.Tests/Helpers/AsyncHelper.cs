namespace Analyzers.Tests.Helpers;

/// <summary>
/// Helper to execute async methods synchronously when it truly cannot be avoided.
/// Strips the <see cref="SynchronizationContext"/> to prevent deadlocks,
/// and uses <c>GetAwaiter().GetResult()</c> to unwrap exceptions cleanly.
/// </summary>
internal static class AsyncHelper
{
    /// <summary>
    /// Executes an async <see cref="Task"/> method synchronously.
    /// </summary>
    public static void RunSync(Func<Task> func)
    {
        var previousContext = SynchronizationContext.Current;
        try
        {
            SynchronizationContext.SetSynchronizationContext(null);
            func().GetAwaiter().GetResult();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previousContext);
        }
    }

    /// <summary>
    /// Executes an async <see cref="Task{TResult}"/> method synchronously.
    /// </summary>
    public static TResult RunSync<TResult>(Func<Task<TResult>> func)
    {
        var previousContext = SynchronizationContext.Current;
        try
        {
            SynchronizationContext.SetSynchronizationContext(null);
            return func().GetAwaiter().GetResult();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previousContext);
        }
    }
}
