using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using OutSystems.UltimatePDF_ExternalLogic.TenantTests.Models;

namespace OutSystems.UltimatePDF_ExternalLogic.TenantTests.Fixtures;

public sealed class OdcTenantFixture : IAsyncLifetime {

    public HttpClient Client { get; private set; } = null!;
    public string TestPageUrl { get; private set; } = string.Empty;

    public async ValueTask InitializeAsync() {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var tenantEndpoint  = config["TenantEndpoint"]!;
        var environmentKey  = config["EnvironmentKey"]!;
        var apiClientId     = config["ApiClientId"]!;
        var apiClientSecret = config["ApiClientSecret"]!;
        var applicationKey  = config["ApplicationKey"]!;
        TestPageUrl         = config["TestPageUrl"]!;

        // Step 1 — Authenticate
        using var authClient = new HttpClient();
        var tokenUrl = $"{tenantEndpoint}/identity/connect/token";
        var form = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("grant_type",    "client_credentials"),
            new KeyValuePair<string, string>("client_id",     apiClientId),
            new KeyValuePair<string, string>("client_secret", apiClientSecret),
        ]);
        var tokenResp = await authClient.PostAsync(tokenUrl, form);
        tokenResp.EnsureSuccessStatusCode();
        var tokenJson = await tokenResp.Content.ReadAsStringAsync();
        var token = JsonSerializer.Deserialize<OdcTokenResponse>(tokenJson)!;

        // Step 2 — Generate secret
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        var key = Convert.ToHexString(RandomNumberGenerator.GetBytes(64)).ToLowerInvariant();
        var secret = $"{timestamp} {key}";

        // Step 3 — Push secret to ODC app configuration
        var configUrl = $"{tenantEndpoint}/environments/{environmentKey}/applications/{applicationKey}/configurations";
        var payload = new[] { new OdcConfigEntry("cicd_run_secret", secret) };
        using var portalClient = new HttpClient();
        portalClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.AccessToken);
        var configResp = await portalClient.PutAsJsonAsync(configUrl, payload);
        configResp.EnsureSuccessStatusCode();

        // Step 4 — Build test HttpClient
        Client = new HttpClient {
            BaseAddress = new Uri($"{tenantEndpoint}/UltimatePDFTests/rest/cicd_tests/")
        };
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", secret);
    }

    public async ValueTask DisposeAsync() {
        Client?.Dispose();
        await ValueTask.CompletedTask;
    }
}
