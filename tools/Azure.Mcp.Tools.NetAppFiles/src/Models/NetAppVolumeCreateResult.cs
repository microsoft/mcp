// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Models;

/// <summary>
/// Result of a volume create operation.
/// </summary>
public sealed record NetAppVolumeCreateResult(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("resourceGroup")] string? ResourceGroup,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("serviceLevel")] string? ServiceLevel,
    [property: JsonPropertyName("usageThreshold")] long? UsageThreshold,
    [property: JsonPropertyName("creationToken")] string? CreationToken,
    [property: JsonPropertyName("subnetId")] string? SubnetId,
    [property: JsonPropertyName("protocolTypes")] List<string>? ProtocolTypes);
