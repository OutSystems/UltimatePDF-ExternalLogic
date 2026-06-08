using System;
using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OutSystems.UltimatePDF_ExternalLogic.IntegrationTests.Fixtures;
using OutSystems.UltimatePDF_ExternalLogic.IntegrationTests.TestHelpers;
using OutSystems.UltimatePDF_ExternalLogic.Structures;
using Environment = OutSystems.UltimatePDF_ExternalLogic.Structures.Environment;

namespace OutSystems.UltimatePDF_ExternalLogic.IntegrationTests {

    [Collection("HelloWorldWeb")]
    public class PrintPDFToRestIntegrationTests {

        private readonly HelloWorldWebFixture _web;
        private readonly MockRestReceiverFixture _rest;

        public PrintPDFToRestIntegrationTests(HelloWorldWebFixture web, MockRestReceiverFixture rest) {
            _web = web;
            _rest = rest;
            _rest.Reset();
        }

        private static UltimatePDF_ExternalLogic NewUltimatePDF() =>
            new UltimatePDF_ExternalLogic(NullLogger.Instance);

        private RestCaller StoreRestCaller() => new RestCaller {
            BaseUrl   = _rest.BaseUrl,
            Module    = "/api",
            StorePath = "/store",
            LogPath   = "/logs",
            Token     = "test-token",
        };

        [Fact]
        public void PrintPDF_ToRest_HappyPath_PostsPdfToRestEndpoint() {
            // Arrange
            var ultimatePdf = NewUltimatePDF();

            // Act
            ultimatePdf.PrintPDF_ToRest(
                url: _web.BaseUrl,
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: new Environment(),
                cookies: Array.Empty<Cookie>(),
                paper: new Paper(),
                documentProperties: null,
                timeoutSeconds: 60,
                collectLogs: false,
                attachFilesLogs: false,
                restCaller: StoreRestCaller());

            // Assert
            Assert.Single(_rest.StoredPdfs);
            Assert.Equal("application/pdf", _rest.StoredPdfContentTypes[0]);
            var pdf = _rest.StoredPdfs[0];
            Assert.Equal((byte)'%', pdf[0]);
            Assert.Equal((byte)'P', pdf[1]);
            Assert.Equal((byte)'D', pdf[2]);
            Assert.Equal((byte)'F', pdf[3]);
        }

        [Fact]
        public void PrintPDF_ToRest_WithCollectLogs_PostsPdfAndLogs() {
            // Arrange
            var ultiamtePdf = NewUltimatePDF();

            // Act
            ultiamtePdf.PrintPDF_ToRest(
                url: _web.BaseUrl,
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: new Environment(),
                cookies: Array.Empty<Cookie>(),
                paper: new Paper(),
                documentProperties: null,
                timeoutSeconds: 60,
                collectLogs: true,
                attachFilesLogs: false,
                restCaller: StoreRestCaller());

            // Assert
            Assert.Single(_rest.StoredPdfs);
            Assert.Single(_rest.StoredLogs);
            var zipBytes = _rest.StoredLogs[0];
            Assert.True(zipBytes.Length > 0);
            using var ms = new MemoryStream(zipBytes);
            using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
            Assert.NotEmpty(archive.Entries);
        }

        [Fact]
        public void PrintPDF_ToRest_WithAttachFilesLogs_LogsZipContainsInputAndOutput() {
            // Arrange
            var ultiamtePdf = NewUltimatePDF();

            // Act
            ultiamtePdf.PrintPDF_ToRest(
                url: _web.BaseUrl,
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: new Environment(),
                cookies: Array.Empty<Cookie>(),
                paper: new Paper(),
                documentProperties: null,
                timeoutSeconds: 60,
                collectLogs: true,
                attachFilesLogs: true,
                restCaller: StoreRestCaller());

            // Assert
            ZipAssert.ContainsEntry(_rest.StoredLogs[0], "input.html");
            ZipAssert.ContainsEntry(_rest.StoredLogs[0], "output.pdf");
        }

        [Fact]
        public void PrintPDF_ToRest_RestEndpointReturns4xx_LogsError() {
            // Arrange — /fail returns 400; PrintPDF_ToRest silently swallows the exception
            // but must log it. This test documents and pins that behavior.
            var spy = new SpyLogger();
            var ultiamtePdf = new UltimatePDF_ExternalLogic(spy);
            var failCaller = new RestCaller {
                BaseUrl   = _rest.BaseUrl,
                Module    = "/api",
                StorePath = "/fail",
                LogPath   = "/logs",
                Token     = "test-token",
            };

            // Act
            ultiamtePdf.PrintPDF_ToRest(
                url: _web.BaseUrl,
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: new Environment(),
                cookies: Array.Empty<Cookie>(),
                paper: new Paper(),
                documentProperties: null,
                timeoutSeconds: 60,
                collectLogs: true,
                attachFilesLogs: false,
                restCaller: failCaller);

            // Assert — the 4xx response must be surfaced as at least one error log entry
            Assert.True(spy.ErrorCount > 0, "Expected at least one error to be logged for a 4xx REST response");
        }
    }
}
