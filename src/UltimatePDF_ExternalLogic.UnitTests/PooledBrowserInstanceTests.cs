using System.Diagnostics;
using Moq;
using OutSystems.UltimatePDF_ExternalLogic.BrowserExecution;
using PuppeteerSharp;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests;

public class PooledBrowserInstanceTests {

    [Fact]
    public void IsHealthy_FreshInstance_ReturnsTrue() {
        // Arrange
        var browserMock = new Mock<IBrowser>();
        browserMock.Setup(b => b.Process).Returns(Process.GetCurrentProcess());
        var instance = new PooledBrowserInstance(browserMock.Object);

        // Assert
        Assert.True(instance.IsHealthy);
    }

    [Fact]
    public async Task CloseAsync_MarksUnhealthy_BeforeUnderlyingBrowserCloseResolves() {
        // Arrange — mimics PuppeteerSharp 25.x: CloseAsync() is in-flight and
        // Process.HasExited is still false when a subsequent request checks IsHealthy.
        var browserMock = new Mock<IBrowser>();
        var closeGate = new TaskCompletionSource();
        browserMock.Setup(b => b.CloseAsync()).Returns(closeGate.Task);
        browserMock.Setup(b => b.Process).Returns(Process.GetCurrentProcess());
        var instance = new PooledBrowserInstance(browserMock.Object);
        Assert.True(instance.IsHealthy);

        // Act
        var closeTask = instance.CloseAsync();

        // Assert — unhealthy immediately, before browser.CloseAsync() has resolved
        Assert.False(instance.IsHealthy);

        closeGate.SetResult();
        await closeTask;
    }
}
