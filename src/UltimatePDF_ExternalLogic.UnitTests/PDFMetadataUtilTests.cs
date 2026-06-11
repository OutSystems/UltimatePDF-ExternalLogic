using OutSystems.UltimatePDF_ExternalLogic.Structures;
using OutSystems.UltimatePDF_ExternalLogic.Test.Helpers;
using OutSystems.UltimatePDF_ExternalLogic.Utils;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests;

public class PDFMetadataUtilTests {

    [Fact]
    public void ApplyMetadata_AllFieldsPopulated_WritesAllValues() {
        // Arrange
        var input = PdfFactory.CreateMinimal();
        var originalCreationDate = PdfFactory.OpenImport(input).Info.CreationDate;
        var properties = new DocumentProperties {
            Title = "Q1 Report",
            Author = "Acme",
            Subject = "Quarterly results",
            Keywords = "finance, q1",
            Creator = "Ultimate PDF",
            Company = "Acme Corp",
            Producer = "Ultimate PDF 1.x",
            Copyright = "(c) 2026 Acme",
            Language = "en-US",
            Source = "ERP-Prod",
        };

        // Act
        var before = System.DateTime.UtcNow;
        var output = PDFMetadataUtil.ApplyMetadata(input, properties);
        var after = System.DateTime.UtcNow;

        // Assert
        var doc = PdfFactory.OpenImport(output);
        Assert.Equal("Q1 Report", doc.Info.Title);
        Assert.Equal("Acme", doc.Info.Author);
        Assert.Equal("Quarterly results", doc.Info.Subject);
        Assert.Equal("finance, q1", doc.Info.Keywords);
        Assert.Equal("Ultimate PDF", doc.Info.Creator);
        Assert.Equal("Acme Corp", doc.Info.Elements.GetString("/Company"));
        // PdfSharp 6.2 wraps Producer as "PDFsharp X.Y (Original: <our value>)" on save.
        Assert.Contains("Ultimate PDF 1.x", doc.Info.Elements.GetString("/Producer"));
        Assert.Equal("(c) 2026 Acme", doc.Info.Elements.GetString("/Copyright"));
        Assert.Equal("en-US", doc.Internals.Catalog.Elements.GetString("/Lang"));
        Assert.Equal("ERP-Prod", doc.Info.Elements.GetString("/Source"));
        // ModificationDate is set to the embedding moment; CreationDate is preserved.
        // PdfSharp serializes PDF dates at second granularity, so allow a 1s slack on each side.
        var modifiedUtc = doc.Info.ModificationDate.ToUniversalTime();
        Assert.InRange(modifiedUtc, before.AddSeconds(-1), after.AddSeconds(1));
        Assert.Equal(originalCreationDate, doc.Info.CreationDate);
    }

    [Fact]
    public void ApplyMetadata_AllFieldsEmpty_ReturnsInputUnchanged() {
        // Arrange
        var input = PdfFactory.CreateMinimal();
        var properties = new DocumentProperties();

        // Act
        var output = PDFMetadataUtil.ApplyMetadata(input, properties);

        // Assert
        Assert.Same(input, output);
    }

    [Fact]
    public void ApplyMetadata_MixedFields_WritesOnlyPopulatedEntries() {
        // Arrange
        var input = PdfFactory.CreateMinimal();
        var properties = new DocumentProperties {
            Title = "Only Title",
            Company = "Acme",
        };

        // Act
        var output = PDFMetadataUtil.ApplyMetadata(input, properties);

        // Assert
        var doc = PdfFactory.OpenImport(output);
        Assert.Equal("Only Title", doc.Info.Title);
        Assert.Equal("Acme", doc.Info.Elements.GetString("/Company"));
        Assert.False(doc.Info.Elements.ContainsKey("/Author"));
        // /Producer is always set by PdfSharp on save; skip presence assertion.
        Assert.False(doc.Info.Elements.ContainsKey("/Copyright"));
        Assert.False(doc.Info.Elements.ContainsKey("/Source"));
        Assert.False(doc.Internals.Catalog.Elements.ContainsKey("/Lang"));
    }

    [Fact]
    public void ApplyMetadata_NonAscii_RoundTripsValues() {
        // Arrange
        var input = PdfFactory.CreateMinimal();
        var properties = new DocumentProperties {
            Title = "Relatório Trimestral",
            Author = "José",
        };

        // Act
        var output = PDFMetadataUtil.ApplyMetadata(input, properties);

        // Assert
        var doc = PdfFactory.OpenImport(output);
        Assert.Equal("Relatório Trimestral", doc.Info.Title);
        Assert.Equal("José", doc.Info.Author);
    }

    [Fact]
    public void ApplyMetadata_SurrogateAndCjkCharacters_RoundTripsValues() {
        // Arrange
        var input = PdfFactory.CreateMinimal();
        var properties = new DocumentProperties {
            Title = "第1四半期レポート",           // CJK + katakana
            Author = "山田　太郎",                 // CJK with full-width space
            Subject = "四半期の結果",              // hiragana + CJK
            Keywords = "財務, Q1, レポート",       // mixed CJK / katakana / ASCII
            Creator = "アルティメットPDF",         // katakana + ASCII
            Company = "株式会社アクメ",            // kanji + katakana
            // Surrogate-pair emoji (U+1F4C4 PAGE FACING UP) to exercise UTF-16 surrogates
            Producer = "UltimatePDF \U0001F4C4",
            Copyright = "© 2026 株式会社アクメ",
        };

        // Act
        var output = PDFMetadataUtil.ApplyMetadata(input, properties);

        // Assert
        var doc = PdfFactory.OpenImport(output);
        Assert.Equal("第1四半期レポート", doc.Info.Title);
        Assert.Equal("山田　太郎", doc.Info.Author);
        Assert.Equal("四半期の結果", doc.Info.Subject);
        Assert.Equal("財務, Q1, レポート", doc.Info.Keywords);
        Assert.Equal("アルティメットPDF", doc.Info.Creator);
        Assert.Equal("株式会社アクメ", doc.Info.Elements.GetString("/Company"));
        Assert.Contains("UltimatePDF \U0001F4C4", doc.Info.Elements.GetString("/Producer"));
        Assert.Equal("© 2026 株式会社アクメ", doc.Info.Elements.GetString("/Copyright"));
    }
}
