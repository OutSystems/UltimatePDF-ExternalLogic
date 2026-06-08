using System.IO;
using System.Net.Http.Json;
using OutSystems.UltimatePDF_ExternalLogic.TenantTests.Fixtures;
using OutSystems.UltimatePDF_ExternalLogic.TenantTests.Models;
using PdfSharp.Pdf.IO;

namespace OutSystems.UltimatePDF_ExternalLogic.TenantTests;

public class PrintPDFTenantTests(OdcTenantFixture fixture) {

    private readonly HttpClient _client = fixture.Client;
    private readonly string _testPageUrl = fixture.TestPageUrl;

    [Fact]
    public async Task PrintPDF_ReturnsValidPdf() {
        // Arrange
        var request = new PrintPdfRequest { Url = _testPageUrl, TimeoutSeconds = 60 };

        // Act
        var response = await _client.PostAsJsonAsync("PrintPDF", request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var bytes = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
        Assert.True(bytes.Length > 1000);
        Assert.Equal((byte)'%', bytes[0]);
        Assert.Equal((byte)'P', bytes[1]);
        Assert.Equal((byte)'D', bytes[2]);
        Assert.Equal((byte)'F', bytes[3]);
        Assert.Equal((byte)'-', bytes[4]);
    }

    [Fact]
    public async Task PrintPDF_WithDocumentProperties_EmbedsMetadata() {
        // Arrange
        var props = new DocumentPropertiesDto { Title = "Test", Author = "CI", Subject = "Smoke" };
        var request = new PrintPdfRequest {
            Url = _testPageUrl,
            TimeoutSeconds = 60,
            DocumentProperties = props,
        };

        // Act
        var response = await _client.PostAsJsonAsync("PrintPDF", request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var bytes = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
        using var ms = new MemoryStream(bytes);
        var doc = PdfSharp.Pdf.IO.PdfReader.Open(ms, PdfDocumentOpenMode.Import);
        Assert.Equal("Test",  doc.Info.Title);
        Assert.Equal("CI",    doc.Info.Author);
        Assert.Equal("Smoke", doc.Info.Subject);
    }
}
