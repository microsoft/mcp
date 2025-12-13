// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Lightweight projection of PrivateEndpointConnection with commonly useful metadata.
/// </summary>
public sealed record PrivateEndpointConnectionInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("privateEndpointId")] string? PrivateEndpointId,
    [property: JsonPropertyName("connectionState")] string? ConnectionState,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState);
