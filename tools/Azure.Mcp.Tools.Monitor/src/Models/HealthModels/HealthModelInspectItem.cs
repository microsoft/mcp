// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Models.HealthModels;

/// <summary>
/// One selected entity: the CloudHealth API payloads that describe it, passed through unchanged apart from the fields a
/// projection leaves out, and the path of a walk. Only <see cref="Path"/> and <see cref="Errors"/> are the tool's own.
/// </summary>
public sealed record HealthModelInspectItem(
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [property: Description("The entity names from the walk's start to this entity, '/'-joined; set when --walk selected it.")]
    string? Path,
    [property: Description("The HealthModelEntity resource as the CloudHealth API returns it.")]
    JsonElement Entity,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [property: Description("The entity's getHistory response (EntityHistoryResult) as the CloudHealth API returns it.")]
    JsonElement? History = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [property: Description("One getSignalHistory response (EntitySignalHistoryResult) per signal as the CloudHealth API returns it.")]
    List<JsonElement>? SignalHistory = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [property: Description("The entity's getDataAnnotations response (EntityGetDataAnnotationsResult) as the CloudHealth API returns it.")]
    JsonElement? Annotations = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [property: Description("Reads for this entity that failed, each naming the read and the service's error.")]
    List<string>? Errors = null);
