using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace OutSystems.UltimatePDF_ExternalLogic.IntegrationTests.TestHelpers;

internal static class LoopbackWebHostFactory {
    /// <summary>
    /// Creates, configures, and starts a Kestrel loopback web app bound to an ephemeral port.
    /// Callers add routes before calling StartAsync via the returned builder's app instance.
    /// Returns the started <see cref="WebApplication"/> and the resolved base URL.
    /// </summary>
    internal static async Task<(WebApplication app, string baseUrl)> StartAsync(
        System.Action<WebApplication> configureRoutes) {
        var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions());
        builder.WebHost.UseKestrelCore();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddRoutingCore();

        var app = builder.Build();
        configureRoutes(app);
        await app.StartAsync();

        var baseUrl = app.Services
            .GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!.Addresses.First();

        return (app, baseUrl);
    }
}
