using PuppeteerSharp;

namespace OutSystems.UltimatePDF_ExternalLogic.BrowserExecution;
public class PooledBrowserInstance(IBrowser browser) {

    public IBrowser Browser { get { return browser; } }

    public bool IsHealthy {
        get { return !browser.Process.HasExited; }
    }
}
