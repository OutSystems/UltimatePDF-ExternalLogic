using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace OutSystems.HeadlessChromium.Puppeteer.BrowserProducts {
    internal class FirefoxBrowserProduct : BrowserProduct {

        public override Product Product { get { return Product.Firefox; } }

        public override string ProcessName { get { return "firefox"; } }

        public override string RevisionFolder { get { return ".local-firefox"; } }

        public override void AssertHealthyRevision(RevisionInfo revision) {
            base.AssertHealthyRevision(revision);

            // TODO
        }

        public override NavigationOptions DefaultNavigationOptions() {
            return new NavigationOptions() {
                Timeout = 0,
                WaitUntil = new WaitUntilNavigation[] {
                            WaitUntilNavigation.DOMContentLoaded,
                            WaitUntilNavigation.Load
                }
            };
        }

    }

}
