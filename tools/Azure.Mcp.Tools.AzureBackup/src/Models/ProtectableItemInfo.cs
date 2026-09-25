// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record ProtectableItemInfo(
    string? Id,
    string Name,
    string? ProtectableItemType,
    string? WorkloadType,
    string? FriendlyName,
    string? ServerName,
    string? ParentName,
    string? ProtectionState,
    string? ContainerName);
