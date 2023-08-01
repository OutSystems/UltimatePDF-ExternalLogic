namespace OutSystems.UltimatePDF_ExternalLogic.Tests {
    public class Tests {
        private static string OsLogoLink = @"https://www.outsystems.com/-/media/themes/outsystems/website/site-theme/imgs/logo.svg";

        private static Structures.Viewport GetDefaultViewport() {
            var viewport = new Structures.Viewport();
            return viewport;
        }

        private static Structures.Environment GetDefaultEnvironment() {
            var env = new Structures.Environment();
            return env;
        }

        private static Structures.Paper GetDefaultPaper() {
            var paper = new Structures.Paper();
            return paper;
        }

        [SetUp]
        public void Setup() {
        }

        [Test]
        public void GeneratePDF() {
            var ultimatePDF_ExternalLogic = new UltimatePDF_ExternalLogic();
            var pdf = ultimatePDF_ExternalLogic.PrintPDF(
                url: OsLogoLink,
                viewport: GetDefaultViewport(),
                environment: GetDefaultEnvironment(),
                cookies: Array.Empty<Structures.Cookie>(),
                paper: GetDefaultPaper(),
                timeoutSeconds: 60,
                collectLogs: true,
                out byte[] logsZipFile);
            Assert.Pass();
        }
      
        /*
   [OSAction(Description = "Generate PDF file from a webpage, and send the information using a REST endpoint")]
   public void PrintPDF_ToRest(
       [OSParameter(DataType = OSDataType.Text, Description = "URL of the page to download")]
   string url,
       [OSParameter(Description = "Bowser viewport configuration")]
   Structures.Viewport viewport,
       [OSParameter(Description = "Environment information")]
   Structures.Environment environment,
       [OSParameter(Description = "List of cookies to add to the browser when accessing the page")]
   IEnumerable<Structures.Cookie> cookies,
       [OSParameter(Description = "PDF paper configuration")]
   Structures.Paper paper,
       [OSParameter(DataType = OSDataType.Integer, Description = "Browser render execution timeout in seconds")]
   int timeoutSeconds,
       [OSParameter(DataType = OSDataType.Boolean, Description = "Collects execution logs. If False LogsZipFile will be empty.")]
   bool collectLogs,
       [OSParameter(Description = "Rest call configuration")]
   Structures.RestCaller restCaller);*/
    }
}