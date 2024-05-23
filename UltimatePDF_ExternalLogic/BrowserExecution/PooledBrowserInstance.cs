using System;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace OutSystems.UltimatePDF_ExternalLogic.BrowserExecution {
    public class PooledBrowserInstance {

        private readonly IBrowser browser;

        public IBrowser Browser { get { return browser; } }

        public bool IsHealthy {
            get { return !browser.Process.HasExited; }
        }

        public PooledBrowserInstance(IBrowser browser) {
            this.browser = browser;
        }
    }
}
