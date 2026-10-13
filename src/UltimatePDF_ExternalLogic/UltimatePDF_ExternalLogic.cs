using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using OutSystems.ExternalLibraries.SDK;
using OutSystems.ExternalLibraries.SDK.FileStorage;
using OutSystems.UltimatePDF_ExternalLogic.BrowserExecution;
using OutSystems.UltimatePDF_ExternalLogic.Management;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;
using OutSystems.UltimatePDF_ExternalLogic.Structures;
using OutSystems.UltimatePDF_ExternalLogic.Utils;
using PuppeteerSharp;
using UltimatePDF_ExternalLogic.Utils;

namespace OutSystems.UltimatePDF_ExternalLogic {
    public class UltimatePDF_ExternalLogic : IUltimatePDF_ExternalLogic {

        private readonly ILogger _odcLogger;
        private readonly Lazy<IFileStorage> _storage;

        /// <summary>
        /// Constructor the ODC platform uses, receiving the platform's logger.
        /// </summary>
        /// <remarks>
        /// The storage instance is resolved lazily. <c>FileStorage.GetInstance</c> depends on a loader
        /// that the External Library Worker generates at ODC build time, so resolving it eagerly would
        /// make this class impossible to construct anywhere else, including in the existing tests.
        /// </remarks>
        public UltimatePDF_ExternalLogic(ILogger logger)
            : this(new Lazy<IFileStorage>(FileStorage.GetInstance), logger) {
        }

        /// <summary>Test constructor that injects a storage implementation.</summary>
        internal UltimatePDF_ExternalLogic(IFileStorage storage, ILogger logger)
            : this(new Lazy<IFileStorage>(() => storage), logger) {
        }

        private UltimatePDF_ExternalLogic(Lazy<IFileStorage> storage, ILogger logger) {
            _storage = storage;
            _odcLogger = logger;
        }

        private IFileStorage Storage => _storage.Value;

        private static byte[] InnerPrintPDF(string url, Viewport viewport, Structures.Environment environment,
                                     IEnumerable<Cookie> cookies, Paper paper, DocumentProperties? documentProperties,
                                     int timeoutSeconds, Logger logger) {

            if (!UrlUtils.IsValidHttpsUri(url)) {
                logger.Error($"The url:'{url}' is not a well-formed, absolute URI with an HTTPS scheme.");
                throw new UriFormatException("Not a well-formed, absolute URI with an HTTPS scheme");
            }

            var viewportOpt = new ViewPortOptions() {
                Width = viewport.Width,
                Height = viewport.Height
            };

            var options = new PdfOptions() {
                OmitBackground = true,
                PrintBackground = true,
                PreferCSSPageSize = true
            };

            if (paper.UseCustomPaper && paper.Width > 0 && paper.Height > 0) {
                options.Width = $"{paper.Width}cm";
                options.Height = $"{paper.Height}cm";
                options.PreferCSSPageSize = false;
            }

            if (paper.UseCustomMargins && paper.MarginTop >= 0 && paper.MarginRight >= 0
                && paper.MarginBottom >= 0 && paper.MarginLeft >= 0) {
                options.MarginOptions.Top = $"{paper.MarginTop}cm";
                options.MarginOptions.Right = $"{paper.MarginRight}cm";
                options.MarginOptions.Bottom = $"{paper.MarginBottom}cm";
                options.MarginOptions.Left = $"{paper.MarginLeft}cm";
            }

            Uri.TryCreate(url, UriKind.Absolute, out var uri);

            if (uri == null) {
                throw new UriFormatException();
            }

            IEnumerable<CookieParam> cookieParams = cookies.Select(c => new CookieParam() {
                Name = c.Name,
                Value = c.Value,
                Domain = uri.Host,
                HttpOnly = c.HttpOnly
            });

            byte[] pdf = Array.Empty<byte>();

            try {
                pdf = AsyncUtils.StartAndWait(
                    () => UltimatePDFExecutionContext.PrintPDF(uri, environment.BaseURL, environment.Locale,
                                             environment.Timezone, cookieParams, viewportOpt,
                                             options, timeoutSeconds, logger));
                pdf = PDFMetadataUtil.ApplyMetadata(pdf, documentProperties ?? default, logger);
            } catch (Exception ex) {
                logger.Error(ex, "Something went wrong generating the PDF");
            }

            return pdf;
        }

