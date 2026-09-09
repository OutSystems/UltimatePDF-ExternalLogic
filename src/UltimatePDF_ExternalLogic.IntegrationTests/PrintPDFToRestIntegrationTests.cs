using System.IO.Compression;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using OutSystems.UltimatePDF_ExternalLogic.IntegrationTests.Fixtures;
using OutSystems.UltimatePDF_ExternalLogic.Structures;
using OutSystems.UltimatePDF_ExternalLogic.Test.Helpers;
using Environment = OutSystems.UltimatePDF_ExternalLogic.Structures.Environment;

namespace OutSystems.UltimatePDF_ExternalLogic.IntegrationTests;

[Collection("HelloWorldWeb")]
public class PrintPDFToRestIntegrationTests {

    private readonly HelloWorldWebFixture web;
    private readonly MockRestReceiverFixture rest;

    public PrintPDFToRestIntegrationTests(HelloWorldWebFixture web, MockRestReceiverFixture rest) {
        this.web = web;
        this.rest = rest;
        this.rest.Reset();
    }

    private static UltimatePDF_ExternalLogic NewUltimatePDF() =>
        new UltimatePDF_ExternalLogic(NullLogger.Instance);

    private RestCaller StoreRestCaller() => new RestCaller {
        BaseUrl = rest.BaseUrl,
        Module = "/api",
        StorePath = "/store",
        LogPath = "/logs",
        Token = "test-token",
    };

    /// <summary>
    /// Identical to <see cref="StoreRestCaller"/> apart from the opt-in flag, so a difference in
    /// what the receiver sees can only come from the payload format.
    /// </summary>
    private RestCaller Base64RestCaller() {
        var restCaller = StoreRestCaller();
        restCaller.SendBinariesAsBase64 = true;
        return restCaller;
    }

    private static byte[] DecodeBase64Body(byte[] body) =>
        Convert.FromBase64String(Encoding.ASCII.GetString(body));

    private static void AssertIsPdf(byte[] bytes) {
        Assert.Equal((byte)'%', bytes[0]);
        Assert.Equal((byte)'P', bytes[1]);
        Assert.Equal((byte)'D', bytes[2]);
        Assert.Equal((byte)'F', bytes[3]);
    }

    [IntegrationFact]
    public void PrintPDF_ToRest_HappyPath_PostsPdfToRestEndpoint() {
        // Arrange
        var ultimatePdf = NewUltimatePDF();

        // Act
        ultimatePdf.PrintPDF_ToRest(
            url: web.BaseUrl,
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Environment(),
            cookies: Array.Empty<Cookie>(),
            paper: new Paper(),
            documentProperties: null,
            timeoutSeconds: 60,
            collectLogs: false,
            attachFilesLogs: false,
            restCaller: StoreRestCaller());

        // Assert
        Assert.Single(rest.StoredPdfs);
        Assert.Equal("application/pdf", rest.StoredPdfContentTypes[0]);
        var pdf = rest.StoredPdfs[0];
        Assert.Equal((byte)'%', pdf[0]);
        Assert.Equal((byte)'P', pdf[1]);
        Assert.Equal((byte)'D', pdf[2]);
        Assert.Equal((byte)'F', pdf[3]);
    }

    [IntegrationFact]
    public void PrintPDF_ToRest_Base64_PostsBase64PdfToRestEndpoint() {
        // Arrange
        var ultimatePdf = NewUltimatePDF();

        // Act
        ultimatePdf.PrintPDF_ToRest(
            url: web.BaseUrl,
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Environment(),
            cookies: Array.Empty<Cookie>(),
            paper: new Paper(),
            documentProperties: null,
            timeoutSeconds: 60,
            collectLogs: false,
            attachFilesLogs: false,
            restCaller: Base64RestCaller());

        // Assert
        Assert.Single(rest.StoredPdfs);
        Assert.Equal("text/plain", rest.StoredPdfContentTypes[0]);

        // Assert on the decoded bytes, so the test fails if encoding silently stops happening.
        AssertIsPdf(DecodeBase64Body(rest.StoredPdfs[0]));
    }

