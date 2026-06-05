using System;
using System.Threading;
using System.Threading.Tasks;
using UltimatePDF_ExternalLogic.Cleanup;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {
    public class BrowserInstanceOrphanBackgroundCleanupTests {

        [Fact]
        public async Task Stop_CausesCleanupToExit() {
            // Arrange — 60-second periodicity; Stop() cancels immediately without waiting for the delay
            var sut = new BrowserInstanceOrphanBackgroundCleanup(periodicitySeconds: 60);
            var cleanupTask = Task.Run(sut.Cleanup);

            // Act
            sut.Stop();

            // Assert — cancellation fires immediately; task should complete well within 3 s
            await cleanupTask.WaitAsync(TimeSpan.FromSeconds(3), TestContext.Current.CancellationToken);
        }

        [Fact]
        public async Task Cleanup_WithoutStop_DoesNotCompleteQuickly() {
            // Arrange
            var sut = new BrowserInstanceOrphanBackgroundCleanup(periodicitySeconds: 60);

            // Act
            var cleanupTask = Task.Run(sut.Cleanup);
            var completed = await Task.WhenAny(
                cleanupTask,
                Task.Delay(500, TestContext.Current.CancellationToken)) == cleanupTask;

            // Assert — task must still be waiting for the 60-second delay
            Assert.False(completed);

            // Cleanup — cancel to unblock the background task
            sut.Stop();
            await cleanupTask.WaitAsync(TimeSpan.FromSeconds(3), TestContext.Current.CancellationToken);
        }
    }
}
