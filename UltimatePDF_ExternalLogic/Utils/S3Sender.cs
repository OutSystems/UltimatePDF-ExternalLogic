using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;

namespace UltimatePDF_ExternalLogic.Utils {
    internal class S3Sender {
        private readonly string pdfPreSignedUrl;
        private readonly string logsPreSignedUrl;
        private readonly Logger logger;

        public S3Sender(string pdfPreSignedUrl, string logsPreSignedUrl, Logger logger) {
            this.pdfPreSignedUrl = pdfPreSignedUrl;
            this.logsPreSignedUrl = logsPreSignedUrl;
            this.logger = logger;
        }

        internal async Task S3SendPDFAsync(byte[] pdf) {
            if(string.IsNullOrEmpty(this.pdfPreSignedUrl)) {
                logger.Log("The PreSigned URL to store the PDF is null or empty.");
                return;
            }

            if(pdf == null || pdf.Length == 0) {
                logger.Log("The PDF binary is null or empty.");
                return;
            }

            logger.Log($"Sending the generated PDF to S3. Calling to {pdfPreSignedUrl}.");

            using (HttpClient client = new HttpClient()) {
                HttpResponseMessage response = await client.PutAsync(pdfPreSignedUrl, new ByteArrayContent(pdf));
                response.EnsureSuccessStatusCode();
            }

            logger.Log($"PDF successfully sent to S3.");
        }

        internal async Task S3SendLogsAsync() {
            if (string.IsNullOrEmpty(this.logsPreSignedUrl)) {
                logger.Log("The PreSigned URL to store the PDF is null or empty.");
                return;
            }

            logger.Log($"Sending the generated Logs to S3. Calling to {logsPreSignedUrl}.");

            using (HttpClient client = new HttpClient()) {
                HttpResponseMessage response = await client.PutAsync(logsPreSignedUrl, new ByteArrayContent(logger.GetZipFile()));
                response.EnsureSuccessStatusCode();
            }

            logger.Log($"Logs successfully sent to S3.");
        }
    }
}
