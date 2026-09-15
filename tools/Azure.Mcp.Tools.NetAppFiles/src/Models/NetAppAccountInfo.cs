// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Models;

/// <summary>
/// Lightweight projection of a NetApp account with commonly useful metadata.
/// </summary>
public sealed record NetAppAccountInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("resourceGroup")] string? ResourceGroup,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("activeDirectoryId")] string? ActiveDirectoryId,
    [property: JsonPropertyName("encryption")] string? Encryption,
    [property: JsonPropertyName("disableShowmount")] bool? DisableShowmount);
