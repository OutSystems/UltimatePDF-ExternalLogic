using System.Text.Json.Serialization;

namespace OutSystems.UltimatePDF_ExternalLogic.E2ETests.Models;

internal sealed class OdcEnvironmentItem {
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("key")]
    public string Key { get; init; } = string.Empty;
}
