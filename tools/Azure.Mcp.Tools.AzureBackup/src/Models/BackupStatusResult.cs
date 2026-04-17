// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record BackupStatusResult(
    string? DatasourceId,
    string? ProtectionStatus,
    string? VaultId,
    string? PolicyName,
    DateTimeOffset? LastBackupTime,
    string? LastBackupStatus,
    string? HealthStatus);
