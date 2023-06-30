using HeadlessChromium.Puppeteer.Lambda.Dotnet;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace OutSystems.HeadlessChromium.Puppeteer.BrowserProducts {

    public abstract class BrowserProduct {

        public abstract Product Product { get; }

        public abstract string ProcessName { get; }

        public abstract string RevisionFolder { get; }


        public bool IsHealthyRevision(RevisionInfo revision) {
            try {
                AssertHealthyRevision(revision);
                return true;
            } catch {
                return false;
            }
        }


        public virtual void AssertHealthyRevision(RevisionInfo revision) {
            if (!File.Exists(revision.ExecutablePath)) {
                throw new FileNotFoundException($"Could not find browser executable at {revision.ExecutablePath}", revision.ExecutablePath);
            }
        }


        public Task<IBrowser> LaunchAsync(RevisionInfo revision, string userDataDir, bool ignoreHttpsErrors, ILoggerFactory loggerFactory) {
            var browserLauncher = new HeadlessChromiumPuppeteerLauncher(loggerFactory);
            return browserLauncher.LaunchAsync();
        }

        public virtual NavigationOptions DefaultNavigationOptions() {
            return new NavigationOptions() {
                Timeout = 0,
                WaitUntil = new WaitUntilNavigation[] {
                            WaitUntilNavigation.DOMContentLoaded,
                            WaitUntilNavigation.Load,
                            WaitUntilNavigation.Networkidle0
                }
            };
        }

        protected virtual string[] DefaultBrowserArgs() {
            return new string[] {
            };
        }
    }

}
