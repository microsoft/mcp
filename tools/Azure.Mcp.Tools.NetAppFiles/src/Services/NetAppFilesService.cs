// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.NetAppFiles.Services;

public class NetAppFilesService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ILogger<NetAppFilesService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), INetAppFilesService
{
    private readonly ILogger<NetAppFilesService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<ResourceQueryResults<NetAppAccountInfo>> GetAccountDetails(
        string? account,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        if (string.IsNullOrEmpty(account))
        {
            try
            {
                return await ExecuteResourceQueryAsync(
                    "Microsoft.NetApp/netAppAccounts",
                    null,
                    subscription,
                    retryPolicy,
                    ConvertToAccountInfoModel,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing NetApp accounts in Subscription: {Subscription}", subscription);
                throw;
            }
        }
        else
        {
            try
            {
                var netAppAccount = await ExecuteSingleResourceQueryAsync(
                    "Microsoft.NetApp/netAppAccounts",
                    resourceGroup: null,
                    subscription: subscription,
                    retryPolicy: retryPolicy,
                    converter: ConvertToAccountInfoModel,
                    additionalFilter: $"name =~ '{EscapeKqlString(account)}'",
                    cancellationToken: cancellationToken);

                if (netAppAccount == null)
                {
                    throw new KeyNotFoundException($"NetApp Files account '{account}' not found in subscription '{subscription}'.");
                }

                return new ResourceQueryResults<NetAppAccountInfo>([netAppAccount], false);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving NetApp Files account details for '{account}': {ex.Message}", ex);
            }
        }
    }

    public async Task<ResourceQueryResults<CapacityPoolInfo>> GetPoolDetails(
        string? account,
        string? pool,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var filters = new List<string>();
        if (!string.IsNullOrEmpty(account))
        {
            filters.Add($"name startswith '{EscapeKqlString(account)}/'");
        }
        if (!string.IsNullOrEmpty(pool) && !string.IsNullOrEmpty(account))
        {
            filters.Add($"name =~ '{EscapeKqlString(account)}/{EscapeKqlString(pool)}'");
        }
        else if (!string.IsNullOrEmpty(pool))
        {
            filters.Add($"name endswith '/{EscapeKqlString(pool)}'");
        }

        string? additionalFilter = filters.Count > 0 ? string.Join(" and ", filters) : null;

        try
        {
            return await ExecuteResourceQueryAsync(
                "Microsoft.NetApp/netAppAccounts/capacityPools",
                null,
                subscription,
                retryPolicy,
                ConvertToPoolInfoModel,
                additionalFilter: additionalFilter,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving NetApp Files capacity pool details. Subscription: {Subscription}", subscription);
            throw;
        }
    }

    public async Task<ResourceQueryResults<NetAppVolumeInfo>> GetVolumeDetails(
        string? account,
        string? pool,
        string? volume,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var filters = new List<string>();
        if (!string.IsNullOrEmpty(account))
        {
            filters.Add($"name startswith '{EscapeKqlString(account)}/'");
        }
        if (!string.IsNullOrEmpty(pool) && !string.IsNullOrEmpty(account))
        {
            filters.Add($"name startswith '{EscapeKqlString(account)}/{EscapeKqlString(pool)}/'");
        }
        if (!string.IsNullOrEmpty(volume))
        {
            filters.Add($"name endswith '/{EscapeKqlString(volume)}'");
        }

        string? additionalFilter = filters.Count > 0 ? string.Join(" and ", filters) : null;

        try
        {
            return await ExecuteResourceQueryAsync(
                "Microsoft.NetApp/netAppAccounts/capacityPools/volumes",
                null,
                subscription,
                retryPolicy,
                ConvertToVolumeInfoModel,
                additionalFilter: additionalFilter,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving NetApp Files volume details. Subscription: {Subscription}", subscription);
            throw;
        }
    }

    private static CapacityPoolInfo ConvertToPoolInfoModel(JsonElement item)
    {
        CapacityPoolData? poolData = CapacityPoolData.FromJson(item);
        if (poolData == null)
            throw new InvalidOperationException("Failed to parse NetApp capacity pool data");

        return new CapacityPoolInfo(
            Name: poolData.ResourceName ?? "Unknown",
            Location: poolData.Location,
            ResourceGroup: poolData.ResourceGroup,
            ProvisioningState: poolData.Properties?.ProvisioningState,
            ServiceLevel: poolData.Properties?.ServiceLevel,
            Size: poolData.Properties?.Size,
            QosType: poolData.Properties?.QosType,
            CoolAccess: poolData.Properties?.CoolAccess,
            EncryptionType: poolData.Properties?.EncryptionType);
    }

    private static NetAppAccountInfo ConvertToAccountInfoModel(JsonElement item)
    {
        NetAppAccountData? netAppAccount = NetAppAccountData.FromJson(item);
        if (netAppAccount == null)
            throw new InvalidOperationException("Failed to parse NetApp account data");

        return new NetAppAccountInfo(
            Name: netAppAccount.ResourceName ?? "Unknown",
            Location: netAppAccount.Location,
            ResourceGroup: netAppAccount.ResourceGroup,
            ProvisioningState: netAppAccount.Properties?.ProvisioningState,
            ActiveDirectoryId: netAppAccount.Properties?.ActiveDirectories?.FirstOrDefault()?.ActiveDirectoryId,
            Encryption: netAppAccount.Properties?.Encryption?.KeySource,
            DisableShowmount: netAppAccount.Properties?.DisableShowmount);
    }

    private static NetAppVolumeInfo ConvertToVolumeInfoModel(JsonElement item)
    {
        NetAppVolumeData? volumeData = NetAppVolumeData.FromJson(item);
        if (volumeData == null)
            throw new InvalidOperationException("Failed to parse NetApp volume data");

        return new NetAppVolumeInfo(
            Name: volumeData.ResourceName ?? "Unknown",
            Location: volumeData.Location,
            ResourceGroup: volumeData.ResourceGroup,
            ProvisioningState: volumeData.Properties?.ProvisioningState,
            ServiceLevel: volumeData.Properties?.ServiceLevel,
            UsageThreshold: volumeData.Properties?.UsageThreshold,
            CreationToken: volumeData.Properties?.CreationToken,
            SubnetId: volumeData.Properties?.SubnetId,
            ProtocolTypes: volumeData.Properties?.ProtocolTypes,
            NetworkFeatures: volumeData.Properties?.NetworkFeatures);
    }

    public async Task<ResourceQueryResults<BackupPolicyInfo>> GetBackupPolicyDetails(
        string? account,
        string? backupPolicy,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var filters = new List<string>();
        if (!string.IsNullOrEmpty(account))
        {
            filters.Add($"name startswith '{EscapeKqlString(account)}/'");
        }
        if (!string.IsNullOrEmpty(backupPolicy) && !string.IsNullOrEmpty(account))
        {
            filters.Add($"name =~ '{EscapeKqlString(account)}/{EscapeKqlString(backupPolicy)}'");
        }
        else if (!string.IsNullOrEmpty(backupPolicy))
        {
            filters.Add($"name endswith '/{EscapeKqlString(backupPolicy)}'");
        }

        string? additionalFilter = filters.Count > 0 ? string.Join(" and ", filters) : null;

        try
        {
            return await ExecuteResourceQueryAsync(
                "Microsoft.NetApp/netAppAccounts/backupPolicies",
                null,
                subscription,
                retryPolicy,
                ConvertToBackupPolicyInfoModel,
                additionalFilter: additionalFilter,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving NetApp Files backup policy details. Subscription: {Subscription}", subscription);
            throw;
        }
    }

    private static BackupPolicyInfo ConvertToBackupPolicyInfoModel(JsonElement item)
    {
        BackupPolicyData? backupPolicyData = BackupPolicyData.FromJson(item);
        if (backupPolicyData == null)
            throw new InvalidOperationException("Failed to parse NetApp backup policy data");

        return new BackupPolicyInfo(
            Name: backupPolicyData.ResourceName ?? "Unknown",
            Location: backupPolicyData.Location,
            ResourceGroup: backupPolicyData.ResourceGroup,
            ProvisioningState: backupPolicyData.Properties?.ProvisioningState,
            DailyBackupsToKeep: backupPolicyData.Properties?.DailyBackupsToKeep,
            WeeklyBackupsToKeep: backupPolicyData.Properties?.WeeklyBackupsToKeep,
            MonthlyBackupsToKeep: backupPolicyData.Properties?.MonthlyBackupsToKeep,
            VolumeBackupsCount: backupPolicyData.Properties?.VolumeBackupsCount,
            Enabled: backupPolicyData.Properties?.Enabled);
    }

    public async Task<ResourceQueryResults<BackupVaultInfo>> GetBackupVaultDetails(
        string? account,
        string? backupVault,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var filters = new List<string>();
        if (!string.IsNullOrEmpty(account))
        {
            filters.Add($"name startswith '{EscapeKqlString(account)}/'");
        }
        if (!string.IsNullOrEmpty(backupVault) && !string.IsNullOrEmpty(account))
        {
            filters.Add($"name =~ '{EscapeKqlString(account)}/{EscapeKqlString(backupVault)}'");
        }
        else if (!string.IsNullOrEmpty(backupVault))
        {
            filters.Add($"name endswith '/{EscapeKqlString(backupVault)}'");
        }

        string? additionalFilter = filters.Count > 0 ? string.Join(" and ", filters) : null;

        try
        {
            return await ExecuteResourceQueryAsync(
                "Microsoft.NetApp/netAppAccounts/backupVaults",
                null,
                subscription,
                retryPolicy,
                ConvertToBackupVaultInfoModel,
                additionalFilter: additionalFilter,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving NetApp Files backup vault details. Subscription: {Subscription}", subscription);
            throw;
        }
    }

    private static BackupVaultInfo ConvertToBackupVaultInfoModel(JsonElement item)
    {
        BackupVaultData? backupVaultData = BackupVaultData.FromJson(item);
        if (backupVaultData == null)
            throw new InvalidOperationException("Failed to parse NetApp backup vault data");

        return new BackupVaultInfo(
            Name: backupVaultData.ResourceName ?? "Unknown",
            Location: backupVaultData.Location,
            ResourceGroup: backupVaultData.ResourceGroup,
            ProvisioningState: backupVaultData.Properties?.ProvisioningState);
    }

    public async Task<ResourceQueryResults<BackupInfo>> GetBackupDetails(
        string? account,
        string? backupVault,
        string? backup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var filters = new List<string>();
        if (!string.IsNullOrEmpty(account))
        {
            filters.Add($"name startswith '{EscapeKqlString(account)}/'");
        }
        if (!string.IsNullOrEmpty(backupVault) && !string.IsNullOrEmpty(account))
        {
            filters.Add($"name startswith '{EscapeKqlString(account)}/{EscapeKqlString(backupVault)}/'");
        }
        else if (!string.IsNullOrEmpty(backupVault))
        {
            filters.Add($"name contains '/{EscapeKqlString(backupVault)}/'");
        }
        if (!string.IsNullOrEmpty(backup))
        {
            filters.Add($"name endswith '/{EscapeKqlString(backup)}'");
        }

        string? additionalFilter = filters.Count > 0 ? string.Join(" and ", filters) : null;

        try
        {
            return await ExecuteResourceQueryAsync(
                "Microsoft.NetApp/netAppAccounts/backupVaults/backups",
                null,
                subscription,
                retryPolicy,
                ConvertToBackupInfoModel,
                additionalFilter: additionalFilter,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving NetApp Files backup details. Subscription: {Subscription}", subscription);
            throw;
        }
    }

    private static BackupInfo ConvertToBackupInfoModel(JsonElement item)
    {
        BackupData? backupData = BackupData.FromJson(item);
        if (backupData == null)
            throw new InvalidOperationException("Failed to parse NetApp backup data");

        return new BackupInfo(
            Name: backupData.ResourceName ?? "Unknown",
            Location: backupData.Location,
            ResourceGroup: backupData.ResourceGroup,
            ProvisioningState: backupData.Properties?.ProvisioningState,
            BackupType: backupData.Properties?.BackupType,
            Size: backupData.Properties?.Size,
            Label: backupData.Properties?.Label,
            CreationDate: backupData.Properties?.CreationDate);
    }

    public async Task<ResourceQueryResults<SnapshotInfo>> GetSnapshotDetails(
        string? account,
        string? pool,
        string? volume,
        string? snapshot,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var filters = new List<string>();
        if (!string.IsNullOrEmpty(account))
        {
            filters.Add($"name startswith '{EscapeKqlString(account)}/'");
        }
        if (!string.IsNullOrEmpty(pool) && !string.IsNullOrEmpty(account))
        {
            filters.Add($"name startswith '{EscapeKqlString(account)}/{EscapeKqlString(pool)}/'");
        }
        if (!string.IsNullOrEmpty(volume))
        {
            filters.Add($"name contains '/{EscapeKqlString(volume)}/'");
        }
        if (!string.IsNullOrEmpty(snapshot))
        {
            filters.Add($"name endswith '/{EscapeKqlString(snapshot)}'");
        }

        string? additionalFilter = filters.Count > 0 ? string.Join(" and ", filters) : null;

        try
        {
            return await ExecuteResourceQueryAsync(
                "Microsoft.NetApp/netAppAccounts/capacityPools/volumes/snapshots",
                null,
                subscription,
                retryPolicy,
                ConvertToSnapshotInfoModel,
                additionalFilter: additionalFilter,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving NetApp Files snapshot details. Subscription: {Subscription}", subscription);
            throw;
        }
    }

    private static SnapshotInfo ConvertToSnapshotInfoModel(JsonElement item)
    {
        SnapshotData? snapshotData = SnapshotData.FromJson(item);
        if (snapshotData == null)
            throw new InvalidOperationException("Failed to parse NetApp snapshot data");

        return new SnapshotInfo(
            Name: snapshotData.ResourceName ?? "Unknown",
            Location: snapshotData.Location,
            ResourceGroup: snapshotData.ResourceGroup,
            ProvisioningState: snapshotData.Properties?.ProvisioningState,
            Created: snapshotData.Properties?.Created);
    }

    public async Task<ResourceQueryResults<ReplicationStatusInfo>> GetReplicationStatusDetails(
        string? account,
        string? pool,
        string? volume,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var filters = new List<string>
        {
            "isnotnull(properties.dataProtection.replication)"
        };
        if (!string.IsNullOrEmpty(account))
        {
            filters.Add($"name startswith '{EscapeKqlString(account)}/'");
        }
        if (!string.IsNullOrEmpty(pool) && !string.IsNullOrEmpty(account))
        {
            filters.Add($"name startswith '{EscapeKqlString(account)}/{EscapeKqlString(pool)}/'");
        }
        if (!string.IsNullOrEmpty(volume))
        {
            filters.Add($"name endswith '/{EscapeKqlString(volume)}'");
        }

        string additionalFilter = string.Join(" and ", filters);

        try
        {
            return await ExecuteResourceQueryAsync(
                "Microsoft.NetApp/netAppAccounts/capacityPools/volumes",
                null,
                subscription,
                retryPolicy,
                ConvertToReplicationStatusInfoModel,
                additionalFilter: additionalFilter,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving NetApp Files replication status details. Subscription: {Subscription}", subscription);
            throw;
        }
    }

    private static ReplicationStatusInfo ConvertToReplicationStatusInfoModel(JsonElement item)
    {
        ReplicationStatusData? data = ReplicationStatusData.FromJson(item);
        if (data == null)
            throw new InvalidOperationException("Failed to parse NetApp volume replication status data");

        return new ReplicationStatusInfo(
            Name: data.ResourceName ?? "Unknown",
            Location: data.Location,
            ResourceGroup: data.ResourceGroup,
            EndpointType: data.Properties?.DataProtection?.Replication?.EndpointType,
            ReplicationSchedule: data.Properties?.DataProtection?.Replication?.ReplicationSchedule,
            RemoteVolumeResourceId: data.Properties?.DataProtection?.Replication?.RemoteVolumeResourceId,
            RemoteVolumeRegion: data.Properties?.DataProtection?.Replication?.RemoteVolumeRegion,
            ReplicationId: data.Properties?.DataProtection?.Replication?.ReplicationId);
    }

    public async Task<ResourceQueryResults<SnapshotPolicyInfo>> GetSnapshotPolicyDetails(
        string? account,
        string? snapshotPolicy,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var filters = new List<string>();
        if (!string.IsNullOrEmpty(account))
        {
            filters.Add($"name startswith '{EscapeKqlString(account)}/'");
        }
        if (!string.IsNullOrEmpty(snapshotPolicy) && !string.IsNullOrEmpty(account))
        {
            filters.Add($"name =~ '{EscapeKqlString(account)}/{EscapeKqlString(snapshotPolicy)}'");
        }
        else if (!string.IsNullOrEmpty(snapshotPolicy))
        {
            filters.Add($"name endswith '/{EscapeKqlString(snapshotPolicy)}'");
        }

        string? additionalFilter = filters.Count > 0 ? string.Join(" and ", filters) : null;

        try
        {
            return await ExecuteResourceQueryAsync(
                "Microsoft.NetApp/netAppAccounts/snapshotPolicies",
                null,
                subscription,
                retryPolicy,
                ConvertToSnapshotPolicyInfoModel,
                additionalFilter: additionalFilter,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving NetApp Files snapshot policy details. Subscription: {Subscription}", subscription);
            throw;
        }
    }

    private static SnapshotPolicyInfo ConvertToSnapshotPolicyInfoModel(JsonElement item)
    {
        SnapshotPolicyData? snapshotPolicyData = SnapshotPolicyData.FromJson(item);
        if (snapshotPolicyData == null)
            throw new InvalidOperationException("Failed to parse NetApp snapshot policy data");

        return new SnapshotPolicyInfo(
            Name: snapshotPolicyData.ResourceName ?? "Unknown",
            Location: snapshotPolicyData.Location,
            ResourceGroup: snapshotPolicyData.ResourceGroup,
            ProvisioningState: snapshotPolicyData.Properties?.ProvisioningState,
            Enabled: snapshotPolicyData.Properties?.Enabled,
            HourlyScheduleMinute: snapshotPolicyData.Properties?.HourlySchedule?.Minute,
            HourlyScheduleSnapshotsToKeep: snapshotPolicyData.Properties?.HourlySchedule?.SnapshotsToKeep,
            DailyScheduleHour: snapshotPolicyData.Properties?.DailySchedule?.Hour,
            DailyScheduleMinute: snapshotPolicyData.Properties?.DailySchedule?.Minute,
            DailyScheduleSnapshotsToKeep: snapshotPolicyData.Properties?.DailySchedule?.SnapshotsToKeep,
            WeeklyScheduleDay: snapshotPolicyData.Properties?.WeeklySchedule?.Day,
            WeeklyScheduleSnapshotsToKeep: snapshotPolicyData.Properties?.WeeklySchedule?.SnapshotsToKeep,
            MonthlyScheduleDaysOfMonth: snapshotPolicyData.Properties?.MonthlySchedule?.DaysOfMonth,
            MonthlyScheduleSnapshotsToKeep: snapshotPolicyData.Properties?.MonthlySchedule?.SnapshotsToKeep);
    }

    public async Task<ResourceQueryResults<VolumeGroupInfo>> GetVolumeGroupDetails(
        string? account,
        string? volumeGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var filters = new List<string>();
        if (!string.IsNullOrEmpty(account))
        {
            filters.Add($"name startswith '{EscapeKqlString(account)}/'");
        }
        if (!string.IsNullOrEmpty(volumeGroup) && !string.IsNullOrEmpty(account))
        {
            filters.Add($"name =~ '{EscapeKqlString(account)}/{EscapeKqlString(volumeGroup)}'");
        }
        else if (!string.IsNullOrEmpty(volumeGroup))
        {
            filters.Add($"name endswith '/{EscapeKqlString(volumeGroup)}'");
        }

        string? additionalFilter = filters.Count > 0 ? string.Join(" and ", filters) : null;

        try
        {
            return await ExecuteResourceQueryAsync(
                "Microsoft.NetApp/netAppAccounts/volumeGroups",
                null,
                subscription,
                retryPolicy,
                ConvertToVolumeGroupInfoModel,
                additionalFilter: additionalFilter,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving NetApp Files volume group details. Subscription: {Subscription}", subscription);
            throw;
        }
    }

    private static VolumeGroupInfo ConvertToVolumeGroupInfoModel(JsonElement item)
    {
        VolumeGroupData? volumeGroupData = VolumeGroupData.FromJson(item);
        if (volumeGroupData == null)
            throw new InvalidOperationException("Failed to parse NetApp volume group data");

        return new VolumeGroupInfo(
            Name: volumeGroupData.ResourceName ?? "Unknown",
            Location: volumeGroupData.Location,
            ResourceGroup: volumeGroupData.ResourceGroup,
            ProvisioningState: volumeGroupData.Properties?.ProvisioningState,
            GroupMetaDataApplicationType: volumeGroupData.Properties?.GroupMetaData?.ApplicationType,
            GroupMetaDataApplicationIdentifier: volumeGroupData.Properties?.GroupMetaData?.ApplicationIdentifier,
            GroupMetaDataDescription: volumeGroupData.Properties?.GroupMetaData?.GroupDescription);
    }
}
