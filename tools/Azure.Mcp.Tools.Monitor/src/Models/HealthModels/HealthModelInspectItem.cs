// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Monitor.Services;

namespace Azure.Mcp.Tools.Monitor.Models.HealthModels;

/// <summary>
/// One selected entity: the CloudHealth API payloads that describe it, passed through unchanged apart from the fields a
/// projection leaves out, and the path of a walk. Only <see cref="Path"/> and <see cref="Errors"/> are the tool's own.
/// </summary>
public sealed record HealthModelInspectItem(
    [property: JsonPropertyName("path"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [property: Description("The entity names from the walk's start to this entity, '/'-joined; set when --walk selected it.")]
    string? Path,
    [property: JsonPropertyName("entity")]
    [property: Description($"The HealthModelEntity resource as the CloudHealth API {HealthModelApi.Version} returns it: {HealthModelApi.Contract}")]
    JsonElement Entity)
{
    [JsonPropertyName("history"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description($"The entity's getHistory response (EntityHistoryResult) as the CloudHealth API {HealthModelApi.Version} returns it: {HealthModelApi.Contract}")]
    public JsonElement? History { get; init; }

    [JsonPropertyName("signalHistory"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description($"One getSignalHistory response (EntitySignalHistoryResult) per signal as the CloudHealth API {HealthModelApi.Version} returns it: {HealthModelApi.Contract}")]
    public List<JsonElement>? SignalHistory { get; init; }

    [JsonPropertyName("annotations"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description($"The entity's getDataAnnotations response (EntityGetDataAnnotationsResult) as the CloudHealth API {HealthModelApi.Version} returns it: {HealthModelApi.Contract}")]
    public JsonElement? Annotations { get; init; }

    [JsonPropertyName("errors"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Reads for this entity that failed, each naming the read and the service's error.")]
    public List<string>? Errors { get; init; }
}
