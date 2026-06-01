namespace OutSystems.UltimatePDF_ExternalLogic.TenantTests.Models;

internal sealed class PrintPdfRequest {
    public string Url { get; set; } = string.Empty;
    public CookieEntry[] Cookies { get; set; } = [];
    public bool AttachFilesLogs { get; set; }
    public bool CollectLogs { get; set; }
    public int TimeoutSeconds { get; set; } = 60;
    public ViewportDto? Viewport { get; set; }
    public PaperDto? Paper { get; set; }
    public EnvironmentDto? Environment { get; set; }
    public DocumentPropertiesDto? DocumentProperties { get; set; }
}

internal sealed class CookieEntry {
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool HttpOnly { get; set; }
}

internal sealed class ViewportDto {
    public int Width { get; set; }
    public int Height { get; set; }
}

internal sealed class PaperDto {
    public bool UseCustomPaper { get; set; }
}

internal sealed class EnvironmentDto {
    public string BaseURL { get; set; } = string.Empty;
}

internal sealed class DocumentPropertiesDto {
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
}
