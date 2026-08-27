using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HeadlessChromium.Puppeteer.Lambda.Dotnet;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;
using PuppeteerSharp;
using UltimatePDF_ExternalLogic.Utils;

namespace OutSystems.UltimatePDF_ExternalLogic.BrowserExecution;
public class BrowserInstancePool {

    private readonly SemaphoreSlim mutex = new(1, 1);

    private static readonly List<PooledBrowserInstance> pool = new();

    public BrowserInstancePool() {
    }

    private async Task<PooledBrowserInstance> NewBrowserInstance(Logger logger) {
        using var activity = Activity.Current?.Source.StartActivity("BrowserInstancePool.NewBrowserInstance");
        await mutex.WaitAsync();
        try {
            var instance = pool.FirstOrDefault(i => i.IsHealthy);

            if (instance == null) {
                logger.Log("Create new Browser Instance");

                var browserLauncher = new HeadlessChromiumPuppeteerLauncher(logger.GetLoggerFactory("browser.txt"));
                var browser = await browserLauncher.LaunchAsync();
                instance = new PooledBrowserInstance(browser);
                pool.Add(instance);
            }

            return instance;
        } finally {
            mutex.Release();
        }
    }

    public async Task<PooledPage> NewPooledPage(Logger logger) {
        using var activity = Activity.Current?.Source.StartActivity("BrowserInstancePool.NewPooledPage");
        var instance = await NewBrowserInstance(logger);
        var page = await instance.Browser.NewPageAsync();
        var pooledPage = new PooledPage(page, logger);
        return pooledPage;
    }
}
