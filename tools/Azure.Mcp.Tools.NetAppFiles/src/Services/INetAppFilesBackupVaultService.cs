// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.NetAppFiles.Models;

namespace Azure.Mcp.Tools.NetAppFiles.Services;

public interface INetAppFilesBackupVaultService
{
    Task<NetAppFilesBackupVault> CreateBackupVaultAsync(
        string account,
        string backupVault,
        string location,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<NetAppFilesBackupVault> GetBackupVaultAsync(
        string account,
        string backupVault,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<NetAppFilesBackupVault> UpdateBackupVaultAsync(
        string account,
        string backupVault,
        IDictionary<string, string> tags,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
