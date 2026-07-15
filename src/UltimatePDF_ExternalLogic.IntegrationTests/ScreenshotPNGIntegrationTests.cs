using System.IO.Compression;
using Microsoft.Extensions.Logging.Abstractions;
using OutSystems.UltimatePDF_ExternalLogic.IntegrationTests.Fixtures;
using OutSystems.UltimatePDF_ExternalLogic.Structures;
using OutSystems.UltimatePDF_ExternalLogic.Test.Helpers;
using Environment = OutSystems.UltimatePDF_ExternalLogic.Structures.Environment;

namespace OutSystems.UltimatePDF_ExternalLogic.IntegrationTests;

[Collection("HelloWorldWeb")]
public class ScreenshotPNGIntegrationTests {

    private readonly HelloWorldWebFixture web;

    public ScreenshotPNGIntegrationTests(HelloWorldWebFixture web) => this.web = web;

    private static UltimatePDF_ExternalLogic NewUltimatePDF() =>
        new UltimatePDF_ExternalLogic(NullLogger.Instance);

    [IntegrationFact]
    public void ScreenshotPNG_HelloWorld_ReturnsPngBytes() {
        // Arrange
        var ultiamtePdf = NewUltimatePDF();

        // Act
        var png = ultiamtePdf.ScreenshotPNG(
            url: web.BaseUrl,
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Environment(),
            cookies: Array.Empty<Cookie>(),
            paper: new Paper(),
            screenshotOptions: new ScreenshotOptions(),
            timeoutSeconds: 60,
            collectLogs: false,
            attachFilesLogs: false,
            logsZipFile: out _);

        // Assert — PNG magic bytes: 0x89 P N G
        Assert.NotNull(png);
        Assert.True(png.Length > 100);
        Assert.Equal(0x89, png[0]);
        Assert.Equal(0x50, png[1]); // P
        Assert.Equal(0x4E, png[2]); // N
        Assert.Equal(0x47, png[3]); // G
    }

    [IntegrationFact]
    public void ScreenshotPNG_WithDocumentProperties_EmbedsPngMetadata() {
        // Arrange
        var ultiamtePdf = NewUltimatePDF();
        var props = new DocumentProperties { Title = "Screenshot Test" };
        var options = new ScreenshotOptions { DocumentProperties = props };

        // Act
        var png = ultiamtePdf.ScreenshotPNG(
            url: web.BaseUrl,
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Environment(),
            cookies: Array.Empty<Cookie>(),
            paper: new Paper(),
            screenshotOptions: options,
            timeoutSeconds: 60,
            collectLogs: false,
            attachFilesLogs: false,
            logsZipFile: out _);

        // Assert — PNG file is valid and contains a tEXt/iTXt chunk with Title=="Screenshot Test"
        Assert.NotNull(png);
        Assert.True(png.Length > 100);
        Assert.Equal(0x89, png[0]);
        var chunks = PngChunkReader.ReadAll(png);
        var titleChunk = chunks.First(c => c.Type is "tEXt" or "iTXt");
        var (keyword, value) = titleChunk.Type == "tEXt"
            ? PngChunkReader.DecodeTextChunk(titleChunk)
            : PngChunkReader.DecodeITextChunk(titleChunk);
        Assert.Equal("Title", keyword);
        Assert.Equal("Screenshot Test", value);
    }
    [IntegrationFact]
    public void ScreenshotPNG_WithCollectLogs_ReturnsNonEmptyZip() {
        // Arrange
        var ultiamtePdf = NewUltimatePDF();

        // Act
        _ = ultiamtePdf.ScreenshotPNG(
            url: web.BaseUrl,
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Environment(),
            cookies: Array.Empty<Cookie>(),
            paper: new Paper(),
            screenshotOptions: new ScreenshotOptions(),
            timeoutSeconds: 60,
            collectLogs: true,
            attachFilesLogs: false,
            logsZipFile: out var logsZip);

        // Assert
        Assert.NotNull(logsZip);
        Assert.True(logsZip.Length > 0);
        using var ms = new MemoryStream(logsZip);
        using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
        Assert.NotEmpty(archive.Entries);
    }

    [IntegrationFact]
    public void ScreenshotPNG_WithAttachFilesLogs_LogsZipContainsInputAndOutput() {
        // Arrange
        var ultiamtePdf = NewUltimatePDF();

        // Act
        _ = ultiamtePdf.ScreenshotPNG(
            url: web.BaseUrl,
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Environment(),
            cookies: Array.Empty<Cookie>(),
            paper: new Paper(),
            screenshotOptions: new ScreenshotOptions(),
            timeoutSeconds: 60,
            collectLogs: true,
            attachFilesLogs: true,
            logsZipFile: out var logsZip);

        // Assert
        Assert.NotNull(logsZip);
        ZipAssert.ContainsEntry(logsZip, "input.html");
        ZipAssert.ContainsEntry(logsZip, "output.png");
    }
}
