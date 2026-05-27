using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace OutSystems.UltimatePDF_ExternalLogic.LayoutPrintPipeline {
    public class LayoutPrint : IDisposable {

        private readonly int layoutNumber;
        private readonly PrintSection section;
        private readonly PdfDocument document;
        private readonly int firstPage;

        public LayoutPrint(int layoutNumber, PrintSection section, byte[] content) {
            this.layoutNumber = layoutNumber;
            this.section = section;
            this.firstPage = section.NextPage;

            using var stream = new MemoryStream(content);
            document = PdfReader.Open(stream);
        }

        public void MergeBackground(byte[] background) {
            XPdfForm backgroundForm;
            using var stream = new MemoryStream(background);
            backgroundForm = XPdfForm.FromStream(stream);

            foreach (var page in document.Pages) {
                using var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Prepend);
                var pageBox = new XRect(0, 0, page.Width.Point, page.Height.Point);
                gfx.DrawImage(backgroundForm, pageBox);
            }
        }

        public void MergeHeader(byte[] header) {
            using var stream = new MemoryStream(header);
            XPdfForm headerForm = XPdfForm.FromStream(stream);

            int skip = document.PageCount > headerForm.PageCount ? document.PageCount - headerForm.PageCount : 0;
            for (int i = 0; i < document.PageCount && i < headerForm.PageCount; i++) {
                PdfPage page = document.Pages[skip + i];
                headerForm.PageIndex = i;

                using var gfx = XGraphics.FromPdfPage(page);
                var headerBox = new XRect(0, 0, page.Width.Point, headerForm.Page?.Height.Point ?? XUnit.FromPoint(0).Point);
                gfx.DrawImage(headerForm, headerBox);
            }
        }

        public void MergeFooter(byte[] footer) {
            using var stream = new MemoryStream(footer);
            var footerForm = XPdfForm.FromStream(stream);

            for (int i = 0; i < document.PageCount && i < footerForm.PageCount; i++) {
                PdfPage page = document.Pages[i];
                footerForm.PageIndex = i;

                using var gfx = XGraphics.FromPdfPage(page);
                XUnit footerFormPageHeight = footerForm.Page?.Height ?? XUnit.FromPoint(0);
                var headerBox = new XRect(0, page.Height.Point - footerFormPageHeight.Point, page.Width.Point, footerFormPageHeight.Point);
                gfx.DrawImage(footerForm, headerBox);
            }
        }

        public static byte[] Concatenate(IList<LayoutPrint> pdfs) {
            var first = pdfs[0];

            foreach (var pdf in pdfs.Skip(1)) {

                using var stream = new MemoryStream();
                /*
                 * Cannot import pages from a pdf that was not opened with PdfDocumentOpenMode.Import,
                 * so we save the PDF and open it back again.
                 */
                pdf.document.Save(stream);
                pdf.Dispose();
                stream.Position = 0;

                using var pdfImport = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
                foreach (var page in pdfImport.Pages) {
                    first.document.AddPage(page);
                }
            }

            using (var stream = new MemoryStream()) {
                first.document.Save(stream);
                first.Dispose();

                return stream.ToArray();
            }
        }

        public int LayoutNumber {
            get { return layoutNumber; }
        }

        public int FirstPage {
            get { return firstPage; }
        }

        public int Pages {
            get { return document.PageCount; }
        }

        public int LastPage {
            get { return section.FirstPage + section.Pages - 1; }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                document.Close();
            }
        }
    }
}
