using System;
using System.Threading;
using System.Threading.Tasks;

namespace UltimatePDF_ExternalLogic.Cleanup {
    internal class BrowserInstanceOrphanBackgroundCleanup : AbstractCleanupTask {

        private readonly int periodicity;
        private readonly CancellationTokenSource _cts = new();

        public BrowserInstanceOrphanBackgroundCleanup(int periodicitySeconds) {
            this.periodicity = periodicitySeconds;
        }

        public void Stop() {
            _cts.Cancel();
        }

        public override async Task Cleanup() {
            try {
                while (true) {
                    await Task.Delay(TimeSpan.FromSeconds(periodicity), _cts.Token);
                }
            } catch (OperationCanceledException) { }
        }
    }
}
