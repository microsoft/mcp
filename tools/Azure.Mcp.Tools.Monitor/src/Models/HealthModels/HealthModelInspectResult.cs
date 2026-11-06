// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Monitor.Services;

namespace Azure.Mcp.Tools.Monitor.Models.HealthModels;

/// <summary>The selected entities of one health model, a page at a time.</summary>
public sealed record HealthModelInspectResult(
    [property: JsonPropertyName("entities")] List<HealthModelInspectItem> Entities,
    [property: JsonPropertyName("total"), Description("How many entities the selection matched in all.")] int Total)
{
    [JsonPropertyName("nextMarker"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("Pass to --next-marker, with the same other options, for the next page.")]
    public string? NextMarker { get; init; }

    [JsonPropertyName("truncated"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("The model has more entities or relationships than one inspection lists, so the selection is incomplete.")]
    public bool? Truncated { get; init; }

    [JsonPropertyName("signalDefinitions"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description($"The HealthModelSignalDefinition resources the returned signals name, as the CloudHealth API {HealthModelApi.Version} returns them: {HealthModelApi.Contract}")]
    public List<JsonElement>? SignalDefinitions { get; init; }

    [JsonPropertyName("fields"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description("With --fields: every field path the selected payloads hold, for choosing --fields.")]
    public List<string>? Fields { get; init; }

    [JsonPropertyName("id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull), Description("The health model's resource ID.")]
    public string? Id { get; init; }

    [JsonPropertyName("readAt"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull), Description("When the model was read.")]
    public DateTimeOffset? ReadAt { get; init; }
}
