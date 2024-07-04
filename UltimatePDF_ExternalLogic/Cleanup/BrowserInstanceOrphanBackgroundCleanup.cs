using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using OutSystems.UltimatePDF_ExternalLogic.BrowserExecution;

namespace UltimatePDF_ExternalLogic.Cleanup {
    internal class BrowserInstanceOrphanBackgroundCleanup : AbstractCleanupTask {

        private readonly int periodicity;
        private bool stopped;


        public BrowserInstanceOrphanBackgroundCleanup(int periodicitySeconds) {
            this.periodicity = periodicitySeconds;
        }

        public void Stop() {
            stopped = true;
        }

        private bool Expired(Process process, DateTime expiryThreshold) {
            try {
                return process.StartTime < expiryThreshold;
            } catch {
                return true;
            }
        }

        private bool Contains(ICollection<PooledBrowserInstance> snapshot, Process process) {
            foreach (var instance in snapshot) {
                if (instance.Browser.Process.Id == process.Id) {
                    return true;
                }
            }

            return false;
        }

        public override async Task Cleanup() {
            while (!stopped) {
                await Task.Delay(TimeSpan.FromSeconds(periodicity));
            }
        }
    }
}
