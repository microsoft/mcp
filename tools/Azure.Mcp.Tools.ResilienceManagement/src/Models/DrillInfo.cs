// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record DrillInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string? ResourceType = null,
    [property: JsonPropertyName("location")] string? Location = null,
    [property: JsonPropertyName("tags")] IReadOnlyDictionary<string, string>? Tags = null,
    [property: JsonPropertyName("properties")] JsonElement Properties = default,
    [property: JsonPropertyName("systemData")] JsonElement SystemData = default);
