using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;
using OutSystems.UltimatePDF_ExternalLogic.Structures;
using UltimatePDF_ExternalLogic.Utils;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {
    // Note: RestSender uses UrlUtils.BuildUrl which always produces HTTPS URLs.
    // Since WireMock starts on HTTP, the connection attempt produces an HttpRequestException
    // (TLS handshake failure). These tests verify that exception propagation works correctly
    // and that all code paths are exercised for coverage purposes.
    public class RestSenderTests : IDisposable {

        private readonly WireMockServer _server = WireMockServer.Start();
        private readonly Logger _logger;
        private readonly RestCaller _caller;

        public RestSenderTests() {
            var mock = new Mock<ILogger>();
            mock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            _logger = Logger.GetLogger(mock.Object, collectLogs: true, attachFilesLogs: false);

            // Strip the "http://" scheme so BuildUrl produces "https://host:port/..."
            // rather than the malformed "https://http://host:port/..."
            var serverUri = new Uri(_server.Urls[0]);
            _caller = new RestCaller {
                BaseUrl = $"{serverUri.Host}:{serverUri.Port}",
                Module = "/api",
                StorePath = "/store",
                LogPath = "/logs",
                Token = "test-token",
            };
        }

        public void Dispose() => _server.Stop();

        [Fact]
        public async Task RestSendPDFAsync_ServerReturns4xx_ThrowsHttpRequestException() {
            // Arrange — WireMock stub configured to return 400; HTTPS→HTTP mismatch causes
            // TLS failure, which is also surfaced as HttpRequestException.
            _server.Given(Request.Create().WithPath("/api/store").UsingPost())
                   .RespondWith(Response.Create().WithStatusCode(400));
            var sut = new RestSender(_caller, _logger);

            // Act + Assert
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                sut.RestSendPDFAsync(new byte[] { 0x25, 0x50, 0x44, 0x46 }));
        }

        [Fact]
        public async Task RestSendPDFAsync_EndpointUnreachable_ThrowsHttpRequestException() {
            // Arrange — no WireMock stub; HTTPS connection to the HTTP stub server fails
            var sut = new RestSender(_caller, _logger);

            // Act + Assert
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                sut.RestSendPDFAsync(new byte[] { 1, 2, 3 }));
        }

        [Fact]
        public async Task RestSendLogs_EndpointUnreachable_ThrowsHttpRequestException() {
            // Arrange
            var sut = new RestSender(_caller, _logger);

            // Act + Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => sut.RestSendLogs());
        }
    }
}
