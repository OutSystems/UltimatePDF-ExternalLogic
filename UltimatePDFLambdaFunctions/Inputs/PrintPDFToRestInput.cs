using OutSystems.UltimatePDF_ExternalLogic.Structures;
using Environment = OutSystems.UltimatePDF_ExternalLogic.Structures.Environment;

namespace UltimatePDFLambdaFunctions.Inputs
{
    public class PrintPDFToRestInput
    {
        public string Url { get; init; } = String.Empty;
        public Viewport Viewport { get; init; } = new Viewport { Width = 1366, Height = 768 };
        public Environment Environment { get; init; } = new Environment
        {
            BaseURL = "",
            Locale = "en",
            Timezone = "Europe/Lisbon"
        };
        public IEnumerable<Cookie> Cookies { get; init; } = Array.Empty<Cookie>();
        public Paper Paper { get; init; } = new Paper
        {
            UseCustomPaper = false,
            Width = 21,
            Height = 29.7M,
            UseCustomMargins = false,
            MarginTop = 2.54M,
            MarginRight = 2.54M,
            MarginBottom = 2.54M,
            MarginLeft = 2.54M
        };
        public int TimeoutSeconds { get; init; } = 120;
        public bool CollectLogs { get; init; } = false;
        public bool AttachFilesLogs { get; init; } = false;
        public RestCaller RestCaller { get; init; } = default;
    }
}
