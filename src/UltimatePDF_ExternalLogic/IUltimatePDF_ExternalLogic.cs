using System.Collections.Generic;
using OutSystems.ExternalLibraries.SDK;
using OutSystems.UltimatePDF_ExternalLogic.Structures;

namespace OutSystems.UltimatePDF_ExternalLogic {

    /// <summary>
    /// The IUltimatePDF_ExternalLogic interface defines the methods (exposed as server actions)
    /// to generate PDF reports by using modern web technologies.
    /// </summary>
    [OSInterface(
        Description = "Generate PDF reports by using modern web technologies in OutSystems Developer Cloud (ODC) apps.",
        Name = "UltimatePDF_ExternalLogic",
        IconResourceName = "UltimatePDF_ExternalLogic.resources.PrintLayout.png")]
    public interface IUltimatePDF_ExternalLogic {

        /// <summary>
        /// Generate PDF file from a webpage. Optionally embeds document metadata into the resulting file.
        /// </summary>
        [OSAction(Description = "Generate PDF file from a webpage.",
            ReturnName = "PDF")]
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
            Structures.DocumentProperties? documentProperties = null);

        /// <summary>
        /// Generate PDF file from a webpage, and send the information using a REST endpoint.
        /// Optionally embeds document metadata into the resulting file.
        /// </summary>
        [OSAction(Description = "Generate PDF file from a webpage, and send the information using a REST endpoint.")]
        public void PrintPDF_ToRest(
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
            [OSParameter(Description = "Rest call configuration")]
            Structures.RestCaller restCaller,
            [OSParameter(Description = "Document metadata applied to the generated PDF")]
            Structures.DocumentProperties? documentProperties = null);

        /// <summary>
        /// Generate PDF file from a webpage, and upload it to a S3 PreSigned URL.
        /// Optionally embeds document metadata into the resulting file.
        /// </summary>
        [OSAction(Description = "Generate PDF file from a webpage, and upload it to a S3 PreSigned URL.")]
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
            Structures.DocumentProperties? documentProperties = null);

        /// <summary>
        /// Generate PDF file from a webpage and store it as a File, returning a reference to it.
        /// Optionally embeds document metadata into the resulting file.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="PrintPDF"/>, the PDF content never crosses the platform boundary, so the
        /// 5.5MB payload limit does not apply to it. The 95 second execution limit still does, and the
        /// PDF is still assembled in memory before being stored.
        /// </remarks>
        [OSAction(Description = "Generate PDF file from a webpage and store it as a File.",
            ReturnName = "PDF",
            ReturnType = OSDataType.File,
            ReturnDescription = "Reference to the stored PDF")]
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
            Structures.DocumentProperties? documentProperties = null);

        /// <summary>
        /// Generate PNG file from a webpage. Optionally embeds document metadata into the resulting file.
        /// </summary>
        [OSAction(Description = "Generate PNG file from a webpage.",
            ReturnName = "PNG")]
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
            out byte[] logsZipFile);

        /// <summary>
        /// Generate PNG file from a webpage and store it as a File, returning a reference to it.
        /// Optionally embeds document metadata into the resulting file.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="ScreenshotPNG"/>, the PNG content never crosses the platform boundary, so
        /// the 5.5MB payload limit does not apply to it. The 95 second execution limit still does.
        /// </remarks>
        [OSAction(Description = "Generate PNG file from a webpage and store it as a File.",
            ReturnName = "PNG",
            ReturnType = OSDataType.File,
            ReturnDescription = "Reference to the stored PNG")]
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
            out Structures.LogZipFiles logs);

    }
}
