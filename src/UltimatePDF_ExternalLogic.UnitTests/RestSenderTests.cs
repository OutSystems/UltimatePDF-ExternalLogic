using System.IO.Compression;
using System.Net;
using System.Text;
using OutSystems.UltimatePDF_ExternalLogic.Structures;
using OutSystems.UltimatePDF_ExternalLogic.Test.Helpers;
using UltimatePDF_ExternalLogic.Utils;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests;

/// <summary>
/// Pins the exact wire request <see cref="RestSender"/> produces in both payload modes, driven
/// through the internal <c>RestSender(RestCaller, Logger, HttpMessageHandler)</c> constructor so no
/// browser and no network are involved.
/// </summary>
public class RestSenderTests {

    private const string StoreUrl = "https://example.test/api/store";
    private const string LogsUrl = "https://example.test/api/logs";

    /// <summary>
    /// Captures everything about the outbound request that the acceptance criteria talk about, and
    /// returns a configurable response so failure paths can be driven too.
    /// </summary>
    private sealed class RecordingHttpMessageHandler : HttpMessageHandler {
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
        public string? ResponseBody { get; set; }
        public string? ResponseContentType { get; set; }

        public int Calls { get; private set; }
        public HttpMethod? Method { get; private set; }
        public Uri? RequestUri { get; private set; }
        public string? Authorization { get; private set; }
        public string? ContentType { get; private set; }
        public byte[] Body { get; private set; } = Array.Empty<byte>();

