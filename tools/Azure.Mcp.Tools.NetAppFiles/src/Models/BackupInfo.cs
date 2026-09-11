// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Models;

/// <summary>
/// Lightweight projection of a NetApp backup with commonly useful metadata.
/// </summary>
public sealed record BackupInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("resourceGroup")] string? ResourceGroup,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("backupType")] string? BackupType,
    [property: JsonPropertyName("size")] long? Size,
    [property: JsonPropertyName("label")] string? Label,
    [property: JsonPropertyName("creationDate")] string? CreationDate);
