using System;
using Microsoft.Extensions.Logging.Abstractions;
using OutSystems.UltimatePDF_ExternalLogic.IntegrationTests.Fixtures;
using OutSystems.UltimatePDF_ExternalLogic.Structures;
using Environment = OutSystems.UltimatePDF_ExternalLogic.Structures.Environment;

namespace OutSystems.UltimatePDF_ExternalLogic.IntegrationTests {

    [Collection("HelloWorldWeb")]
    public class ScreenshotPNGIntegrationTests {

        private readonly HelloWorldWebFixture _web;

        public ScreenshotPNGIntegrationTests(HelloWorldWebFixture web) => _web = web;

        private static UltimatePDF_ExternalLogic NewSut() =>
            new UltimatePDF_ExternalLogic(NullLogger.Instance);

        [Fact]
        public void ScreenshotPNG_HelloWorld_ReturnsPngBytes() {
            // Arrange
            var sut = NewSut();

            // Act
            var png = sut.ScreenshotPNG(
                url: _web.BaseUrl,
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: new Environment(),
                cookies: Array.Empty<Cookie>(),
                paper: new Paper(),
                screenshotOptions: new ScreenshotOptions(),
                timeoutSeconds: 60,
                collectLogs: false,
                attachFilesLogs: false,
                logsZipFile: out _);

            // Assert — PNG magic bytes: 0x89 P N G
            Assert.NotNull(png);
            Assert.True(png.Length > 100);
            Assert.Equal(0x89, png[0]);
            Assert.Equal(0x50, png[1]); // P
            Assert.Equal(0x4E, png[2]); // N
            Assert.Equal(0x47, png[3]); // G
        }

        [Fact]
        public void ScreenshotPNG_WithDocumentProperties_EmbedsPngMetadata() {
            // Arrange
            var sut = NewSut();
            var props = new DocumentProperties { Title = "Screenshot Test" };
            var options = new ScreenshotOptions { DocumentProperties = props };

            // Act
            var png = sut.ScreenshotPNG(
                url: _web.BaseUrl,
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: new Environment(),
                cookies: Array.Empty<Cookie>(),
                paper: new Paper(),
                screenshotOptions: options,
                timeoutSeconds: 60,
                collectLogs: false,
                attachFilesLogs: false,
                logsZipFile: out _);

            // Assert — PNG file is valid and contains "Title" text somewhere in the metadata chunks
            Assert.NotNull(png);
            Assert.True(png.Length > 100);
            Assert.Equal(0x89, png[0]);
            var pngText = System.Text.Encoding.Latin1.GetString(png);
            Assert.Contains("Title", pngText);
        }
    }
}
