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

            // Act — Process is null on the mock; WaitForExit falls into the broad catch block.
            // The meaningful signal is that CloseAsync was called exactly once.
            await sut.Cleanup();

            // Assert
            mockBrowser.Verify(b => b.CloseAsync(), Times.Once);
        }

        [Fact]
        public async Task Cleanup_BrowserCloseThrows_DoesNotPropagate() {
            // Arrange
            var mockBrowser = new Mock<IBrowser>();
            mockBrowser.Setup(b => b.CloseAsync()).ThrowsAsync(new InvalidOperationException("browser gone"));
            var sut = new BrowserInstanceCleanup(mockBrowser.Object);

            // Act
            Exception? caught = null;
            try { await sut.Cleanup(); } catch (Exception e) { caught = e; }

            // Assert — the broad catch must swallow the exception
            Assert.Null(caught);
        }

        [Fact]
        public async Task Cleanup_BrowserProcessWaitThrows_DoesNotPropagate() {
            // Arrange — CloseAsync succeeds; Process is null on a Moq IBrowser, causing
            // NullReferenceException on WaitForExit, which the broad catch must swallow.
            var mockBrowser = new Mock<IBrowser>();
            mockBrowser.Setup(b => b.CloseAsync()).Returns(Task.CompletedTask);
            var sut = new BrowserInstanceCleanup(mockBrowser.Object);

            // Act
            Exception? caught = null;
            try { await sut.Cleanup(); } catch (Exception e) { caught = e; }

            // Assert
            Assert.Null(caught);
        }
    }
}
