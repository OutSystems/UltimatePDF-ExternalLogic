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

        [Fact]
        public void Warning_NullLogger_DoesNotInvokeUnderlyingLogger() {
            // Arrange
            var mock = new Mock<ILogger>();
            var nullLogger = Logger.GetLogger(mock.Object, collectLogs: false, attachFilesLogs: false);

            // Act
            nullLogger.Warning("hello");
            nullLogger.Warning(new Exception(), "boom");

            // Assert
            mock.Verify(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
