using System.IO.Compression;
using Microsoft.Extensions.Logging.Abstractions;
using OutSystems.ExternalLibraries.SDK.FileStorage;
using OutSystems.UltimatePDF_ExternalLogic.IntegrationTests.Fixtures;
using OutSystems.UltimatePDF_ExternalLogic.Structures;
using OutSystems.UltimatePDF_ExternalLogic.Test.Helpers;
using Environment = OutSystems.UltimatePDF_ExternalLogic.Structures.Environment;

namespace OutSystems.UltimatePDF_ExternalLogic.IntegrationTests;

/// <summary>
/// Exercises ScreenshotPNG_ToFile against a real browser, with storage faked for the same reason as
/// in <see cref="PrintPDFToFileIntegrationTests"/>.
/// </summary>
[Collection("HelloWorldWeb")]
public class ScreenshotPNGToFileIntegrationTests {

    private readonly HelloWorldWebFixture web;

    public ScreenshotPNGToFileIntegrationTests(HelloWorldWebFixture web) => this.web = web;

    private static UltimatePDF_ExternalLogic NewUltimatePDF(IFileStorage storage) =>
        new UltimatePDF_ExternalLogic(storage, NullLogger.Instance);

    [IntegrationFact]
    public void ScreenshotPNG_ToFile_HelloWorld_StoresValidPngUnderDefaultName() {
        // Arrange
        var storage = new InMemoryFileStorage();
        var ultimatePdf = NewUltimatePDF(storage);

        // Act
        var file = ultimatePdf.ScreenshotPNG_ToFile(
            url: web.BaseUrl,
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Environment(),
            cookies: Array.Empty<Cookie>(),
            paper: new Paper(),
            screenshotOptions: new ScreenshotOptions(),
            timeoutSeconds: 60,
            collectLogs: false,
            attachFilesLogs: false,
            fileName: "",
            logsZipFile: out var logsFile);

        // Assert
        var stored = Assert.Single(storage.Stored);
        Assert.Equal("screenshot.png", stored.FileName);
        Assert.Equal("image/png", stored.ContentType);

        // PNG magic bytes: 0x89 P N G
        Assert.True(stored.Content.Length > 100);
        Assert.Equal(0x89, stored.Content[0]);
        Assert.Equal(0x50, stored.Content[1]);
        Assert.Equal(0x4E, stored.Content[2]);
        Assert.Equal(0x47, stored.Content[3]);

        Assert.Equal("image/png", file.ContentType);
        Assert.Equal(stored.Content.LongLength, file.Size);
        Assert.Empty(logsFile);
    }

    [IntegrationFact]
    public void ScreenshotPNG_ToFile_WithFileName_UsesIt() {
        // Arrange
        var storage = new InMemoryFileStorage();
        var ultimatePdf = NewUltimatePDF(storage);

        // Act
        _ = ultimatePdf.ScreenshotPNG_ToFile(
            url: web.BaseUrl,
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Environment(),
            cookies: Array.Empty<Cookie>(),
            paper: new Paper(),
            screenshotOptions: new ScreenshotOptions(),
            timeoutSeconds: 60,
            collectLogs: false,
            attachFilesLogs: false,
            fileName: "dashboard.png",
            logsZipFile: out _);

        // Assert
        Assert.Equal("dashboard.png", Assert.Single(storage.Stored).FileName);
    }

    [IntegrationFact]
    public void ScreenshotPNG_ToFile_WithDocumentProperties_EmbedsPngMetadataInTheStoredFile() {
        // Arrange
        var storage = new InMemoryFileStorage();
        var ultimatePdf = NewUltimatePDF(storage);
        var options = new ScreenshotOptions {
            DocumentProperties = new DocumentProperties { Title = "Screenshot Test" },
        };

        // Act
        _ = ultimatePdf.ScreenshotPNG_ToFile(
            url: web.BaseUrl,
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Environment(),
            cookies: Array.Empty<Cookie>(),
            paper: new Paper(),
            screenshotOptions: options,
            timeoutSeconds: 60,
            collectLogs: false,
            attachFilesLogs: false,
            fileName: "",
            logsZipFile: out _);

        // Assert — the metadata chunks PNGMetadataUtil writes survive into storage
        var chunks = PngChunkReader.ReadAll(Assert.Single(storage.Stored).Content);
        Assert.Contains(chunks, c => c.Type is "tEXt" or "iTXt");
    }

    [IntegrationFact]
    public void ScreenshotPNG_ToFile_WithCollectLogs_ReturnsLogsAsBinaryDataAndStoresOnlyThePng() {
        // Arrange
        var storage = new InMemoryFileStorage();
        var ultimatePdf = NewUltimatePDF(storage);

        // Act
        _ = ultimatePdf.ScreenshotPNG_ToFile(
            url: web.BaseUrl,
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Environment(),
            cookies: Array.Empty<Cookie>(),
            paper: new Paper(),
            screenshotOptions: new ScreenshotOptions(),
            timeoutSeconds: 60,
            collectLogs: true,
            attachFilesLogs: false,
            fileName: "dashboard.png",
            logsZipFile: out var logsFile);

        // Assert — only the PNG goes to storage; the logs stay Binary Data, as in ScreenshotPNG
        Assert.Equal("dashboard.png", Assert.Single(storage.Stored).FileName);

        Assert.NotEmpty(logsFile);
        using var ms = new MemoryStream(logsFile);
        using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
        Assert.NotEmpty(archive.Entries);
    }
}
