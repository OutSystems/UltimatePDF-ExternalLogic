namespace OutSystems.UltimatePDF_ExternalLogic.E2ETests.Models;

/// <summary>
/// Body for the PrintPDF_ToFile REST method. Same shape as <see cref="PrintPdfRequest"/> plus the
/// FileName the action uses to name the stored File.
/// </summary>
internal sealed class PrintPdfToFileRequest {
    public string Url { get; set; } = string.Empty;
    public CookieEntry[] Cookies { get; set; } = [];
    public bool AttachFilesLogs { get; set; }
    public bool CollectLogs { get; set; }
    public int TimeoutSeconds { get; set; } = 60;
    public string FileName { get; set; } = string.Empty;
    public ViewportDto? Viewport { get; set; }
    public PaperDto? Paper { get; set; }
    public EnvironmentDto? Environment { get; set; }
    public DocumentPropertiesDto? DocumentProperties { get; set; }
}
