using System.Diagnostics;
using PuppeteerSharp;

namespace UltimatePDF_ExternalLogic.Cleanup;
internal class BrowserInstanceCleanup(IBrowser browser) : AbstractCleanupTask {
    private const int BROWSER_CLOSE_TIMEOUT_MS = 10 * 1000;

    public override async Task Cleanup() {
        using var activity = Activity.Current?.Source.StartActivity("BrowserInstanceCleanup.Cleanup");
        try {
            await browser.CloseAsync();
            browser.Process.WaitForExit(BROWSER_CLOSE_TIMEOUT_MS);
        } catch {
        }
    }

}
