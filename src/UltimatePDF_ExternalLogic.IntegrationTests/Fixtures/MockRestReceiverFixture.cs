using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace OutSystems.UltimatePDF_ExternalLogic.IntegrationTests.Fixtures {

    public sealed class MockRestReceiverFixture : IAsyncLifetime, IAsyncDisposable {

        private WebApplication? _app;
        private readonly ConcurrentQueue<(string ct, byte[] body)> _pdfs = new();
        private readonly ConcurrentQueue<(string ct, byte[] body)> _logs = new();

        public string BaseUrl { get; private set; } = string.Empty;

        public IReadOnlyList<byte[]> StoredPdfs =>
            _pdfs.Select(x => x.body).ToList();
        public IReadOnlyList<string> StoredPdfContentTypes =>
            _pdfs.Select(x => x.ct).ToList();
        public IReadOnlyList<byte[]> StoredLogs =>
            _logs.Select(x => x.body).ToList();

        public void Reset() {
            while (_pdfs.TryDequeue(out _)) { }
            while (_logs.TryDequeue(out _)) { }
        }

        public async ValueTask InitializeAsync() {
            var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions());
            builder.WebHost.UseKestrelCore();
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Services.AddRoutingCore();

            var app = builder.Build();

            app.MapPost("/api/store", async (HttpRequest req) => {
                using var ms = new System.IO.MemoryStream();
                await req.Body.CopyToAsync(ms);
                _pdfs.Enqueue((req.ContentType ?? string.Empty, ms.ToArray()));
                return Results.Ok();
            });

            app.MapPost("/api/logs", async (HttpRequest req) => {
                using var ms = new System.IO.MemoryStream();
                await req.Body.CopyToAsync(ms);
                _logs.Enqueue((req.ContentType ?? string.Empty, ms.ToArray()));
                return Results.Ok();
            });

            app.MapPost("/api/fail", () => Results.BadRequest());

            await app.StartAsync();

            BaseUrl = app.Services
                .GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()!.Addresses.First();
            _app = app;
        }

        public async ValueTask DisposeAsync() {
            if (_app is not null) {
                await _app.DisposeAsync();
                _app = null;
            }
        }
    }
}
