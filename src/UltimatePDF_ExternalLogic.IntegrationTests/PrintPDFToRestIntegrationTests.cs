using System;
using System.IO;
using System.IO.Compression;
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

        private static UltimatePDF_ExternalLogic NewSut() =>
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
            var sut = NewSut();

            // Act
            sut.PrintPDF_ToRest(
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
            var sut = NewSut();

            // Act
            sut.PrintPDF_ToRest(
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
            var sut = NewSut();

            // Act
            sut.PrintPDF_ToRest(
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
        public void PrintPDF_ToRest_RestEndpointReturns4xx_DoesNotThrow() {
            // Arrange
            var sut = NewSut();
            var failCaller = new RestCaller {
                BaseUrl   = _rest.BaseUrl,
                Module    = "/api",
                StorePath = "/fail",
                LogPath   = "/logs",
                Token     = "test-token",
            };

            // Act + Assert (no exception = pass)
            sut.PrintPDF_ToRest(
                url: _web.BaseUrl,
                viewport: new Viewport { Width = 800, Height = 600 },
                environment: new Environment(),
                cookies: Array.Empty<Cookie>(),
                paper: new Paper(),
                documentProperties: null,
                timeoutSeconds: 60,
                collectLogs: false,
                attachFilesLogs: false,
                restCaller: failCaller);
        }
    }
}
