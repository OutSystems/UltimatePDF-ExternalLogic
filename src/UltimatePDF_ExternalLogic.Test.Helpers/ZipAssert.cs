using System.IO.Compression;
using Xunit;

namespace OutSystems.UltimatePDF_ExternalLogic.Test.Helpers;

public static class ZipAssert {

    public static void ContainsEntry(byte[] zip, string entryName) {
        using var ms = new MemoryStream(zip);
        using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
        Assert.Contains(archive.Entries, e => e.FullName == entryName);
    }
}