﻿using OutSystems.ODC_UltimatePDF_Service.BrowserExecution;
using OutSystems.ODC_UltimatePDF_Service.Management.Troubleshooting;
using OutSystems.ExternalLibraries.SDK;
using OutSystems.ODC_UltimatePDF_Service.Utils;
using PuppeteerSharp;

namespace OutSystems.ODC_UltimatePDF_Service {
    public class ODC_UltimatePDF_Service : IODC_UltimatePDF_Service {

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
            [OSParameter(DataType = OSDataType.BinaryData, Description = "PDF generation task logs")]
            out byte[] logsZipFile) {

            Logger logger = Logger.Instance;

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
                pdf = AsyncUtils.StartAndWait(() => execution.PrintPDF(uri, environment.BaseURL, cookieParams, viewportOpt, options, timeoutSeconds, logger));
            } catch (Exception ex) {
                logger.Error(ex);
            }
            
            logsZipFile = logger.GetZipFile();
            return pdf;
        }
    }
}