using System;
using HeadlessChromium.Puppeteer.Lambda.Dotnet;
using Microsoft.Extensions.Logging;

var logger = new LoggerFactory();
var browserLauncher = new HeadlessChromiumPuppeteerLauncher(logger);

using (var browser = await browserLauncher.LaunchAsync())
using (var page = await browser.NewPageAsync()) {
    await page.GoToAsync("https://google.com");
    await page.ScreenshotDataAsync();
}