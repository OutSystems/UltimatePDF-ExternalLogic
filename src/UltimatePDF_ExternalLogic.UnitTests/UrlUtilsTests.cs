using UltimatePDF_ExternalLogic.Utils;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {
    public class UrlUtilsTests {

        // BuildUrl tests

        [Fact]
        public void BuildUrl_BaseWithoutHttpsPrefix_AddsPrefix() {
            // Arrange + Act
            var result = UrlUtils.BuildUrl("example.com", "/m", "/p");

            // Assert
            Assert.StartsWith("https://", result);
        }

        [Fact]
        public void BuildUrl_BaseWithHttpsPrefix_NoDoublePrefix() {
            // Arrange + Act
            var result = UrlUtils.BuildUrl("https://example.com", "/m", "/p");

            // Assert
            Assert.Equal(1, result.Split("https://").Length - 1);
        }

        [Fact]
        public void BuildUrl_ModuleWithoutLeadingSlash_AddsSlash() {
            // Arrange + Act
            var result = UrlUtils.BuildUrl("https://example.com", "module", "/path");

            // Assert
            Assert.Contains("/module", result);
        }

        [Fact]
        public void BuildUrl_PathWithoutLeadingSlash_AddsSlash() {
            // Arrange + Act
            var result = UrlUtils.BuildUrl("https://example.com", "/module", "path");

            // Assert
            Assert.Contains("/path", result);
        }

        [Fact]
        public void BuildUrl_BaseWithTrailingSlash_RemovesTrailingSlash() {
            // Arrange + Act
            var result = UrlUtils.BuildUrl("https://example.com/", "/m", "/p");

            // Assert
            Assert.DoesNotContain("//m", result);
        }

        [Fact]
        public void BuildUrl_ModuleWithTrailingSlash_RemovesTrailingSlash() {
            // Arrange + Act
            var result = UrlUtils.BuildUrl("https://example.com", "/module/", "/path");

            // Assert
            Assert.DoesNotContain("//path", result);
        }

        [Fact]
        public void BuildUrl_AllSegments_CombinesCorrectly() {
            // Arrange + Act
            var result = UrlUtils.BuildUrl("https://example.com", "/api", "/resource");

            // Assert
            Assert.Equal("https://example.com/api/resource", result);
        }

        // GetFilenameFromUrl tests

        [Fact]
        public void GetFilenameFromUrl_SimpleFilename_ReturnsFilename() {
            // Arrange + Act
            var result = UrlUtils.GetFilenameFromUrl("https://example.com/files/report.pdf");

            // Assert
            Assert.Equal("report.pdf", result);
        }

        [Fact]
        public void GetFilenameFromUrl_MultiSegmentPath_ReturnsLastSegment() {
            // Arrange + Act
            var result = UrlUtils.GetFilenameFromUrl("https://example.com/a/b/c/file.txt");

            // Assert
            Assert.Equal("file.txt", result);
        }

        [Fact]
        public void GetFilenameFromUrl_NoExtension_ReturnsSegment() {
            // Arrange + Act
            var result = UrlUtils.GetFilenameFromUrl("https://example.com/path/document");

            // Assert
            Assert.Equal("document", result);
        }

        // IsValidHttpsUri tests

        [Fact]
        public void IsValidHttpsUri_ValidHttpsUrl_ReturnsTrue() {
            // Arrange + Act + Assert
            Assert.True(UrlUtils.IsValidHttpsUri("https://example.com/path"));
        }

        [Fact]
        public void IsValidHttpsUri_ValidHttpUrl_ReturnsTrue() {
            // Arrange + Act + Assert
            Assert.True(UrlUtils.IsValidHttpsUri("http://example.com/path"));
        }

        [Fact]
        public void IsValidHttpsUri_FtpScheme_ReturnsFalse() {
            // Arrange + Act + Assert
            Assert.False(UrlUtils.IsValidHttpsUri("ftp://example.com/path"));
        }

        [Fact]
        public void IsValidHttpsUri_NullOrEmpty_ReturnsFalse() {
            // Arrange + Act + Assert
            Assert.False(UrlUtils.IsValidHttpsUri(null));
            Assert.False(UrlUtils.IsValidHttpsUri(""));
            Assert.False(UrlUtils.IsValidHttpsUri("   "));
        }

        [Fact]
        public void IsValidHttpsUri_RelativeUrl_ReturnsFalse() {
            // Arrange + Act + Assert
            Assert.False(UrlUtils.IsValidHttpsUri("/relative/path"));
        }
    }
}
