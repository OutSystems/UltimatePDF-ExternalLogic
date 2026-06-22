using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;
using OutSystems.UltimatePDF_ExternalLogic.Structures;

namespace UltimatePDF_ExternalLogic.Utils {
    internal class RestSender : IDisposable {
        private readonly RestCaller restCaller;
        private readonly Logger logger;
        private readonly HttpClient client;

        public RestSender(RestCaller restCaller, Logger logger)
            : this(restCaller, logger, new HttpClientHandler()) { }

        internal RestSender(RestCaller restCaller, Logger logger, HttpMessageHandler handler) {
            this.restCaller = restCaller;
            this.logger = logger;
            client = new HttpClient(handler);
        }

        internal async Task RestSendPDFAsync(byte[] pdf) {
            var restEndpoint = UrlUtils.BuildUrl(restCaller.BaseUrl, restCaller.Module, restCaller.StorePath);

            logger.Log($"Sending the generated PDF using a REST API. Calling to {restEndpoint}.");

            await RestCall(restEndpoint, restCaller.Token, "application/pdf", pdf);

            logger.Log($"PDF successfully sent via REST API.");
        }

        internal async Task RestSendLogs() {
            var restEndpoint = UrlUtils.BuildUrl(restCaller.BaseUrl, restCaller.Module, restCaller.LogPath);

            logger.Log($"Sending the generated Logs using a REST API. Calling to {restEndpoint}.");

            await RestCall(restEndpoint, restCaller.Token, "application/zip", logger.GetZipFile());

            logger.Log($"Logs successfully sent via REST API.");
        }

        private async Task RestCall(string endpoint, string token, string contentType, byte[] binary) {
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Content = new StreamContent(new MemoryStream(binary));
            request.Content.Headers.Add("Content-Type", contentType);
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public void Dispose() {
            client.Dispose();
        }
    }
}
