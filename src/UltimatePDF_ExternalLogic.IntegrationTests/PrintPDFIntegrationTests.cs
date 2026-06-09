using System;
using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Logging.Abstractions;
using OutSystems.UltimatePDF_ExternalLogic.IntegrationTests.Fixtures;
using OutSystems.UltimatePDF_ExternalLogic.IntegrationTests.TestHelpers;
using OutSystems.UltimatePDF_ExternalLogic.Structures;
using PdfSharp.Pdf.IO;
using OutSystems.UltimatePDF_ExternalLogic.Test.Helpers;
using Environment = OutSystems.UltimatePDF_ExternalLogic.Structures.Environment;

namespace OutSystems.UltimatePDF_ExternalLogic.IntegrationTests {

    [Collection("HelloWorldWeb")]
    public class PrintPDFIntegrationTests {

        private readonly HelloWorldWebFixture _web;
        private readonly MockRestReceiverFixture _rest;

        public PrintPDFIntegrationTests(HelloWorldWebFixture web, MockRestReceiverFixture rest) {
            _web = web;
            _rest = rest;
            _rest.Reset();
        }

        private static UltimatePDF_ExternalLogic NewUltimatePDF() =>
            new UltimatePDF_ExternalLogic(NullLogger.Instance);

        [Fact]
        public void PrintPDF_HelloWorld_ReturnsValidPdf() {
            // Arrange
            var ultimatePdf = NewUltimatePDF();

            // Act
            var pdf = ultimatePdf.PrintPDF(
                url: _web.BaseUrl,
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: new Environment(),
                cookies: Array.Empty<Cookie>(),
                paper: new Paper(),
                documentProperties: null,
                timeoutSeconds: 60,
                collectLogs: false,
                attachFilesLogs: false,
                logsZipFile: out _);

            // Assert
            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 1000);
            Assert.Equal((byte)'%', pdf[0]);
            Assert.Equal((byte)'P', pdf[1]);
            Assert.Equal((byte)'D', pdf[2]);
            Assert.Equal((byte)'F', pdf[3]);
            Assert.Equal((byte)'-', pdf[4]);

            using var ms = new MemoryStream(pdf);
            var doc = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
            Assert.True(doc.PageCount >= 1);
        }

        [Fact]
        public void PrintPDF_WithDocumentProperties_EmbedsAllMetadata() {
            // Arrange
            var ultimatePdf = NewUltimatePDF();
            var properties = new DocumentProperties {
                Title = "Q1 Report",
                Author = "Acme",
                Subject = "Quarterly results",
                Keywords = "finance, q1",
                Creator = "Ultimate PDF",
                Company = "Acme Corp",
                Producer = "Ultimate PDF 1.x",
                Copyright = "(c) 2026 Acme",
                Language = "en-US",
                Source = "ERP-Prod",
            };

            // Act
            var pdf = ultimatePdf.PrintPDF(
                url: _web.BaseUrl,
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: new Environment(),
                cookies: Array.Empty<Cookie>(),
                paper: new Paper(),
                documentProperties: properties,
                timeoutSeconds: 60,
                collectLogs: false,
                attachFilesLogs: false,
                logsZipFile: out _);

            // Assert
            using var ms = new MemoryStream(pdf);
            var doc = PdfReader.Open(ms, PdfDocumentOpenMode.Import);

            Assert.Equal("Q1 Report", doc.Info.Title);
            Assert.Equal("Acme", doc.Info.Author);
            Assert.Equal("Quarterly results", doc.Info.Subject);
            Assert.Equal("finance, q1", doc.Info.Keywords);
            Assert.Equal("Ultimate PDF", doc.Info.Creator);
            Assert.Equal("Acme Corp", doc.Info.Elements.GetString("/Company"));
            // PdfSharp 6.2 wraps Producer as "PDFsharp X.Y (Original: <our value>)" on save.
            Assert.Contains("Ultimate PDF 1.x", doc.Info.Elements.GetString("/Producer"));
            Assert.Equal("(c) 2026 Acme", doc.Info.Elements.GetString("/Copyright"));
            Assert.Equal("en-US", doc.Internals.Catalog.Elements.GetString("/Lang"));
            Assert.Equal("ERP-Prod", doc.Info.Elements.GetString("/Source"));
        }

        [Fact]
        public void PrintPDF_CustomMargins_ProducesValidPdf() {
            // Arrange
            var ultimatePdf = NewUltimatePDF();
            var paper = new Paper {
                UseCustomMargins = true,
                MarginTop = 1, MarginRight = 1, MarginBottom = 1, MarginLeft = 1,
            };

            // Act
            var pdf = ultimatePdf.PrintPDF(
                url: _web.BaseUrl,
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: new Environment(),
                cookies: Array.Empty<Cookie>(),
                paper: paper,
                documentProperties: null,
                timeoutSeconds: 60,
                collectLogs: false,
                attachFilesLogs: false,
                logsZipFile: out _);

            // Assert
            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 1000);

