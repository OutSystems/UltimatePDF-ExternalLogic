using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using OutSystems.UltimatePDF_ExternalLogic.Structures;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {

    /// <summary>
    /// Wiring-level tests that exercise the entry points of UltimatePDF_ExternalLogic
    /// without spinning up PuppeteerSharp. They cover input validation, exception
    /// propagation, and the ScreenshotOptions → DocumentProperties composition that
    /// replaces the former 10-field manual mapping.
    /// </summary>
    public class UltimatePDFWiringTests {

        [Fact]
        public void InnerPrintPDF_InvalidHttpsUrl_PropagatesUriFormatException() {
            // Arrange — the URL fails UrlUtils.IsValidHttpsUri, which throws UriFormatException.
            // The test confirms that after the Phase 2 rethrow change the exception escapes
            // InnerPrintPDF (previously it would have been swallowed and returned as empty bytes).
            var logger = Management.Troubleshooting.Logger.GetLogger(
                NullLogger<Management.Troubleshooting.Logger>.Instance,
                collectLogs: false, attachFilesLogs: false);

            // Act + Assert
            Assert.Throws<UriFormatException>(() => UltimatePDF_ExternalLogic.InnerPrintPDF(
                url: "ftp://not-https.example.com/file",
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: new Structures.Environment(),
                cookies: new List<Cookie>(),
                paper: new Paper(),
                documentProperties: null,
                timeoutSeconds: 30,
                logger: logger));
        }

        [Fact]
        public void ScreenshotOptions_DocumentProperties_RoundTripsAllFields() {
            // After Task 1.3 the 10-field manual rehydration in ScreenshotPNG is gone:
            // ScreenshotOptions composes DocumentProperties directly. This test pins that
            // every field on the nested DocumentProperties is reachable with the value
            // the caller set, so adding an 11th metadata field requires no mapping fix.
            // Arrange
            var options = new ScreenshotOptions {
                FullPage = true,
                TransparentBackground = true,
                DocumentProperties = new DocumentProperties {
                    Title = "T-value",
                    Author = "A-value",
                    Subject = "S-value",
                    Keywords = "K-value",
                    Creator = "Cr-value",
                    Company = "Co-value",
                    Producer = "P-value",
                    Copyright = "Cp-value",
                    Language = "L-value",
                    Source = "Sr-value",
                },
            };

            // Act + Assert
            Assert.Equal("T-value", options.DocumentProperties.Title);
            Assert.Equal("A-value", options.DocumentProperties.Author);
            Assert.Equal("S-value", options.DocumentProperties.Subject);
            Assert.Equal("K-value", options.DocumentProperties.Keywords);
            Assert.Equal("Cr-value", options.DocumentProperties.Creator);
            Assert.Equal("Co-value", options.DocumentProperties.Company);
            Assert.Equal("P-value", options.DocumentProperties.Producer);
            Assert.Equal("Cp-value", options.DocumentProperties.Copyright);
            Assert.Equal("L-value", options.DocumentProperties.Language);
            Assert.Equal("Sr-value", options.DocumentProperties.Source);
            Assert.True(options.FullPage);
            Assert.True(options.TransparentBackground);
        }
    }
}
