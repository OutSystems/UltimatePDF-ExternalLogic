using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PdfSharp.Drawing;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf;
using PuppeteerSharp;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;
using System;

namespace OutSystems.UltimatePDF_ExternalLogic.LayoutPrintPipeline {
    internal class Pipeline {
        private LayoutPrint[] layouts = Array.Empty<LayoutPrint>();


        public async Task Initialize(IPage page) {
            layouts = await page.EvaluateFunctionAsync<LayoutPrint[]>("window?.UltimatePDF?.getLayouts || function(){}");
        }

        public bool HasLayouts { get { return layouts != null && layouts.Length > 0; } }



        public async Task<byte[]> Render(IPage page, Logger logger) {
            PdfDocument[] documents = new PdfDocument[layouts.Length];

            PdfOptions mainContentOptions = new PdfOptions() {
                OmitBackground = true,
                PrintBackground = true,
                PreferCSSPageSize = true,
            };
            PdfOptions otherOptions = new PdfOptions() {
                OmitBackground = true,
                PrintBackground = true,
                PreferCSSPageSize = true,
            };

            for (var i = 0; i < layouts.Length; i++) {
                await page.EvaluateFunctionAsync("window.UltimatePDF.selectLayout", i);

                // Render main content and store its document
                await page.EvaluateFunctionAsync("window.UltimatePDF.prepareForMainContent");
                byte[] content = await page.PdfDataAsync(mainContentOptions);
                logger.Attach($"layout-{i}-content.pdf", content);

                using (MemoryStream stream = new MemoryStream(content)) {
                    documents[i] = PdfReader.Open(stream);
                    layouts[i].Pages = documents[i].PageCount;
                }

                if (layouts[i].HasPageBackground) {
                    // Render page background and merge into the document
                    await page.EvaluateFunctionAsync("window.UltimatePDF.prepareForPageBackground");
                    byte[] background = await page.PdfDataAsync(otherOptions);
                    logger.Attach($"layout-{i}-background.pdf", background);

                    MergeBackground(documents[i], background);
                }
            }



            /* Propagate the FirstPageNumber forwards */
            if (!layouts[0].RestartPageNumber) {
                layouts[0].FirstPageNumber = 1;
            }
            for (var i = 0; i < layouts.Length; i++) {
                if (i + 1 < layouts.Length) {
                    if (!layouts[i + 1].RestartPageNumber) {
                        layouts[i + 1].FirstPageNumber = layouts[i].FirstPageNumber + layouts[i].Pages;
                    }
                }
            }

            /* Propagate the LastPageNumber backwards */
            layouts[layouts.Length - 1].LastPageNumber = layouts[layouts.Length - 1].FirstPageNumber + layouts[layouts.Length - 1].Pages - 1;
            for (var i = layouts.Length - 1; i >= 0; i--) {
                if (i - 1 >= 0) {
                    if (!layouts[i].RestartPageNumber) {
                        layouts[i - 1].LastPageNumber = layouts[i].LastPageNumber;
                    } else {
                        layouts[i - 1].LastPageNumber = layouts[i - 1].FirstPageNumber + layouts[i - 1].Pages - 1;
                    }
                }
            }




            for (var i = 0; i < layouts.Length; i++) {
                if (layouts[i].HasHeader || layouts[i].HasFooter || layouts[i].HasBottomContent) {
                    await page.EvaluateFunctionAsync("window.UltimatePDF.selectLayout", i);
                }

                if (layouts[i].HasHeader || layouts[i].HasFooter) {
                    logger.Log($"layout-{i} pagination: {layouts[i].Pages} pages, starting at {layouts[i].FirstPageNumber}, last page is {layouts[i].LastPageNumber}");
                    await page.EvaluateFunctionAsync("window.UltimatePDF.updatePagination", layouts[i].Pages, layouts[i].FirstPageNumber, layouts[i].LastPageNumber);
                }

                if (layouts[i].HasHeader && layouts[i].PagesWithHeader > 0) {
                    // Render headers and merge into the document
                    await page.EvaluateFunctionAsync("window.UltimatePDF.prepareForHeader");
                    byte[] header = await page.PdfDataAsync(otherOptions);
                    logger.Attach($"layout-{i}-header.pdf", header);

                    MergeHeaders(documents[i], header);
                }

                if (layouts[i].HasBottomContent) {
                    // Render bottom content at the bottom of the last page
                    await page.EvaluateFunctionAsync("window.UltimatePDF.prepareForBottomContent");
                    byte[] bottomContent = await page.PdfDataAsync(otherOptions);
                    logger.Attach($"layout-{i}-bottom-content.pdf", bottomContent);

                    MergeBottomContent(documents[i], bottomContent);
                }

                if (layouts[i].HasFooter && layouts[i].PagesWithFooter > 0) {
                    // Render footers and merge into the document
                    await page.EvaluateFunctionAsync("window.UltimatePDF.prepareForFooter");
                    byte[] footer = await page.PdfDataAsync(otherOptions);
                    logger.Attach($"layout-{i}-footer.pdf", footer);

                    MergeFooters(documents[i], footer);
                }
            }

            return Concatenate(documents);
        }



