using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutSystems.UltimatePDF_ExternalLogic.LayoutPrintPipeline {
    public class PrintSection {

        private readonly int firstPage;
        private int pages;

        public PrintSection(int firstPage) {
            this.firstPage = firstPage;
        }

        public void AddPages(int pages) {
            this.pages += pages;
        }

        public int FirstPage {
            get { return firstPage; }
        }

        public int NextPage {
            get { return firstPage + pages; }
        }

        public int Pages {
            get { return pages; }
        }

    }
}
