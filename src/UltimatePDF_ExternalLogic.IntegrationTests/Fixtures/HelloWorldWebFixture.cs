using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace OutSystems.UltimatePDF_ExternalLogic.IntegrationTests.Fixtures {

    public sealed class HelloWorldWebFixture : IAsyncLifetime, IAsyncDisposable {

        private WebApplication? _app;

        public string BaseUrl { get; private set; } = string.Empty;

        public async ValueTask InitializeAsync() {
            // CreateEmptyBuilder skips all default config providers and hosted services, so the
            // fixture does not register inotify file watchers. Hosts with a low
            // fs.inotify.max_user_instances limit (default WSL2 and many container hosts) would
            // otherwise fail with IOException during WebApplication.CreateBuilder().
            var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions());
            builder.WebHost.UseKestrelCore();
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Services.AddRoutingCore();

            var app = builder.Build();
            app.MapGet("/", () => Results.Content(
                "<html><body>Hello, world!</body></html>",
                "text/html; charset=utf-8"));

            await app.StartAsync();

            var addresses = app.Services
                .GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()!.Addresses;
            BaseUrl = addresses.First();
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