            using var ms = new MemoryStream(pdf);
            var doc = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
            Assert.True(doc.PageCount >= 1);
        }

        [Fact]
        public void PrintPDF_WithCookies_ProducesValidPdf() {
            // Arrange
            var ultimatePdf = NewUltimatePDF();
            var cookies = new[] { new Cookie { Name = "session", Value = "abc123" } };

            // Act
            var pdf = ultimatePdf.PrintPDF(
                url: _web.BaseUrl,
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: new Environment(),
                cookies: cookies,
                paper: new Paper(),
                documentProperties: null,
                timeoutSeconds: 60,
                collectLogs: false,
                attachFilesLogs: false,
                logsZipFile: out _);

            // Assert
            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 1000);

            using var ms = new MemoryStream(pdf);
            var doc = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
            Assert.True(doc.PageCount >= 1);
        }

        [Fact]
        public void PrintPDF_WithCollectLogs_ReturnsNonEmptyZip() {
            // Arrange
            var ultimatePdf = NewUltimatePDF();

            // Act
            _ = ultimatePdf.PrintPDF(
                url: _web.BaseUrl,
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: new Environment(),
                cookies: Array.Empty<Cookie>(),
                paper: new Paper(),
                documentProperties: null,
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

        [Fact]
        public void PrintPDF_WithAttachFilesLogs_LogsZipContainsInputAndOutput() {
            // Arrange
            var ultimatePdf = NewUltimatePDF();

            // Act
            _ = ultimatePdf.PrintPDF(
                url: _web.BaseUrl,
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: new Environment(),
                cookies: Array.Empty<Cookie>(),
                paper: new Paper(),
                documentProperties: null,
                timeoutSeconds: 60,
                collectLogs: true,
                attachFilesLogs: true,
                logsZipFile: out var logsZip);

            // Assert
            Assert.NotNull(logsZip);
            Assert.True(logsZip.Length > 0);
            ZipAssert.ContainsEntry(logsZip, "input.html");
            ZipAssert.ContainsEntry(logsZip, "output.pdf");
        }

        [Fact]
        public void PrintPDF_ToS3_HappyPath_UploadsPdfToPresignedUrl() {
            // Arrange — use the mock REST server's PUT route as a stand-in for S3 presigned URL
            var ultimatePdf = NewUltimatePDF();
            var s3Endpoints = new S3Endpoints {
                PdfPreSignedUrl = $"{_rest.BaseUrl}/s3/object",
            };

            // Act
            ultimatePdf.PrintPDF_ToS3(
                url: _web.BaseUrl,
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: new Environment(),
                cookies: Array.Empty<Cookie>(),
                paper: new Paper(),
                documentProperties: null,
                timeoutSeconds: 60,
                collectLogs: false,
                attachFilesLogs: false,
                s3Endpoints: s3Endpoints);

            // Assert — one object uploaded, starting with PDF magic bytes
            Assert.Single(_rest.StoredS3Objects);
            var pdf = _rest.StoredS3Objects[0];
            Assert.Equal((byte)'%', pdf[0]);
            Assert.Equal((byte)'P', pdf[1]);
            Assert.Equal((byte)'D', pdf[2]);
            Assert.Equal((byte)'F', pdf[3]);
            Assert.Equal((byte)'-', pdf[4]);
        }

        [Fact]
        public void PrintPDF_ToS3_EmptyPreSignedUrl_DoesNotUpload() {
            // Arrange — empty pre-signed URLs trigger the early-return branch in S3Sender
            var ultimatePdf = NewUltimatePDF();

            // Act
            ultimatePdf.PrintPDF_ToS3(
                url: _web.BaseUrl,
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: new Environment(),
                cookies: Array.Empty<Cookie>(),
                paper: new Paper(),
                documentProperties: null,
                timeoutSeconds: 60,
                collectLogs: false,
                attachFilesLogs: false,
                s3Endpoints: new S3Endpoints()); // PdfPreSignedUrl and LogsPreSignedUrl default to ""

            // Assert — early return means nothing was uploaded
            Assert.Empty(_rest.StoredS3Objects);
        }

        [Fact]
        public void PrintPDF_WithLocale_ProducesValidPdf() {
            // Arrange
            var ultimatePdf = NewUltimatePDF();
            var env = new Environment { Locale = "en-US" };

            // Act
            var pdf = ultimatePdf.PrintPDF(
                url: _web.BaseUrl,
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: env,
                cookies: Array.Empty<Cookie>(),
                paper: new Paper(),
                documentProperties: null,
                timeoutSeconds: 60,
                collectLogs: false,
                attachFilesLogs: false,
                logsZipFile: out _);

            // Assert
            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 1000);

            using var ms = new MemoryStream(pdf);
            var doc = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
            Assert.True(doc.PageCount >= 1);
        }

        [Fact]
        public void PrintPDF_WithTimezone_ProducesValidPdf() {
            // Arrange
            var ultimatePdf = NewUltimatePDF();
            var env = new Environment { Timezone = "Europe/Lisbon" };

            // Act
            var pdf = ultimatePdf.PrintPDF(
                url: _web.BaseUrl,
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: env,
                cookies: Array.Empty<Cookie>(),
                paper: new Paper(),
                documentProperties: null,
                timeoutSeconds: 60,
                collectLogs: false,
                attachFilesLogs: false,
                logsZipFile: out _);

            // Assert
            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 1000);

            using var ms = new MemoryStream(pdf);
            var doc = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
            Assert.True(doc.PageCount >= 1);
        }
    }
}
