using System;
using System.IO;
using OutSystems.UltimatePDF_ExternalLogic.Structures;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace OutSystems.UltimatePDF_ExternalLogic.Utils {
    internal static class PDFMetadataUtil {

        /// <summary>
        /// Applies metadata properties to a PDF document
        /// </summary>
        /// <param name="pdfBytes">The PDF document as byte array</param>
        /// <param name="properties">The metadata properties to apply</param>
        /// <returns>The PDF with metadata applied as byte array</returns>
        public static byte[] ApplyMetadata(byte[] pdfBytes, PDFProperties properties) {
            if (pdfBytes == null || pdfBytes.Length == 0) {
                return pdfBytes;
            }

            if (IsEmptyProperties(properties)) {
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

                document.Info.CreationDate = DateTime.Now;
                document.Info.ModificationDate = DateTime.Now;

                document.Save(outputStream, false);
                return outputStream.ToArray();
            } catch {
                return pdfBytes;
            }
        }

        private static bool IsEmptyProperties(PDFProperties properties) {
            return string.IsNullOrWhiteSpace(properties.Title) &&
                   string.IsNullOrWhiteSpace(properties.Author) &&
                   string.IsNullOrWhiteSpace(properties.Subject) &&
                   string.IsNullOrWhiteSpace(properties.Keywords) &&
                   string.IsNullOrWhiteSpace(properties.Creator) &&
                   string.IsNullOrWhiteSpace(properties.Company);
        }
    }
}
