using System;
using PuppeteerSharp;

namespace OutSystems.UltimatePDF_ExternalLogic.BrowserExecution {
    public class PooledBrowserInstance {

        public readonly IBrowser browser;

        public bool IsHealthy {
            get { return !browser.Process.HasExited; }
        }

        public PooledBrowserInstance(IBrowser browser) {
            this.browser = browser;
        }
    }
}
