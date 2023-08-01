using System;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace OutSystems.UltimatePDF_ExternalLogic.BrowserExecution {
    public class PooledPage : IDisposable {

        private readonly PooledBrowserInstance instance;
        public readonly IPage Page;


        public PooledPage(PooledBrowserInstance instance, IPage page) {
            this.instance = instance;
            this.Page = page;
        }


        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this.instance.OnJobFinished();

                Task.Run(Page.BrowserContext.CloseAsync);
            }
        }
    }
}
