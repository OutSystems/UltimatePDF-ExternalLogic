using System;
using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Moq;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {
    public class LoggerTests {

        private static Mock<ILogger> EnabledMock() {
            var mock = new Mock<ILogger>();
            mock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            return mock;
        }

        // NullLogger (collectLogs: false) tests

        [Fact]
        public void GetLogger_CollectLogsFalse_ReturnsDisabledLogger() {
            // Arrange
            var mock = new Mock<ILogger>();

            // Act
            var logger = Logger.GetLogger(mock.Object, collectLogs: false, attachFilesLogs: false);

            // Assert
            Assert.False(logger.IsEnabled);
        }

        [Fact]
        public void NullLogger_Log_DoesNotForwardToILogger() {
            // Arrange
            var mock = new Mock<ILogger>();
            var logger = Logger.GetLogger(mock.Object, collectLogs: false, attachFilesLogs: false);

            // Act
            logger.Log("ignored");
            logger.Warning("ignored");

            // Assert
            mock.Verify(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void NullLogger_GetZipFile_ReturnsEmpty() {
            // Arrange
            var mock = new Mock<ILogger>();
            var logger = Logger.GetLogger(mock.Object, collectLogs: false, attachFilesLogs: false);

            // Act
            var zip = logger.GetZipFile();

            // Assert
            Assert.Empty(zip);
        }

        [Fact]
        public void NullLogger_Attach_DoesNotStoreAttachment() {
            // Arrange
            var mock = new Mock<ILogger>();
            var logger = Logger.GetLogger(mock.Object, collectLogs: false, attachFilesLogs: false);

            // Act
            logger.Attach("file.pdf", new byte[] { 1, 2, 3 });
            var zip = logger.GetZipFile();

            // Assert — zip is empty because NullLogger discards everything
            Assert.Empty(zip);
        }

        // Active logger (collectLogs: true) tests

        [Fact]
        public void GetLogger_CollectLogsTrue_ReturnsEnabledLogger() {
            // Arrange
            var mock = EnabledMock();

            // Act
            var logger = Logger.GetLogger(mock.Object, collectLogs: true, attachFilesLogs: false);

            // Assert
            Assert.True(logger.IsEnabled);
        }

        [Fact]
        public void Logger_Log_ForwardsToILogger() {
            // Arrange
            var mock = EnabledMock();
            var logger = Logger.GetLogger(mock.Object, collectLogs: true, attachFilesLogs: false);

            // Act
            logger.Log("hello");

            // Assert
            mock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Logger_LogWithConditionFalse_DoesNotForward() {
            // Arrange
            var mock = EnabledMock();
            var logger = Logger.GetLogger(mock.Object, collectLogs: true, attachFilesLogs: false);

            // Act
            logger.Log("skipped", condition: false);

            // Assert
            mock.Verify(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void Logger_LogWithConditionTrue_Forwards() {
            // Arrange
            var mock = EnabledMock();
            var logger = Logger.GetLogger(mock.Object, collectLogs: true, attachFilesLogs: false);

            // Act
            logger.Log("included", condition: true);

            // Assert
            mock.Verify(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        // Attach / GetZipFile tests

        [Fact]
        public void Logger_Attach_AttachFilesLogsTrue_StoresFile() {
            // Arrange
            var mock = EnabledMock();
            var logger = Logger.GetLogger(mock.Object, collectLogs: true, attachFilesLogs: true);
            var content = new byte[] { 1, 2, 3 };

            // Act
            logger.Attach("sample.pdf", content);
            var zip = logger.GetZipFile();

            // Assert
            using var ms = new MemoryStream(zip);
            using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
            Assert.Single(archive.Entries, e => e.Name == "sample.pdf");
        }

        [Fact]
        public void Logger_Attach_AttachFilesLogsFalse_DoesNotStoreFile() {
            // Arrange
            var mock = EnabledMock();
            var logger = Logger.GetLogger(mock.Object, collectLogs: true, attachFilesLogs: false);

            // Act
            logger.Attach("sample.pdf", new byte[] { 1, 2, 3 });
            var zip = logger.GetZipFile();

            // Assert — zip is not empty (it's a valid archive) but has no entries
            using var ms = new MemoryStream(zip);
            using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
            Assert.Empty(archive.Entries);
        }

        [Fact]
        public void Logger_AttachString_AttachFilesLogsTrue_StoresEntry() {
            // Arrange
            var mock = EnabledMock();
            var logger = Logger.GetLogger(mock.Object, collectLogs: true, attachFilesLogs: true);

            // Act
            logger.Attach("page.html", "<html/>");
            var zip = logger.GetZipFile();

            // Assert
            using var ms = new MemoryStream(zip);
            using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
            Assert.Single(archive.Entries, e => e.Name == "page.html");
        }

        // GetLoggerFactory tests

        [Fact]
        public void Logger_GetLoggerFactory_ReturnsNonNull() {
            // Arrange
            var mock = EnabledMock();
            var logger = Logger.GetLogger(mock.Object, collectLogs: true, attachFilesLogs: false);

            // Act
            var factory = logger.GetLoggerFactory("browser.log");

            // Assert
            Assert.NotNull(factory);
        }

        [Fact]
        public void Logger_GetLoggerFactory_WritesToZip() {
            // Arrange
            var mock = EnabledMock();
            var logger = Logger.GetLogger(mock.Object, collectLogs: true, attachFilesLogs: false);
            var factory = logger.GetLoggerFactory("browser.log");
            var inner = factory.CreateLogger("Test");

            // Act
            inner.LogInformation("test message");
            var zip = logger.GetZipFile();

            // Assert
            using var ms = new MemoryStream(zip);
            using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
            Assert.Single(archive.Entries, e => e.Name == "browser.log");
        }

        [Fact]
        public void NullLogger_GetLoggerFactory_ReturnsNullLoggerFactory() {
            // Arrange
            var mock = new Mock<ILogger>();
            var logger = Logger.GetLogger(mock.Object, collectLogs: false, attachFilesLogs: false);

            // Act
            var factory = logger.GetLoggerFactory("any.log");

            // Assert — NullLogger returns NullLoggerFactory which creates NullLoggers
            Assert.NotNull(factory);
            Assert.NotNull(factory.CreateLogger("cat"));
        }

        // Error tests

        [Fact]
        public void Logger_Error_StringOverload_LogsError() {
            // Arrange
            var mock = EnabledMock();
            var logger = Logger.GetLogger(mock.Object, collectLogs: true, attachFilesLogs: false);

            // Act
            logger.Error("something went wrong");

            // Assert
            mock.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Logger_Error_ExceptionOverload_LogsTrace() {
            // Arrange
            var mock = EnabledMock();
            var logger = Logger.GetLogger(mock.Object, collectLogs: true, attachFilesLogs: false);
            var ex = new InvalidOperationException("oops");

            // Act — Error(Exception, string) maps to LogTrace internally
            logger.Error(ex, "oops context");

            // Assert
            mock.Verify(x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                ex,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
