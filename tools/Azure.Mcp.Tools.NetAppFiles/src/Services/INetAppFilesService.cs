// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.NetAppFiles.Models;


namespace Azure.Mcp.Tools.NetAppFiles.Services;

public interface INetAppFilesService
{
    Task<ResourceQueryResults<NetAppAccountInfo>> GetAccountDetails(
        string? account,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<ResourceQueryResults<CapacityPoolInfo>> GetPoolDetails(
        string? account,
        string? pool,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<ResourceQueryResults<NetAppVolumeInfo>> GetVolumeDetails(
        string? account,
        string? pool,
        string? volume,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<ResourceQueryResults<BackupPolicyInfo>> GetBackupPolicyDetails(
        string? account,
        string? backupPolicy,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<ResourceQueryResults<BackupVaultInfo>> GetBackupVaultDetails(
        string? account,
        string? backupVault,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<ResourceQueryResults<BackupInfo>> GetBackupDetails(
        string? account,
        string? backupVault,
        string? backup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<ResourceQueryResults<SnapshotInfo>> GetSnapshotDetails(
        string? account,
        string? pool,
        string? volume,
        string? snapshot,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<ResourceQueryResults<ReplicationStatusInfo>> GetReplicationStatusDetails(
        string? account,
        string? pool,
        string? volume,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<ResourceQueryResults<SnapshotPolicyInfo>> GetSnapshotPolicyDetails(
        string? account,
        string? snapshotPolicy,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<ResourceQueryResults<VolumeGroupInfo>> GetVolumeGroupDetails(
        string? account,
        string? volumeGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
