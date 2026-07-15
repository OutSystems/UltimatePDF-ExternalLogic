using System.Text.Json.Serialization;

namespace OutSystems.UltimatePDF_ExternalLogic.E2ETests.Models;

internal sealed class OdcDeployedAssetItem {
    [JsonPropertyName("key")]
    public string Key { get; init; } = string.Empty;

    [JsonPropertyName("deployments")]
    public OdcAssetDeployment[] Deployments { get; init; } = [];
}

internal sealed class OdcAssetDeployment {
    [JsonPropertyName("environmentKey")]
    public string EnvironmentKey { get; init; } = string.Empty;
}
