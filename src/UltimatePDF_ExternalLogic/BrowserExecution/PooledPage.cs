using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;
using PuppeteerSharp;

namespace OutSystems.UltimatePDF_ExternalLogic.BrowserExecution;
public class PooledPage(IPage page, Logger logger) : IDisposable {

    public IPage Page {
        get { return page; }
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
