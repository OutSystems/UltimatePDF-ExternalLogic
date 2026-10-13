using Microsoft.Extensions.Logging.Abstractions;
using OutSystems.ExternalLibraries.SDK;
using OutSystems.UltimatePDF_ExternalLogic.Structures;
using OutSystems.UltimatePDF_ExternalLogic.Test.Helpers;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests;

/// <summary>
/// Covers LogOptions, which owns both the logger construction and the storing of the logs ZIP for
/// the File actions. The storage call is captured through the delegate the type takes, so none of
/// this needs a browser or real file storage.
/// </summary>
public class LogOptionsTests {

    private readonly List<(string Name, string ContentType, byte[] Content)> stored = new();

    private IOSFile Store(string name, string contentType, byte[] content) {
        stored.Add((name, contentType, content));

        return new FakeOSFile { ContentType = contentType, Size = content.LongLength };
    }

    [Fact]
    public void StoreZip_LogsNotCollected_LeavesTheFileNullAndStoresNothing() {
        // A disabled logger produces an empty ZIP. Storing it would leave a useless 0 byte file in
        // storage and hand back a reference to it, so nothing should be written at all.
        var options = new LogOptions { CollectLogs = false, AttachFilesLogs = false };
        var logger = options.CreateLogger(NullLogger.Instance);

        var result = LogOptions.StoreZip(logger, "document.pdf", Store);

        Assert.Null(result.LogsZipFile);
        Assert.Empty(stored);
    }

    [Fact]
    public void StoreZip_LogsCollected_StoresZipNamedAfterTheArtifact() {
        var options = new LogOptions { CollectLogs = true, AttachFilesLogs = false };
        var logger = options.CreateLogger(NullLogger.Instance);
        logger.Log("something worth keeping");

        var result = LogOptions.StoreZip(logger, "invoice.pdf", Store);

        Assert.NotNull(result.LogsZipFile);

        var write = Assert.Single(stored);
        Assert.Equal("invoice-logs.zip", write.Name);
        Assert.Equal("application/zip", write.ContentType);
        Assert.NotEmpty(write.Content);
    }
}
