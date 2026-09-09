using System.Collections.Concurrent;
using OutSystems.UltimatePDF_ExternalLogic.IntegrationTests.TestHelpers;

namespace OutSystems.UltimatePDF_ExternalLogic.IntegrationTests.Fixtures;

public sealed class MockRestReceiverFixture : IAsyncLifetime, IAsyncDisposable {

    private WebApplication? _app;
    private readonly ConcurrentQueue<(string ct, byte[] body)> _pdfs = new();
    private readonly ConcurrentQueue<(string ct, byte[] body)> _logs = new();
    private readonly ConcurrentQueue<byte[]> _s3Objects = new();

    public string BaseUrl { get; private set; } = string.Empty;

    public IReadOnlyList<byte[]> StoredPdfs =>
        _pdfs.Select(x => x.body).ToList();
    public IReadOnlyList<string> StoredPdfContentTypes =>
        _pdfs.Select(x => x.ct).ToList();
    public IReadOnlyList<byte[]> StoredLogs =>
        _logs.Select(x => x.body).ToList();
    public IReadOnlyList<string> StoredLogContentTypes =>
        _logs.Select(x => x.ct).ToList();
    public IReadOnlyList<byte[]> StoredS3Objects =>
        _s3Objects.ToList();

    public void Reset() {
        while (_pdfs.TryDequeue(out _)) { }
        while (_logs.TryDequeue(out _)) { }
        while (_s3Objects.TryDequeue(out _)) { }
    }

    public async ValueTask InitializeAsync() {
        var (app, baseUrl) = await LoopbackWebHostFactory.StartAsync(a => {
            a.MapPost("/api/store", async (HttpRequest req) => {
                using var ms = new System.IO.MemoryStream();
                await req.Body.CopyToAsync(ms);
                _pdfs.Enqueue((req.ContentType ?? string.Empty, ms.ToArray()));
                return Results.Ok();
            });

            a.MapPost("/api/logs", async (HttpRequest req) => {
                using var ms = new System.IO.MemoryStream();
                await req.Body.CopyToAsync(ms);
                _logs.Enqueue((req.ContentType ?? string.Empty, ms.ToArray()));
                return Results.Ok();
            });

            a.MapPost("/api/fail", () => Results.BadRequest());

            a.MapPut("/s3/object", async (HttpRequest req) => {
                using var ms = new System.IO.MemoryStream();
                await req.Body.CopyToAsync(ms);
                _s3Objects.Enqueue(ms.ToArray());
                return Results.Ok();
            });
        });
        _app = app;
        BaseUrl = baseUrl;
    }

    public async ValueTask DisposeAsync() {
        if (_app is not null) {
            await _app.DisposeAsync();
            _app = null;
        }
    }
}