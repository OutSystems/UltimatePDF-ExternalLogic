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

    private static LogOptions NoLogs() =>
        new LogOptions { CollectLogs = false, AttachFilesLogs = false };

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
            logOptions: NoLogs(),
            fileName: "",
            logs: out var logs,
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

        // Logs were not requested, so nothing extra was stored
        Assert.Null(logs.LogsZipFile);
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
            logOptions: NoLogs(),
            fileName: "invoice-2026.pdf",
            logs: out _,
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
            logOptions: NoLogs(),
            fileName: "",
            logs: out _,
            documentProperties: properties);

        // Assert — metadata survives the trip through storage, as it does for PrintPDF
        using var ms = new MemoryStream(Assert.Single(storage.Stored).Content);
        var doc = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
        Assert.Equal("Q1 Report", doc.Info.Title);
        Assert.Equal("Acme", doc.Info.Author);
    }

    [IntegrationFact]
    public void PrintPDF_ToFile_WithCollectLogs_StoresLogsAsAFileNamedAfterThePdf() {
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
            logOptions: new LogOptions { CollectLogs = true, AttachFilesLogs = false },
            fileName: "invoice.pdf",
            logs: out var logs,
            documentProperties: null);

        // Assert — the PDF and its logs, as two separate Files. The logs being a File rather than
        // Binary Data is what keeps them clear of the 5.5MB payload limit.
        Assert.Equal(2, storage.Stored.Count);
        Assert.NotNull(logs.LogsZipFile);

        var stored = storage.Find(logs.LogsZipFile!);
        Assert.Equal("invoice-logs.zip", stored.FileName);
        Assert.Equal("application/zip", stored.ContentType);

        using var ms = new MemoryStream(stored.Content);
        using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
        Assert.NotEmpty(archive.Entries);
    }

    [IntegrationFact]
    public void PrintPDF_ToFile_WithAttachFilesLogs_StoresLogsContainingInputAndOutput() {
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
            logOptions: new LogOptions { CollectLogs = true, AttachFilesLogs = true },
            fileName: "",
            logs: out var logs,
            documentProperties: null);

        // Assert
        Assert.NotNull(logs.LogsZipFile);

        var content = storage.Find(logs.LogsZipFile!).Content;
        ZipAssert.ContainsEntry(content, "input.html");
        ZipAssert.ContainsEntry(content, "output.pdf");
    }
}
