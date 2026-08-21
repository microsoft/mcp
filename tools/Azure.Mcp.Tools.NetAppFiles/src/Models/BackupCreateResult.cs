// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Models;

/// <summary>
/// Result of a backup create operation.
/// </summary>
public sealed record BackupCreateResult(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("resourceGroup")] string? ResourceGroup,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("volumeResourceId")] string? VolumeResourceId,
    [property: JsonPropertyName("label")] string? Label,
    [property: JsonPropertyName("backupType")] string? BackupType,
    [property: JsonPropertyName("size")] long? Size);
