using System;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using OutSystems.UltimatePDF_ExternalLogic.IntegrationTests.Fixtures;
using OutSystems.UltimatePDF_ExternalLogic.Structures;
using PdfSharp.Pdf.IO;
using Environment = OutSystems.UltimatePDF_ExternalLogic.Structures.Environment;

namespace OutSystems.UltimatePDF_ExternalLogic.IntegrationTests {

    [Collection("HelloWorldWeb")]
    public class PrintPDFIntegrationTests {

        private readonly HelloWorldWebFixture _web;

        public PrintPDFIntegrationTests(HelloWorldWebFixture web) {
            _web = web;
        }

        private static UltimatePDF_ExternalLogic NewSut() =>
            new UltimatePDF_ExternalLogic(NullLogger.Instance);

        [Fact]
        public void PrintPDF_HelloWorld_ReturnsValidPdf() {
            // Arrange
            var sut = NewSut();

            // Act
            var pdf = sut.PrintPDF(
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
            var sut = NewSut();
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
            var pdf = sut.PrintPDF(
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
    }
}