        public string BodyAsAscii => Encoding.ASCII.GetString(Body);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) {
            Calls++;
            Method = request.Method;
            RequestUri = request.RequestUri;
            Authorization = request.Headers.TryGetValues("Authorization", out var values)
                ? string.Join(",", values)
                : null;
            ContentType = request.Content?.Headers.TryGetValues("Content-Type", out var contentTypes) == true
                ? string.Join(",", contentTypes!)
                : null;
            Body = request.Content is null
                ? Array.Empty<byte>()
                : await request.Content.ReadAsByteArrayAsync(cancellationToken);

            var response = new HttpResponseMessage(Status) {
                Content = new StringContent(ResponseBody ?? string.Empty)
            };
            response.Content.Headers.Remove("Content-Type");
            if (ResponseContentType is not null) {
                response.Content.Headers.Add("Content-Type", ResponseContentType);
            }
            return response;
        }
    }

    private static RestCaller NewRestCaller() => new RestCaller {
        BaseUrl = "https://example.test",
        Module = "/api",
        StorePath = "/store",
        LogPath = "/logs",
        Token = "test-token",
    };

    private static RestCaller Base64RestCaller() {
        var restCaller = NewRestCaller();
        restCaller.SendBinariesAsBase64 = true;
        return restCaller;
    }

    [Fact]
    public async Task RestSendPDFAsync_DefaultMode_SendsRawPdfBytesAsApplicationPdf() {
        // Arrange
        var handler = new RecordingHttpMessageHandler();
        var pdf = PdfFactory.CreateMinimal();
        // Deliberately built without mentioning SendBinariesAsBase64, to prove the bool default.
        using var sender = new RestSender(NewRestCaller(), new SpyLogger(), handler);

        // Act
        await sender.RestSendPDFAsync(pdf);

        // Assert
        Assert.Equal(1, handler.Calls);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal(StoreUrl, handler.RequestUri?.AbsoluteUri);
        Assert.Equal("Bearer test-token", handler.Authorization);
        Assert.Equal("application/pdf", handler.ContentType);
        Assert.Equal(pdf, handler.Body);
    }

    [Fact]
    public async Task RestSendPDFAsync_Base64Mode_SendsBase64TextThatDecodesToThePdf() {
        // Arrange
        var handler = new RecordingHttpMessageHandler();
        var pdf = PdfFactory.CreateMinimal();
        using var sender = new RestSender(Base64RestCaller(), new SpyLogger(), handler);

        // Act
        await sender.RestSendPDFAsync(pdf);

        // Assert
        Assert.Equal("text/plain", handler.ContentType);

        // Convert.FromBase64String is a strict decoder: it rejects a data: prefix, surrounding
        // quotes, and a JSON wrapper, so a successful decode is itself an assertion.
        var encoded = handler.BodyAsAscii;
        Assert.Equal(pdf, Convert.FromBase64String(encoded));

        Assert.Equal(Convert.ToBase64String(pdf), encoded);
        Assert.DoesNotContain('\r', encoded);
        Assert.DoesNotContain('\n', encoded);
        Assert.DoesNotContain("data:", encoded);
        Assert.DoesNotContain('"', encoded);
    }

    [Fact]
    public async Task RestSendLogs_DefaultMode_SendsRawZipBytesAsApplicationZip() {
        // Arrange
        var handler = new RecordingHttpMessageHandler();
        using var sender = new RestSender(NewRestCaller(), new SpyLogger(), handler);

        // Act
        await sender.RestSendLogs();

        // Assert
        Assert.Equal(LogsUrl, handler.RequestUri?.AbsoluteUri);
        Assert.Equal("application/zip", handler.ContentType);

        // The ZIP local file header, which a Base64 body could not start with.
        Assert.Equal(new byte[] { (byte)'P', (byte)'K', 0x03, 0x04 }, handler.Body.Take(4).ToArray());
        AssertOpensAsZipWithEntries(handler.Body);
    }

    [Fact]
    public async Task RestSendLogs_Base64Mode_SendsBase64TextThatDecodesToTheZip() {
        // Arrange
        var handler = new RecordingHttpMessageHandler();
        using var sender = new RestSender(Base64RestCaller(), new SpyLogger(), handler);

        // Act
        await sender.RestSendLogs();

        // Assert — the single flag covers the logs upload too, not just the PDF.
        Assert.Equal(LogsUrl, handler.RequestUri?.AbsoluteUri);
        Assert.Equal("text/plain", handler.ContentType);
        AssertOpensAsZipWithEntries(Convert.FromBase64String(handler.BodyAsAscii));
    }

    [Fact]
    public async Task RestSendPDFAsync_BothModes_DifferOnlyInBodyAndContentType() {
        // Arrange
        var pdf = PdfFactory.CreateMinimal();
        var binaryHandler = new RecordingHttpMessageHandler();
        var base64Handler = new RecordingHttpMessageHandler();
        using var binarySender = new RestSender(NewRestCaller(), new SpyLogger(), binaryHandler);
        using var base64Sender = new RestSender(Base64RestCaller(), new SpyLogger(), base64Handler);

        // Act
        await binarySender.RestSendPDFAsync(pdf);
        await base64Sender.RestSendPDFAsync(pdf);

        // Assert — everything except the body and its content type is identical.
        Assert.Equal(binaryHandler.Method, base64Handler.Method);
        Assert.Equal(binaryHandler.RequestUri?.AbsoluteUri, base64Handler.RequestUri?.AbsoluteUri);
        Assert.Equal("Bearer test-token", binaryHandler.Authorization);
        Assert.Equal(binaryHandler.Authorization, base64Handler.Authorization);
        Assert.NotEqual(binaryHandler.ContentType, base64Handler.ContentType);
        Assert.NotEqual(binaryHandler.Body, base64Handler.Body);
    }

    [Fact]
    public async Task RestSendPDFAsync_Base64ModeRejected_ReportsTextPlainAndTheEncodedByteCount() {
        // Arrange — an edge rejection: 403 carrying an HTML page rather than an OutSystems payload.
        var pdf = PdfFactory.CreateMinimal();
        var handler = new RecordingHttpMessageHandler {
            Status = HttpStatusCode.Forbidden,
            ResponseBody = "<html><body>Forbidden</body></html>",
            ResponseContentType = "text/html",
        };
        using var sender = new RestSender(Base64RestCaller(), new SpyLogger(), handler);

        // Act
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => sender.RestSendPDFAsync(pdf));

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Contains("text/plain", exception.Message);
        Assert.DoesNotContain("application/pdf", exception.Message);

        // The encoded length, not the pre-encoding one — asserted explicitly so a regression that
        // reports the payload size before encoding fails here.
        var encodedLength = Convert.ToBase64String(pdf).Length;
        Assert.NotEqual(pdf.Length, encodedLength);
        Assert.Contains($"Sent {encodedLength} bytes as text/plain", exception.Message);
        Assert.DoesNotContain($"Sent {pdf.Length} bytes", exception.Message);
    }

    private static void AssertOpensAsZipWithEntries(byte[] zipBytes) {
        using var ms = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
        Assert.NotEmpty(archive.Entries);
    }
}
