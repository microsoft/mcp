// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.NetAppFiles.Models;

namespace Azure.Mcp.Tools.NetAppFiles.Services;

public interface INetAppFilesBackupPolicyService
{
    Task<NetAppFilesBackupPolicy> CreateBackupPolicyAsync(
        string account,
        string backupPolicy,
        string location,
        int dailyBackupsToKeep,
        int weeklyBackupsToKeep,
        int monthlyBackupsToKeep,
        bool enabled,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<NetAppFilesBackupPolicy> GetBackupPolicyAsync(
        string account,
        string backupPolicy,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<NetAppFilesBackupPolicy> UpdateBackupPolicyAsync(
        string account,
        string backupPolicy,
        int? dailyBackupsToKeep,
        int? weeklyBackupsToKeep,
        int? monthlyBackupsToKeep,
        bool? enabled,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
