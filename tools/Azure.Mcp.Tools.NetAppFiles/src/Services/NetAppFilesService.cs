// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services.Models;
using Azure.ResourceManager;
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

    public async Task<NetAppVolumeCreateResult> CreateVolume(
        string account,
        string pool,
        string volume,
        string resourceGroup,
        string location,
        string creationToken,
        long usageThreshold,
        string subnetId,
        string subscription,
        string? serviceLevel = null,
        List<string>? protocolTypes = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(account), account),
            (nameof(pool), pool),
            (nameof(volume), volume),
            (nameof(resourceGroup), resourceGroup),
            (nameof(location), location),
            (nameof(creationToken), creationToken),
            (nameof(subnetId), subnetId),
            (nameof(subscription), subscription));

        try
        {
            ArmClient armClient = await CreateArmClientWithApiVersionAsync(
                "Microsoft.NetApp/netAppAccounts/capacityPools/volumes",
                "2024-03-01",
                tenant,
                retryPolicy,
                cancellationToken);

            var resourceId = new ResourceIdentifier(
                $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/capacityPools/{pool}/volumes/{volume}");

            var createContent = new NetAppVolumeCreateOrUpdateContent
            {
                Location = location,
                Properties = new NetAppVolumeCreateProperties
                {
                    CreationToken = creationToken,
                    UsageThreshold = usageThreshold,
                    SubnetId = subnetId,
                    ServiceLevel = serviceLevel ?? "Premium",
                    ProtocolTypes = protocolTypes ?? ["NFSv3"]
                }
            };

            var result = await CreateOrUpdateGenericResourceAsync(
                armClient,
                resourceId,
                location,
                createContent,
                NetAppFilesJsonContext.Default.NetAppVolumeCreateOrUpdateContent,
                cancellationToken);

            if (!result.HasData)
            {
                return new NetAppVolumeCreateResult(
                    Id: null,
                    Name: null,
                    Type: null,
                    Location: null,
                    ResourceGroup: null,
                    ProvisioningState: null,
                    ServiceLevel: null,
                    UsageThreshold: null,
                    CreationToken: null,
                    SubnetId: null,
                    ProtocolTypes: null);
            }

            return new NetAppVolumeCreateResult(
                Id: result.Data.Id.ToString(),
                Name: result.Data.Name,
                Type: result.Data.ResourceType.ToString(),
                Location: result.Data.Location,
                ResourceGroup: resourceGroup,
                ProvisioningState: GetPropertyString(result.Data.Properties, "provisioningState"),
                ServiceLevel: GetPropertyString(result.Data.Properties, "serviceLevel"),
                UsageThreshold: GetPropertyLong(result.Data.Properties, "usageThreshold"),
                CreationToken: GetPropertyString(result.Data.Properties, "creationToken"),
                SubnetId: GetPropertyString(result.Data.Properties, "subnetId"),
                ProtocolTypes: GetPropertyStringList(result.Data.Properties, "protocolTypes"));
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating NetApp Files volume '{volume}': {ex.Message}", ex);
        }
    }

    public async Task<NetAppAccountCreateResult> CreateAccount(
        string account,
        string resourceGroup,
        string location,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(account), account),
            (nameof(resourceGroup), resourceGroup),
            (nameof(location), location),
            (nameof(subscription), subscription));

        try
        {
            ArmClient armClient = await CreateArmClientWithApiVersionAsync(
                "Microsoft.NetApp/netAppAccounts",
                "2024-03-01",
                tenant,
                retryPolicy,
                cancellationToken);

            var resourceId = new ResourceIdentifier(
                $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}");

            var createContent = new NetAppAccountCreateOrUpdateContent
            {
                Location = location,
                Properties = new NetAppAccountCreateProperties()
            };

            var result = await CreateOrUpdateGenericResourceAsync(
                armClient,
                resourceId,
                location,
                createContent,
                NetAppFilesJsonContext.Default.NetAppAccountCreateOrUpdateContent,
                cancellationToken);

            if (!result.HasData)
            {
                return new NetAppAccountCreateResult(
                    Id: null,
                    Name: null,
                    Type: null,
                    Location: null,
                    ResourceGroup: null,
                    ProvisioningState: null);
            }

            return new NetAppAccountCreateResult(
                Id: result.Data.Id.ToString(),
                Name: result.Data.Name,
                Type: result.Data.ResourceType.ToString(),
                Location: result.Data.Location,
                ResourceGroup: resourceGroup,
                ProvisioningState: GetPropertyString(result.Data.Properties, "provisioningState"));
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating NetApp Files account '{account}': {ex.Message}", ex);
        }
    }

    public async Task<BackupPolicyCreateResult> CreateBackupPolicy(
        string account,
        string backupPolicy,
        string resourceGroup,
        string location,
        string subscription,
        int? dailyBackupsToKeep = null,
        int? weeklyBackupsToKeep = null,
        int? monthlyBackupsToKeep = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(account), account),
            (nameof(backupPolicy), backupPolicy),
            (nameof(resourceGroup), resourceGroup),
            (nameof(location), location),
            (nameof(subscription), subscription));

        try
        {
            ArmClient armClient = await CreateArmClientWithApiVersionAsync(
                "Microsoft.NetApp/netAppAccounts/backupPolicies",
                "2024-03-01",
                tenant,
                retryPolicy,
                cancellationToken);

            var resourceId = new ResourceIdentifier(
                $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/backupPolicies/{backupPolicy}");

            var createContent = new BackupPolicyCreateOrUpdateContent
            {
                Location = location,
                Properties = new BackupPolicyCreateProperties
                {
                    DailyBackupsToKeep = dailyBackupsToKeep,
                    WeeklyBackupsToKeep = weeklyBackupsToKeep,
                    MonthlyBackupsToKeep = monthlyBackupsToKeep
                }
            };

            var result = await CreateOrUpdateGenericResourceAsync(
                armClient,
                resourceId,
                location,
                createContent,
                NetAppFilesJsonContext.Default.BackupPolicyCreateOrUpdateContent,
                cancellationToken);

            if (!result.HasData)
            {
                return new BackupPolicyCreateResult(
                    Id: null,
                    Name: null,
                    Type: null,
                    Location: null,
                    ResourceGroup: null,
                    ProvisioningState: null,
                    DailyBackupsToKeep: null,
                    WeeklyBackupsToKeep: null,
                    MonthlyBackupsToKeep: null,
                    Enabled: null);
            }

            return new BackupPolicyCreateResult(
                Id: result.Data.Id.ToString(),
                Name: result.Data.Name,
                Type: result.Data.ResourceType.ToString(),
                Location: result.Data.Location,
                ResourceGroup: resourceGroup,
                ProvisioningState: GetPropertyString(result.Data.Properties, "provisioningState"),
                DailyBackupsToKeep: GetPropertyInt(result.Data.Properties, "dailyBackupsToKeep"),
                WeeklyBackupsToKeep: GetPropertyInt(result.Data.Properties, "weeklyBackupsToKeep"),
                MonthlyBackupsToKeep: GetPropertyInt(result.Data.Properties, "monthlyBackupsToKeep"),
                Enabled: GetPropertyBool(result.Data.Properties, "enabled"));
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating NetApp Files backup policy '{backupPolicy}': {ex.Message}", ex);
        }
    }

    public async Task<BackupCreateResult> CreateBackup(
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
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(account), account),
            (nameof(backupVault), backupVault),
            (nameof(backup), backup),
            (nameof(resourceGroup), resourceGroup),
            (nameof(location), location),
            (nameof(volumeResourceId), volumeResourceId),
            (nameof(subscription), subscription));

        try
        {
            ArmClient armClient = await CreateArmClientWithApiVersionAsync(
                "Microsoft.NetApp/netAppAccounts/backupVaults/backups",
                "2024-03-01",
                tenant,
                retryPolicy,
                cancellationToken);

            var resourceId = new ResourceIdentifier(
                $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/backupVaults/{backupVault}/backups/{backup}");

            var createContent = new BackupCreateOrUpdateContent
            {
                Location = location,
                Properties = new BackupCreateProperties
                {
                    VolumeResourceId = volumeResourceId,
                    Label = label
                }
            };

            var result = await CreateOrUpdateGenericResourceAsync(
                armClient,
                resourceId,
                location,
                createContent,
                NetAppFilesJsonContext.Default.BackupCreateOrUpdateContent,
                cancellationToken);

            if (!result.HasData)
            {
                return new BackupCreateResult(
                    Id: null,
                    Name: null,
                    Type: null,
                    Location: null,
                    ResourceGroup: null,
                    ProvisioningState: null,
                    VolumeResourceId: null,
                    Label: null,
                    BackupType: null,
                    Size: null);
            }

            return new BackupCreateResult(
                Id: result.Data.Id.ToString(),
                Name: result.Data.Name,
                Type: result.Data.ResourceType.ToString(),
                Location: result.Data.Location,
                ResourceGroup: resourceGroup,
                ProvisioningState: GetPropertyString(result.Data.Properties, "provisioningState"),
                VolumeResourceId: GetPropertyString(result.Data.Properties, "volumeResourceId"),
                Label: GetPropertyString(result.Data.Properties, "label"),
                BackupType: GetPropertyString(result.Data.Properties, "backupType"),
                Size: GetPropertyLong(result.Data.Properties, "size"));
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating NetApp Files backup '{backup}': {ex.Message}", ex);
        }
    }

    public async Task<BackupVaultCreateResult> CreateBackupVault(
        string account,
        string backupVault,
        string resourceGroup,
        string location,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(account), account),
            (nameof(backupVault), backupVault),
            (nameof(resourceGroup), resourceGroup),
            (nameof(location), location),
            (nameof(subscription), subscription));

        try
        {
            ArmClient armClient = await CreateArmClientWithApiVersionAsync(
                "Microsoft.NetApp/netAppAccounts/backupVaults",
                "2024-03-01",
                tenant,
                retryPolicy,
                cancellationToken);

            var resourceId = new ResourceIdentifier(
                $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/backupVaults/{backupVault}");

            var createContent = new BackupVaultCreateOrUpdateContent
            {
                Location = location,
                Properties = new BackupVaultCreateProperties()
            };

            var result = await CreateOrUpdateGenericResourceAsync(
                armClient,
                resourceId,
                location,
                createContent,
                NetAppFilesJsonContext.Default.BackupVaultCreateOrUpdateContent,
                cancellationToken);

            if (!result.HasData)
            {
                return new BackupVaultCreateResult(
                    Id: null,
                    Name: null,
                    Type: null,
                    Location: null,
                    ResourceGroup: null,
                    ProvisioningState: null);
            }

            return new BackupVaultCreateResult(
                Id: result.Data.Id.ToString(),
                Name: result.Data.Name,
                Type: result.Data.ResourceType.ToString(),
                Location: result.Data.Location,
                ResourceGroup: resourceGroup,
                ProvisioningState: GetPropertyString(result.Data.Properties, "provisioningState"));
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating NetApp Files backup vault '{backupVault}': {ex.Message}", ex);
        }
    }

    public async Task<CapacityPoolCreateResult> CreatePool(
        string account,
        string pool,
        string resourceGroup,
        string location,
        long size,
        string subscription,
        string? serviceLevel = null,
        string? qosType = null,
        bool? coolAccess = null,
        string? encryptionType = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(account), account),
            (nameof(pool), pool),
            (nameof(resourceGroup), resourceGroup),
            (nameof(location), location),
            (nameof(subscription), subscription));

        try
        {
            ArmClient armClient = await CreateArmClientWithApiVersionAsync(
                "Microsoft.NetApp/netAppAccounts/capacityPools",
                "2024-03-01",
                tenant,
                retryPolicy,
                cancellationToken);

            var resourceId = new ResourceIdentifier(
                $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/capacityPools/{pool}");

            var createContent = new CapacityPoolCreateOrUpdateContent
            {
                Location = location,
                Properties = new CapacityPoolCreateProperties
                {
                    Size = size,
                    ServiceLevel = serviceLevel ?? "Premium",
                    QosType = qosType,
                    CoolAccess = coolAccess,
                    EncryptionType = encryptionType
                }
            };

            var result = await CreateOrUpdateGenericResourceAsync(
                armClient,
                resourceId,
                location,
                createContent,
                NetAppFilesJsonContext.Default.CapacityPoolCreateOrUpdateContent,
                cancellationToken);

            if (!result.HasData)
            {
                return new CapacityPoolCreateResult(
                    Id: null,
                    Name: null,
                    Type: null,
                    Location: null,
                    ResourceGroup: null,
                    ProvisioningState: null,
                    ServiceLevel: null,
                    Size: null,
                    QosType: null,
                    CoolAccess: null,
                    EncryptionType: null);
            }

            return new CapacityPoolCreateResult(
                Id: result.Data.Id.ToString(),
                Name: result.Data.Name,
                Type: result.Data.ResourceType.ToString(),
                Location: result.Data.Location,
                ResourceGroup: resourceGroup,
                ProvisioningState: GetPropertyString(result.Data.Properties, "provisioningState"),
                ServiceLevel: GetPropertyString(result.Data.Properties, "serviceLevel"),
                Size: GetPropertyLong(result.Data.Properties, "size"),
                QosType: GetPropertyString(result.Data.Properties, "qosType"),
                CoolAccess: GetPropertyBool(result.Data.Properties, "coolAccess"),
                EncryptionType: GetPropertyString(result.Data.Properties, "encryptionType"));
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating NetApp Files capacity pool '{pool}': {ex.Message}", ex);
        }
    }

    public async Task<SnapshotCreateResult> CreateSnapshot(
        string account,
        string pool,
        string volume,
        string snapshot,
        string resourceGroup,
        string location,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(account), account),
            (nameof(pool), pool),
            (nameof(volume), volume),
            (nameof(snapshot), snapshot),
            (nameof(resourceGroup), resourceGroup),
            (nameof(location), location),
            (nameof(subscription), subscription));

        try
        {
            ArmClient armClient = await CreateArmClientWithApiVersionAsync(
                "Microsoft.NetApp/netAppAccounts/capacityPools/volumes/snapshots",
                "2024-03-01",
                tenant,
                retryPolicy,
                cancellationToken);

            var resourceId = new ResourceIdentifier(
                $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/capacityPools/{pool}/volumes/{volume}/snapshots/{snapshot}");

            var createContent = new SnapshotCreateOrUpdateContent
            {
                Location = location,
                Properties = new SnapshotCreateProperties()
            };

            var result = await CreateOrUpdateGenericResourceAsync(
                armClient,
                resourceId,
                location,
                createContent,
                NetAppFilesJsonContext.Default.SnapshotCreateOrUpdateContent,
                cancellationToken);

            if (!result.HasData)
            {
                return new SnapshotCreateResult(
                    Id: null,
                    Name: null,
                    Type: null,
                    Location: null,
                    ResourceGroup: null,
                    ProvisioningState: null,
                    Created: null);
            }

            return new SnapshotCreateResult(
                Id: result.Data.Id.ToString(),
                Name: result.Data.Name,
                Type: result.Data.ResourceType.ToString(),
                Location: result.Data.Location,
                ResourceGroup: resourceGroup,
                ProvisioningState: GetPropertyString(result.Data.Properties, "provisioningState"),
                Created: GetPropertyString(result.Data.Properties, "created"));
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating NetApp Files snapshot '{snapshot}': {ex.Message}", ex);
        }
    }

    public async Task<SnapshotPolicyCreateResult> CreateSnapshotPolicy(
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
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(account), account),
            (nameof(snapshotPolicy), snapshotPolicy),
            (nameof(resourceGroup), resourceGroup),
            (nameof(location), location),
            (nameof(subscription), subscription));

        try
        {
            ArmClient armClient = await CreateArmClientWithApiVersionAsync(
                "Microsoft.NetApp/netAppAccounts/snapshotPolicies",
                "2024-03-01",
                tenant,
                retryPolicy,
                cancellationToken);

            var resourceId = new ResourceIdentifier(
                $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/snapshotPolicies/{snapshotPolicy}");

            var properties = new SnapshotPolicyCreateProperties
            {
                Enabled = true
            };

            if (hourlyScheduleMinute.HasValue || hourlyScheduleSnapshotsToKeep.HasValue)
            {
                properties.HourlySchedule = new HourlyScheduleCreate
                {
                    Minute = hourlyScheduleMinute,
                    SnapshotsToKeep = hourlyScheduleSnapshotsToKeep
                };
            }

            if (dailyScheduleHour.HasValue || dailyScheduleMinute.HasValue || dailyScheduleSnapshotsToKeep.HasValue)
            {
                properties.DailySchedule = new DailyScheduleCreate
                {
                    Hour = dailyScheduleHour,
                    Minute = dailyScheduleMinute,
                    SnapshotsToKeep = dailyScheduleSnapshotsToKeep
                };
            }

            if (weeklyScheduleDay != null || weeklyScheduleSnapshotsToKeep.HasValue)
            {
                properties.WeeklySchedule = new WeeklyScheduleCreate
                {
                    Day = weeklyScheduleDay,
                    SnapshotsToKeep = weeklyScheduleSnapshotsToKeep
                };
            }

            if (monthlyScheduleDaysOfMonth != null || monthlyScheduleSnapshotsToKeep.HasValue)
            {
                properties.MonthlySchedule = new MonthlyScheduleCreate
                {
                    DaysOfMonth = monthlyScheduleDaysOfMonth,
                    SnapshotsToKeep = monthlyScheduleSnapshotsToKeep
                };
            }

            var createContent = new SnapshotPolicyCreateOrUpdateContent
            {
                Location = location,
                Properties = properties
            };

            var result = await CreateOrUpdateGenericResourceAsync(
                armClient,
                resourceId,
                location,
                createContent,
                NetAppFilesJsonContext.Default.SnapshotPolicyCreateOrUpdateContent,
                cancellationToken);

            if (!result.HasData)
            {
                return new SnapshotPolicyCreateResult(
                    Id: null,
                    Name: null,
                    Type: null,
                    Location: null,
                    ResourceGroup: null,
                    ProvisioningState: null,
                    Enabled: null,
                    HourlyScheduleMinute: null,
                    HourlyScheduleSnapshotsToKeep: null,
                    DailyScheduleHour: null,
                    DailyScheduleMinute: null,
                    DailyScheduleSnapshotsToKeep: null,
                    WeeklyScheduleDay: null,
                    WeeklyScheduleSnapshotsToKeep: null,
                    MonthlyScheduleDaysOfMonth: null,
                    MonthlyScheduleSnapshotsToKeep: null);
            }

            return new SnapshotPolicyCreateResult(
                Id: result.Data.Id.ToString(),
                Name: result.Data.Name,
                Type: result.Data.ResourceType.ToString(),
                Location: result.Data.Location,
                ResourceGroup: resourceGroup,
                ProvisioningState: GetPropertyString(result.Data.Properties, "provisioningState"),
                Enabled: GetPropertyBool(result.Data.Properties, "enabled"),
                HourlyScheduleMinute: GetNestedPropertyInt(result.Data.Properties, "hourlySchedule", "minute"),
                HourlyScheduleSnapshotsToKeep: GetNestedPropertyInt(result.Data.Properties, "hourlySchedule", "snapshotsToKeep"),
                DailyScheduleHour: GetNestedPropertyInt(result.Data.Properties, "dailySchedule", "hour"),
                DailyScheduleMinute: GetNestedPropertyInt(result.Data.Properties, "dailySchedule", "minute"),
                DailyScheduleSnapshotsToKeep: GetNestedPropertyInt(result.Data.Properties, "dailySchedule", "snapshotsToKeep"),
                WeeklyScheduleDay: GetNestedPropertyString(result.Data.Properties, "weeklySchedule", "day"),
                WeeklyScheduleSnapshotsToKeep: GetNestedPropertyInt(result.Data.Properties, "weeklySchedule", "snapshotsToKeep"),
                MonthlyScheduleDaysOfMonth: GetNestedPropertyString(result.Data.Properties, "monthlySchedule", "daysOfMonth"),
                MonthlyScheduleSnapshotsToKeep: GetNestedPropertyInt(result.Data.Properties, "monthlySchedule", "snapshotsToKeep"));
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating NetApp Files snapshot policy '{snapshotPolicy}': {ex.Message}", ex);
        }
    }

    public async Task<VolumeGroupCreateResult> CreateVolumeGroup(
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
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(account), account),
            (nameof(volumeGroup), volumeGroup),
            (nameof(resourceGroup), resourceGroup),
            (nameof(location), location),
            (nameof(applicationType), applicationType),
            (nameof(applicationIdentifier), applicationIdentifier),
            (nameof(subscription), subscription));

        try
        {
            ArmClient armClient = await CreateArmClientWithApiVersionAsync(
                "Microsoft.NetApp/netAppAccounts/volumeGroups",
                "2024-03-01",
                tenant,
                retryPolicy,
                cancellationToken);

            var resourceId = new ResourceIdentifier(
                $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/volumeGroups/{volumeGroup}");

            var createContent = new VolumeGroupCreateOrUpdateContent
            {
                Location = location,
                Properties = new VolumeGroupCreateProperties
                {
                    GroupMetaData = new VolumeGroupCreateMetaData
                    {
                        ApplicationType = applicationType,
                        ApplicationIdentifier = applicationIdentifier,
                        GroupDescription = groupDescription
                    }
                }
            };

            var result = await CreateOrUpdateGenericResourceAsync(
                armClient,
                resourceId,
                location,
                createContent,
                NetAppFilesJsonContext.Default.VolumeGroupCreateOrUpdateContent,
                cancellationToken);

            if (!result.HasData)
            {
                return new VolumeGroupCreateResult(
                    Id: null,
                    Name: null,
                    Type: null,
                    Location: null,
                    ResourceGroup: null,
                    ProvisioningState: null,
                    GroupMetaDataApplicationType: null,
                    GroupMetaDataApplicationIdentifier: null,
                    GroupMetaDataDescription: null);
            }

            return new VolumeGroupCreateResult(
                Id: result.Data.Id.ToString(),
                Name: result.Data.Name,
                Type: result.Data.ResourceType.ToString(),
                Location: result.Data.Location,
                ResourceGroup: resourceGroup,
                ProvisioningState: GetPropertyString(result.Data.Properties, "provisioningState"),
                GroupMetaDataApplicationType: GetNestedPropertyString(result.Data.Properties, "groupMetaData", "applicationType"),
                GroupMetaDataApplicationIdentifier: GetNestedPropertyString(result.Data.Properties, "groupMetaData", "applicationIdentifier"),
                GroupMetaDataDescription: GetNestedPropertyString(result.Data.Properties, "groupMetaData", "groupDescription"));
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating NetApp Files volume group '{volumeGroup}': {ex.Message}", ex);
        }
    }

    private static string? GetPropertyString(BinaryData? properties, string propertyName)
    {
        if (properties == null) return null;
        var doc = JsonDocument.Parse(properties);
        return doc.RootElement.TryGetProperty(propertyName, out var value) ? value.GetString() : null;
    }

    private static long? GetPropertyLong(BinaryData? properties, string propertyName)
    {
        if (properties == null) return null;
        var doc = JsonDocument.Parse(properties);
        return doc.RootElement.TryGetProperty(propertyName, out var value) ? value.GetInt64() : null;
    }

    private static List<string>? GetPropertyStringList(BinaryData? properties, string propertyName)
    {
        if (properties == null) return null;
        var doc = JsonDocument.Parse(properties);
        if (!doc.RootElement.TryGetProperty(propertyName, out var value)) return null;
        return value.EnumerateArray().Select(e => e.GetString()!).ToList();
    }

    private static int? GetPropertyInt(BinaryData? properties, string propertyName)
    {
        if (properties == null) return null;
        var doc = JsonDocument.Parse(properties);
        return doc.RootElement.TryGetProperty(propertyName, out var value) ? value.GetInt32() : null;
    }

    private static bool? GetPropertyBool(BinaryData? properties, string propertyName)
    {
        if (properties == null) return null;
        var doc = JsonDocument.Parse(properties);
        return doc.RootElement.TryGetProperty(propertyName, out var value) ? value.GetBoolean() : null;
    }

    private static int? GetNestedPropertyInt(BinaryData? properties, string parentPropertyName, string childPropertyName)
    {
        if (properties == null) return null;
        var doc = JsonDocument.Parse(properties);
        if (!doc.RootElement.TryGetProperty(parentPropertyName, out var parent)) return null;
        return parent.TryGetProperty(childPropertyName, out var value) ? value.GetInt32() : null;
    }

    private static string? GetNestedPropertyString(BinaryData? properties, string parentPropertyName, string childPropertyName)
    {
        if (properties == null) return null;
        var doc = JsonDocument.Parse(properties);
        if (!doc.RootElement.TryGetProperty(parentPropertyName, out var parent)) return null;
        return parent.TryGetProperty(childPropertyName, out var value) ? value.GetString() : null;
    }
}
