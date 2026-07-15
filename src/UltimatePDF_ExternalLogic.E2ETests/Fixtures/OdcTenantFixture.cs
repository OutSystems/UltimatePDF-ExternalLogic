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

        var tenantEndpoint = config["TenantEndpoint"]!;
        var apiClientId = config["ApiClientId"]!;
        var apiClientSecret = config["ApiClientSecret"]!;
        TestPageUrl = config["TestPageUrl"]!;

        ValidateConfiguration(tenantEndpoint, apiClientId, apiClientSecret, TestPageUrl);

        // Step 1 — Discover token endpoint via OpenID configuration
        var token = await GetAuthToken(tenantEndpoint, apiClientId, apiClientSecret);

        using var portalClient = new HttpClient();
        portalClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.AccessToken);

        // Step 2 — Discover EnvironmentKey via Portfolios API
        string? environmentKey = await GetEnvironmentKey(tenantEndpoint, portalClient);

        // Step 3 — Check if "Ultimate PDF Tests" is already deployed in the Development environment
        string? applicationKey = await CheckTestAppIsDeployed(tenantEndpoint, portalClient, environmentKey);

        // Step 4 — Generate secret
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        var key = Convert.ToHexString(RandomNumberGenerator.GetBytes(64)).ToLowerInvariant();
        var secret = $"{timestamp} {key}";

        // Step 5 — Resolve the environment's default app hostname
        string appHostname = await GetAppHostname(tenantEndpoint, portalClient, environmentKey);

        // Step 5a — Probe cicd_test endpoint to verify deployed version is compatible
        var probeUrl = $"https://{appHostname}/UltimatePDFTests/rest/cicd_tests/swagger.json";
        var probeResp = await portalClient.GetAsync(probeUrl);
        if (probeResp.StatusCode == System.Net.HttpStatusCode.NotFound) {
            throw new InvalidOperationException(
                "Test Setup Failed: 'Ultimate PDF Tests' is deployed but the 'cicd_tests' " +
                "REST service is missing. Publish the latest version of 'Ultimate PDF Tests.oml' and retry.");
        }

        // Step 6 — Fetch deployed configuration to extract key, revisionBaseline, and setting key
        (string configKey, int revisionBase, string cicdSettingKey) = await FetchTestAppConfiguration(tenantEndpoint, portalClient, environmentKey, applicationKey);

        // Step 7 — Push secret to ODC app configuration
        await PushSecretConfiguration(tenantEndpoint, portalClient, environmentKey, applicationKey, secret, configKey, revisionBase, cicdSettingKey);

        // Step 8 — Trigger ApplyConfigs so the new setting takes effect in the environment
        var applyOp = await TriggerApplyConfigs(tenantEndpoint, portalClient, environmentKey, applicationKey, revisionBase);

        // Poll until ApplyConfigs finishes so the secret is live before tests run
        var applyPollUrl = $"{tenantEndpoint}/api/deployments/v1/deployment-operations/{applyOp.Key}";
        var applyDeadline = DateTime.UtcNow.AddMinutes(2);
        while (DateTime.UtcNow < applyDeadline) {
            await Task.Delay(TimeSpan.FromSeconds(5));
            var applyPollResp = await portalClient.GetAsync(applyPollUrl);
            if (applyPollResp.IsSuccessStatusCode) {
                var applyStatus = JsonSerializer.Deserialize<OdcPublishOperationResponse>(
                    await applyPollResp.Content.ReadAsStringAsync())!;
                if (string.Equals(applyStatus.Status, "Finished", StringComparison.OrdinalIgnoreCase))
                    break;
            }
        }

        // Step 9 — Build test HttpClient against the resolved app hostname
        Client = new HttpClient {
            BaseAddress = new Uri($"https://{appHostname}/UltimatePDFTests/rest/cicd_tests/")
        };
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", secret);
    }

    private static async Task<string?> CheckTestAppIsDeployed(string tenantEndpoint, HttpClient portalClient, string? environmentKey) {
        var appsUrl = $"{tenantEndpoint}/api/portfolios/v1/deployed-assets?nameContains={Uri.EscapeDataString("Ultimate PDF Tests")}";
        var appsResp = await portalClient.GetAsync(appsUrl);
        if (!appsResp.IsSuccessStatusCode) {
            var errBody = await appsResp.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Test Setup Failed: Could not retrieve deployed applications list. Details: {errBody}");
        }
        var appsJson = await appsResp.Content.ReadAsStringAsync();
        var appsDoc = JsonSerializer.Deserialize<JsonElement>(appsJson);
        var assets = appsDoc.GetProperty("results").Deserialize<OdcDeployedAssetItem[]>() ?? [];
        var existingAsset = assets.FirstOrDefault(a =>
            a.Deployments.Any(d =>
                string.Equals(d.EnvironmentKey, environmentKey, StringComparison.OrdinalIgnoreCase)));
        var deployNeeded = existingAsset is null;
        string? applicationKey = existingAsset?.Key;

        if (deployNeeded) {
            // Step 3b-i — Create upload slot
            var uploadsUrl = $"{tenantEndpoint}/api/deployments/v1/uploads";
            var uploadsResp = await portalClient.PostAsync(uploadsUrl, content: null);
            if (!uploadsResp.IsSuccessStatusCode) {
                var errBody = await uploadsResp.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Test Setup Failed: Deployment of 'Ultimate PDF Tests.oml' failed. Details: {errBody}");
            }
            var uploadSlot = JsonSerializer.Deserialize<OdcUploadUrlResponse>(
                await uploadsResp.Content.ReadAsStringAsync())!;

            // Step 3b-ii — Push OML to S3 presigned URL (no Bearer token)
            var omlPath = Path.Combine(AppContext.BaseDirectory, "Ultimate PDF Tests.oml");
            using var s3Client = new HttpClient();
            await using var omlStream = File.OpenRead(omlPath);
            var s3Content = new StreamContent(omlStream);
            s3Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            var s3Resp = await s3Client.PutAsync(uploadSlot.UploadUrl, s3Content);
            if (!s3Resp.IsSuccessStatusCode) {
                var errBody = await s3Resp.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Test Setup Failed: Deployment of 'Ultimate PDF Tests.oml' failed. Details: {errBody}");
            }

            // Step 3b-iii — Create asset revision (sets applicationKey)
            var assetsUrl = $"{tenantEndpoint}/api/deployments/v1/assets";
            var assetPayload = new OdcAssetCreationRequest {
                FileUri = uploadSlot.UploadUrl,
                AssetCreationDetails = new OdcAssetCreationDetails {
                    AssetName = "Ultimate PDF Tests",
                    AssetType = "Application"
                }
            };
            var assetsResp = await portalClient.PostAsJsonAsync(assetsUrl, assetPayload);
            if (!assetsResp.IsSuccessStatusCode) {
                var errBody = await assetsResp.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Test Setup Failed: Deployment of 'Ultimate PDF Tests.oml' failed. Details: {errBody}");
            }
            var assetRevision = JsonSerializer.Deserialize<OdcAssetRevisionResponse>(
                await assetsResp.Content.ReadAsStringAsync())!;
            applicationKey = assetRevision.ApplicationKey;

            // Step 3c-i — Trigger publish to Development environment
            var publishOmlUrl = $"{tenantEndpoint}/api/deployments/v1/publish-operations";
            var publishOmlPayload = new PublishOperationRequest(
                Operation: "Publish",
                AssetKey: applicationKey,
                Revision: assetRevision.Revision,
                EnvironmentKey: environmentKey);
            var publishOmlResp = await portalClient.PostAsJsonAsync(publishOmlUrl, publishOmlPayload);
            if (!publishOmlResp.IsSuccessStatusCode) {
                var errBody = await publishOmlResp.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Test Setup Failed: Deployment of 'Ultimate PDF Tests.oml' failed. Details: {errBody}");
            }
            var publishOp = JsonSerializer.Deserialize<OdcPublishOperationResponse>(
                await publishOmlResp.Content.ReadAsStringAsync())!;

            // Step 3c-ii — Poll until publish completes
            var pollUrl = $"{tenantEndpoint}/api/deployments/v1/publish-operations/{publishOp.Key}";
            var deadline = DateTime.UtcNow.AddMinutes(10);
            var published = false;
            while (DateTime.UtcNow < deadline) {
                await Task.Delay(TimeSpan.FromSeconds(10));
                var pollResp = await portalClient.GetAsync(pollUrl);
                if (!pollResp.IsSuccessStatusCode) {
                    var errBody = await pollResp.Content.ReadAsStringAsync();
                    throw new InvalidOperationException(
                        $"Test Setup Failed: Deployment of 'Ultimate PDF Tests.oml' failed. Details: {errBody}");
                }
                var pollStatus = JsonSerializer.Deserialize<OdcPublishOperationResponse>(
                    await pollResp.Content.ReadAsStringAsync())!;
                if (string.Equals(pollStatus.Status, "Succeeded", StringComparison.OrdinalIgnoreCase)) {
                    published = true;
                    break;
                }
                if (string.Equals(pollStatus.Status, "Failed", StringComparison.OrdinalIgnoreCase)) {
                    throw new InvalidOperationException(
                        "Test Setup Failed: Deployment of 'Ultimate PDF Tests.oml' failed. " +
                        "Details: Publish operation failed.");
                }
            }
            if (!published) {
                throw new InvalidOperationException(
                    "Test Setup Failed: Deployment of 'Ultimate PDF Tests.oml' failed. " +
                    "Details: Deployment timed out after 10 minutes.");
            }
        } // end if (deployNeeded)

        return applicationKey;
    }

    private static async Task<(string configKey, int revisionBase, string cicdSettingKey)> FetchTestAppConfiguration(string tenantEndpoint, HttpClient portalClient, string? environmentKey, string? applicationKey) {
        var getConfigUrl = $"{tenantEndpoint}/api/asset-configurations/v1/environments/{environmentKey}/applications/{applicationKey!}/revisions/deployed/configurations";
        var getResp = await portalClient.GetAsync(getConfigUrl);
        if (!getResp.IsSuccessStatusCode) {
            var errBody = await getResp.Content.ReadAsStringAsync();
            throw new HttpRequestException($"GET {getConfigUrl} → {(int)getResp.StatusCode} {getResp.ReasonPhrase}: {errBody}");
        }
        var configDoc = JsonSerializer.Deserialize<JsonElement>(await getResp.Content.ReadAsStringAsync());
        var configKey = configDoc.GetProperty("key").GetString()!;
        var revisionBase = configDoc.GetProperty("revisionBaseline").GetInt32();
        var cicdSettingKey = configDoc.GetProperty("settings").EnumerateArray()
            .First(s => s.GetProperty("name").GetString() == "cicd_run_secret")
            .GetProperty("key").GetString()!;
        return (configKey, revisionBase, cicdSettingKey);
    }

    private static async Task PushSecretConfiguration(string tenantEndpoint, HttpClient portalClient, string? environmentKey, string? applicationKey, string secret, string configKey, int revisionBase, string cicdSettingKey) {
        var patchConfigUrl = $"{tenantEndpoint}/api/asset-configurations/v1/environments/{environmentKey}/applications/{applicationKey!}/configurations";
        var payload = new OdcConfigurationPayload(
            Key: configKey,
            RevisionBaseline: revisionBase,
            Settings: [new OdcConfigSetting(cicdSettingKey, secret)]);
        var configResp = await portalClient.PatchAsJsonAsync(patchConfigUrl, payload);
        if (!configResp.IsSuccessStatusCode) {
            var errBody = await configResp.Content.ReadAsStringAsync();
            throw new HttpRequestException($"PATCH {patchConfigUrl} → {(int)configResp.StatusCode} {configResp.ReasonPhrase}: {errBody}");
        }
    }

    private static async Task<OdcPublishOperationResponse> TriggerApplyConfigs(string tenantEndpoint, HttpClient portalClient, string? environmentKey, string applicationKey, int revisionBase) {
        var publishUrl = $"{tenantEndpoint}/api/deployments/v1/deployment-operations";
        var publishPayload = new PublishOperationRequest(
            Operation: "ApplyConfigs",
            AssetKey: applicationKey!,
            Revision: revisionBase,
            EnvironmentKey: environmentKey);
        var publishResp = await portalClient.PostAsJsonAsync(publishUrl, publishPayload);
        if (!publishResp.IsSuccessStatusCode) {
            var errBody = await publishResp.Content.ReadAsStringAsync();
            throw new HttpRequestException($"POST {publishUrl} → {(int)publishResp.StatusCode} {publishResp.ReasonPhrase}: {errBody}");
        }
        var applyOp = JsonSerializer.Deserialize<OdcPublishOperationResponse>(
            await publishResp.Content.ReadAsStringAsync())!;
        return applyOp;
    }

    private static async Task<string> GetAppHostname(string tenantEndpoint, HttpClient portalClient, string? environmentKey) {
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
        return appHostname;
    }

    private static async Task<string?> GetEnvironmentKey(string tenantEndpoint, HttpClient portalClient) {
        var environmentsUrl = $"{tenantEndpoint}/api/portfolios/v1/environments";
        var environmentsResp = await portalClient.GetAsync(environmentsUrl);
        if (!environmentsResp.IsSuccessStatusCode) {
            throw new InvalidOperationException(
                "Test Setup Failed: Could not retrieve 'Development' environment key. " +
                $"Status: {(int)environmentsResp.StatusCode}");
        }
        var environmentsJson = await environmentsResp.Content.ReadAsStringAsync();
        var environmentsDoc = JsonSerializer.Deserialize<JsonElement>(environmentsJson);
        var environments = environmentsDoc.GetProperty("results")
            .Deserialize<OdcEnvironmentItem[]>() ?? [];
        var environmentKey = environments
            .FirstOrDefault(e =>
                string.Equals(e.Name, "Development", StringComparison.OrdinalIgnoreCase))
            ?.Key;
        if (string.IsNullOrEmpty(environmentKey)) {
            throw new InvalidOperationException(
                "Test Setup Failed: Could not retrieve 'Development' environment key. " +
                "Status: NotFound");
        }

        return environmentKey;
    }

    private static async Task<OdcTokenResponse> GetAuthToken(string tenantEndpoint, string apiClientId, string apiClientSecret) {
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
        return token;
    }

    public async ValueTask DisposeAsync() {
        Client?.Dispose();
        await ValueTask.CompletedTask;
    }

    private static void ValidateConfiguration(
        string tenantEndpoint,
        string apiClientId, string apiClientSecret,
        string testPageUrl) {
        var errors = new List<string>();

        Check(tenantEndpoint, "TenantEndpoint", "https://example.outsystems.dev");
        Check(apiClientId, "ApiClientId", "000000-ApiClientId-000000");
        Check(apiClientSecret, "ApiClientSecret", "000000-ApiClientSecret-000000=");
        Check(testPageUrl, "TestPageUrl", "https://example.com/report");

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