    [IntegrationFact]
    public void PrintPDF_ToRest_WithCollectLogs_PostsPdfAndLogs() {
        // Arrange
        var ultiamtePdf = NewUltimatePDF();

        // Act
        ultiamtePdf.PrintPDF_ToRest(
            url: web.BaseUrl,
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Environment(),
            cookies: Array.Empty<Cookie>(),
            paper: new Paper(),
            documentProperties: null,
            timeoutSeconds: 60,
            collectLogs: true,
            attachFilesLogs: false,
            restCaller: StoreRestCaller());

        // Assert
        Assert.Single(rest.StoredPdfs);
        Assert.Single(rest.StoredLogs);
        Assert.Equal("application/pdf", rest.StoredPdfContentTypes[0]);
        Assert.Equal("application/zip", rest.StoredLogContentTypes[0]);
        var zipBytes = rest.StoredLogs[0];
        Assert.True(zipBytes.Length > 0);
        using var ms = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
        Assert.NotEmpty(archive.Entries);
    }

    [IntegrationFact]
    public void PrintPDF_ToRest_Base64WithCollectLogs_PostsBase64PdfAndBase64Logs() {
        // Arrange
        var ultimatePdf = NewUltimatePDF();

        // Act
        ultimatePdf.PrintPDF_ToRest(
            url: web.BaseUrl,
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Environment(),
            cookies: Array.Empty<Cookie>(),
            paper: new Paper(),
            documentProperties: null,
            timeoutSeconds: 60,
            collectLogs: true,
            attachFilesLogs: false,
            restCaller: Base64RestCaller());

        // Assert
        Assert.Single(rest.StoredPdfs);
        Assert.Single(rest.StoredLogs);
        Assert.Equal("text/plain", rest.StoredPdfContentTypes[0]);
        Assert.Equal("text/plain", rest.StoredLogContentTypes[0]);

        AssertIsPdf(DecodeBase64Body(rest.StoredPdfs[0]));

        var zipBytes = DecodeBase64Body(rest.StoredLogs[0]);
        using var ms = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
        Assert.NotEmpty(archive.Entries);
    }

    [IntegrationFact]
    public void PrintPDF_ToRest_WithAttachFilesLogs_LogsZipContainsInputAndOutput() {
        // Arrange
        var ultiamtePdf = NewUltimatePDF();

        // Act
        ultiamtePdf.PrintPDF_ToRest(
            url: web.BaseUrl,
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Environment(),
            cookies: Array.Empty<Cookie>(),
            paper: new Paper(),
            documentProperties: null,
            timeoutSeconds: 60,
            collectLogs: true,
            attachFilesLogs: true,
            restCaller: StoreRestCaller());

        // Assert
        ZipAssert.ContainsEntry(rest.StoredLogs[0], "input.html");
        ZipAssert.ContainsEntry(rest.StoredLogs[0], "output.pdf");
    }

    [IntegrationFact]
    public void PrintPDF_ToRest_Base64WithFailingLogPath_StillStoresThePdf() {
        // Arrange — /api/fail returns 400, standing in for a rejected logs upload.
        var ultimatePdf = NewUltimatePDF();
        var restCaller = Base64RestCaller();
        restCaller.LogPath = "/fail";

        // Act — a logs-upload failure is logged, not rethrown, so this must not throw.
        ultimatePdf.PrintPDF_ToRest(
            url: web.BaseUrl,
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Environment(),
            cookies: Array.Empty<Cookie>(),
            paper: new Paper(),
            documentProperties: null,
            timeoutSeconds: 60,
            collectLogs: true,
            attachFilesLogs: false,
            restCaller: restCaller);

        // Assert
        Assert.Single(rest.StoredPdfs);
        Assert.Equal("text/plain", rest.StoredPdfContentTypes[0]);
        AssertIsPdf(DecodeBase64Body(rest.StoredPdfs[0]));
        Assert.Empty(rest.StoredLogs);
    }
}
