using System.Collections.Generic;
using System.IO;
using OutSystems.UltimatePDF_ExternalLogic.LayoutPrintPipeline;
using OutSystems.UltimatePDF_ExternalLogic.UnitTests.TestHelpers;
using PdfSharp.Pdf.IO;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {
    public class LayoutPrintTests {

        [Fact]
        public void Constructor_ValidPdf_SetsFirstPage() {
            // Arrange
            var section = new PrintSection(firstPage: 3);

            // Act
            using var lp = new LayoutPrint(0, section, PdfFactory.CreateMinimal());

            // Assert — FirstPage equals section.NextPage at construction time (NextPage == firstPage when Pages == 0)
            Assert.Equal(3, lp.FirstPage);
        }

        [Fact]
        public void Pages_ReturnsDocumentPageCount() {
            // Arrange
            var section = new PrintSection(1);

            // Act
            using var lp = new LayoutPrint(0, section, PdfFactory.CreateMinimal(1));

            // Assert
            Assert.Equal(1, lp.Pages);
        }

        [Fact]
        public void Pages_MultiPage_ReturnsCorrectCount() {
            // Arrange
            var section = new PrintSection(1);

            // Act
            using var lp = new LayoutPrint(0, section, PdfFactory.CreateMinimal(3));

            // Assert
            Assert.Equal(3, lp.Pages);
        }

        [Fact]
        public void LastPage_ReturnsFirstPlusPagesMinusOne() {
            // Arrange — section.FirstPage=3, section.Pages=2 → LastPage = 3+2-1 = 4
            var section = new PrintSection(3);
            section.AddPages(2);

            // Act
            using var lp = new LayoutPrint(0, section, PdfFactory.CreateMinimal());

            // Assert
            Assert.Equal(4, lp.LastPage);
        }

        [Fact]
        public void LayoutNumber_ReturnsConstructorValue() {
            // Arrange
            var section = new PrintSection(1);

            // Act
            using var lp = new LayoutPrint(7, section, PdfFactory.CreateMinimal());

            // Assert
            Assert.Equal(7, lp.LayoutNumber);
        }

        [Fact]
        public void MergeBackground_SinglePage_EmbedsPdfForm() {
            // Arrange — do NOT use 'using'; Concatenate disposes the LayoutPrint
            var section = new PrintSection(1);
            var lp = new LayoutPrint(0, section, PdfFactory.CreateMinimal());

            // Act
            lp.MergeBackground(PdfFactory.CreateMinimal());
            var result = LayoutPrint.Concatenate(new List<LayoutPrint> { lp });

            // Assert — XPdfForm was drawn and persisted as an XObject resource
            using var ms = new MemoryStream(result);
            var doc = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
            Assert.NotNull(doc.Pages[0].Resources.Elements.GetDictionary("/XObject"));
        }

        [Fact]
        public void MergeHeader_SkipZero_EmbedsPdfForm() {
            // Arrange — doc pages == header pages → skip == 0, header drawn on pages[0]
            var section = new PrintSection(1);
            var lp = new LayoutPrint(0, section, PdfFactory.CreateMinimal(1));

            // Act
            lp.MergeHeader(PdfFactory.CreateMinimal(1));
            var result = LayoutPrint.Concatenate(new List<LayoutPrint> { lp });

            // Assert
            using var ms = new MemoryStream(result);
            var doc = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
            Assert.NotNull(doc.Pages[0].Resources.Elements.GetDictionary("/XObject"));
        }

        [Fact]
        public void MergeHeader_SkipPositive_EmbedsPdfForm() {
            // Arrange — doc has 3 pages, header has 2 → skip == 1, drawn on pages[1] and pages[2]
            var section = new PrintSection(1);
            var lp = new LayoutPrint(0, section, PdfFactory.CreateMinimal(3));

            // Act
            lp.MergeHeader(PdfFactory.CreateMinimal(2));
            var result = LayoutPrint.Concatenate(new List<LayoutPrint> { lp });

            // Assert — pages[0] is not drawn; pages[1] carries the XObject
            using var ms = new MemoryStream(result);
            var doc = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
            Assert.NotNull(doc.Pages[1].Resources.Elements.GetDictionary("/XObject"));
        }

        [Fact]
        public void MergeFooter_EmbedsPdfForm() {
            // Arrange
            var section = new PrintSection(1);
            var lp = new LayoutPrint(0, section, PdfFactory.CreateMinimal(2));

            // Act
            lp.MergeFooter(PdfFactory.CreateMinimal(2));
            var result = LayoutPrint.Concatenate(new List<LayoutPrint> { lp });

            // Assert — footer drawn on every page
            using var ms = new MemoryStream(result);
            var doc = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
            Assert.NotNull(doc.Pages[0].Resources.Elements.GetDictionary("/XObject"));
        }

        [Fact]
        public void Concatenate_TwoPdfs_CombinesPageCount() {
            // Arrange
            var s1 = new PrintSection(1);
            var lp1 = new LayoutPrint(0, s1, PdfFactory.CreateMinimal(1));
            var s2 = new PrintSection(2);
            var lp2 = new LayoutPrint(1, s2, PdfFactory.CreateMinimal(2));

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
            var lp = new LayoutPrint(0, section, PdfFactory.CreateMinimal());

            // Act + Assert — Dispose is a no-throw contract; explicit assertion documents intent
            lp.Dispose();
            Assert.True(true);
        }
    }
}
