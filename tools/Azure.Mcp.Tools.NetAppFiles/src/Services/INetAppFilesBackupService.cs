// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Tools.NetAppFiles.Models;

namespace Azure.Mcp.Tools.NetAppFiles.Services;

public interface INetAppFilesBackupService
{
    Task<NetAppFilesBackup> CreateBackupAsync(
        string account,
        string backupVault,
        string backup,
        ResourceIdentifier volumeResourceId,
        string? label,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<NetAppFilesBackup> GetBackupAsync(
        string account,
        string backupVault,
        string backup,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<NetAppFilesBackup> UpdateBackupAsync(
        string account,
        string backupVault,
        string backup,
        string label,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
