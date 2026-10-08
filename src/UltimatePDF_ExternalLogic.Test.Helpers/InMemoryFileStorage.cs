using OutSystems.ExternalLibraries.SDK;
using OutSystems.ExternalLibraries.SDK.FileStorage;

namespace OutSystems.UltimatePDF_ExternalLogic.Test.Helpers;

/// <summary>
/// An <see cref="IFileStorage"/> that keeps content in memory, recording every write so tests can
/// assert on the name, content type and bytes the library stored.
/// </summary>
/// <remarks>
/// The real implementations are not published on NuGet and <see cref="FileStorage.GetInstance"/>
/// resolves a loader that only exists after an ODC build, so this fake is the only way to exercise
/// the File actions off-platform.
/// </remarks>
public sealed class InMemoryFileStorage : IFileStorage {

    /// <summary>One write, in the order it happened.</summary>
    public sealed record StoredFile(string FileName, string ContentType, byte[] Content, IOSFile Reference);

    private readonly List<StoredFile> stored = new();
    private readonly Dictionary<string, StoredFile> storedByKey = new(StringComparer.Ordinal);

    /// <summary>Everything written so far, oldest first.</summary>
    public IReadOnlyList<StoredFile> Stored => stored;

    /// <summary>Looks up a write by the File reference handed back to the caller.</summary>
    public StoredFile Find(IOSFile file) {
        if (!storedByKey.TryGetValue(file.Key, out var match)) {
            throw new KeyNotFoundException($"No file was stored with the key '{file.Key}'.");
        }

        return match;
    }

    public Task<IOSFile> CreateFileAsync(string fileName, string contentType, byte[] content,
            IDictionary<string, string>? additionalUserData = null, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(Store(fileName, contentType, content));
    }

    public async Task<IOSFile> CreateFileFromStreamAsync(string fileName, string contentType, Stream contentStream,
            IDictionary<string, string>? additionalUserData = null, CancellationToken cancellationToken = default) {
        using var buffer = new MemoryStream();
        await contentStream.CopyToAsync(buffer, cancellationToken);

        return Store(fileName, contentType, buffer.ToArray());
    }

    public async Task<IOSFile> CreateFileFromWriterAsync(string fileName, string contentType,
            Func<Stream, CancellationToken, Task> writeAsync,
            IDictionary<string, string>? additionalUserData = null, CancellationToken cancellationToken = default) {
        using var buffer = new MemoryStream();
        await writeAsync(buffer, cancellationToken);

        return Store(fileName, contentType, buffer.ToArray());
    }

    public Task<byte[]> ReadFileAsync(IOSFile file, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(Find(file).Content);
    }

    public Task<Stream> ReadFileAsStreamAsync(IOSFile file, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult<Stream>(new MemoryStream(Find(file).Content, writable: false));
    }

    public Task<Stream> ReadFileRangeAsync(IOSFile file, long startByte, long endByte,
            CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();

        var content = Find(file).Content;
        var length = (int)Math.Min(endByte - startByte + 1, content.LongLength - startByte);

        return Task.FromResult<Stream>(new MemoryStream(content, (int)startByte, length, writable: false));
    }

    public Task<IFileMetadata> GetFileMetadataAsync(IOSFile file, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();

        var userData = new Dictionary<string, string>(StringComparer.Ordinal) {
            [IFileMetadata.Keys.FileName] = Find(file).FileName,
        };

        return Task.FromResult<IFileMetadata>(new FakeFileMetadata(userData));
    }

    private IOSFile Store(string fileName, string contentType, byte[] content) {
        var reference = new FakeOSFile {
            ContentType = contentType,
            Size = content.LongLength,
        };

        var record = new StoredFile(fileName, contentType, content, reference);
        stored.Add(record);
        storedByKey[reference.Key] = record;

        return reference;
    }
}
