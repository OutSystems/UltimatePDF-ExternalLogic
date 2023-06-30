using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutSystems.ExternalLibraries.SDK;
using System.Xml.Linq;

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
		/// 
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
            int timeoutSeconds);

        /// <summary>
        /// 
        /// </summary>
        [OSAction(Description = "")]
        public void Management_SetupLocalBrowser(
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string product,
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string revision,
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string temporaryFolder);

        /// <summary>
        /// 
        /// </summary>
        [OSAction(Description = "",
            ReturnName = "PNG")]
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
            int timeoutSeconds);

        /// <summary>
        /// 
        /// </summary>
        [OSAction(Description = "")]
        public void MssManagement_SetupLocalBrowserFromZip(
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string product,
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string revision,
            [OSParameter(DataType =OSDataType.BinaryData, Description = "")]
            byte[] zip,
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string temporaryFolder);

        /// <summary>
        /// 
        /// </summary>
        [OSAction(Description = "",
            ReturnName = "ActiveProcesses")]
        public IEnumerable<Structures.Management_Process> Management_GetActiveProcesses();

        /// <summary>
        /// 
        /// </summary>
        [OSAction(Description = "",
            ReturnName = "ActiveProcesses")]
        public void Management_KillProcess(
            [OSParameter(DataType = OSDataType.Integer, Description = "")]
            int processId);

        /// <summary>
        /// 
        /// </summary>
        [OSAction(Description = "",
            ReturnName = "TroubleshootingZipFile")]
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
            string defaultFirefoxRevision);

        /// <summary>
        /// 
        /// </summary>
        [OSAction(Description = "",
            ReturnName = "BrowserRevisions")]
        public IEnumerable<Structures.Management_BrowserRevision> Management_GetLocalBrowserRevisions(
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string temporaryFolder);

        /// <summary>
        /// 
        /// </summary>
        [OSAction(Description = "")]
        public void Management_DeleteFolder(
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string path);

        /// <summary>
        /// 
        /// </summary>
        [OSAction(Description = "",
            ReturnName = "TemporaryFiles")]
        public IEnumerable<Structures.Management_TemporaryFile> Management_GetTemporaryFiles(
            [OSParameter(DataType = OSDataType.Text, Description = "")]
            string temporaryFolder);

        /// <summary>
        /// 
        /// </summary>
        [OSAction(Description = "")]
        public void Management_CaptureLogs();

        /// <summary>
        /// 
        /// </summary>
        [OSAction(Description = "",
            ReturnName = "LogsZipFile")]
        public byte[] Management_GetCapturedLogs();
    }
}
