using System.Diagnostics;
using System.Text;
using PuppeteerSharp.Media;
using PuppeteerSharp;
using System.Web;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;
using OutSystems.UltimatePDF_ExternalLogic.LayoutPrintPipeline;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

namespace OutSystems.UltimatePDF_ExternalLogic.BrowserExecution {
    internal class UltimatePDFExecutionContext {

        private const string USER_AGENT_SUFFIX = "UltimatePDF/1.0";

        private static readonly BrowserInstancePool pool = new ();

        public async static Task<byte[]> PrintPDF(
            Uri uri, string baseUrl, string locale, string timezone, IEnumerable<CookieParam> cookies,
            ViewPortOptions viewport, PdfOptions options, int timeoutSeconds, Logger logger) {
            
            logger.Log("Page open...");

            Stopwatch sw = new();
            sw.Start();

            using var pooled = await pool.NewPooledPage(logger);
            await SetupPage(pooled.Page, uri, viewport, locale, timezone, sslOffloadingHeader: "", cookies, timeoutSeconds);

            logger.Log("Page opened in " + sw.ElapsedMilliseconds + "ms");
            await pooled.Page.WaitForSelectorAsync(":root:not(.ultimate-pdf-is-not-ready)");
            logger.Log("Page opened and ready in " + sw.ElapsedMilliseconds + "ms");

            if (!string.IsNullOrEmpty(baseUrl)) {
                await pooled.Page.EvaluateExpressionAsync("window?.UltimatePDF?.setBaseUrl?.('" + HttpUtility.JavaScriptStringEncode(baseUrl) + "')");
            }

            bool hasLayouts = await pooled.Page.EvaluateExpressionAsync<bool>("window?.UltimatePDF?.hasLayouts?.()");
            if (hasLayouts) {
                logger.Log("Using UltimatePDF layout pipeline");
                logger.Log("Render PDF with layout in " + sw.ElapsedMilliseconds + "ms");
                byte[] pdf = await RenderLayoutPipeline(pooled, logger);
                logger.Log("Finish rendering PDF in " + sw.ElapsedMilliseconds + "ms");
                return pdf;
            } else {
                logger.Log("Render PDF without layout in " + sw.ElapsedMilliseconds + "ms");
                await InjectCustomStylesAsync(pooled.Page, ref options);
                byte[] pdf = await pooled.Page.PdfDataAsync(options);
                logger.Log("Finish rendering PDF in " + sw.ElapsedMilliseconds + "ms");
                return pdf;
            }
        }

        public async static Task<byte[]> ScreenshotPNG(
            Uri uri, string baseUrl, string locale, string timezone, IEnumerable<CookieParam> cookies,
            ViewPortOptions viewport, ScreenshotOptions options, int timeoutSeconds, Logger logger) {
            
            logger.Log("Page open...");

            var sw = new Stopwatch();
            sw.Start();

            using var pooled = await pool.NewPooledPage(logger);
            await SetupPage(pooled.Page, uri, viewport, locale, timezone, sslOffloadingHeader: "", cookies, timeoutSeconds);

            logger.Log("Page opened in " + sw.ElapsedMilliseconds + "ms");
            await pooled.Page.WaitForSelectorAsync(":root:not(.ultimate-pdf-is-not-ready)");
            logger.Log("Page is ready");

            if (!string.IsNullOrEmpty(baseUrl)) {
                await pooled.Page.EvaluateExpressionAsync("window?.UltimatePDF?.setBaseUrl?.('" + HttpUtility.JavaScriptStringEncode(baseUrl) + "')");
            }

            if (logger.IsEnabled) {
                string html = await pooled.Page.GetContentAsync();
                logger.Attach("input.html", Encoding.UTF8.GetBytes(html));
            }

            byte[] png = await pooled.Page.ScreenshotDataAsync(options);
            logger.Attach("output.png", png);
            return png;
        }

        private static async Task SetupPage(
            IPage page, Uri uri, ViewPortOptions viewport, string locale, string timezone, string sslOffloadingHeader, 
            IEnumerable<CookieParam> cookies, int timeout) {
            
            await page.SetViewportAsync(viewport);

            string originalUserAgent = await page.Browser.GetUserAgentAsync();
            await page.SetUserAgentAsync($"{originalUserAgent} {USER_AGENT_SUFFIX}");

            if (!string.IsNullOrEmpty(locale)) {
                await page.SetExtraHttpHeadersAsync(GetLocaleHeaders(locale));
                await page.EvaluateExpressionOnNewDocumentAsync(GetLocaleExpressionToEvaluate(locale));
            }

            if (!string.IsNullOrEmpty(timezone)) {
                await page.EmulateTimezoneAsync(timezone);
            }

            if (!string.IsNullOrEmpty(sslOffloadingHeader)) {
                var httpHeaders = new Dictionary<string, string>();
                string[] headerParts = sslOffloadingHeader.Split(new char[] { ':' }, 2);
                httpHeaders.Add(headerParts[0].Trim(), headerParts[1].Trim());

                await page.SetExtraHttpHeadersAsync(httpHeaders);
            }

            if (cookies.Any()) {
                await page.SetCookieAsync(cookies.ToArray());
            }

            var navigationOptions = new NavigationOptions() {
                WaitUntil = new WaitUntilNavigation[] {
                            WaitUntilNavigation.DOMContentLoaded,
                            WaitUntilNavigation.Load,
                            WaitUntilNavigation.Networkidle0
                }
            };

            if (timeout > 0) {
                navigationOptions.Timeout = timeout * 1000;
            }

            lock (page.Browser) {
                page.GoToAsync(uri.ToString(), navigationOptions).Wait();
            }

            if (!string.IsNullOrEmpty(locale)) {
                await page.EvaluateExpressionAsync("window?.UltimatePDF?.runtime?.setLocale?.('" + HttpUtility.JavaScriptStringEncode(locale) + "')");
            }
        }

