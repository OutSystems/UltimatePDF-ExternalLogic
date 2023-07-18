namespace OutSystems.UltimatePDF_ExternalLogic.Utils {
    public static class AsyncUtils {

        /* Runs async code with default scheduler and waits for the result */
        public static T StartAndWait<T>(Func<Task<T>> @async) {
            TaskFactory tf = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);
            Task<Task<T>> task = tf.StartNew(@async);
            return task.Unwrap().GetAwaiter().GetResult();
        }


    }
}
