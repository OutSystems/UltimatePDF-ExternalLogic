using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.UltimatePDF_ExternalLogic.Structures {

    /// <summary>
    /// Rest PDF Store information. Required for the external logic to call the API.
    /// </summary>
    [OSStructure(Description = "S3 PreSigned URLs for Ultimate PDf to use to store the resulting binaries.")]
    public struct S3Endpoints {
        /// <summary>
        /// PreSigned URL to store the PDF
        /// </summary>
        [OSStructureField(Description = "PreSigned URL to store the PDF.")]
        public string PdfPreSignedUrl;

        /// <summary>
        /// PreSigned URL to store the Logs
        /// </summary>
        [OSStructureField(Description = "PreSigned URL to store the Logs.")]
        public string LogsPreSignedUrl;
    }
}
