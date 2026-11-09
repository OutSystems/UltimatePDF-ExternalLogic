using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UltimatePDF_ExternalLogic.Utils;
internal class ResourceAccessor {
    public static string GetResource(string resourceName) {
        using var activity = Activity.Current?.Source.StartActivity("ResourceAccessor.GetResource");
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) {
            throw new ArgumentException("Resource not found");
        }
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
