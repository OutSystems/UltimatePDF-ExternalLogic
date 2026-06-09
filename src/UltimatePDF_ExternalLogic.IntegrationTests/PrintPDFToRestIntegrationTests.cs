using System.IO.Compression;
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

    [Fact]
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

    [Fact]
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
        var zipBytes = rest.StoredLogs[0];
        Assert.True(zipBytes.Length > 0);
        using var ms = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
        Assert.NotEmpty(archive.Entries);
    }

    [Fact]
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
}
