using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;
using UltimatePDF_ExternalLogic.Utils;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {
    public class S3SenderTests : IDisposable {

        private readonly WireMockServer _server = WireMockServer.Start();
        private readonly Logger _logger;

        public S3SenderTests() {
            var mock = new Mock<ILogger>();
            mock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            _logger = Logger.GetLogger(mock.Object, collectLogs: true, attachFilesLogs: false);
        }

        public void Dispose() => _server.Stop();

        [Fact]
        public async Task S3SendPDFAsync_EmptyUrl_LogsAndReturns() {
            // Arrange — empty URL triggers the first early-return guard
            var sut = new S3Sender(string.Empty, string.Empty, _logger);

            // Act + Assert — no exception and no HTTP call
            await sut.S3SendPDFAsync(new byte[] { 1, 2, 3 });
            Assert.Empty(_server.LogEntries);
        }

        [Fact]
        public async Task S3SendPDFAsync_NullPdf_LogsAndReturns() {
            // Arrange — valid URL so the first guard passes, null PDF triggers second guard
            var sut = new S3Sender(_server.Urls[0] + "/pdf", string.Empty, _logger);

            // Act + Assert — no HTTP call made
            await sut.S3SendPDFAsync(null!);
            Assert.Empty(_server.LogEntries);
        }

        [Fact]
        public async Task S3SendPDFAsync_EmptyPdf_LogsAndReturns() {
            // Arrange — empty byte array triggers the second guard
            var sut = new S3Sender(_server.Urls[0] + "/pdf", string.Empty, _logger);

            // Act + Assert — no HTTP call made
            await sut.S3SendPDFAsync(Array.Empty<byte>());
            Assert.Empty(_server.LogEntries);
        }

        [Fact]
        public async Task S3SendPDFAsync_ValidUrl_PutsContent() {
            // Arrange
            _server.Given(Request.Create().WithPath("/pdf").UsingPut())
                   .RespondWith(Response.Create().WithStatusCode(200));
            var sut = new S3Sender(_server.Urls[0] + "/pdf", string.Empty, _logger);

            // Act
            await sut.S3SendPDFAsync(new byte[] { 1, 2, 3 });

            // Assert
            Assert.Single(_server.LogEntries);
        }

        [Fact]
        public async Task S3SendLogsAsync_EmptyUrl_LogsAndReturns() {
            // Arrange — empty logsPreSignedUrl triggers the early-return guard
            var sut = new S3Sender(string.Empty, string.Empty, _logger);

            // Act + Assert — no HTTP call
            await sut.S3SendLogsAsync();
            Assert.Empty(_server.LogEntries);
        }

        [Fact]
        public async Task S3SendLogsAsync_ValidUrl_PutsZipContent() {
            // Arrange
            _server.Given(Request.Create().WithPath("/logs").UsingPut())
                   .RespondWith(Response.Create().WithStatusCode(200));
            var sut = new S3Sender(string.Empty, _server.Urls[0] + "/logs", _logger);

            // Act
            await sut.S3SendLogsAsync();

            // Assert
            Assert.Single(_server.LogEntries);
        }
    }
}
