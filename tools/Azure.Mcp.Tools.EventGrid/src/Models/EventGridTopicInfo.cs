// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.EventGrid.Models;

// Lightweight projection of EventGridTopicData with commonly useful metadata.
// Keep property names stable; only add new nullable properties to extend.
public sealed record EventGridTopicInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("endpoint")] string? Endpoint,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("publicNetworkAccess")] string? PublicNetworkAccess,
    [property: JsonPropertyName("inputSchema")] string? InputSchema);
