using Microsoft.Extensions.Logging.Abstractions;
using OutSystems.ExternalLibraries.SDK.FileStorage;
using OutSystems.UltimatePDF_ExternalLogic.Structures;
using OutSystems.UltimatePDF_ExternalLogic.Test.Helpers;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests;

/// <summary>
/// Wiring-level tests that exercise the entry points of UltimatePDF_ExternalLogic
/// without spinning up PuppeteerSharp.
/// </summary>
public class UltimatePDFWiringTests {

    private static UltimatePDF_ExternalLogic NewUltimatePDF() =>
        new UltimatePDF_ExternalLogic(NullLogger.Instance);

    private static UltimatePDF_ExternalLogic NewUltimatePDF(IFileStorage storage) =>
        new UltimatePDF_ExternalLogic(storage, NullLogger.Instance);

    [Fact]
    public void PrintPDF_InvalidHttpsUrl_PropagatesUriFormatException() {
        // Arrange — the URL fails UrlUtils.IsValidHttpsUri, which throws UriFormatException
        // before PuppeteerSharp is ever invoked, so we can exercise the public entry point
        // without a real browser.
        var entry = NewUltimatePDF();

        // Act + Assert
        Assert.Throws<UriFormatException>(() => entry.PrintPDF(
            url: "ftp://not-https.example.com/file",
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Structures.Environment(),
            cookies: new List<Cookie>(),
            paper: new Paper(),
            documentProperties: null,
            timeoutSeconds: 30,
            collectLogs: false,
            attachFilesLogs: false,
            logsZipFile: out _));
    }

    [Fact]
    public void ScreenshotPNG_InvalidUrl_ThrowsUriFormatException() {
        var ultimatePdf = NewUltimatePDF();

        Assert.Throws<UriFormatException>(() => ultimatePdf.ScreenshotPNG(
            url: "ftp://bad.example.com",
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Structures.Environment(),
            cookies: new List<Cookie>(),
            paper: new Paper(),
            screenshotOptions: new ScreenshotOptions(),
            timeoutSeconds: 30,
            collectLogs: false,
            attachFilesLogs: false,
            logsZipFile: out _));
    }

    [Fact]
    public void PrintPDF_ToRest_InvalidUrl_ThrowsUriFormatException() {
        var ultimatePdf = NewUltimatePDF();

        Assert.Throws<UriFormatException>(() => ultimatePdf.PrintPDF_ToRest(
            url: "ftp://bad.example.com",
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Structures.Environment(),
            cookies: new List<Cookie>(),
            paper: new Paper(),
            documentProperties: null,
            timeoutSeconds: 30,
            collectLogs: false,
            attachFilesLogs: false,
            restCaller: new RestCaller()));
    }

    [Fact]
    public void PrintPDF_ToS3_InvalidUrl_ThrowsUriFormatException() {
        var ultimatePdf = NewUltimatePDF();

        Assert.Throws<UriFormatException>(() => ultimatePdf.PrintPDF_ToS3(
            url: "ftp://bad.example.com",
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Structures.Environment(),
            cookies: new List<Cookie>(),
            paper: new Paper(),
            documentProperties: null,
            timeoutSeconds: 30,
            collectLogs: false,
            attachFilesLogs: false,
            s3Endpoints: new S3Endpoints()));
    }

    [Fact]
    public void Constructor_WithPlatformLogger_DoesNotResolveStorage() {
        // FileStorage.GetInstance resolves a loader the External Library Worker only generates during
        // an ODC build, so eager resolution here would throw and make the class unusable in every
        // test below. Constructing without a storage argument has to stay safe off-platform.
        var entry = new UltimatePDF_ExternalLogic(NullLogger.Instance);

        Assert.NotNull(entry);
    }

    [Fact]
    public void PrintPDF_ToFile_InvalidHttpsUrl_ThrowsAndStoresNothing() {
        // Arrange — URL validation happens before the browser runs and before anything is stored.
        var storage = new InMemoryFileStorage();
        var entry = NewUltimatePDF(storage);

        // Act + Assert
        Assert.Throws<UriFormatException>(() => entry.PrintPDF_ToFile(
            url: "ftp://not-https.example.com/file",
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Structures.Environment(),
            cookies: new List<Cookie>(),
            paper: new Paper(),
            timeoutSeconds: 30,
            collectLogs: false,
            attachFilesLogs: false,
            fileName: "",
            logsZipFile: out _,
            documentProperties: null));

        Assert.Empty(storage.Stored);
    }

    [Fact]
    public void ScreenshotPNG_ToFile_InvalidHttpsUrl_ThrowsAndStoresNothing() {
        var storage = new InMemoryFileStorage();
        var entry = NewUltimatePDF(storage);

        Assert.Throws<UriFormatException>(() => entry.ScreenshotPNG_ToFile(
            url: "ftp://bad.example.com",
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Structures.Environment(),
            cookies: new List<Cookie>(),
            paper: new Paper(),
            screenshotOptions: new ScreenshotOptions(),
            timeoutSeconds: 30,
            collectLogs: false,
            attachFilesLogs: false,
            fileName: "",
            logsZipFile: out _));

        Assert.Empty(storage.Stored);
    }
}
