// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Models;

/// <summary>
/// Result of a volume group create operation.
/// </summary>
public sealed record VolumeGroupCreateResult(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("resourceGroup")] string? ResourceGroup,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("groupMetaDataApplicationType")] string? GroupMetaDataApplicationType,
    [property: JsonPropertyName("groupMetaDataApplicationIdentifier")] string? GroupMetaDataApplicationIdentifier,
    [property: JsonPropertyName("groupMetaDataDescription")] string? GroupMetaDataDescription);
