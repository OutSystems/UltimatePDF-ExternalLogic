using System.Diagnostics;

namespace OutSystems.UltimatePDF_ExternalLogic.Utils;
/*
 * Deliberately uses TaskFactory(..., TaskScheduler.Default) rather than Task.Run: this guarantees the
 * delegate never inherits the caller's SynchronizationContext, so GetAwaiter().GetResult() below can't
 * deadlock waiting on a continuation that needs that same context back. Task.Run also always targets
 * TaskScheduler.Default and would likely behave identically, but that depends on the ODC host having
 * no ambient SynchronizationContext of its own - unconfirmed from this repo alone - so this stays as
 * the more defensive, already-proven form rather than risk a hidden deadlock for a cosmetic simplification.
 */
public static class AsyncUtils {

    /* Runs async code with default scheduler and waits for the result */
    public static T StartAndWait<T>(Func<Task<T>> @async) {
        using var activity = Activity.Current?.Source.StartActivity("AsyncUtils.StartAndWait");
        var tf = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);
        Task<Task<T>> task = tf.StartNew(@async);
        return task.Unwrap().GetAwaiter().GetResult();
    }

    /* Runs async code with default scheduler and waits for the result */
    public static void StartAndWait(Func<Task> @async) {
        using var activity = Activity.Current?.Source.StartActivity("AsyncUtils.StartAndWait");
        var tf = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);
        Task<Task> task = tf.StartNew(@async);
        task.Unwrap().GetAwaiter().GetResult();
    }
}
