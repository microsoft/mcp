// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Models;

/// <summary>
/// Lightweight projection of a NetApp volume with commonly useful metadata.
/// </summary>
public sealed record NetAppVolumeInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("resourceGroup")] string? ResourceGroup,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("serviceLevel")] string? ServiceLevel,
    [property: JsonPropertyName("usageThreshold")] long? UsageThreshold,
    [property: JsonPropertyName("creationToken")] string? CreationToken,
    [property: JsonPropertyName("subnetId")] string? SubnetId,
    [property: JsonPropertyName("protocolTypes")] List<string>? ProtocolTypes,
    [property: JsonPropertyName("networkFeatures")] string? NetworkFeatures);
