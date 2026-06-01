using OutSystems.UltimatePDF_ExternalLogic.LayoutPrintPipeline;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {
    public class PrintSectionTests {

        [Fact]
        public void Constructor_SetsFirstPage() {
            // Arrange + Act
            var section = new PrintSection(5);

            // Assert
            Assert.Equal(5, section.FirstPage);
            Assert.Equal(0, section.Pages);
        }

        [Fact]
        public void AddPages_AccumulatesPages() {
            // Arrange
            var section = new PrintSection(1);

            // Act
            section.AddPages(3);
            section.AddPages(4);

            // Assert
            Assert.Equal(7, section.Pages);
        }

        [Fact]
        public void NextPage_ReturnsFirstPlusPagesAccumulated() {
            // Arrange
            var section = new PrintSection(5);

            // Act
            section.AddPages(3);

            // Assert
            Assert.Equal(8, section.NextPage);
        }

        [Fact]
        public void AddPages_ZeroIncrement_PagesUnchanged() {
            // Arrange
            var section = new PrintSection(1);
            section.AddPages(2);

            // Act
            section.AddPages(0);

            // Assert
            Assert.Equal(2, section.Pages);
        }
    }
}
