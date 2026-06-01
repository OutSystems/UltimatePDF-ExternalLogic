using System.IO;
using OutSystems.UltimatePDF_ExternalLogic.Structures;
using OutSystems.UltimatePDF_ExternalLogic.UnitTests.TestHelpers;
using OutSystems.UltimatePDF_ExternalLogic.Utils;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {
    public class PDFMetadataUtilTests {

        private static byte[] CreateMinimalPdf() {
            using var stream = new MemoryStream();
            var document = new PdfDocument();
            document.AddPage();
            document.Save(stream, false);
            return stream.ToArray();
        }

        private static PdfDocument OpenPdf(byte[] bytes) {
            using var stream = new MemoryStream(bytes);
            return PdfReader.Open(stream, PdfDocumentOpenMode.Import);
        }

        [Fact]
        public void ApplyMetadata_AllFieldsPopulated_WritesAllValues() {
            // Arrange
            var input = CreateMinimalPdf();
            var originalCreationDate = OpenPdf(input).Info.CreationDate;
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
            var before = System.DateTime.UtcNow;
            var output = PDFMetadataUtil.ApplyMetadata(input, properties);
            var after = System.DateTime.UtcNow;

            // Assert
            var doc = OpenPdf(output);
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
            // ModificationDate is set to the embedding moment; CreationDate is preserved.
            // PdfSharp serializes PDF dates at second granularity, so allow a 1s slack on each side.
            var modifiedUtc = doc.Info.ModificationDate.ToUniversalTime();
            Assert.InRange(modifiedUtc, before.AddSeconds(-1), after.AddSeconds(1));
            Assert.Equal(originalCreationDate, doc.Info.CreationDate);
        }

        [Fact]
        public void ApplyMetadata_AllFieldsEmpty_ReturnsInputUnchanged() {
            // Arrange
            var input = CreateMinimalPdf();
            var properties = new DocumentProperties();

            // Act
            var output = PDFMetadataUtil.ApplyMetadata(input, properties);

            // Assert
            Assert.Same(input, output);
        }

        [Fact]
        public void ApplyMetadata_MixedFields_WritesOnlyPopulatedEntries() {
            // Arrange
            var input = CreateMinimalPdf();
            var properties = new DocumentProperties {
                Title = "Only Title",
                Company = "Acme",
            };

            // Act
            var output = PDFMetadataUtil.ApplyMetadata(input, properties);

            // Assert
            var doc = OpenPdf(output);
            Assert.Equal("Only Title", doc.Info.Title);
            Assert.Equal("Acme", doc.Info.Elements.GetString("/Company"));
            Assert.False(doc.Info.Elements.ContainsKey("/Author"));
            // /Producer is always set by PdfSharp on save; skip presence assertion.
            Assert.False(doc.Info.Elements.ContainsKey("/Copyright"));
            Assert.False(doc.Info.Elements.ContainsKey("/Source"));
            Assert.False(doc.Internals.Catalog.Elements.ContainsKey("/Lang"));
        }

        [Fact]
        public void ApplyMetadata_NonAscii_RoundTripsValues() {
            // Arrange
            var input = CreateMinimalPdf();
            var properties = new DocumentProperties {
                Title = "Relatório Trimestral",
                Author = "José",
            };

            // Act
            var output = PDFMetadataUtil.ApplyMetadata(input, properties);

            // Assert
            var doc = OpenPdf(output);
            Assert.Equal("Relatório Trimestral", doc.Info.Title);
            Assert.Equal("José", doc.Info.Author);
        }

        [Fact]
        public void ApplyMetadata_CorruptInput_ReturnsInputAndLogsWarning() {
            // Arrange
            var input = new byte[] { 1, 2, 3 };
            var properties = new DocumentProperties { Title = "X" };
            var spy = new LoggerSpy();

            // Act
            var output = PDFMetadataUtil.ApplyMetadata(input, properties, spy);

            // Assert
            Assert.Same(input, output);
            Assert.Equal(1, spy.WarningCalls);
            Assert.NotNull(spy.LastException);
            Assert.Contains("Failed to embed PDF metadata", spy.LastWarningMessage);
        }
    }
}
