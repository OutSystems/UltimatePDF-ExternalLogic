using System;
using System.Collections.Generic;
using System.IO;
using OutSystems.UltimatePDF_ExternalLogic.LayoutPrintPipeline;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {
    public class LayoutPrintTests {

        private static byte[] CreateMinimalPdf(int pages = 1) {
            using var stream = new MemoryStream();
            var doc = new PdfDocument();
            for (int i = 0; i < pages; i++) doc.AddPage();
            doc.Save(stream, false);
            return stream.ToArray();
        }

        [Fact]
        public void Constructor_ValidPdf_SetsFirstPage() {
            // Arrange
            var section = new PrintSection(firstPage: 3);

            // Act
            using var lp = new LayoutPrint(0, section, CreateMinimalPdf());

            // Assert — FirstPage equals section.NextPage at construction time (NextPage == firstPage when Pages == 0)
            Assert.Equal(3, lp.FirstPage);
        }

        [Fact]
        public void Pages_ReturnsDocumentPageCount() {
            // Arrange
            var section = new PrintSection(1);

            // Act
            using var lp = new LayoutPrint(0, section, CreateMinimalPdf(1));

            // Assert
            Assert.Equal(1, lp.Pages);
        }

        [Fact]
        public void Pages_MultiPage_ReturnsCorrectCount() {
            // Arrange
            var section = new PrintSection(1);

            // Act
            using var lp = new LayoutPrint(0, section, CreateMinimalPdf(3));

            // Assert
            Assert.Equal(3, lp.Pages);
        }

        [Fact]
        public void LastPage_ReturnsFirstPlusPagesMinusOne() {
            // Arrange — section.FirstPage=3, section.Pages=2 → LastPage = 3+2-1 = 4
            var section = new PrintSection(3);
            section.AddPages(2);

            // Act
            using var lp = new LayoutPrint(0, section, CreateMinimalPdf());

            // Assert
            Assert.Equal(4, lp.LastPage);
        }

        [Fact]
        public void LayoutNumber_ReturnsConstructorValue() {
            // Arrange
            var section = new PrintSection(1);

            // Act
            using var lp = new LayoutPrint(7, section, CreateMinimalPdf());

            // Assert
            Assert.Equal(7, lp.LayoutNumber);
        }

        [Fact]
        public void MergeBackground_SinglePage_NoException() {
            // Arrange
            var section = new PrintSection(1);
            using var lp = new LayoutPrint(0, section, CreateMinimalPdf());

            // Act + Assert
            lp.MergeBackground(CreateMinimalPdf());
        }

        [Fact]
        public void MergeHeader_SkipZero_NoException() {
            // Arrange — doc pages == header pages → skip == 0
            var section = new PrintSection(1);
            using var lp = new LayoutPrint(0, section, CreateMinimalPdf(1));

            // Act + Assert
            lp.MergeHeader(CreateMinimalPdf(1));
        }

        [Fact]
        public void MergeHeader_SkipPositive_NoException() {
            // Arrange — doc has 3 pages, header has 2 → skip == 1
            var section = new PrintSection(1);
            using var lp = new LayoutPrint(0, section, CreateMinimalPdf(3));

            // Act + Assert
            lp.MergeHeader(CreateMinimalPdf(2));
        }

        [Fact]
        public void MergeFooter_NoException() {
            // Arrange
            var section = new PrintSection(1);
            using var lp = new LayoutPrint(0, section, CreateMinimalPdf(2));

            // Act + Assert
            lp.MergeFooter(CreateMinimalPdf(2));
        }

        [Fact]
        public void Concatenate_TwoPdfs_CombinesPageCount() {
            // Arrange
            var s1 = new PrintSection(1);
            var lp1 = new LayoutPrint(0, s1, CreateMinimalPdf(1));
            var s2 = new PrintSection(2);
            var lp2 = new LayoutPrint(1, s2, CreateMinimalPdf(2));

            // Act
            var result = LayoutPrint.Concatenate(new List<LayoutPrint> { lp1, lp2 });

            // Assert
            using var ms = new MemoryStream(result);
            var doc = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
            Assert.Equal(3, doc.PageCount);
        }

        [Fact]
        public void Dispose_CanBeCalledWithoutError() {
            // Arrange
            var section = new PrintSection(1);
            var lp = new LayoutPrint(0, section, CreateMinimalPdf());

            // Act + Assert
            lp.Dispose();
        }
    }
}
