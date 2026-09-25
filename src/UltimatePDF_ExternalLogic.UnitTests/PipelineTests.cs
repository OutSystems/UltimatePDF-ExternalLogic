using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OutSystems.UltimatePDF_ExternalLogic.LayoutPrintPipeline;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;
using OutSystems.UltimatePDF_ExternalLogic.Test.Helpers;
using PdfSharp.Pdf.IO;
using PuppeteerSharp;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests;

public class PipelineTests {

    private static readonly Logger NullLog = Logger.GetLogger(
        NullLogger<Logger>.Instance, collectLogs: false, attachFilesLogs: false);

    private static Mock<IPage> BuildPageMock(Pipeline.LayoutPrint[] layouts) {
        var mock = new Mock<IPage>();
        mock.Setup(p => p.EvaluateFunctionAsync<Pipeline.LayoutPrint[]>(It.IsAny<string>(), It.IsAny<object[]>()))
            .ReturnsAsync(layouts);
        mock.Setup(p => p.EvaluateFunctionAsync(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns(Task.CompletedTask);
        mock.Setup(p => p.PdfDataAsync(It.IsAny<PdfOptions>()))
            .ReturnsAsync(PdfFactory.CreateMinimal());
        return mock;
    }

    [Fact]
    public void HasLayouts_DefaultPipeline_ReturnsFalse() {
        // Arrange + Act
        var pipeline = new Pipeline();

        // Assert
        Assert.False(pipeline.HasLayouts);
    }

    [Fact]
    public async Task Render_SingleLayoutNoOverlays_ReturnsValidPdf() {
        // Arrange — layout with no backgrounds, headers, or footers
        var layouts = new[] { new Pipeline.LayoutPrint() };
        var page = BuildPageMock(layouts).Object;
        var pipeline = new Pipeline();
        await pipeline.Initialize(page);

        // Act
        var result = await pipeline.Render(page, NullLog);

        // Assert
        using var ms = new MemoryStream(result);
        var doc = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
        Assert.Equal(1, doc.PageCount);
    }

    [Fact]
    public async Task Render_SingleLayoutWithBackground_ReturnsPdf() {
        // Arrange — background merge path: PdfDataAsync called twice (content + background)
        var layouts = new[] { new Pipeline.LayoutPrint { HasPageBackground = true } };
        var page = BuildPageMock(layouts).Object;
        var pipeline = new Pipeline();
        await pipeline.Initialize(page);

        // Act
        var result = await pipeline.Render(page, NullLog);

        // Assert
        using var ms = new MemoryStream(result);
        var doc = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
        Assert.Equal(1, doc.PageCount);
    }

    [Fact]
    public async Task Render_TwoLayouts_ConcatenatesAllDocuments() {
        // Arrange — two single-page layouts; Concatenate produces a two-page PDF
        var layouts = new[] { new Pipeline.LayoutPrint(), new Pipeline.LayoutPrint() };
        var page = BuildPageMock(layouts).Object;
        var pipeline = new Pipeline();
        await pipeline.Initialize(page);

        // Act
        var result = await pipeline.Render(page, NullLog);

        // Assert
        using var ms = new MemoryStream(result);
        var doc = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
        Assert.Equal(2, doc.PageCount);
    }
}
