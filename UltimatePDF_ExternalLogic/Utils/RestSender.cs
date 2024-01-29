using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;
using OutSystems.UltimatePDF_ExternalLogic.Structures;

namespace UltimatePDF_ExternalLogic.Utils {
    internal class RestSender {
        private readonly RestCaller restCaller;
        private readonly Logger logger;

        public RestSender(RestCaller restCaller, Logger logger) {
            this.restCaller = restCaller;
            this.logger = logger;
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

        private static async Task RestCall(string endpoint, string token, string contentType, byte[] binary) {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Content = new StreamContent(new MemoryStream(binary));
            request.Content.Headers.Add("Content-Type", contentType);
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            await response.Content.ReadAsStringAsync();
        }
    }
}
