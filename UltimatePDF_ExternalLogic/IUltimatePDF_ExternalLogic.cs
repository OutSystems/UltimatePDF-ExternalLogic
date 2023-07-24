using System.Collections.Generic;
using OutSystems.ExternalLibraries.SDK;

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
		/// Generate PDF file from a webpage
		/// </summary>
        [OSAction(Description = "Generate PDF file from a webpage",
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
            [OSParameter(DataType = OSDataType.BinaryData, Description = "PDF generation task logs")]
            out byte[] logsZipFile);

        /// <summary>
        /// Generate PDF file from a webpage
        /// </summary>
        [OSAction(Description = "Generate PDF file from a webpage, and send the information using a REST endpoint")]
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
            [OSParameter(Description = "Rest call configuration")]
            Structures.RestCaller restCaller);
    }
}
