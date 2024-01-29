using System;
using System.Diagnostics.SymbolStore;
using System.Threading.Tasks;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;
using PuppeteerSharp;

namespace OutSystems.UltimatePDF_ExternalLogic.BrowserExecution {
    public class PooledPage : IDisposable {

        private readonly Logger logger;
        private readonly IPage page;

        public IPage Page {
            get { return page; }
        }

        public PooledPage(IPage page, Logger logger) {
            this.page = page;
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
