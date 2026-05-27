using System;
using System.IO;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;
using OutSystems.UltimatePDF_ExternalLogic.Structures;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace OutSystems.UltimatePDF_ExternalLogic.Utils {
    internal static class PDFMetadataUtil {

        /// <summary>
        /// Applies metadata properties to a PDF document
        /// </summary>
        /// <param name="pdfBytes">The PDF document as byte array</param>
        /// <param name="properties">The metadata properties to apply</param>
        /// <param name="logger">Optional logger for non-fatal embed failures</param>
        /// <returns>The PDF with metadata applied as byte array</returns>
        public static byte[] ApplyMetadata(byte[] pdfBytes, DocumentProperties properties, Logger? logger = null) {
            if (pdfBytes == null || pdfBytes.Length == 0) {
                return pdfBytes;
            }

            if (properties.IsEmpty()) {
                return pdfBytes;
            }

            try {
                using var inputStream = new MemoryStream(pdfBytes);
                using var outputStream = new MemoryStream();

                var document = PdfReader.Open(inputStream, PdfDocumentOpenMode.Modify);

                if (!string.IsNullOrWhiteSpace(properties.Title)) {
                    document.Info.Title = properties.Title;
                }

                if (!string.IsNullOrWhiteSpace(properties.Author)) {
                    document.Info.Author = properties.Author;
                }

                if (!string.IsNullOrWhiteSpace(properties.Subject)) {
                    document.Info.Subject = properties.Subject;
                }

                if (!string.IsNullOrWhiteSpace(properties.Keywords)) {
                    document.Info.Keywords = properties.Keywords;
                }

                if (!string.IsNullOrWhiteSpace(properties.Creator)) {
                    document.Info.Creator = properties.Creator;
                }

                if (!string.IsNullOrWhiteSpace(properties.Company)) {
                    document.Info.Elements.SetString("/Company", properties.Company);
                }

                if (!string.IsNullOrWhiteSpace(properties.Producer)) {
                    document.Info.Elements.SetString("/Producer", properties.Producer);
                }

                if (!string.IsNullOrWhiteSpace(properties.Copyright)) {
                    document.Info.Elements.SetString("/Copyright", properties.Copyright);
                }

                if (!string.IsNullOrWhiteSpace(properties.Language)) {
                    document.Internals.Catalog.Elements.SetString("/Lang", properties.Language);
                }

                if (!string.IsNullOrWhiteSpace(properties.Source)) {
                    document.Info.Elements.SetString("/Source", properties.Source);
                }

                document.Info.ModificationDate = DateTime.UtcNow;

                document.Save(outputStream, false);
                return outputStream.ToArray();
            } catch (Exception ex) when (ex is PdfSharpException or IOException or InvalidOperationException or ArgumentException) {
                logger?.Warning(ex, "Failed to embed PDF metadata; returning original bytes.");
                return pdfBytes;
            }
        }

    }
}
