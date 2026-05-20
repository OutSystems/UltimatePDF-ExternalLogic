using System.Threading.Tasks;
using OutSystems.UltimatePDF_ExternalLogic.Utils;

namespace UltimatePDF_ExternalLogic.Cleanup {
    internal abstract class AbstractCleanupTask {
        public void StartInBackground() {
            Task.Run(Cleanup);
        }

        public void StartAndWait() {
            AsyncUtils.StartAndWait(Cleanup);
        }

        public abstract Task Cleanup();
    }
}
