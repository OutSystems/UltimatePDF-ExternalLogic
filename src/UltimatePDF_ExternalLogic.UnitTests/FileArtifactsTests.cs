using OutSystems.UltimatePDF_ExternalLogic.Utils;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests;

/// <summary>
/// Covers the naming and validation behind PrintPDF_ToFile and ScreenshotPNG_ToFile. These are the
/// parts of the File actions reachable without a browser, so they are tested here rather than in the
/// integration project.
/// </summary>
public class FileArtifactsTests {

    [Theory]
    // ODC sends an unfilled optional Text parameter as an empty string, so blank is the common case
    // and must produce a usable name rather than an empty one. Whitespace counts as blank.
    [InlineData("", "document.pdf")]
    [InlineData("   ", "document.pdf")]
    [InlineData("\t", "document.pdf")]
    [InlineData(null, "document.pdf")]
    [InlineData("invoice-2026.pdf", "invoice-2026.pdf")]
    [InlineData("  invoice.pdf  ", "invoice.pdf")]
    public void EnsurePdfFileName_FallsBackOrTrims(string? given, string expected) {
        Assert.Equal(expected, FileArtifacts.EnsurePdfFileName(given));
    }

    [Theory]
    [InlineData("", "screenshot.png")]
    [InlineData("   ", "screenshot.png")]
    [InlineData(null, "screenshot.png")]
    [InlineData("dashboard.png", "dashboard.png")]
    [InlineData("  dashboard.png  ", "dashboard.png")]
    public void EnsurePngFileName_FallsBackOrTrims(string? given, string expected) {
        Assert.Equal(expected, FileArtifacts.EnsurePngFileName(given));
    }

    [Theory]
    [InlineData("document.pdf", "document-logs.zip")]
    [InlineData("screenshot.png", "screenshot-logs.zip")]
    [InlineData("invoice.2026.pdf", "invoice.2026-logs.zip")]
    [InlineData("noextension", "noextension-logs.zip")]
    // Path.GetFileNameWithoutExtension(".pdf") is empty, which would leave just "-logs.zip".
    [InlineData(".pdf", ".pdf-logs.zip")]
    public void LogsFileNameFor_NamesLogsAfterTheArtifact(string artifact, string expected) {
        Assert.Equal(expected, FileArtifacts.LogsFileNameFor(artifact));
    }

    [Fact]
    public void EnsureNotEmpty_EmptyContent_ThrowsNamingTheArtifact() {
        var error = Assert.Throws<Exception>(() => FileArtifacts.EnsureNotEmpty(Array.Empty<byte>(), "PDF"));

        Assert.Contains("PDF", error.Message);
        Assert.Contains("no file was stored", error.Message);
    }

    [Fact]
    public void EnsureNotEmpty_ContentPresent_DoesNotThrow() {
        FileArtifacts.EnsureNotEmpty(new byte[] { 1 }, "PDF");
    }
}
