using OutSystems.UltimatePDF_ExternalLogic.Utils;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests;

/// <summary>
/// Covers the naming and validation behind PrintPDF_ToFile and ScreenshotPNG_ToFile. These are the
/// parts of the File actions reachable without a browser, so they are tested here rather than in the
/// integration project.
/// </summary>
public class FileArtifactsTests {

    [Theory]
    [InlineData("", "document.pdf")]
    [InlineData("   ", "document.pdf")]
    [InlineData("\t", "document.pdf")]
    public void EnsureFileName_BlankName_FallsBackToDefault(string given, string expected) {
        // ODC sends an unfilled optional Text parameter as an empty string, so blank is the common
        // case and must produce a usable name rather than an empty one.
        Assert.Equal(expected, FileArtifacts.EnsureFileName(given, FileArtifacts.DefaultPdfFileName));
    }

    [Fact]
    public void EnsureFileName_NullName_FallsBackToDefault() {
        Assert.Equal("screenshot.png", FileArtifacts.EnsureFileName(null, FileArtifacts.DefaultPngFileName));
    }

    [Fact]
    public void EnsureFileName_NameGiven_IsUsed() {
        Assert.Equal("invoice-2026.pdf", FileArtifacts.EnsureFileName("invoice-2026.pdf", FileArtifacts.DefaultPdfFileName));
    }

    [Fact]
    public void EnsureFileName_NameWithSurroundingWhitespace_IsTrimmed() {
        Assert.Equal("invoice.pdf", FileArtifacts.EnsureFileName("  invoice.pdf  ", FileArtifacts.DefaultPdfFileName));
    }

    [Fact]
    public void MaxLogsPayloadBytes_IsTheDocumentedPlatformLimit() {
        // 5.5MB, the same figure PrintPDF enforces. Pinned as a literal so a refactor of the
        // arithmetic cannot quietly move the limit.
        Assert.Equal(5_767_168, FileArtifacts.MaxLogsPayloadBytes);
    }

    [Fact]
    public void LogsExceedPayloadLimit_LogsWithinTheLimit_IsFalse() {
        Assert.False(FileArtifacts.LogsExceedPayloadLimit(new byte[1024]));
        Assert.False(FileArtifacts.LogsExceedPayloadLimit(Array.Empty<byte>()));
    }

    [Fact]
    public void LogsExceedPayloadLimit_LogsExactlyAtTheLimit_IsFalse() {
        // The limit is the largest payload that still fits, so the boundary itself must pass.
        Assert.False(FileArtifacts.LogsExceedPayloadLimit(new byte[FileArtifacts.MaxLogsPayloadBytes]));
    }

    [Fact]
    public void LogsExceedPayloadLimit_LogsOneByteOverTheLimit_IsTrue() {
        Assert.True(FileArtifacts.LogsExceedPayloadLimit(new byte[FileArtifacts.MaxLogsPayloadBytes + 1]));
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
