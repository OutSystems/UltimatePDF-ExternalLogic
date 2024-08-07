﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HeadlessChromium.Puppeteer.Lambda.Dotnet;
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
            var instance = await NewBrowserInstance(logger);
            var page = await instance.Browser.NewPageAsync();
            var pooledPage = new PooledPage(page, logger);
            return pooledPage;
        }

        private static void RegisterPageEventHandlers(Logger logger, IPage page) {
            RegisterPageErrorHandler(logger, page);
            RegisterErrorHandler(logger, page);
            RegisterConsoleHandler(logger, page);
        }

        private static void RegisterResponseHandler(Logger logger, IPage page) {
            page.Response += (sender, e) => {
                var filename = UrlUtils.GetFilenameFromUrl(e.Response.Url);
                logger.Log($"Got filename {filename}");
            };
        }

        private static void RegisterConsoleHandler(Logger logger, IPage page) {
            page.Console += (sender, e) => {
                logger.Log($"Console - ");
                for (var i = 0; i < e.Message.Args.Count; i++) {
                    logger.Log($"\t[{i}]: {e.Message.Args[i]}");
                }
            };
        }

        private static void RegisterErrorHandler(Logger logger, IPage page) {
            page.Error += (o, e) => {
                logger.Error($"Error Event - {e.Error}");
            };
        }

        private static void RegisterPageErrorHandler(Logger logger, IPage page) {
            page.PageError += (o, e) => {
                logger.Error($"Page Error Event - {e.Message}");
            };
        }
    }
}
