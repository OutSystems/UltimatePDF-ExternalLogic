using System;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;
using OutSystems.UltimatePDF_ExternalLogic.Structures;

namespace UltimatePDF_ExternalLogic.Utils;
internal class RestSender : IDisposable {
    /// <summary>
    /// Content type used when the payload is Base64-encoded. Deliberately not a binary media type:
    /// an edge layer rejecting binary POSTs is what this mode exists to get past.
    /// </summary>
    private const string Base64ContentType = "text/plain";

    private readonly RestCaller restCaller;
    private readonly Logger logger;
    private readonly HttpClient client;

    /// <summary>
    /// How the payload is described in the execution log, so a 403 in a customer trace can be
    /// attributed to the right mode without guessing which body shape was sent.
    /// </summary>
    private string PayloadFormat => restCaller.SendBinariesAsBase64 ? "Base64 text" : "raw binary";

    public RestSender(RestCaller restCaller, Logger logger)
        : this(restCaller, logger, new HttpClientHandler()) { }

    internal RestSender(RestCaller restCaller, Logger logger, HttpMessageHandler handler) {
        this.restCaller = restCaller;
        this.logger = logger;
        client = new HttpClient(handler);
    }

    internal async Task RestSendPDFAsync(byte[] pdf) {
        using var activity = Activity.Current?.Source.StartActivity("RestSender.RestSendPDFAsync");
        var restEndpoint = UrlUtils.BuildUrl(restCaller.BaseUrl, restCaller.Module, restCaller.StorePath);

        logger.Log($"Sending the generated PDF using a REST API as {PayloadFormat}. " +
            $"Calling to {restEndpoint}.");

        await RestCall(restEndpoint, restCaller.Token, "application/pdf", pdf);

        logger.Log($"PDF successfully sent via REST API.");
    }

    internal async Task RestSendLogs() {
        using var activity = Activity.Current?.Source.StartActivity("RestSender.RestSendLogs");
        var restEndpoint = UrlUtils.BuildUrl(restCaller.BaseUrl, restCaller.Module, restCaller.LogPath);

        logger.Log($"Sending the generated Logs using a REST API as {PayloadFormat}. " +
            $"Calling to {restEndpoint}.");

        await RestCall(restEndpoint, restCaller.Token, "application/zip", logger.GetZipFile());

        logger.Log($"Logs successfully sent via REST API.");
    }

    private async Task RestCall(string endpoint, string token, string contentType, byte[] binary) {
        using var activity = Activity.Current?.Source.StartActivity("RestSender.RestCall");
        var body = restCaller.SendBinariesAsBase64 ? EncodeBase64(binary) : binary;
        var sentContentType = restCaller.SendBinariesAsBase64 ? Base64ContentType : contentType;

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Content = new StreamContent(new MemoryStream(body));
        request.Content.Headers.Add("Content-Type", sentContentType);
        using var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode) {
            await logger.AttachAsync(
                "store-rest-response-body.html",
                () => response.Content.ReadAsByteArrayAsync());
            throw new HttpRequestException(
                await DescribeFailureAsync(response, endpoint, sentContentType, body.Length),
                null,
                response.StatusCode);
        }
    }

    /// <summary>
    /// Encodes to RFC 4648 Base64 as ASCII bytes: no line breaks, no prefix, no byte-order mark.
    /// Encoding straight to bytes keeps this to a single extra copy of the payload, and produces
    /// exactly the same characters as <see cref="Convert.ToBase64String(byte[])"/>.
    /// </summary>
    private static byte[] EncodeBase64(byte[] binary) {
        using var activity = Activity.Current?.Source.StartActivity("RestSender.EncodeBase64");
        var encoded = new byte[Base64.GetMaxEncodedToUtf8Length(binary.Length)];
        var status = Base64.EncodeToUtf8(binary, encoded, out _, out var written);

        if (status != OperationStatus.Done || written != encoded.Length) {
            throw new InvalidOperationException(
                $"Base64 encoding of {binary.Length} bytes failed ({status}, wrote {written} of {encoded.Length}).");
        }

        return encoded;
    }

    /// <summary>
    /// Builds a diagnosable description of a failed REST call. The response body and its content
    /// type are included because they are what distinguishes an application-level rejection (an
    /// OutSystems error payload) from an edge rejection (a WAF or IP filter HTML page), which is
    /// otherwise indistinguishable from the status code alone.
    /// </summary>
    private static async Task<string> DescribeFailureAsync(
        HttpResponseMessage response, string endpoint, string sentContentType, int sentBytes) {
        using var activity = Activity.Current?.Source.StartActivity("RestSender.DescribeFailureAsync");
        var responseContentType = response.Content.Headers.ContentType?.ToString() ?? "<none>";

        return $"POST {endpoint} failed with {(int)response.StatusCode} ({response.ReasonPhrase}). " +
            $"Sent {sentBytes} bytes as {sentContentType}. " +
            $"Response content type: {responseContentType}. " +
            $"Response body: store-rest-response-body.html";
    }

    public void Dispose() {
        client.Dispose();
    }
}
