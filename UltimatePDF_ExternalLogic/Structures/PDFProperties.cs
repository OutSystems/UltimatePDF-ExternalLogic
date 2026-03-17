using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.UltimatePDF_ExternalLogic.Structures {

    [OSStructure(Description = "PDF document metadata properties like title, author, subject, and keywords.")]
    public struct PDFProperties {

        /// <summary>
        /// Document title
        /// </summary>
        [OSStructureField(DataType = OSDataType.Text, Description = "The title of the PDF document")]
        public string Title;

        /// <summary>
        /// Document author
        /// </summary>
        [OSStructureField(DataType = OSDataType.Text, Description = "The author of the PDF document")]
        public string Author;

        /// <summary>
        /// Document subject
        /// </summary>
        [OSStructureField(DataType = OSDataType.Text, Description = "The subject of the PDF document")]
        public string Subject;

        /// <summary>
        /// Document keywords
        /// </summary>
        [OSStructureField(DataType = OSDataType.Text, Description = "Keywords for the PDF document (comma-separated)")]
        public string Keywords;

        /// <summary>
        /// Document creator application
        /// </summary>
        [OSStructureField(DataType = OSDataType.Text, Description = "The name of the application that created the original document")]
        public string Creator;

        /// <summary>
        /// Company name
        /// </summary>
        [OSStructureField(DataType = OSDataType.Text, Description = "The company that created the document")]
        public string Company;
    }
}
