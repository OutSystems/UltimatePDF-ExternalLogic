using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HeadlessChromium.Puppeteer.Lambda.Dotnet;
using Microsoft.Extensions.Logging;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;
using PuppeteerSharp;
using UltimatePDF_ExternalLogic.Utils;

namespace OutSystems.UltimatePDF_ExternalLogic.BrowserExecution {
    public class BrowserInstancePool {

        private readonly SemaphoreSlim mutex = new(1, 1);

        private static readonly List<PooledBrowserInstance> pool = new();

        public BrowserInstancePool() {
        }

        private async Task<PooledBrowserInstance> NewBrowserInstance(Logger logger) {
            await mutex.WaitAsync();
            try {
                pool.RemoveAll(i => !i.IsHealthy);
                var instance = pool.FirstOrDefault();
                var requestLog = logger.GetLoggerFactory("request.txt").CreateLogger("BrowserInstancePool");

                if (instance == null) {
                    requestLog.LogInformation("Creating new Browser Instance");
                    logger.Log("Create new Browser Instance");

                    var browserLauncher = new HeadlessChromiumPuppeteerLauncher(logger.GetLoggerFactory("browser.txt"));
                    var browser = await browserLauncher.LaunchAsync();
                    instance = new PooledBrowserInstance(browser);
                    pool.Add(instance);
                } else {
                    requestLog.LogInformation("Reusing existing Browser Instance");
                }

                return instance;
            } finally {
                mutex.Release();
            }
        }

        public async Task<PooledPage> NewPooledPage(Logger logger) {
            var instance = await NewBrowserInstance(logger);
            var page = await instance.Browser.NewPageAsync();
            var pooledPage = new PooledPage(page, instance, logger);
            return pooledPage;
        }

        internal static void ResetForTests() {
            pool.Clear();
        }
    }
}
