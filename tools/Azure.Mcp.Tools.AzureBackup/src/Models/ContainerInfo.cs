// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record ContainerInfo(
    string Name,
    string? FriendlyName,
    string? RegistrationStatus,
    string? HealthStatus,
    string? ProtectableObjectType,
    string? BackupManagementType,
    string? SourceResourceId,
    string? WorkloadType,
    DateTimeOffset? LastUpdatedTime);
