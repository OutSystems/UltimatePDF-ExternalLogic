using OutSystems.UltimatePDF_ExternalLogic.Test.Helpers;

namespace OutSystems.UltimatePDF_ExternalLogic.IntegrationTests.Fixtures;

public sealed class BrowserPoolResetFixture {
    public BrowserPoolResetFixture() => BrowserPoolTestReset.Reset();
}
