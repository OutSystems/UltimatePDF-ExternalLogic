using System;
using System.Threading;
using System.Threading.Tasks;
using UltimatePDF_ExternalLogic.Cleanup;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {
    public class AbstractCleanupTaskTests {

        private sealed class FakeCleanupTask : AbstractCleanupTask {
            private int _count;
            public int Count => _count;
            private readonly TaskCompletionSource<bool>? _tcs;

            public FakeCleanupTask() { }
            public FakeCleanupTask(TaskCompletionSource<bool> tcs) => _tcs = tcs;

            public override Task Cleanup() {
                Interlocked.Increment(ref _count);
                _tcs?.SetResult(true);
                return Task.CompletedTask;
            }
        }

        [Fact]
        public void StartAndWait_CallsCleanup() {
            // Arrange
            var task = new FakeCleanupTask();

            // Act
            task.StartAndWait();

            // Assert
            Assert.Equal(1, task.Count);
        }

        [Fact]
        public void StartInBackground_CleanupEventuallyRuns() {
            // Arrange
            var tcs = new TaskCompletionSource<bool>();
            var task = new FakeCleanupTask(tcs);

            // Act
            task.StartInBackground();

            // Assert
            Assert.True(tcs.Task.Wait(TimeSpan.FromSeconds(5)));
        }
    }
}
