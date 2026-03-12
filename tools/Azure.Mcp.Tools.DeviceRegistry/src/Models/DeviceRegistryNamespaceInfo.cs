// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.DeviceRegistry.Models;

/// <summary>
/// Lightweight projection of a Device Registry Namespace resource.
/// </summary>
public sealed record DeviceRegistryNamespaceInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("uuid")] string? Uuid,
    [property: JsonPropertyName("resourceGroup")] string? ResourceGroup,
    [property: JsonPropertyName("type")] string? Type);
