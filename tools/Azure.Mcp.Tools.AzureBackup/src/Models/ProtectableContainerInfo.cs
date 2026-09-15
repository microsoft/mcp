// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record ProtectableContainerInfo(
    string Name,
    string? FriendlyName,
    string? ContainerType,
    string? BackupManagementType,
    string? SourceResourceId,
    string? HealthStatus);
