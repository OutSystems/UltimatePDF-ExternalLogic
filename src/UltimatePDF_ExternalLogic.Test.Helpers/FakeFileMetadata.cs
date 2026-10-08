using OutSystems.ExternalLibraries.SDK.FileStorage;

namespace OutSystems.UltimatePDF_ExternalLogic.Test.Helpers;

/// <summary>
/// File metadata for tests. The platform's metadata only carries user data, including the original
/// file name, and never the size — the size lives on the File reference itself.
/// </summary>
public sealed class FakeFileMetadata : IFileMetadata {

    private readonly Dictionary<string, string> userData;

    public FakeFileMetadata(IDictionary<string, string>? userData = null) {
        this.userData = userData is null
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            : new Dictionary<string, string>(userData, StringComparer.Ordinal);
    }

    public IReadOnlyDictionary<string, string> UserData => userData;

    public string? GetMetadata(string key) => userData.TryGetValue(key, out var value) ? value : null;

    public bool TryGetMetadata(string key, out string? value) => userData.TryGetValue(key, out value);
}
