using System;
using System.Threading.Tasks;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;
using PuppeteerSharp;

namespace OutSystems.UltimatePDF_ExternalLogic.BrowserExecution {
    public class PooledPage : IDisposable {

        private readonly Logger logger;
        private readonly IPage page;
        private readonly PooledBrowserInstance instance;

        public IPage Page {
            get { return page; }
        }

        public PooledBrowserInstance Instance {
            get { return instance; }
        }

        public PooledPage(IPage page, PooledBrowserInstance instance, Logger logger) {
            this.page = page;
            this.instance = instance;
            this.logger = logger;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                logger.Log("Closing page");
                Task.Run(() => page.CloseAsync());
            }
        }
    }
}
