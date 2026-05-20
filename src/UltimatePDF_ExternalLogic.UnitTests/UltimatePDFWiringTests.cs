using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using OutSystems.UltimatePDF_ExternalLogic.Structures;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {

    /// <summary>
    /// Wiring-level tests that exercise the entry points of UltimatePDF_ExternalLogic
    /// without spinning up PuppeteerSharp.
    /// </summary>
    public class UltimatePDFWiringTests {

        [Fact]
        public void PrintPDF_InvalidHttpsUrl_PropagatesUriFormatException() {
            // Arrange — the URL fails UrlUtils.IsValidHttpsUri, which throws UriFormatException
            // before PuppeteerSharp is ever invoked, so we can exercise the public entry point
            // without a real browser.
            var entry = new UltimatePDF_ExternalLogic(NullLogger.Instance);

            // Act + Assert
            Assert.Throws<UriFormatException>(() => entry.PrintPDF(
                url: "ftp://not-https.example.com/file",
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: new Structures.Environment(),
                cookies: new List<Cookie>(),
                paper: new Paper(),
                documentProperties: null,
                timeoutSeconds: 30,
                collectLogs: false,
                attachFilesLogs: false,
                logsZipFile: out _));
        }
    }
}
