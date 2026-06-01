using System;
using UltimatePDF_ExternalLogic.Utils;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {
    public class ResourceAccessorTests {

        // Confirm resource names via:
        // string.Join("\n", typeof(ResourceAccessor).Assembly.GetManifestResourceNames())

        [Fact]
        public void GetResource_ExistingResource_ReturnsContent() {
            // Arrange
            const string name = "UltimatePDF_ExternalLogic.resources.version";

            // Act
            var content = ResourceAccessor.GetResource(name);

            // Assert
            Assert.NotNull(content);
            Assert.NotEmpty(content);
        }

        [Fact]
        public void GetResource_NonExistentResource_ThrowsArgumentException() {
            // Arrange
            const string name = "does.not.exist.txt";

            // Act + Assert
            Assert.Throws<ArgumentException>(() => ResourceAccessor.GetResource(name));
        }
    }
}
