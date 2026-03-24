// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Models;

/// <summary>
/// Lightweight projection of a NetApp volume group with commonly useful metadata.
/// </summary>
public sealed record VolumeGroupInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("resourceGroup")] string? ResourceGroup,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("groupMetaDataApplicationType")] string? GroupMetaDataApplicationType,
    [property: JsonPropertyName("groupMetaDataApplicationIdentifier")] string? GroupMetaDataApplicationIdentifier,
    [property: JsonPropertyName("groupMetaDataDescription")] string? GroupMetaDataDescription);
