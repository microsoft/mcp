// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Compute.Models;

/// <summary>
/// Represents metadata about the spot placement score diagnostic resource,
/// including supported resource types.
/// </summary>
public sealed record SpotPlacementMetadataInfo(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("resourceType")] string? ResourceType,
    [property: JsonPropertyName("supportedResourceTypes")] IReadOnlyList<string> SupportedResourceTypes);
