using System.IO;
using System.IO.Compression;
using System.Linq;

namespace OutSystems.UltimatePDF_ExternalLogic.IntegrationTests.TestHelpers {

    internal static class ZipAssert {

        internal static void ContainsEntry(byte[] zip, string entryName) {
            using var ms = new MemoryStream(zip);
            using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
            Assert.Contains(archive.Entries, e => e.FullName == entryName);
        }
    }
}
