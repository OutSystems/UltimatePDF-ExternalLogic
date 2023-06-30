using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace OutSystems.HeadlessChromium.Puppeteer.BrowserProducts {
    internal class BrowserProductFactory {

        private static readonly BrowserProductFactory instance = new BrowserProductFactory();

        private readonly BrowserProduct[] browserProducts = new BrowserProduct[] {
            new ChromiumBrowserProduct(),
            new FirefoxBrowserProduct()
        };

        private BrowserProductFactory() {
        }


        public static BrowserProductFactory Instance {
            get { return instance; }
        }

        public BrowserProduct[] BrowserProducts {
            get { return browserProducts; }
        }


        public BrowserProduct FromProduct(Product product) {
            switch (product) {
                case Product.Chrome:
                    return new ChromiumBrowserProduct();
                case Product.Firefox:
                    return new FirefoxBrowserProduct();
                default:
                    throw new InvalidOperationException("Unknown product " + product.ToString());
            }
        }

    }
}
