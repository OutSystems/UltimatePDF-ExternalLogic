using System.Threading.Tasks;
using PuppeteerSharp;

namespace OutSystems.UltimatePDF_ExternalLogic.BrowserExecution {
    public class PooledBrowserInstance {

        private readonly IBrowser browser;
        private volatile bool closing;

        public IBrowser Browser { get { return browser; } }

        public bool IsHealthy {
            get { return !closing && !browser.Process.HasExited; }
        }

        public PooledBrowserInstance(IBrowser browser) {
            this.browser = browser;
        }

        public async Task CloseAsync() {
            closing = true;
            await browser.CloseAsync();
        }
    }
}
