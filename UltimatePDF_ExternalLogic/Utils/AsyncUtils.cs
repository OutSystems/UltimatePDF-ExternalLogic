using System;
using System.Threading;
using System.Threading.Tasks;

namespace OutSystems.UltimatePDF_ExternalLogic.Utils {
    public static class AsyncUtils {

        /* Runs async code with default scheduler and waits for the result */
        public static T StartAndWait<T>(Func<Task<T>> @async) {
            TaskFactory tf = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);
            Task<Task<T>> task = tf.StartNew(@async);
            return task.Unwrap().GetAwaiter().GetResult();
        }

        /* Runs async code with default scheduler and waits for the result */
        public static void StartAndWait(Func<Task> @async) {
            TaskFactory tf = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);
            Task<Task> task = tf.StartNew(@async);
            task.Unwrap().GetAwaiter().GetResult();
        }
    }
}
