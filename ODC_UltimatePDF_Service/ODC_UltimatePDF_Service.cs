using OutSystems.ODC_UltimatePDF_Service.BrowserExecution;
using OutSystems.ODC_UltimatePDF_Service.Management.Troubleshooting;
using OutSystems.ODC_UltimatePDF_Service.Structures;
using OutSystems.ExternalLibraries.SDK;
using OutSystems.HeadlessChromium.Puppeteer.BrowserRevision;
using OutSystems.HeadlessChromium.Puppeteer.Utils;
using PuppeteerSharp;
using HeadlessChromium.Puppeteer.Lambda.Dotnet;

namespace OutSystems.ODC_UltimatePDF_Service {
    public class ODC_UltimatePDF_Service : IODC_UltimatePDF_Service {
        public void Management_CaptureLogs() {
            throw new NotImplementedException();
        }

        public void Management_DeleteFolder(
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string path) {
            throw new NotImplementedException();
        }

        public IEnumerable<Management_Process> Management_GetActiveProcesses() {
            throw new NotImplementedException();
        }

        public byte[] Management_GetCapturedLogs() {
            throw new NotImplementedException();
        }

        public IEnumerable<Management_BrowserRevision> Management_GetLocalBrowserRevisions(
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string temporaryFolder) {
            throw new NotImplementedException();
        }

        public IEnumerable<Management_TemporaryFile> Management_GetTemporaryFiles(
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string temporaryFolder) {
            throw new NotImplementedException();
        }

        public byte[] Management_GetTroubleshootingZipFile(
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string serviceModuleName,
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string temporaryFolder,
            [OSParameter(DataType = OSDataType.Boolean, Description = "")]
            bool preventAutomaticDownloads,
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string defaultBrowserProduct,
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string defaultChromiumRevision,
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string defaultFirefoxRevision) {
            throw new NotImplementedException();
        }

        public void Management_KillProcess([OSParameter(DataType = OSDataType.Integer, Description = "")] int ssProcessId) {
            throw new NotImplementedException();
        }

        public void Management_SetupLocalBrowser([OSParameter(DataType = OSDataType.Text, Description = "")] string product, [OSParameter(DataType = OSDataType.Text, Description = "")] string revision, [OSParameter(DataType = OSDataType.Text, Description = "")] string temporaryFolder) {
            throw new NotImplementedException();
        }

        public void MssManagement_SetupLocalBrowserFromZip([OSParameter(DataType = OSDataType.Text, Description = "")] string product, [OSParameter(DataType = OSDataType.Text, Description = "")] string revision, [OSParameter(DataType = OSDataType.BinaryData, Description = "")] byte[] zip, [OSParameter(DataType = OSDataType.Text, Description = "")] string temporaryFolder) {
            throw new NotImplementedException();
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
            int timeoutSeconds) {

            Logger logger = Logger.Instance;

            var revisionManager = new BrowserRevisionManager(ODCUltimatePDFExecutionContext.GetTempDirectory());
            var execution = new ODCUltimatePDFExecutionContext(revisionManager);

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

            IEnumerable<CookieParam> cookieParams = cookies.Select(c => new CookieParam() {
                Name = c.Name,
                Value = c.Value,
                Domain = uri.Host,
                HttpOnly = c.HttpOnly
            });

            return AsyncUtils.StartAndWait(() => execution.PrintPDF(uri, environment.BaseURL, cookieParams, viewportOpt, options, timeoutSeconds, logger));
        }

        public byte[] PrintPDF([OSParameter(DataType = OSDataType.Text, Description = "")] string url, [OSParameter(DataType = OSDataType.Text, Description = "")] string absoluteURL, [OSParameter(DataType = OSDataType.Text, Description = "")] string temporaryFolder, [OSParameter(DataType = OSDataType.Text, Description = "")] string revision, [OSParameter(Description = "")] Viewport viewport, [OSParameter(Description = "")] Structures.Environment environment, [OSParameter(Description = "")] IEnumerable<Cookie> cookies, [OSParameter(Description = "")] Paper paper, [OSParameter(DataType = OSDataType.Integer, Description = "")] int timeoutSeconds) {
            throw new NotImplementedException();
        }

        public byte[] ScreenshotPNG(
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string url,
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string absoluteURL,
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string temporaryFolder,
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string product,
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string revision,
            [OSParameter(Description = "")]
            Structures.Viewport viewport,
            [OSParameter(Description = "")]
            Structures.Environment environment,
            [OSParameter(Description = "")]
            IEnumerable<Structures.Cookie> cookies,
            [OSParameter(DataType = OSDataType.Boolean, Description = "")]
            bool fullPage,
            [OSParameter(DataType = OSDataType.Boolean, Description = "")]
            bool transparentBackground,
            [OSParameter(DataType = OSDataType.Integer, Description = "")]
            int timeoutSeconds) {
            throw new NotImplementedException();
        }
    }
}
