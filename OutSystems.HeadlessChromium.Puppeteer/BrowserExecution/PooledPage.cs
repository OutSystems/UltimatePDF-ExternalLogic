using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace OutSystems.HeadlessChromium.Puppeteer.BrowserExecution {
    public class PooledPage : IDisposable {

        private readonly PooledBrowserInstance instance;
        public readonly IPage Page;


        public PooledPage(PooledBrowserInstance instance, IPage page) {
            this.instance = instance;
            this.Page = page;
        }


        public void Dispose() {
            this.instance.OnJobFinished();

            Task.Run(Page.BrowserContext.CloseAsync);
        }
    }
}