        public byte[] PrintPDF(
            [OSParameter(DataType = OSDataType.Text, Description = "URL of the page to download")]
            string url,
            [OSParameter(Description = "Bowser viewport configuration")]
            Structures.Viewport viewport,
            [OSParameter(Description = "Environment information")]
            Structures.Environment environment,
            [OSParameter(Description = "List of cookies to add to the browser when accessing the page")]
            IEnumerable<Structures.Cookie> cookies,
            [OSParameter(Description = "PDF paper configuration")]
            Structures.Paper paper,
            [OSParameter(DataType = OSDataType.Integer, Description = "Browser render execution timeout in seconds")]
            int timeoutSeconds,
            [OSParameter(DataType = OSDataType.Boolean, Description = "Collects execution logs. If False LogsZipFile will be empty.")]
            bool collectLogs,
            [OSParameter(DataType = OSDataType.Boolean, Description = "Attach PDF and HTML files to the logs.")]
            bool attachFilesLogs,
            [OSParameter(DataType = OSDataType.BinaryData, Description = "PDF generation task logs")]
            out byte[] logsZipFile,
            [OSParameter(Description = "Document metadata applied to the generated PDF")]
            Structures.DocumentProperties? documentProperties = null) {

            var logger = Logger.GetLogger(_odcLogger, collectLogs, attachFilesLogs);

            var pdf = InnerPrintPDF(url, viewport, environment, cookies, paper, documentProperties, timeoutSeconds, logger);

            logsZipFile = logger.GetZipFile();

            float mb = ((pdf.Length + logsZipFile.Length) / 1024f) / 1024f;

            if (mb > 5.5) {
                throw new Exception($"Output payload is too large ({mb}MB), maximum allowed is 5.5MB. To overcome this limitation use PrintPDF to REST action.");
            }

            return pdf;
        }

        public void PrintPDF_ToRest(
            [OSParameter(DataType = OSDataType.Text, Description = "URL of the page to download")]
            string url,
            [OSParameter(Description = "Bowser viewport configuration")]
            Viewport viewport,
            [OSParameter(Description = "Environment information")]
            Structures.Environment environment,
            [OSParameter(Description = "List of cookies to add to the browser when accessing the page")]
            IEnumerable<Structures.Cookie> cookies,
            [OSParameter(Description = "PDF paper configuration")]
            Paper paper,
            [OSParameter(DataType = OSDataType.Integer, Description = "Browser render execution timeout in seconds")]
            int timeoutSeconds,
            [OSParameter(DataType = OSDataType.Boolean, Description = "Collects execution logs. If False LogsZipFile will be empty.")]
            bool collectLogs,
            [OSParameter(DataType = OSDataType.Boolean, Description = "Attach PDF and HTML files to the logs.")]
            bool attachFilesLogs,
            [OSParameter(Description = "Rest call configuration")]
            RestCaller restCaller,
            [OSParameter(Description = "Document metadata applied to the generated PDF")]
            Structures.DocumentProperties? documentProperties = null) {

            var logger = Logger.GetLogger(_odcLogger, collectLogs, attachFilesLogs);

            logger.Log($"Print PDF Rest call for {restCaller.Token}");

            using var restSender = new RestSender(restCaller, logger);

            var pdf = InnerPrintPDF(url, viewport, environment, cookies, paper, documentProperties, timeoutSeconds, logger);

            logger.Log($"Prepare to send information to the REST API");

            try {
                AsyncUtils.StartAndWait(() => restSender.RestSendPDFAsync(pdf));
            } catch (Exception ex) {
                logger.Error(ex, "Error sending PDF using REST API.");
            }

            if (collectLogs) {
                AsyncUtils.StartAndWait(restSender.RestSendLogs);
            }
        }

