namespace OutSystems.UltimatePDF_ExternalLogic.TenantTests.Models;

internal sealed class OdcTokenResponse {
    [System.Text.Json.Serialization.JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;
}
