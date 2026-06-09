using OutSystems.UltimatePDF_ExternalLogic.IntegrationTests.TestHelpers;

namespace OutSystems.UltimatePDF_ExternalLogic.IntegrationTests.Fixtures;

public sealed class HelloWorldWebFixture : IAsyncLifetime, IAsyncDisposable {

    private WebApplication? _app;

    public string BaseUrl { get; private set; } = string.Empty;

    public async ValueTask InitializeAsync() {
        var (app, baseUrl) = await LoopbackWebHostFactory.StartAsync(a => {
            a.MapGet("/", () => Results.Content(
                "<html><body>Hello, world!</body></html>",
                "text/html; charset=utf-8"));
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