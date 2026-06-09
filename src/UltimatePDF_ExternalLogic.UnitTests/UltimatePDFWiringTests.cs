using Microsoft.Extensions.Logging.Abstractions;
using OutSystems.UltimatePDF_ExternalLogic.Structures;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests;

/// <summary>
/// Wiring-level tests that exercise the entry points of UltimatePDF_ExternalLogic
/// without spinning up PuppeteerSharp.
/// </summary>
public class UltimatePDFWiringTests {

    private static UltimatePDF_ExternalLogic NewUltimatePDF() =>
        new UltimatePDF_ExternalLogic(NullLogger.Instance);

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
}
