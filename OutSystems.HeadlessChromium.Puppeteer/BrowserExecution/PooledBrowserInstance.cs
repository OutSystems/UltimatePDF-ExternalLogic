using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutSystems.HeadlessChromium.Puppeteer.BrowserProducts;
using OutSystems.HeadlessChromium.Puppeteer.BrowserRevision;
using PuppeteerSharp;

namespace OutSystems.HeadlessChromium.Puppeteer.BrowserExecution {
    public class PooledBrowserInstance {

        public readonly BrowserRevisionManager revisionManager;
        public readonly BrowserProduct product;
        public readonly RevisionInfo revision;
        public readonly IBrowser browser;
        public readonly string userDataDir;


        private readonly DateTime browserStarted;
        private int jobCount;
        private DateTime? lastJobStarted;
        private DateTime? lastJobFinished;
        private TimeSpan? minJobDuration = null;
        private TimeSpan? maxJobDuration = null;
        private TimeSpan totalJobDuration = TimeSpan.Zero;


        public PooledBrowserInstance(BrowserRevisionManager revisionManager, IBrowser browser) {
            this.revisionManager = revisionManager;
            this.browser = browser;
            this.browserStarted = DateTime.UtcNow;
        }


        public bool Matches(BrowserProduct product, RevisionInfo revision, bool ignoreHttpsErrors) {
            return this.product.Product == product.Product &&
                this.revision.Revision == revision.Revision &&
                this.browser.IgnoreHTTPSErrors == ignoreHttpsErrors;
        }


        public void OnJobStarted() {
            this.jobCount++;
            this.lastJobStarted = DateTime.UtcNow;
            this.lastJobFinished = null;
        }

        public void OnJobFinished() {
            lastJobFinished = DateTime.UtcNow;

            var duration = lastJobFinished.Value.Subtract(lastJobStarted.Value);
            if (!minJobDuration.HasValue || duration < minJobDuration.Value) {
                minJobDuration = duration;
            }
            if (!maxJobDuration.HasValue || duration > maxJobDuration.Value) {
                maxJobDuration = duration;
            }
            totalJobDuration = totalJobDuration.Add(duration);
        }




        public DateTime BrowserStarted {
            get { return browserStarted; }
        }


        public int JobCount {
            get { return jobCount; }
        }

        public DateTime? LastJobStarted {
            get { return lastJobStarted; }
        }

        public DateTime? LastJobFinished {
            get { return lastJobFinished; }
        }

        public TimeSpan LastJobDuration {
            get {
                if (this.lastJobStarted != null) {
                    var finished = this.lastJobFinished ?? DateTime.UtcNow;
                    return finished.Subtract(lastJobStarted.Value);
                } else {
                    return TimeSpan.Zero;
                }
            }
        }

        public TimeSpan MinJobDuration {
            get { return minJobDuration ?? LastJobDuration; }
        }

        public TimeSpan MaxJobDuration {
            get { return maxJobDuration ?? LastJobDuration; }
        }

        public TimeSpan AverageJobDuration {
            get {
                if (jobCount == 0) {
                    return LastJobDuration;
                } else {
                    return TimeSpan.FromSeconds(totalJobDuration.TotalSeconds / jobCount);
                }
            }
        }


        public bool IsHealthy {
            get {
                return !browser.Process.HasExited &&
                    revisionManager.IsHealthyRevision(revision);
            }
        }

    }
}
