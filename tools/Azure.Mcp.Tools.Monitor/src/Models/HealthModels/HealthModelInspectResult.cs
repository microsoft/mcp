// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Models.HealthModels;

/// <summary>The selected entities of one health model, a page at a time.</summary>
public sealed record HealthModelInspectResult(
    List<HealthModelInspectItem> Entities,
    [property: Description("How many entities the selection matched in all.")] int Total,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [property: Description("Pass to --cursor, with the same other options, for the next page.")]
    string? NextCursor = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [property: Description("The model has more entities or relationships than one inspection lists, so the selection is incomplete.")]
    bool? Truncated = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [property: Description("The HealthModelSignalDefinition resources the returned signals name, as the CloudHealth API returns them.")]
    List<JsonElement>? SignalDefinitions = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull), Description("The health model's resource ID.")]
    string? Id = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull), Description("When the model was read.")]
    DateTimeOffset? ReadAt = null);
