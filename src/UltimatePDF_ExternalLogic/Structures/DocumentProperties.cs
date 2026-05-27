using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.UltimatePDF_ExternalLogic.Structures {

    [OSStructure(Description = "Document metadata properties applied to generated PDFs and PNG "
                             + "screenshots. All fields are optional; when every field is empty "
                             + "or whitespace, no metadata is embedded.")]
    public struct DocumentProperties {

        /// <summary>
        /// Document title
        /// </summary>
        [OSStructureField(DataType = OSDataType.Text, Description = "The title of the document")]
        public string Title;

        /// <summary>
        /// Document author
        /// </summary>
        [OSStructureField(DataType = OSDataType.Text, Description = "The author of the document")]
        public string Author;

        /// <summary>
        /// Document subject
        /// </summary>
        [OSStructureField(DataType = OSDataType.Text, Description = "The subject of the document")]
        public string Subject;

        /// <summary>
        /// Document keywords
        /// </summary>
        [OSStructureField(DataType = OSDataType.Text, Description = "Keywords for the document (comma-separated)")]
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

        /// <summary>
        /// Producing application
        /// </summary>
        [OSStructureField(DataType = OSDataType.Text,
            Description = "Application producing the file. Embedded as the '/Producer' value in the PDF Info "
                        + "dictionary (PdfSharp prepends its own producer string when saving) and written as "
                        + "the PNG 'Software' text chunk.")]
        public string Producer;

        /// <summary>
        /// Copyright statement
        /// </summary>
        [OSStructureField(DataType = OSDataType.Text,
            Description = "Copyright or rights statement.")]
        public string Copyright;

        /// <summary>
        /// Document language (BCP-47)
        /// </summary>
        [OSStructureField(DataType = OSDataType.Text,
            Description = "Document language as a BCP-47 tag (e.g. en-US, pt-PT). Passed through unchanged.")]
        public string Language;

        /// <summary>
        /// Originating system or data source
        /// </summary>
        [OSStructureField(DataType = OSDataType.Text,
            Description = "Originating system or data source description.")]
        public string Source;

        /// <summary>
        /// Returns true when every metadata field is null, empty, or whitespace.
        /// Used by the PDF and PNG metadata utilities to short-circuit embedding.
        /// </summary>
        public bool IsEmpty() {
            return string.IsNullOrWhiteSpace(Title)
                && string.IsNullOrWhiteSpace(Author)
                && string.IsNullOrWhiteSpace(Subject)
                && string.IsNullOrWhiteSpace(Keywords)
                && string.IsNullOrWhiteSpace(Creator)
                && string.IsNullOrWhiteSpace(Company)
                && string.IsNullOrWhiteSpace(Producer)
                && string.IsNullOrWhiteSpace(Copyright)
                && string.IsNullOrWhiteSpace(Language)
                && string.IsNullOrWhiteSpace(Source);
        }
    }
}
