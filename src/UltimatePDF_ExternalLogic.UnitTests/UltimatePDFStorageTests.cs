using Microsoft.Extensions.Logging.Abstractions;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests;

public class UltimatePDFStorageTests {

    [Fact]
    public void Constructor_WithPlatformLogger_DoesNotResolveStorage() {
        // FileStorage.GetInstance resolves a loader the External Library Worker only generates during
        // an ODC build, so it must stay behind a Lazy. Resolving it in the constructor would make the
        // class impossible to construct off-platform and take every other test down with it.
        var entry = new UltimatePDF_ExternalLogic(NullLogger.Instance);

        Assert.NotNull(entry);
    }
}
