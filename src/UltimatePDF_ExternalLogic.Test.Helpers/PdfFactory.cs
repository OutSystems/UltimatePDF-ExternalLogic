using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace OutSystems.UltimatePDF_ExternalLogic.Test.Helpers;

internal static class PdfFactory {
    internal static byte[] CreateMinimal(int pages = 1) {
        using var stream = new MemoryStream();
        var doc = new PdfDocument();
        for (int i = 0; i < pages; i++) doc.AddPage();
        doc.Save(stream, false);
        return stream.ToArray();
    }

    internal static PdfDocument OpenEditable(int pages = 1) {
        var doc = new PdfDocument();
        for (int i = 0; i < pages; i++) doc.AddPage();
        return doc;
    }

    internal static PdfDocument OpenImport(byte[] bytes) {
        using var stream = new MemoryStream(bytes);
        return PdfReader.Open(stream, PdfDocumentOpenMode.Import);
    }
}