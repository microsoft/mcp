// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Details for a Recovery Services vault protection container.
/// </summary>
public sealed record BackupContainerInfo(
    string Name,
    string? FriendlyName,
    string? ContainerType,
    string? BackupManagementType,
    string? SourceResourceId,
    string? RegistrationStatus,
    string? HealthStatus,
    int? ProtectedItemCount);
