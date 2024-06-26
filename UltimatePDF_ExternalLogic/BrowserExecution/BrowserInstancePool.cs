﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HeadlessChromium.Puppeteer.Lambda.Dotnet;
using Microsoft.Extensions.Logging;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;
using PuppeteerSharp;

namespace OutSystems.UltimatePDF_ExternalLogic.BrowserExecution {
    public class BrowserInstancePool {

        private readonly SemaphoreSlim mutex = new(1, 1);

        private static readonly List<PooledBrowserInstance> pool = new();

        public BrowserInstancePool() {
        }

        private async Task<PooledBrowserInstance> NewBrowserInstance(Logger logger) {
            await mutex.WaitAsync();
            try {
                var instance = pool.FirstOrDefault(i => i.IsHealthy);

                if(instance == null) {
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
            var instance = await NewBrowserInstance(logger);
            var page = await instance.Browser.NewPageAsync();
            return new PooledPage(page, logger);
        }
    }
}
