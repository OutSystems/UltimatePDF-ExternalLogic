using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.UltimatePDF_ExternalLogic.Test.Helpers;

/// <summary>
/// A File reference for tests. The SDK's own implementation of <see cref="IOSFile"/> is internal to
/// the platform, so tests need their own.
/// </summary>
public sealed class FakeOSFile : IOSFile {

    public string Key { get; init; } = Guid.NewGuid().ToString();

    public string Version { get; init; } = "1";

    public string ContentType { get; init; } = "application/octet-stream";

    public long Size { get; init; }
}
