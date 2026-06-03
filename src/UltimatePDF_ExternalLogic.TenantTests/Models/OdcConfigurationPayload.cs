using System.Text.Json.Serialization;

namespace OutSystems.UltimatePDF_ExternalLogic.TenantTests.Models;

internal sealed record OdcConfigSetting(
    [property: JsonPropertyName("key")]   string Key,
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("filename")] string Filename = "");

internal sealed record OdcConfigurationPayload(
    [property: JsonPropertyName("key")]               string Key,
    [property: JsonPropertyName("revisionBaseline")]  int RevisionBaseline,
    [property: JsonPropertyName("settings")]          OdcConfigSetting[] Settings);