        public void PrintPDF_ToS3(
            [OSParameter(DataType = OSDataType.Text, Description = "URL of the page to download")]
            string url,
            [OSParameter(Description = "Bowser viewport configuration")]
            Viewport viewport,
            [OSParameter(Description = "Environment information")]
            Structures.Environment environment,
            [OSParameter(Description = "List of cookies to add to the browser when accessing the page")]
            IEnumerable<Structures.Cookie> cookies,
            [OSParameter(Description = "PDF paper configuration")]
            Paper paper,
            [OSParameter(DataType = OSDataType.Integer, Description = "Browser render execution timeout in seconds")]
            int timeoutSeconds,
            [OSParameter(DataType = OSDataType.Boolean, Description = "Collects execution logs. If False LogsZipFile will be empty")]
            bool collectLogs,
            [OSParameter(DataType = OSDataType.Boolean, Description = "Attach PDF and HTML files to the logs")]
            bool attachFilesLogs,
            [OSParameter(Description = "S3 PreSigned URLs for Ultimate PDf to use to store the resulting binaries")]
            S3Endpoints s3Endpoints,
            [OSParameter(Description = "Document metadata applied to the generated PDF")]
            Structures.DocumentProperties? documentProperties = null) {

            var logger = Logger.GetLogger(_odcLogger, collectLogs, attachFilesLogs);

            S3Sender s3Sender = new(s3Endpoints.PdfPreSignedUrl, s3Endpoints.LogsPreSignedUrl, logger);

            var pdf = InnerPrintPDF(url, viewport, environment, cookies, paper, documentProperties, timeoutSeconds, logger);

            logger.Log($"Prepare to send information to the REST API");

            AsyncUtils.StartAndWait(() => s3Sender.S3SendPDFAsync(pdf));
            AsyncUtils.StartAndWait(() => s3Sender.S3SendLogsAsync());
        }

        public IOSFile PrintPDF_ToFile(
            [OSParameter(DataType = OSDataType.Text, Description = "URL of the page to download")]
            string url,
            [OSParameter(Description = "Bowser viewport configuration")]
            Structures.Viewport viewport,
            [OSParameter(Description = "Environment information")]
            Structures.Environment environment,
            [OSParameter(Description = "List of cookies to add to the browser when accessing the page")]
            IEnumerable<Structures.Cookie> cookies,
            [OSParameter(Description = "PDF paper configuration")]
            Structures.Paper paper,
            [OSParameter(DataType = OSDataType.Integer, Description = "Browser render execution timeout in seconds")]
            int timeoutSeconds,
            [OSParameter(Description = "Troubleshooting log settings for this task")]
            Structures.LogOptions logOptions,
            [OSParameter(DataType = OSDataType.Text, Description = "Name for the stored PDF file. Empty defaults to document.pdf")]
            string fileName,
            [OSParameter(Description = "PDF generation task logs, stored as a File")]
            out Structures.LogZipFiles logs,
            [OSParameter(Description = "Document metadata applied to the generated PDF")]
            Structures.DocumentProperties? documentProperties = null) {

            var logger = logOptions.CreateLogger(_odcLogger);

            var name = FileArtifacts.EnsurePdfFileName(fileName);

            var pdf = InnerPrintPDF(url, viewport, environment, cookies, paper, documentProperties, timeoutSeconds, logger);

            FileArtifacts.EnsureNotEmpty(pdf, "PDF");

            var file = StoreFile(name, FileArtifacts.PdfContentType, pdf);

            logs = LogOptions.StoreZip(logger, name, StoreFile);

            return file;
        }

        public byte[] ScreenshotPNG(
            [OSParameter(DataType = OSDataType.Text, Description = "URL of the page to download")]
            string url,
            [OSParameter(Description = "Bowser viewport configuration")]
            Structures.Viewport viewport,
            [OSParameter(Description = "Environment information")]
            Structures.Environment environment,
            [OSParameter(Description = "List of cookies to add to the browser when accessing the page")]
            IEnumerable<Structures.Cookie> cookies,
            [OSParameter(Description = "PNG paper configuration")]
            Structures.Paper paper,
            [OSParameter(Description = "PNG screenshot options")]
            Structures.ScreenshotOptions screenshotOptions,
            [OSParameter(DataType = OSDataType.Integer, Description = "Browser render execution timeout in seconds")]
            int timeoutSeconds,
            [OSParameter(DataType = OSDataType.Boolean, Description = "Collects execution logs. If False LogsZipFile will be empty.")]
            bool collectLogs,
            [OSParameter(DataType = OSDataType.Boolean, Description = "Attach PDF and HTML files to the logs.")]
            bool attachFilesLogs,
            [OSParameter(DataType = OSDataType.BinaryData, Description = "PDF generation task logs")]
            out byte[] logsZipFile) {

            var logger = Logger.GetLogger(_odcLogger, collectLogs, attachFilesLogs);

            var png = InnerScreenshotPNG(url, viewport, environment, cookies, screenshotOptions, timeoutSeconds, logger);

            logsZipFile = logger.GetZipFile();

            return png;
        }

