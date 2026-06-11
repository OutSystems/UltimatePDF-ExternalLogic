using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using OutSystems.UltimatePDF_ExternalLogic.E2ETests.Models;

namespace OutSystems.UltimatePDF_ExternalLogic.E2ETests.Fixtures;

public sealed class OdcTenantFixture : IAsyncLifetime {

    public HttpClient Client { get; private set; } = null!;
    public string TestPageUrl { get; private set; } = string.Empty;

    public async ValueTask InitializeAsync() {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var tenantEndpoint  = config["TenantEndpoint"]!;
        var environmentKey  = config["EnvironmentKey"]!;
        var apiClientId     = config["ApiClientId"]!;
        var apiClientSecret = config["ApiClientSecret"]!;
        var applicationKey  = config["ApplicationKey"]!;
        TestPageUrl         = config["TestPageUrl"]!;

        ValidateConfiguration(tenantEndpoint, environmentKey, apiClientId, apiClientSecret, applicationKey, TestPageUrl);

        // Step 1 — Discover token endpoint via OpenID configuration
        using var authClient = new HttpClient();
        var oidcUrl = $"{tenantEndpoint}/identity/.well-known/openid-configuration";
        var oidcResp = await authClient.GetAsync(oidcUrl);
        oidcResp.EnsureSuccessStatusCode();
        var oidcJson = await oidcResp.Content.ReadAsStringAsync();
        var oidcDoc = JsonSerializer.Deserialize<JsonElement>(oidcJson);
        var tokenUrl = oidcDoc.GetProperty("token_endpoint").GetString()!;

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

        // Step 3 — Resolve the environment's default app hostname
        using var portalClient = new HttpClient();
        portalClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.AccessToken);

        var domainsUrl = $"{tenantEndpoint}/api/environment-configurations/v1/environments/{environmentKey}/domains";
        var domainsResp = await portalClient.GetAsync(domainsUrl);
        if (!domainsResp.IsSuccessStatusCode) {
            var errBody = await domainsResp.Content.ReadAsStringAsync();
            throw new HttpRequestException($"GET {domainsUrl} → {(int)domainsResp.StatusCode} {domainsResp.ReasonPhrase}: {errBody}");
        }
        var domainsDoc = JsonSerializer.Deserialize<JsonElement>(await domainsResp.Content.ReadAsStringAsync());
        var appHostname = domainsDoc.GetProperty("results").EnumerateArray()
            .First(d => d.GetProperty("isDefault").GetBoolean())
            .GetProperty("hostname").GetString()!;

        // Step 4 — Fetch deployed configuration to extract key, revisionBaseline, and setting key
        var getConfigUrl = $"{tenantEndpoint}/api/asset-configurations/v1/environments/{environmentKey}/applications/{applicationKey}/revisions/deployed/configurations";
        var getResp = await portalClient.GetAsync(getConfigUrl);
        if (!getResp.IsSuccessStatusCode) {
            var errBody = await getResp.Content.ReadAsStringAsync();
            throw new HttpRequestException($"GET {getConfigUrl} → {(int)getResp.StatusCode} {getResp.ReasonPhrase}: {errBody}");
        }
        var configDoc = JsonSerializer.Deserialize<JsonElement>(await getResp.Content.ReadAsStringAsync());
        var configKey       = configDoc.GetProperty("key").GetString()!;
        var revisionBase    = configDoc.GetProperty("revisionBaseline").GetInt32();
        var cicdSettingKey  = configDoc.GetProperty("settings").EnumerateArray()
            .First(s => s.GetProperty("name").GetString() == "cicd_run_secret")
            .GetProperty("key").GetString()!;

        // Step 5 — Push secret to ODC app configuration
        var patchConfigUrl = $"{tenantEndpoint}/api/asset-configurations/v1/environments/{environmentKey}/applications/{applicationKey}/configurations";
        var payload = new OdcConfigurationPayload(
            Key: configKey,
            RevisionBaseline: revisionBase,
            Settings: [new OdcConfigSetting(cicdSettingKey, secret)]);
        var configResp = await portalClient.PatchAsJsonAsync(patchConfigUrl, payload);
        if (!configResp.IsSuccessStatusCode) {
            var errBody = await configResp.Content.ReadAsStringAsync();
            throw new HttpRequestException($"PATCH {patchConfigUrl} → {(int)configResp.StatusCode} {configResp.ReasonPhrase}: {errBody}");
        }

        // Step 6 — Trigger ApplyConfigs so the new setting takes effect in the environment
        var publishUrl = $"{tenantEndpoint}/api/deployments/v1/deployment-operations";
        var publishPayload = new PublishOperationRequest(
            Operation: "ApplyConfigs",
            AssetKey: applicationKey,
            Revision: revisionBase,
            EnvironmentKey: environmentKey);
        var publishResp = await portalClient.PostAsJsonAsync(publishUrl, publishPayload);
        if (!publishResp.IsSuccessStatusCode) {
            var errBody = await publishResp.Content.ReadAsStringAsync();
            throw new HttpRequestException($"POST {publishUrl} → {(int)publishResp.StatusCode} {publishResp.ReasonPhrase}: {errBody}");
        }

        // Allow the new secret to propagate to running app instances before we start using it.
        await Task.Delay(TimeSpan.FromSeconds(5));

        // Step 7 — Build test HttpClient against the resolved app hostname
        Client = new HttpClient {
            BaseAddress = new Uri($"https://{appHostname}/UltimatePDFTests/rest/cicd_tests/")
        };
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", secret);
    }

    public async ValueTask DisposeAsync() {
        Client?.Dispose();
        await ValueTask.CompletedTask;
    }

    private static void ValidateConfiguration(
        string tenantEndpoint, string environmentKey,
        string apiClientId, string apiClientSecret,
        string applicationKey, string testPageUrl) {
        var errors = new List<string>();

        Check(tenantEndpoint,  "TenantEndpoint",  "https://example.outsystems.dev");
        Check(apiClientId,     "ApiClientId",     "000000-ApiClientId-000000");
        Check(apiClientSecret, "ApiClientSecret", "000000-ApiClientSecret-000000=");
        Check(environmentKey,  "EnvironmentKey",  "00000000-0000-0000-0000-000000000000");
        Check(applicationKey,  "ApplicationKey",  "0000000-0000-0000-0000-000000000000");
        Check(testPageUrl,     "TestPageUrl",     "https://example.com/report");

        if (errors.Count > 0) {
            throw new InvalidOperationException(
                "E2E test configuration contains placeholder values. " +
                "Copy appsettings.template.json to appsettings.json and fill in real tenant details.\n" +
                string.Join("\n", errors));
        }

        void Check(string value, string key, string placeholder) {
            if (string.IsNullOrWhiteSpace(value))
                errors.Add($"  {key}: missing or empty");
            else if (string.Equals(value, placeholder, StringComparison.OrdinalIgnoreCase))
                errors.Add($"  {key}: still set to the template placeholder \"{placeholder}\"");
        }
    }
}
