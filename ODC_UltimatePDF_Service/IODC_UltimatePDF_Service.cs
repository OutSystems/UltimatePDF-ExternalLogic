using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.ODC_UltimatePDF_Service {

    /// <summary>
    /// The IODC_UltimatePDF_Service interface defines the methods (exposed as server actions)
    /// to generate PDF reports by using modern web technologies.
    /// </summary>
    [OSInterface(
        Description = "Generate PDF reports by using modern web technologies in OutSystems Developer Cloud (ODC) apps.",
        Name = "ODC_UltimatePDF_Service")]
    public interface IODC_UltimatePDF_Service {

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
            [OSParameter(DataType = OSDataType.BinaryData, Description = "PDF generation task logs")]
            out byte[] logsZipFile);
    }
}
