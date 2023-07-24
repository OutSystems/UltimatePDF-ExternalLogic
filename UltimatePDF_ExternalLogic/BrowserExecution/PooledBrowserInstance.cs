using System;
using PuppeteerSharp;

namespace OutSystems.UltimatePDF_ExternalLogic.BrowserExecution {
    public class PooledBrowserInstance {

        public readonly IBrowser browser;

        private DateTime? lastJobStarted;
        private DateTime? lastJobFinished;
        private TimeSpan? minJobDuration = null;
        private TimeSpan? maxJobDuration = null;
        private TimeSpan totalJobDuration = TimeSpan.Zero;


        public PooledBrowserInstance(IBrowser browser) {
            this.browser = browser;
        }

        public void OnJobStarted() {
            this.lastJobStarted = DateTime.UtcNow;
            this.lastJobFinished = null;
        }

        public void OnJobFinished() {
            lastJobFinished = DateTime.UtcNow;

            var duration = lastJobFinished.Value.Subtract(lastJobStarted ?? DateTime.UtcNow);
            if (!minJobDuration.HasValue || duration < minJobDuration.Value) {
                minJobDuration = duration;
            }
            if (!maxJobDuration.HasValue || duration > maxJobDuration.Value) {
                maxJobDuration = duration;
            }
            totalJobDuration = totalJobDuration.Add(duration);
        }
    }
}
