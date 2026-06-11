namespace OutSystems.UltimatePDF_ExternalLogic.E2ETests.Models;

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
    public double Width { get; set; }
    public double Height { get; set; }
    public bool UseCustomMargins { get; set; }
    public double MarginTop { get; set; }
    public double MarginRight { get; set; }
    public double MarginBottom { get; set; }
    public double MarginLeft { get; set; }
}

internal sealed class EnvironmentDto {
    public string BaseURL { get; set; } = string.Empty;
    public string Locale { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
}

internal sealed class DocumentPropertiesDto {
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Keywords { get; set; } = string.Empty;
    public string Creator { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Producer { get; set; } = string.Empty;
    public string Copyright { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}
