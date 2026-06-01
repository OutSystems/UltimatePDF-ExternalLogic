using System.IO;
using System.Reflection;
using OutSystems.UltimatePDF_ExternalLogic.LayoutPrintPipeline;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {
    public class PipelineTests {

        private static byte[] CreateMinimalPdf(int pages = 1) {
            using var stream = new MemoryStream();
            var doc = new PdfDocument();
            for (int i = 0; i < pages; i++) doc.AddPage();
            doc.Save(stream, false);
            return stream.ToArray();
        }

        private static PdfDocument OpenEditable(int pages = 1) {
            var doc = new PdfDocument();
            for (int i = 0; i < pages; i++) doc.AddPage();
            return doc;
        }

        [Fact]
        public void HasLayouts_DefaultPipeline_ReturnsFalse() {
            // Arrange + Act
            var pipeline = new Pipeline();

            // Assert
            Assert.False(pipeline.HasLayouts);
        }

        [Fact]
        public void HasLayouts_NonEmptyLayouts_ReturnsTrue() {
            // Arrange — inject a non-empty layouts array via reflection
            var pipeline = new Pipeline();
            var field = typeof(Pipeline)
                .GetField("layouts", BindingFlags.NonPublic | BindingFlags.Instance)!;
            field.SetValue(pipeline, new Pipeline.LayoutPrint[] { new Pipeline.LayoutPrint() });

            // Act + Assert
            Assert.True(pipeline.HasLayouts);
        }

        [Fact]
        public void MergeBackground_SinglePageDoc_NoException() {
            // Arrange
            var pipeline = new Pipeline();
            using var doc = OpenEditable(1);

            // Act + Assert
            pipeline.MergeBackground(doc, CreateMinimalPdf());
        }

        [Fact]
        public void MergeHeaders_SkipZero_NoException() {
            // Arrange — doc pages == header pages → skip == 0
            var pipeline = new Pipeline();
            using var doc = OpenEditable(1);

            // Act + Assert
            pipeline.MergeHeaders(doc, CreateMinimalPdf(1));
        }

        [Fact]
        public void MergeHeaders_SkipPositive_NoException() {
            // Arrange — doc has 3 pages, header has 2 → skip == 1
            var pipeline = new Pipeline();
            using var doc = OpenEditable(3);

            // Act + Assert
            pipeline.MergeHeaders(doc, CreateMinimalPdf(2));
        }

        [Fact]
        public void MergeFooters_NoException() {
            // Arrange
            var pipeline = new Pipeline();
            using var doc = OpenEditable(2);

            // Act + Assert
            pipeline.MergeFooters(doc, CreateMinimalPdf(2));
        }

        [Fact]
        public void MergeBottomContent_NoException() {
            // Arrange
            var pipeline = new Pipeline();
            using var doc = OpenEditable(1);

            // Act + Assert
            pipeline.MergeBottomContent(doc, CreateMinimalPdf(1));
        }

        [Fact]
        public void CopyHyperlinks_NoAnnotations_NothingCopied() {
            // Arrange
            var pipeline = new Pipeline();
            using var fromDoc = new PdfDocument();
            var fromPage = fromDoc.AddPage();
            using var toDoc = new PdfDocument();
            var toPage = toDoc.AddPage();

            // Act
            pipeline.CopyHyperlinks(fromPage, toPage, yOffset: 0);

            // Assert
            Assert.Equal(0, toPage.Annotations.Count);
        }

        [Fact]
        public void Concatenate_TwoDocuments_AllPagesPresent() {
            // Arrange
            var pipeline = new Pipeline();
            using var doc1 = OpenEditable(1);
            using var doc2 = OpenEditable(2);

            // Act
            var result = pipeline.Concatenate(new[] { doc1, doc2 });

            // Assert
            using var ms = new MemoryStream(result);
            var resultDoc = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
            Assert.Equal(3, resultDoc.PageCount);
        }

        [Fact]
        public void Concatenate_SingleDocument_ReturnsSamePagesCount() {
            // Arrange
            var pipeline = new Pipeline();
            using var doc = OpenEditable(2);

            // Act
            var result = pipeline.Concatenate(new[] { doc });

            // Assert
            using var ms = new MemoryStream(result);
            var resultDoc = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
            Assert.Equal(2, resultDoc.PageCount);
        }
    }
}
