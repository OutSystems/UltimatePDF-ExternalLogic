using System.Text.Json.Serialization;

namespace OutSystems.UltimatePDF_ExternalLogic.E2ETests.Models;

internal sealed record OdcConfigSetting(
    [property: JsonPropertyName("key")]   string Key,
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("filename")] string Filename = "");

internal sealed record OdcConfigurationPayload(
    [property: JsonPropertyName("key")]               string Key,
    [property: JsonPropertyName("revisionBaseline")]  int RevisionBaseline,
    [property: JsonPropertyName("settings")]          OdcConfigSetting[] Settings);
