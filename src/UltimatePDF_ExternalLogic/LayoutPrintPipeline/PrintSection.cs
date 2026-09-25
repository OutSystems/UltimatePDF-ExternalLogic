using System.Diagnostics;

namespace OutSystems.UltimatePDF_ExternalLogic.LayoutPrintPipeline;

public class PrintSection(int firstPage) {

    private int pages;

    public void AddPages(int pages) {
        using var activity = Activity.Current?.Source.StartActivity("PrintSection.AddPages");
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
