using System.Diagnostics;
using System.Threading.Tasks;
using OutSystems.UltimatePDF_ExternalLogic.Utils;

namespace UltimatePDF_ExternalLogic.Cleanup;
internal abstract class AbstractCleanupTask {
    public void StartInBackground() {
        using var activity = Activity.Current?.Source.StartActivity("AbstractCleanupTask.StartInBackground");
        Task.Run(Cleanup);
    }

    public void StartAndWait() {
        using var activity = Activity.Current?.Source.StartActivity("AbstractCleanupTask.StartAndWait");
        AsyncUtils.StartAndWait(Cleanup);
    }

    public abstract Task Cleanup();
}
