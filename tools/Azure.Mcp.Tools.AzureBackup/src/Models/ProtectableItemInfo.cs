// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record ProtectableItemInfo(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("protectableItemType")] string? ProtectableItemType,
    [property: JsonPropertyName("workloadType")] string? WorkloadType,
    [property: JsonPropertyName("friendlyName")] string? FriendlyName,
    [property: JsonPropertyName("serverName")] string? ServerName,
    [property: JsonPropertyName("parentName")] string? ParentName,
    [property: JsonPropertyName("protectionState")] string? ProtectionState,
    [property: JsonPropertyName("containerName")] string? ContainerName);
