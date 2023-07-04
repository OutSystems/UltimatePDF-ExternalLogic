using HeadlessChromium.Puppeteer.Lambda.Dotnet;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace OutSystems.ODC_UltimatePDF_Service.BrowserExecution {
    public class BrowserInstancePool {
        private readonly ICollection<PooledBrowserInstance> pool = new List<PooledBrowserInstance>();
        private readonly SemaphoreSlim mutex = new SemaphoreSlim(1, 1);

        private async Task<PooledBrowserInstance> NewBrowserInstance(ILoggerFactory loggerFactory) {
            var browserLauncher = new HeadlessChromiumPuppeteerLauncher(loggerFactory);

            var browser = await browserLauncher.LaunchAsync();
            var instance = new PooledBrowserInstance(browser);
            pool.Add(instance);

            instance.OnJobStarted();

            return instance;
        }

        public async Task<PooledPage> NewPooledPage(ILoggerFactory loggerFactory) {
            PooledBrowserInstance instance = await NewBrowserInstance(loggerFactory);

            var incognito = await instance.browser.CreateIncognitoBrowserContextAsync();
            IPage page = await incognito.NewPageAsync();
            return new PooledPage(instance, page);
        }
    }
}
