using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeadlessChromium.Puppeteer.Lambda.Dotnet;
using Microsoft.Extensions.Logging;
using OutSystems.HeadlessChromium.Puppeteer.BrowserProducts;
using OutSystems.HeadlessChromium.Puppeteer.BrowserRevision;
using PuppeteerSharp;

namespace OutSystems.HeadlessChromium.Puppeteer.BrowserExecution {
    public class BrowserInstancePool : IDisposable {

        // Maximum time a pooled browser instance will be reused until it expires and is recycled
        private const int RECYCLE_WHEN_EXPIRED_SECONDS = 2 * 60 * 60;

        // Maximum time a pooled browser instance will be inactive before it can be recycled
        private const int RECYCLE_WHEN_INACTIVE_SECONDS = 10 * 60;

        // How frequently the pool will be cleaned up to remove recycled browser instances
        private const int CLEANUP_PERIODICITY_SECONDS = 5 * 60;

        // Safe value for the maximum duration of a job
        private const int MAX_JOB_DURATION_SECONDS = 30 * 60;



        private readonly ICollection<PooledBrowserInstance> pool = new List<PooledBrowserInstance>();
        private readonly SemaphoreSlim mutex = new SemaphoreSlim(1, 1);


        private readonly string poolNamespace;
        private bool disposed;



        public BrowserInstancePool(string poolNamespace) {
            this.poolNamespace = poolNamespace;
        }


        public void Dispose() {
            mutex.Wait();
            try {
                disposed = true;

                pool.Clear();
            } finally {
                mutex.Release();
            }
        }




        private async Task<PooledBrowserInstance> NewBrowserInstance(BrowserRevisionManager revisionManager, ILoggerFactory loggerFactory) {
            await mutex.WaitAsync();
            try {

                var browserLauncher = new HeadlessChromiumPuppeteerLauncher(loggerFactory);

                var browser = await browserLauncher.LaunchAsync();
                var instance = new PooledBrowserInstance(revisionManager, browser);
                pool.Add(instance);

                instance.OnJobStarted();

                return instance;

            } finally {
                mutex.Release();
            }
        }

        public async Task<PooledPage> NewPooledPage(BrowserRevisionManager revisionManager, ILoggerFactory loggerFactory) {
            PooledBrowserInstance instance = await NewBrowserInstance(revisionManager, loggerFactory);

            var incognito = await instance.browser.CreateIncognitoBrowserContextAsync();
            IPage page = await incognito.NewPageAsync();
            return new PooledPage(instance, page);
        }

        public async Task<PooledPage> NewPooledPage(BrowserRevisionManager revisionManager, bool forceNewBrowser, bool ignoreHttpsErrors, ILoggerFactory loggerFactory) {
            return await NewPooledPage(revisionManager, loggerFactory);
        }

        private PooledBrowserInstance FindHealthyBrowserInstance(BrowserProduct product, RevisionInfo revision, bool ignoreHttpsErrors) {
            DateTime expiryThreshold = DateTime.UtcNow.AddSeconds(-RECYCLE_WHEN_EXPIRED_SECONDS);

            foreach (var instance in pool) {
                if (instance.Matches(product, revision, ignoreHttpsErrors) && instance.BrowserStarted > expiryThreshold && instance.IsHealthy) {
                    return instance;
                }
            }

            return null;
        }


        public bool DirectoryMatchesNamespace(DirectoryInfo directory) {
            return directory.Name.StartsWith($"{poolNamespace}.");
        }



        public async Task<ICollection<PooledBrowserInstance>> GetSnapshot() {
            await mutex.WaitAsync();
            try {
                return new List<PooledBrowserInstance>(pool);
            } finally {
                mutex.Release();
            }
        }

        public async Task RemoveRecycled(IEnumerable<PooledBrowserInstance> recycled) {
            await mutex.WaitAsync();
            try {
                foreach (var instance in recycled) {
                    pool.Remove(instance);
                }
            } finally {
                mutex.Release();
            }
        }


    }
}