        private void MergeBackground(PdfDocument document, byte[] background) {
            XPdfForm backgroundForm;
            using (MemoryStream stream = new MemoryStream(background)) {
                backgroundForm = XPdfForm.FromStream(stream);

                foreach (var page in document.Pages) {
                    using (XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Prepend)) {
                        XRect pageBox = new XRect(0, 0, page.Width.Point, page.Height.Point);
                        gfx.DrawImage(backgroundForm, pageBox);
                    }
                }
            }
        }

        private void MergeHeaders(PdfDocument document, byte[] header) {
            using MemoryStream stream = new MemoryStream(header);
            XPdfForm headerForm = XPdfForm.FromStream(stream);

            int skip = document.PageCount > headerForm.PageCount ? document.PageCount - headerForm.PageCount : 0;
            for (int i = 0; i < document.PageCount && i < headerForm.PageCount; i++) {
                PdfPage page = document.Pages[skip + i];
                headerForm.PageIndex = i;

                if (headerForm.Page is not null) {
                    using XGraphics gfx = XGraphics.FromPdfPage(page);
                    XRect headerBox = new XRect(0, 0, page.Width.Point, headerForm.Page.Height.Point);
                    gfx.DrawImage(headerForm, headerBox);
                    CopyHyperlinks(headerForm.Page, page, headerBox.Y);
                }
            }

        }

        private void MergeBottomContent(PdfDocument document, byte[] bottomContent) {
            using MemoryStream stream = new MemoryStream(bottomContent);
            XPdfForm bottomForm = XPdfForm.FromStream(stream);

            PdfPage page = document.Pages[document.PageCount - 1];

            if (bottomForm.Page is not null) {
                using XGraphics gfx = XGraphics.FromPdfPage(page);
                XRect footerBox = new XRect(0, page.Height.Point - bottomForm.Page.Height.Point, page.Width.Point, bottomForm.Page.Height.Point);
                gfx.DrawImage(bottomForm, footerBox);
                CopyHyperlinks(bottomForm.Page, page, footerBox.Y);
            }
        }

        private void MergeFooters(PdfDocument document, byte[] footer) {
            using MemoryStream stream = new MemoryStream(footer);
            XPdfForm footerForm = XPdfForm.FromStream(stream);

            for (int i = 0; i < document.PageCount && i < footerForm.PageCount; i++) {
                PdfPage page = document.Pages[i];
                footerForm.PageIndex = i;

                if (footerForm.Page is not null) {
                    using XGraphics gfx = XGraphics.FromPdfPage(page);
                    XRect footerBox = new XRect(0, page.Height.Point - footerForm.Page.Height.Point, page.Width.Point, footerForm.Page.Height.Point);
                    gfx.DrawImage(footerForm, footerBox);
                    CopyHyperlinks(footerForm.Page, page, footerBox.Y);
                }
            }
        }



        private void CopyHyperlinks(PdfPage from, PdfPage to, double yOffset) {
            foreach (PdfAnnotation annotation in from.Annotations) {
                if (annotation.Elements.GetString(PdfAnnotation.Keys.Subtype) == "/Link") {
                    var dest = annotation.Elements.GetDictionary(PdfAnnotation.Keys.A);
                    var rect = annotation.Elements.GetRectangle(PdfAnnotation.Keys.Rect);
                    if (dest != null && rect != null && dest.Elements.Count > 0) {
                        var uri = dest.Elements.GetString("/URI");
                        var pageRect = new XRect(rect.X1, to.Height.Point - from.Height.Point + rect.Y1 - yOffset, rect.Width, rect.Height);
                        to.Annotations.Add(PdfLinkAnnotation.CreateWebLink(new PdfRectangle(pageRect), uri));
                    }
                }
            }
        }



        private byte[] Concatenate(PdfDocument[] documents) {
            var first = documents[0];

            foreach (var pdf in documents.Skip(1)) {

                using (MemoryStream stream = new MemoryStream()) {
                    /*
                     * Cannot import pages from a pdf that was not opened with PdfDocumentOpenMode.Import,
                     * so we save the PDF and open it back again.
                     */
                    pdf.Save(stream);
                    pdf.Dispose();

                    stream.Position = 0;

                    using (var pdfImport = PdfReader.Open(stream, PdfDocumentOpenMode.Import)) {
                        foreach (var page in pdfImport.Pages) {
                            first.AddPage(page);
                        }
                    }
                }

            }

            using (MemoryStream stream = new MemoryStream()) {
                first.Save(stream);
                first.Dispose();

                return stream.ToArray();
            }
        }


        public class LayoutPrint {
            public bool HasPageBackground { get; set; }
            public bool HasHeader { get; set; }
            public bool HideFirstHeader { get; set; }
            public bool HasBottomContent { get; set; }
            public bool HasFooter { get; set; }
            public bool HideLastFooter { get; set; }

            public bool RestartPageNumber { get; set; }

            public int Pages { get; set; }
            public int FirstPageNumber { get; set; }
            public int LastPageNumber { get; set; }

            public int PagesWithHeader { get { return Pages - (HideFirstHeader ? 1 : 0); } }
            public int PagesWithFooter { get { return Pages - (HideLastFooter ? 1 : 0); } }
        }
    }
}
