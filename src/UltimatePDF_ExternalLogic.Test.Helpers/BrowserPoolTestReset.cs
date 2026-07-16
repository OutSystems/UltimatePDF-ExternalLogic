using OutSystems.UltimatePDF_ExternalLogic.BrowserExecution;

namespace OutSystems.UltimatePDF_ExternalLogic.Test.Helpers;

public static class BrowserPoolTestReset {
    public static void Reset() => BrowserInstancePool.ResetForTests();
}
