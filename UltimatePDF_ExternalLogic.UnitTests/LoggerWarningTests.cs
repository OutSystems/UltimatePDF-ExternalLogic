using System;
using Microsoft.Extensions.Logging;
using Moq;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {
    public class LoggerWarningTests {

        [Fact]
        public void Warning_StringOverload_RoutesToLogWarning() {
            // Arrange
            var mock = new Mock<ILogger>();
            var logger = Logger.GetLogger(mock.Object, collectLogs: true, attachFilesLogs: false);

            // Act
            logger.Warning("hello");

            // Assert
            mock.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Warning_ExceptionOverload_RoutesToLogWarningWithException() {
            // Arrange
            var mock = new Mock<ILogger>();
            var logger = Logger.GetLogger(mock.Object, collectLogs: true, attachFilesLogs: false);
            var ex = new InvalidOperationException("boom");

            // Act
            logger.Warning(ex, "failed");

            // Assert
            mock.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                ex,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void Warning_CollectLogsAndAttachFilesLogsMatrix_BehavesCorrectly(bool collectLogs, bool attachFilesLogs) {
            // Warning forwarding depends only on collectLogs; attachFilesLogs must not affect it.
            // Arrange
            var mock = new Mock<ILogger>();
            var logger = Logger.GetLogger(mock.Object, collectLogs, attachFilesLogs);

            // Act
            logger.Warning("hello");
            logger.Warning(new Exception("boom"), "failed");

            // Assert
            var expectedCalls = collectLogs ? Times.Exactly(2) : Times.Never();
            mock.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                expectedCalls);
        }
    }
}
