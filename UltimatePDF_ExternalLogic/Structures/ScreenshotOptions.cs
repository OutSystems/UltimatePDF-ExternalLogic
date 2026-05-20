using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.UltimatePDF_ExternalLogic.Structures {

    [OSStructure(Description = "Screenshot options")]
    public struct ScreenshotOptions {

        [OSStructureField(Description = "When true, takes a screenshot of the full scrollable page. Defaults to false.")]
        public bool FullPage;

        [OSStructureField(Description = "When true, renders the page background as transparent.")]
        public bool TransparentBackground;

        [OSStructureField(Description = "Document metadata applied to the generated PNG screenshot.")]
        public DocumentProperties? DocumentProperties;
    }
}
