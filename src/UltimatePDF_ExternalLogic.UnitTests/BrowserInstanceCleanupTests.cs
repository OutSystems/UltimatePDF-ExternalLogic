using System;
using System.Threading.Tasks;
using Moq;
using PuppeteerSharp;
using UltimatePDF_ExternalLogic.Cleanup;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {
    public class BrowserInstanceCleanupTests {

        [Fact]
        public async Task Cleanup_CallsBrowserCloseAsync() {
            // Arrange
            var mockBrowser = new Mock<IBrowser>();
            mockBrowser.Setup(b => b.CloseAsync()).Returns(Task.CompletedTask);
            var sut = new BrowserInstanceCleanup(mockBrowser.Object);

            // Act — Process is null on the mock, causing NullRefEx which is swallowed by catch
            await sut.Cleanup();

            // Assert
            mockBrowser.Verify(b => b.CloseAsync(), Times.Once);
        }

        [Fact]
        public async Task Cleanup_BrowserCloseThrows_DoesNotPropagate() {
            // Arrange
            var mockBrowser = new Mock<IBrowser>();
            mockBrowser.Setup(b => b.CloseAsync())
                       .ThrowsAsync(new InvalidOperationException("browser gone"));
            var sut = new BrowserInstanceCleanup(mockBrowser.Object);

            // Act + Assert — must not throw
            await sut.Cleanup();
        }
    }
}
