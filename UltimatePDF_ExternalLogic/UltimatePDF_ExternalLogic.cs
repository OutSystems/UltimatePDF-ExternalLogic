using OutSystems.UltimatePDF_ExternalLogic.BrowserExecution;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;
using OutSystems.ExternalLibraries.SDK;
using OutSystems.UltimatePDF_ExternalLogic.Utils;
using PuppeteerSharp;
using OutSystems.UltimatePDF_ExternalLogic.Structures;

namespace OutSystems.UltimatePDF_ExternalLogic {
    public class UltimatePDF_ExternalLogic : IUltimatePDF_ExternalLogic {

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
            [OSParameter(DataType = OSDataType.BinaryData, Description = "PDF generation task logs")]
            out byte[] logsZipFile) {

            Logger logger = Logger.GetLogger(collectLogs);

            var execution = new ODCUltimatePDFExecutionContext();

            var viewportOpt = new ViewPortOptions() {
                Width = viewport.Width,
                Height = viewport.Height
            };

            PdfOptions options = new PdfOptions() {
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

            if(uri == null) {
                throw new UriFormatException();
            }

            IEnumerable<CookieParam> cookieParams = cookies.Select(c => new CookieParam() {
                Name = c.Name,
                Value = c.Value,
                Domain = uri.Host,
                HttpOnly = c.HttpOnly
            });

            byte[] pdf = new byte[] { };

            try {
                pdf = AsyncUtils.StartAndWait(
                    () => execution.PrintPDF(uri, environment.BaseURL, environment.Locale, 
                                             environment.Timezone, cookieParams, viewportOpt,
                                             options, timeoutSeconds, logger));
            } catch (Exception ex) {
                logger.Error(ex);
            }
            
            logsZipFile = logger.GetZipFile();

            float mb = ((pdf.Length + logsZipFile.Length) / 1024f) / 1024f;

            if(mb > 5.5) {
                throw new Exception($"Output payload is too large ({mb}MB), maximum allowd is 5.5MB. To overcome this limitation use PrintPDF to REST action.");
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
            IEnumerable<Cookie> cookies, 
            [OSParameter(Description = "PDF paper configuration")] 
            Paper paper, 
            [OSParameter(DataType = OSDataType.Integer, Description = "Browser render execution timeout in seconds")] 
            int timeoutSeconds, 
            [OSParameter(DataType = OSDataType.Boolean, Description = "Collects execution logs. If False LogsZipFile will be empty.")] 
            bool collectLogs, 
            [OSParameter(Description = "Rest call configuration")] 
            RestCaller restCaller) {
            var pdf = PrintPDF(url, viewport, environment, cookies, paper, timeoutSeconds, collectLogs, out byte[] logs);

            var execution = new ODCUltimatePDFExecutionContext();
            Logger logger = Logger.GetLogger(collectLogs);

            if (collectLogs) {
                AsyncUtils.StartAndWait(() => execution.RestSendLogs(restCaller, logs, logger));
            }

            AsyncUtils.StartAndWait(() => execution.RestSendPDFAsync(restCaller, pdf, logger));
        }
    }
}
