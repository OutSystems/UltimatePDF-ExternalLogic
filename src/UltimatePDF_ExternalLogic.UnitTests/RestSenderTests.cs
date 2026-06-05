using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;
using OutSystems.UltimatePDF_ExternalLogic.Structures;
using UltimatePDF_ExternalLogic.Utils;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {
    public class RestSenderTests {

        private readonly Logger _logger;
        private readonly RestCaller _caller;

        public RestSenderTests() {
            var mock = new Mock<ILogger>();
            mock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            _logger = Logger.GetLogger(mock.Object, collectLogs: true, attachFilesLogs: false);

            _caller = new RestCaller {
                BaseUrl = "example.com",
                Module = "/api",
                StorePath = "/store",
                LogPath = "/logs",
                Token = "test-token",
            };
        }

        [Fact]
        public async Task RestSendPDFAsync_Server200_PostsPdfWithBearerTokenAndContentType() {
            var handler = new FakeHttpMessageHandler { StatusCode = HttpStatusCode.OK };
            var sut = new RestSender(_caller, _logger, handler);

            await sut.RestSendPDFAsync(new byte[] { 0x25, 0x50, 0x44, 0x46 });

            Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
            Assert.Contains("Bearer test-token", handler.LastRequest.Headers.Authorization?.ToString());
            Assert.Equal("application/pdf", handler.LastRequest.Content?.Headers.ContentType?.MediaType);
        }

        [Fact]
        public async Task RestSendPDFAsync_Server4xx_ThrowsHttpRequestException() {
            var handler = new FakeHttpMessageHandler { StatusCode = HttpStatusCode.BadRequest };
            var sut = new RestSender(_caller, _logger, handler);

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                sut.RestSendPDFAsync(new byte[] { 0x25, 0x50, 0x44, 0x46 }));
        }

        [Fact]
        public async Task RestSendLogs_Server200_PostsZipWithBearerToken() {
            var handler = new FakeHttpMessageHandler { StatusCode = HttpStatusCode.OK };
            var sut = new RestSender(_caller, _logger, handler);

            await sut.RestSendLogs();

            Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
            Assert.Contains("Bearer test-token", handler.LastRequest.Headers.Authorization?.ToString());
            Assert.Equal("application/zip", handler.LastRequest.Content?.Headers.ContentType?.MediaType);
        }

        [Fact]
        public async Task RestSendLogs_Server4xx_ThrowsHttpRequestException() {
            var handler = new FakeHttpMessageHandler { StatusCode = HttpStatusCode.InternalServerError };
            var sut = new RestSender(_caller, _logger, handler);

            await Assert.ThrowsAsync<HttpRequestException>(() => sut.RestSendLogs());
        }

        private sealed class FakeHttpMessageHandler : HttpMessageHandler {
            public HttpStatusCode StatusCode { get; init; } = HttpStatusCode.OK;
            public HttpRequestMessage? LastRequest { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken) {
                LastRequest = request;
                return Task.FromResult(new HttpResponseMessage(StatusCode));
            }
        }
    }
}
