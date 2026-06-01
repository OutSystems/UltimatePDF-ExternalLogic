using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using OutSystems.UltimatePDF_ExternalLogic.Structures;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {
    public class UltimatePDFMoreWiringTests {

        private static UltimatePDF_ExternalLogic NewSut() =>
            new UltimatePDF_ExternalLogic(NullLogger.Instance);

        [Fact]
        public void ScreenshotPNG_InvalidUrl_ThrowsUriFormatException() {
            // Arrange
            var sut = NewSut();

            // Act + Assert
            Assert.Throws<UriFormatException>(() => sut.ScreenshotPNG(
                url: "ftp://bad.example.com",
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: new Structures.Environment(),
                cookies: new List<Cookie>(),
                paper: new Paper(),
                screenshotOptions: new ScreenshotOptions(),
                timeoutSeconds: 30,
                collectLogs: false,
                attachFilesLogs: false,
                logsZipFile: out _));
        }

        [Fact]
        public void PrintPDF_ToRest_InvalidUrl_ThrowsUriFormatException() {
            // Arrange
            var sut = NewSut();

            // Act + Assert
            Assert.Throws<UriFormatException>(() => sut.PrintPDF_ToRest(
                url: "ftp://bad.example.com",
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: new Structures.Environment(),
                cookies: new List<Cookie>(),
                paper: new Paper(),
                documentProperties: null,
                timeoutSeconds: 30,
                collectLogs: false,
                attachFilesLogs: false,
                restCaller: new RestCaller()));
        }

        [Fact]
        public void PrintPDF_ToS3_InvalidUrl_ThrowsUriFormatException() {
            // Arrange
            var sut = NewSut();

            // Act + Assert
            Assert.Throws<UriFormatException>(() => sut.PrintPDF_ToS3(
                url: "ftp://bad.example.com",
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: new Structures.Environment(),
                cookies: new List<Cookie>(),
                paper: new Paper(),
                documentProperties: null,
                timeoutSeconds: 30,
                collectLogs: false,
                attachFilesLogs: false,
                s3Endpoints: new S3Endpoints()));
        }
    }
}
