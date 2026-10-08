using System.IO.Compression;
using Microsoft.Extensions.Logging.Abstractions;
using OutSystems.ExternalLibraries.SDK.FileStorage;
using OutSystems.UltimatePDF_ExternalLogic.IntegrationTests.Fixtures;
using OutSystems.UltimatePDF_ExternalLogic.Structures;
using OutSystems.UltimatePDF_ExternalLogic.Test.Helpers;
using PdfSharp.Pdf.IO;
using Environment = OutSystems.UltimatePDF_ExternalLogic.Structures.Environment;

namespace OutSystems.UltimatePDF_ExternalLogic.IntegrationTests;

/// <summary>
/// Exercises PrintPDF_ToFile against a real browser, with storage faked.
/// </summary>
/// <remarks>
/// Storage has to be faked: the real implementations are not on NuGet and
/// <c>FileStorage.GetInstance</c> resolves a loader the External Library Worker only generates during
/// an ODC build. So these tests prove the render, the naming and what the library hands to storage —
/// the one thing they cannot prove is that ODC's own storage accepts it, which needs a tenant.
/// </remarks>
[Collection("HelloWorldWeb")]
public class PrintPDFToFileIntegrationTests {

    private readonly HelloWorldWebFixture web;

    public PrintPDFToFileIntegrationTests(HelloWorldWebFixture web) => this.web = web;

    private static UltimatePDF_ExternalLogic NewUltimatePDF(IFileStorage storage) =>
        new UltimatePDF_ExternalLogic(storage, NullLogger.Instance);

    [IntegrationFact]
    public void PrintPDF_ToFile_HelloWorld_StoresValidPdfUnderDefaultName() {
        // Arrange
        var storage = new InMemoryFileStorage();
        var ultimatePdf = NewUltimatePDF(storage);

        // Act
        var file = ultimatePdf.PrintPDF_ToFile(
            url: web.BaseUrl,
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Environment(),
            cookies: Array.Empty<Cookie>(),
            paper: new Paper(),
            timeoutSeconds: 60,
            collectLogs: false,
            attachFilesLogs: false,
            fileName: "",
            logsZipFile: out var logsFile,
            documentProperties: null);

        // Assert — one write, named and typed as a PDF
        var stored = Assert.Single(storage.Stored);
        Assert.Equal("document.pdf", stored.FileName);
        Assert.Equal("application/pdf", stored.ContentType);

        // The stored bytes are a real PDF, not a placeholder
        Assert.True(stored.Content.Length > 1000);
        using var ms = new MemoryStream(stored.Content);
        var doc = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
        Assert.True(doc.PageCount >= 1);

        // The returned reference describes the same content
        Assert.Equal("application/pdf", file.ContentType);
        Assert.Equal(stored.Content.LongLength, file.Size);

        // Logs were not requested, so they come back empty and nothing extra was stored
        Assert.Empty(logsFile);
    }

    [IntegrationFact]
    public void PrintPDF_ToFile_WithFileName_UsesIt() {
        // Arrange
        var storage = new InMemoryFileStorage();
        var ultimatePdf = NewUltimatePDF(storage);

        // Act
        _ = ultimatePdf.PrintPDF_ToFile(
            url: web.BaseUrl,
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Environment(),
            cookies: Array.Empty<Cookie>(),
            paper: new Paper(),
            timeoutSeconds: 60,
            collectLogs: false,
            attachFilesLogs: false,
            fileName: "invoice-2026.pdf",
            logsZipFile: out _,
            documentProperties: null);

        // Assert
        Assert.Equal("invoice-2026.pdf", Assert.Single(storage.Stored).FileName);
    }

    [IntegrationFact]
    public void PrintPDF_ToFile_WithDocumentProperties_EmbedsMetadataInTheStoredPdf() {
        // Arrange
        var storage = new InMemoryFileStorage();
        var ultimatePdf = NewUltimatePDF(storage);
        var properties = new DocumentProperties { Title = "Q1 Report", Author = "Acme" };

        // Act
        _ = ultimatePdf.PrintPDF_ToFile(
            url: web.BaseUrl,
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Environment(),
            cookies: Array.Empty<Cookie>(),
            paper: new Paper(),
            timeoutSeconds: 60,
            collectLogs: false,
            attachFilesLogs: false,
            fileName: "",
            logsZipFile: out _,
            documentProperties: properties);

        // Assert — metadata survives the trip through storage, as it does for PrintPDF
        using var ms = new MemoryStream(Assert.Single(storage.Stored).Content);
        var doc = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
        Assert.Equal("Q1 Report", doc.Info.Title);
        Assert.Equal("Acme", doc.Info.Author);
    }

    [IntegrationFact]
    public void PrintPDF_ToFile_WithCollectLogs_ReturnsLogsAsBinaryDataAndStoresOnlyThePdf() {
        // Arrange
        var storage = new InMemoryFileStorage();
        var ultimatePdf = NewUltimatePDF(storage);

        // Act
        _ = ultimatePdf.PrintPDF_ToFile(
            url: web.BaseUrl,
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Environment(),
            cookies: Array.Empty<Cookie>(),
            paper: new Paper(),
            timeoutSeconds: 60,
            collectLogs: true,
            attachFilesLogs: false,
            fileName: "invoice.pdf",
            logsZipFile: out var logsFile,
            documentProperties: null);

        // Assert — only the PDF goes to storage. The logs stay Binary Data, because ODC's generated
        // code cannot bind a File to an out parameter, so this action matches PrintPDF for its logs.
        Assert.Equal("invoice.pdf", Assert.Single(storage.Stored).FileName);

        Assert.NotEmpty(logsFile);
        using var ms = new MemoryStream(logsFile);
        using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
        Assert.NotEmpty(archive.Entries);
    }

    [IntegrationFact]
    public void PrintPDF_ToFile_WithAttachFilesLogs_ReturnsLogsContainingInputAndOutput() {
        // Arrange
        var storage = new InMemoryFileStorage();
        var ultimatePdf = NewUltimatePDF(storage);

        // Act
        _ = ultimatePdf.PrintPDF_ToFile(
            url: web.BaseUrl,
            viewport: new Viewport { Width = 800, Height = 600 },
            environment: new Environment(),
            cookies: Array.Empty<Cookie>(),
            paper: new Paper(),
            timeoutSeconds: 60,
            collectLogs: true,
            attachFilesLogs: true,
            fileName: "",
            logsZipFile: out var logsFile,
            documentProperties: null);

        // Assert
        ZipAssert.ContainsEntry(logsFile, "input.html");
        ZipAssert.ContainsEntry(logsFile, "output.pdf");
    }
}
