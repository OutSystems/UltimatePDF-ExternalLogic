using System.Threading.Tasks;
using PuppeteerSharp;

namespace UltimatePDF_ExternalLogic.Cleanup {
    internal class BrowserInstanceCleanup : AbstractCleanupTask {
        private const int BROWSER_CLOSE_TIMEOUT_MS = 10 * 1000;

        private readonly IBrowser browser;

        public BrowserInstanceCleanup(IBrowser browser) {
            this.browser = browser;
        }


        public override async Task Cleanup() {
            try {
                await browser.CloseAsync();
                browser.Process.WaitForExit(BROWSER_CLOSE_TIMEOUT_MS);
            } catch {
            }
        }

    }
}