        private static Dictionary<string, string> GetLocaleHeaders(string locale) {
            var headers = new Dictionary<string, string>();

            int separatorIndex = locale.IndexOf("-");
            if (separatorIndex > 0) {
                headers.Add("Accept-Language", string.Concat(locale, ",", locale[0..separatorIndex]));
            } else {
                headers.Add("Accept-Language", locale);
            }

            return headers;
        }

        private static string GetLocaleExpressionToEvaluate(string locale) {
            return @"
                Object.defineProperty(navigator, 'language', {
                    get: function() { return '" + HttpUtility.JavaScriptStringEncode(locale) + @"'; }
                };
                Object.defineProperty(navigator, 'languages', {
                    get: function() {
                        var separatorIndex = this.language.indexOf('-');
                        if (separatorIndex > 0) {
                            return [ this.language, this.language.substr(0, separatorIndex) ];
                        } else {
                            return [ this.language ];
                        }
                    }
                };
            ";
        }

        private static async Task<byte[]> RenderLayoutPipeline(PooledPage pooled, Logger logger) {
            PrintSection? currentSection = null;
            var pdfs = new List<LayoutPrint>();

            var options = new PdfOptions() {
                OmitBackground = true,
                PrintBackground = true,
                PreferCSSPageSize = true
            };

            /* Render each layout's content individually, collect them in a list. */
            int i = 1;
            while (await pooled.Page.EvaluateFunctionAsync<bool>("window.UltimatePDF.nextLayout")) {
                int? firstPage = await pooled.Page.EvaluateFunctionAsync<int?>("window.UltimatePDF.firstPageNumber");

                if (firstPage.HasValue) {
                    currentSection = new PrintSection(firstPage.Value);
                } else {
                    currentSection ??= new PrintSection(1);
                }

                byte[] background = null!;
                bool hasBackground = await pooled.Page.EvaluateFunctionAsync<bool>("window.UltimatePDF.prepareBackgroundLayout");
                if (hasBackground) {
                    background = await pooled.Page.PdfDataAsync(options);

                    logger.Attach($"layout-{i}-background.pdf", background);
                }


                await pooled.Page.EvaluateFunctionAsync<bool>("window.UltimatePDF.prepareContentLayout");
                byte[] content = await pooled.Page.PdfDataAsync(options);
                var pdf = new LayoutPrint(i, currentSection, content);

                logger.Attach($"layout-{i}-content.pdf", content);

                if (background != null) {
                    pdf.MergeBackground(background);
                }

                currentSection.AddPages(pdf.Pages);
                pdfs.Add(pdf);

                i++;
            }


            /* For each layout, render its header and footer, and merge them on the pdf */
            foreach (var pdf in pdfs) {
                await pooled.Page.EvaluateFunctionAsync<bool>("window.UltimatePDF.nextLayout");

                bool hasHeader = await pooled.Page.EvaluateFunctionAsync<bool>("window.UltimatePDF.prepareHeaderLayout", pdf.FirstPage, pdf.Pages, pdf.LastPage);
                if (hasHeader) {
                    byte[] header = await pooled.Page.PdfDataAsync(options);

                    logger.Attach($"layout-{pdf.LayoutNumber}-header.pdf", header);

                    pdf.MergeHeader(header);
                }

                bool hasFooter = await pooled.Page.EvaluateFunctionAsync<bool>("window.UltimatePDF.prepareFooterLayout", pdf.FirstPage, pdf.Pages, pdf.LastPage);
                if (hasFooter) {
                    byte[] footer = await pooled.Page.PdfDataAsync(options);

                    logger.Attach($"layout-{pdf.LayoutNumber}-footer.pdf", footer);

                    pdf.MergeFooter(footer);
                }
            }

            /* Concatenate all layouts into a single pdf */
            return LayoutPrint.Concatenate(pdfs);
        }

        private static Task InjectCustomStylesAsync(IPage page, ref PdfOptions options) {
            /*
             * It seems that Puppeteer is not overriding the page styles from the print stylesheet.
             * As a workaround, we inject a <style> tag with the @page overrides at the end of <head>.
             * This issue might be eventually resolved in Puppeteer, and seems to be tracked by https://github.com/GoogleChrome/puppeteer/issues/393
             */
            string overrides = string.Empty;
            if (!options.PreferCSSPageSize && options.Width != null && options.Height != null) {
                overrides += $"size: {options.Width} {options.Height}; ";
            }
            if (options.MarginOptions.Top != null) {
                overrides += $"margin-top: {options.MarginOptions.Top}; ";
            }
            if (options.MarginOptions.Right != null) {
                overrides += $"margin-right: {options.MarginOptions.Right}; ";
            }
            if (options.MarginOptions.Bottom != null) {
                overrides += $"margin-bottom: {options.MarginOptions.Bottom}; ";
            }
            if (options.MarginOptions.Left != null) {
                overrides += $"margin-left: {options.MarginOptions.Left}; ";
            }

            if (!string.IsNullOrEmpty(overrides)) {
                /* Change the options so that Puppeteer respects our overrides */
                options.PreferCSSPageSize = true;
                options.Width = options.Height = null;
                options.MarginOptions = new MarginOptions();

                /* We must add the <style> tag at the end of <body> to make sure it is not overriden */
                string pageOverrides = "@page { " + overrides + "}";
                return page.EvaluateExpressionAsync($"const style = document.createElement('style'); style.innerHTML = '{pageOverrides}'; document.head.appendChild(style);");
            } else {
                return Task.CompletedTask;
            }
        }
    }
}
