// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services.Models;


namespace Azure.Mcp.Tools.NetAppFiles.Services;

public interface INetAppFilesService
{
    Task<ResourceQueryResults<NetAppAccountInfo>> GetAccountDetails(
        string? account,
        string? resourceGroup,
        IReadOnlyList<string>? ids,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<ResourceQueryResults<CapacityPoolInfo>> GetPoolDetails(
        string? account,
        string? pool,
        string? resourceGroup,
        IReadOnlyList<string>? ids,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<ResourceQueryResults<NetAppVolumeInfo>> GetVolumeDetails(
        string? account,
        string? pool,
        string? volume,
        string? resourceGroup,
        IReadOnlyList<string>? ids,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<ResourceQueryResults<BackupPolicyInfo>> GetBackupPolicyDetails(
        string? account,
        string? backupPolicy,
        string? resourceGroup,
        IReadOnlyList<string>? ids,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<ResourceQueryResults<BackupVaultInfo>> GetBackupVaultDetails(
        string? account,
        string? backupVault,
        string? resourceGroup,
        IReadOnlyList<string>? ids,
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

    Task<NetAppVolumeCreateResult> CreateVolume(
        string account,
        string pool,
        string volume,
        string resourceGroup,
        string location,
        string subscription,
        NetAppVolumeCreateParameters parameters,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<NetAppVolumeCreateResult> UpdateVolume(
        string account,
        string pool,
        string volume,
        string resourceGroup,
        string location,
        string subscription,
        long? usageThreshold = null,
        string? serviceLevel = null,
        Dictionary<string, string>? tags = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<NetAppAccountCreateResult> CreateAccount(
        string account,
        string resourceGroup,
        string location,
        string subscription,
        Dictionary<string, string>? tags = null,
        string? keyName = null,
        string? keySource = null,
        string? keyVaultResourceId = null,
        string? keyVaultUri = null,
        string? federatedClientId = null,
        string? userAssignedIdentity = null,
        string? identityType = null,
        JsonElement? userAssignedIdentities = null,
        JsonElement? activeDirectories = null,
        string? nfsV4IdDomain = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<NetAppAccountCreateResult> UpdateAccount(
        string account,
        string resourceGroup,
        string? location,
        string subscription,
        Dictionary<string, string>? tags = null,
        string? keyName = null,
        string? keySource = null,
        string? keyVaultResourceId = null,
        string? keyVaultUri = null,
        string? federatedClientId = null,
        string? userAssignedIdentity = null,
        string? identityType = null,
        JsonElement? userAssignedIdentities = null,
        JsonElement? activeDirectories = null,
        string? nfsV4IdDomain = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<BackupPolicyCreateResult> CreateBackupPolicy(
        string account,
        string backupPolicy,
        string resourceGroup,
        string location,
        string subscription,
        int? dailyBackupsToKeep = null,
        int? weeklyBackupsToKeep = null,
        int? monthlyBackupsToKeep = null,
        bool? enabled = null,
        Dictionary<string, string>? tags = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<BackupPolicyCreateResult> UpdateBackupPolicy(
        string account,
        string backupPolicy,
        string resourceGroup,
        string location,
        string subscription,
        int? dailyBackupsToKeep = null,
        int? weeklyBackupsToKeep = null,
        int? monthlyBackupsToKeep = null,
        bool? enabled = null,
        Dictionary<string, string>? tags = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<BackupCreateResult> CreateBackup(
        string account,
        string backupVault,
        string backup,
        string resourceGroup,
        string location,
        string volumeResourceId,
        string subscription,
        string? label = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<BackupCreateResult> UpdateBackup(
        string account,
        string backupVault,
        string backup,
        string resourceGroup,
        string location,
        string subscription,
        string? label = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<BackupVaultCreateResult> CreateBackupVault(
        string account,
        string backupVault,
        string resourceGroup,
        string location,
        string subscription,
        Dictionary<string, string>? tags = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<BackupVaultCreateResult> UpdateBackupVault(
        string account,
        string backupVault,
        string resourceGroup,
        string location,
        string subscription,
        Dictionary<string, string>? tags = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<CapacityPoolCreateResult> CreatePool(
        string account,
        string pool,
        string resourceGroup,
        string location,
        long size,
        string subscription,
        string? serviceLevel = null,
        long? customThroughputMibps = null,
        string? qosType = null,
        bool? coolAccess = null,
        string? encryptionType = null,
        Dictionary<string, string>? tags = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<CapacityPoolCreateResult> UpdatePool(
        string account,
        string pool,
        string resourceGroup,
        string location,
        string subscription,
        long? size = null,
        long? sizeInBytes = null,
        string? serviceLevel = null,
        string? qosType = null,
        long? customThroughputMibps = null,
        bool? coolAccess = null,
        Dictionary<string, string>? tags = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<SnapshotCreateResult> CreateSnapshot(
        string account,
        string pool,
        string volume,
        string snapshot,
        string resourceGroup,
        string location,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<SnapshotCreateResult> UpdateSnapshot(
        string account,
        string pool,
        string volume,
        string snapshot,
        string resourceGroup,
        string location,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<SnapshotPolicyCreateResult> CreateSnapshotPolicy(
        string account,
        string snapshotPolicy,
        string resourceGroup,
        string location,
        string subscription,
        int? hourlyScheduleMinute = null,
        int? hourlyScheduleSnapshotsToKeep = null,
        int? dailyScheduleHour = null,
        int? dailyScheduleMinute = null,
        int? dailyScheduleSnapshotsToKeep = null,
        string? weeklyScheduleDay = null,
        int? weeklyScheduleSnapshotsToKeep = null,
        string? monthlyScheduleDaysOfMonth = null,
        int? monthlyScheduleSnapshotsToKeep = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<SnapshotPolicyCreateResult> UpdateSnapshotPolicy(
        string account,
        string snapshotPolicy,
        string resourceGroup,
        string location,
        string subscription,
        int? hourlyScheduleMinute = null,
        int? hourlyScheduleSnapshotsToKeep = null,
        int? dailyScheduleHour = null,
        int? dailyScheduleMinute = null,
        int? dailyScheduleSnapshotsToKeep = null,
        string? weeklyScheduleDay = null,
        int? weeklyScheduleSnapshotsToKeep = null,
        string? monthlyScheduleDaysOfMonth = null,
        int? monthlyScheduleSnapshotsToKeep = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<VolumeGroupCreateResult> CreateVolumeGroup(
        string account,
        string volumeGroup,
        string resourceGroup,
        string location,
        string applicationType,
        string applicationIdentifier,
        string subscription,
        string? groupDescription = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<VolumeGroupCreateResult> UpdateVolumeGroup(
        string account,
        string volumeGroup,
        string resourceGroup,
        string location,
        string subscription,
        string? groupDescription = null,
        Dictionary<string, string>? tags = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
