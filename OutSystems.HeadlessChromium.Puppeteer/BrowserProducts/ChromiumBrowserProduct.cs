using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace OutSystems.HeadlessChromium.Puppeteer.BrowserProducts {
    internal class ChromiumBrowserProduct : BrowserProduct {

        public override Product Product { get { return Product.Chrome; } }

        public override string ProcessName { get { return "chrome"; } }

        public override string RevisionFolder { get { return ".local-chromium"; } }

        public override void AssertHealthyRevision(RevisionInfo revision) {
            base.AssertHealthyRevision(revision);

            FileInfo executable = new FileInfo(revision.ExecutablePath);
            DirectoryInfo folder = executable.Directory;

            // These files are required for a healthy startup of Chromium.
            // The list is not exhaustive.
            string[] chromeRequiredFiles = new string[] {
                "chrome.exe",
                "chrome.dll",
                "chrome_elf.dll",
                "icudtl.dat",
                "v8_context_snapshot.bin"
            };

            foreach (string file in chromeRequiredFiles) {
                string requiredFile = Path.Combine(folder.FullName, file);
                if (!File.Exists(requiredFile)) {
                    throw new FileNotFoundException($"Could not find browser required file {requiredFile}", requiredFile);
                }
            }

            // A missing manifest file may cause the side-by-side error:
            // The application has failed to start because its side-by-side configuration is incorrect
            var manifestFiles = executable.Directory.GetFiles("*.manifest");
            if (manifestFiles.Length == 0) {
                throw new FileNotFoundException($"Could not find browser required manifest file");
            }
        }



        protected override string[] DefaultBrowserArgs() {
            return new string[] {
                // Improvements to startup time
                "--proxy-server='direct://'",
                "--proxy-bypass-list=*",

                // Unnecessary features (used by Playwright but not included in PuppeteerSharp by default)
                // Sources:
                // https://github.com/microsoft/playwright/blob/master/src/server/chromium/chromium.ts
                // https://github.com/hardkoded/puppeteer-sharp/blob/master/lib/PuppeteerSharp/ChromiumLauncher.cs
                "--disable-features=Translate,ImprovedCookieControls,LazyFrameLoading,GlobalMediaControls,DestroyProfileOnBrowserClose,MediaRouter",
                "--allow-pre-commit-input",
                "--no-service-autorun",

                // More disabled features
                "--no-sandbox",
                "--disable-cookie-encryption",
                "--disable-crash-reporter",
                "--disable-logging",
            };
        }

    }

}
