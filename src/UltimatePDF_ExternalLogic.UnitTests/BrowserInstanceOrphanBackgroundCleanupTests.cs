using System;
using System.Threading.Tasks;
using UltimatePDF_ExternalLogic.Cleanup;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {
    public class BrowserInstanceOrphanBackgroundCleanupTests {

        [Fact]
        public async Task Stop_CausesCleanupToExit() {
            // Arrange — 1-second periodicity; Stop() is called before the first delay ends
            var sut = new BrowserInstanceOrphanBackgroundCleanup(periodicitySeconds: 1);
            var cleanupTask = Task.Run(sut.Cleanup);

            // Act
            sut.Stop();

            // Assert — task completes within a generous 5 s window
            await cleanupTask.WaitAsync(TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task Cleanup_WithoutStop_DoesNotCompleteQuickly() {
            // Arrange
            var sut = new BrowserInstanceOrphanBackgroundCleanup(periodicitySeconds: 60);

            // Act
            var cleanupTask = Task.Run(sut.Cleanup);
            var completed = await Task.WhenAny(cleanupTask, Task.Delay(200)) == cleanupTask;

            // Assert — task must still be running
            Assert.False(completed);

            // Cleanup
            sut.Stop();
            await cleanupTask.WaitAsync(TimeSpan.FromSeconds(70));
        }
    }
}