        public IOSFile ScreenshotPNG_ToFile(
            [OSParameter(DataType = OSDataType.Text, Description = "URL of the page to download")]
            string url,
            [OSParameter(Description = "Bowser viewport configuration")]
            Structures.Viewport viewport,
            [OSParameter(Description = "Environment information")]
            Structures.Environment environment,
            [OSParameter(Description = "List of cookies to add to the browser when accessing the page")]
            IEnumerable<Structures.Cookie> cookies,
            [OSParameter(Description = "PNG paper configuration")]
            Structures.Paper paper,
            [OSParameter(Description = "PNG screenshot options")]
            Structures.ScreenshotOptions screenshotOptions,
            [OSParameter(DataType = OSDataType.Integer, Description = "Browser render execution timeout in seconds")]
            int timeoutSeconds,
            [OSParameter(Description = "Troubleshooting log settings for this task")]
            Structures.LogOptions logOptions,
            [OSParameter(DataType = OSDataType.Text, Description = "Name for the stored PNG file. Empty defaults to screenshot.png")]
            string fileName,
            [OSParameter(Description = "PNG generation task logs, stored as a File")]
            out Structures.LogZipFiles logs) {

            var logger = logOptions.CreateLogger(_odcLogger);

            var name = FileArtifacts.EnsurePngFileName(fileName);

            var png = InnerScreenshotPNG(url, viewport, environment, cookies, screenshotOptions, timeoutSeconds, logger);

            FileArtifacts.EnsureNotEmpty(png, "PNG");

            var file = StoreFile(name, FileArtifacts.PngContentType, png);

            logs = LogOptions.StoreZip(logger, name, StoreFile);

            return file;
        }

        private static byte[] InnerScreenshotPNG(string url, Structures.Viewport viewport,
                                          Structures.Environment environment, IEnumerable<Structures.Cookie> cookies,
                                          Structures.ScreenshotOptions screenshotOptions, int timeoutSeconds,
                                          Logger logger) {

            if (!UrlUtils.IsValidHttpsUri(url)) {
                logger.Error($"The url:'{url}' is not a well-formed, absolute URI with an HTTPS scheme.");
                throw new UriFormatException("Not a well-formed, absolute URI with an HTTPS scheme");
            }

            var viewportOpt = new ViewPortOptions() {
                Width = viewport.Width,
                Height = viewport.Height
            };

            var options = new PuppeteerSharp.ScreenshotOptions() {
                FullPage = screenshotOptions.FullPage,
                OmitBackground = screenshotOptions.TransparentBackground
            };

            Uri.TryCreate(url, UriKind.Absolute, out var uri);

            if (uri == null) {
                throw new UriFormatException();
            }

            logger.Log($"Input URL: {url}");

            logger.Log($"Browser will retrieve the document from: {uri}");
            logger.Log($"Browser will set a Base URL for relative hyperlinks: {environment.BaseURL}");

            logger.Log($"Locale to be used: {environment.Locale}", !string.IsNullOrEmpty(environment.Locale));
            logger.Log($"Timezone to be used: {environment.Timezone}", !string.IsNullOrEmpty(environment.Timezone));

            IEnumerable<CookieParam> cookieParams = cookies.Select(c => new CookieParam() {
                Name = c.Name,
                Value = c.Value,
                Domain = uri.Host,
                HttpOnly = c.HttpOnly
            });

            byte[] png;
            try {
                png = AsyncUtils.StartAndWait(
                   () => UltimatePDFExecutionContext.ScreenshotPNG(uri, environment.BaseURL, environment.Locale,
                                                 environment.Timezone, cookieParams, viewportOpt,
                                                 options, timeoutSeconds, logger));
            } catch (Exception e) {
                logger.Error(e, "Something went wrong generating the screenshot");
                throw;
            }

            return PNGMetadataUtil.ApplyMetadata(png, screenshotOptions.DocumentProperties ?? default, logger);
        }

        private IOSFile StoreFile(string fileName, string contentType, byte[] content) {
            return AsyncUtils.StartAndWait(() => Storage.CreateFileAsync(fileName, contentType, content));
        }

    }
}
