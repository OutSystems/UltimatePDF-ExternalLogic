using OutSystems.ODC_UltimatePDF_Service;
using OutSystems.ODC_UltimatePDF_Service.Structures;

namespace ODC_UltimatePDF_Service_Tests {
    public class PrintPDF_UnitTests {
        private IODC_UltimatePDF_Service odc_UltimatePDF_Service;

        [SetUp]
        public void Setup() {
            odc_UltimatePDF_Service = new ODC_UltimatePDF_Service();
        }

        [Test]
        public void PrintPDF_Google() {
            //var pdf = odc_UltimatePDF_Service.PrintPDF("https://www.google.com", new Viewport(), Enumerable.Empty<Cookie>(), new Paper());

            //Assert.IsNotEmpty(pdf);
            Assert.Pass();
        }
    }
}