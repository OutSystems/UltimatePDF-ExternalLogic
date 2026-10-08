using System.Net.Http.Json;
using OutSystems.UltimatePDF_ExternalLogic.E2ETests.Fixtures;
using OutSystems.UltimatePDF_ExternalLogic.E2ETests.Models;
using PdfSharp.Pdf.IO;

namespace OutSystems.UltimatePDF_ExternalLogic.E2ETests;

/// <summary>
/// Tenant smoke tests for PrintPDF_ToFile, the action that stores the PDF as a File instead of
/// returning its bytes. These are the only tests that exercise real ODC File storage: everything
/// below this level fakes IFileStorage, because its implementation only exists inside an ODC build.
/// </summary>
/// <remarks>
/// <para>
/// These require a REST method. Adding
/// it means editing <c>oml/Ultimate PDF Tests.oml</c> in ODC Studio and republishing, after which
/// the fixture deploys it and these run for real.
/// </para>
/// <para>
/// The contract they expect, matching the existing <c>PrintPDF</c> method so the two stay symmetric:
/// </para>
/// <list type="bullet">
///   <item>A method named <c>PrintPDF_ToFile</c> on the same <c>cicd_tests</c> REST API.</item>
///   <item>POST, with the JSON body of <see cref="PrintPdfToFileRequest"/>.</item>
///   <item>It calls the external logic action, holds the returned File in a local variable, reads
///   its content back, and returns those bytes as the response body.</item>
/// </list>
/// <para>
/// Returning the content rather than the File's metadata is deliberate. It proves the whole loop —
/// the PDF was generated, written to File storage, and read back out intact — which metadata alone
/// would not, and it lets these assert exactly what the PrintPDF tests assert.
/// </para>
/// </remarks>
public class PrintPDFToFileE2ETests(OdcTenantFixture fixture) {

    private const string Endpoint = "PrintPDF_ToFile";

    private readonly HttpClient client = fixture.Client;
    private readonly string testPageUrl = fixture.TestPageUrl;

    [E2EFact]
    public async Task PrintPDF_ToFile_StoresAndReturnsValidPdf() {
        // Arrange
        var request = new PrintPdfToFileRequest {
            Url = testPageUrl,
            TimeoutSeconds = 60,
            FileName = "e2e-smoke.pdf",
        };

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, request, TestContext.Current.CancellationToken);
        SkipIfEndpointNotPublished(response);

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

    [E2EFact]
    public async Task PrintPDF_ToFile_WithDocumentProperties_EmbedsMetadata() {
        // Arrange — metadata is applied before the bytes are stored, so finding it after the
        // round trip through File storage proves the stored content was not altered.
        var request = new PrintPdfToFileRequest {
            Url = testPageUrl,
            TimeoutSeconds = 60,
            FileName = "e2e-metadata.pdf",
            DocumentProperties = new DocumentPropertiesDto {
                Title = "Test", Author = "CI", Subject = "Smoke",
            },
        };

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, request, TestContext.Current.CancellationToken);
        SkipIfEndpointNotPublished(response);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var bytes = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
        using var ms = new MemoryStream(bytes);
        var doc = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
        Assert.Equal("Test", doc.Info.Title);
        Assert.Equal("CI", doc.Info.Author);
        Assert.Equal("Smoke", doc.Info.Subject);
    }

    /// <summary>
    /// Skips rather than fails when the REST method is absent.
    /// </summary>
    /// <remarks>
    /// The fixture already proved the REST service itself is reachable by fetching its swagger
    /// during setup, so a 404 on this one method means the deployed OML predates it rather than
    /// that the tenant is broken. Skipping keeps the suite honest: these cannot pass until the OML
    /// side lands, and a red test would say something false about the library.
    /// </remarks>
    private static void SkipIfEndpointNotPublished(HttpResponseMessage response) {
        if (response.StatusCode != System.Net.HttpStatusCode.NotFound) {
            return;
        }

        Assert.Skip(
            $"The '{Endpoint}' REST method is not published on the tenant's cicd_tests API. " +
            "Add it to 'oml/Ultimate PDF Tests.oml' in ODC Studio, calling the PrintPDF_ToFile " +
            "action and returning the stored File's content as the response body, then republish. " +
            "See the remarks on PrintPDFToFileE2ETests for the full contract.");
    }
}
