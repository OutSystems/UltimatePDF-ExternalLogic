using System.Text.Json.Serialization;

namespace OutSystems.UltimatePDF_ExternalLogic.E2ETests.Models;

internal sealed record OdcUploadUrlResponse(
    [property: JsonPropertyName("uploadUrl")] string UploadUrl);

internal sealed class OdcAssetCreationRequest {
    [JsonPropertyName("fileUri")]
    public string FileUri { get; init; } = string.Empty;

    [JsonPropertyName("assetCreationDetails")]
    public OdcAssetCreationDetails AssetCreationDetails { get; init; } = new();
}

internal sealed class OdcAssetCreationDetails {
    [JsonPropertyName("assetName")]
    public string AssetName { get; init; } = string.Empty;

    [JsonPropertyName("assetType")]
    public string AssetType { get; init; } = string.Empty;
}

internal sealed record OdcAssetRevisionResponse(
    [property: JsonPropertyName("applicationKey")] string ApplicationKey,
    [property: JsonPropertyName("revision")]       int    Revision);

internal sealed record OdcPublishOperationResponse(
    [property: JsonPropertyName("key")]    string Key,
    [property: JsonPropertyName("status")] string Status);
