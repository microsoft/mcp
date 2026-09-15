// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Models;

/// <summary>
/// Result of a capacity pool create operation.
/// </summary>
public sealed record CapacityPoolCreateResult(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("resourceGroup")] string? ResourceGroup,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("serviceLevel")] string? ServiceLevel,
    [property: JsonPropertyName("size")] long? Size,
    [property: JsonPropertyName("qosType")] string? QosType,
    [property: JsonPropertyName("coolAccess")] bool? CoolAccess,
    [property: JsonPropertyName("encryptionType")] string? EncryptionType);
