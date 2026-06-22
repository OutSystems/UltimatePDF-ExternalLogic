using System.Text.Json.Serialization;

namespace OutSystems.UltimatePDF_ExternalLogic.E2ETests.Models;

internal sealed record PublishOperationRequest(
    [property: JsonPropertyName("operation")]      string Operation,
    [property: JsonPropertyName("assetKey")]       string AssetKey,
    [property: JsonPropertyName("revision")]       int Revision,
    [property: JsonPropertyName("environmentKey")] string EnvironmentKey);
