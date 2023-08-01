using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.UltimatePDF_ExternalLogic.Structures {

    [OSStructure(Description = "Screenshot options")]
    public struct ScreenshotOptions {

        [OSStructureField(Description = "When true, takes a screenshot of the full scrollable page. Defaults to false.")]
        public bool FullPage;

        [OSStructureField(Description = "")]
        public bool TransparentBackground;
    }
}
