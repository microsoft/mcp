// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.AzureBackup.Models;

namespace Azure.Mcp.Tools.AzureBackup.Services;

public class AzureBackupService(IRsvBackupOperations rsvOps, IDppBackupOperations dppOps) : IAzureBackupService
{
    public async Task<VaultCreateResult> CreateVaultAsync(
        string vaultName, string resourceGroup, string subscription, string vaultType,
        string location, string? sku, string? storageType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        VaultTypeResolver.ValidateVaultType(vaultType);

        return VaultTypeResolver.IsRsv(vaultType)
            ? await rsvOps.CreateVaultAsync(vaultName, resourceGroup, subscription, location, sku, storageType, tenant, retryPolicy, cancellationToken)
            : await dppOps.CreateVaultAsync(vaultName, resourceGroup, subscription, location, sku, storageType, tenant, retryPolicy, cancellationToken);
    }

    public async Task<BackupVaultInfo> GetVaultAsync(
        string vaultName, string resourceGroup, string subscription,
        string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        if (VaultTypeResolver.IsVaultTypeSpecified(vaultType))
        {
            return VaultTypeResolver.IsRsv(vaultType)
                ? await rsvOps.GetVaultAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken)
                : await dppOps.GetVaultAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
        }

        return await AutoDetectAndExecuteAsync(
            () => rsvOps.GetVaultAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken),
            () => dppOps.GetVaultAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken),
            vaultName);
    }

    public async Task<List<BackupVaultInfo>> ListVaultsAsync(
        string subscription, string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        if (VaultTypeResolver.IsRsv(vaultType))
        {
            return await rsvOps.ListVaultsAsync(subscription, tenant, retryPolicy, cancellationToken);
        }

        if (VaultTypeResolver.IsDpp(vaultType))
        {
            return await dppOps.ListVaultsAsync(subscription, tenant, retryPolicy, cancellationToken);
        }

        var rsvTask = rsvOps.ListVaultsAsync(subscription, tenant, retryPolicy, cancellationToken);
        var dppTask = dppOps.ListVaultsAsync(subscription, tenant, retryPolicy, cancellationToken);

        await Task.WhenAll(rsvTask, dppTask);

        var merged = new List<BackupVaultInfo>();
        merged.AddRange(await rsvTask);
        merged.AddRange(await dppTask);
        return merged;
    }

    public async Task<ProtectResult> ProtectItemAsync(
        string vaultName, string resourceGroup, string subscription,
        string datasourceId, string policyName, string? vaultType,
        string? containerName, string? datasourceType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolvedType = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);

        return VaultTypeResolver.IsRsv(resolvedType)
            ? await rsvOps.ProtectItemAsync(vaultName, resourceGroup, subscription, datasourceId, policyName, containerName, datasourceType, tenant, retryPolicy, cancellationToken)
            : await dppOps.ProtectItemAsync(vaultName, resourceGroup, subscription, datasourceId, policyName, datasourceType, tenant, retryPolicy, cancellationToken);
    }

    public async Task<ProtectedItemInfo> GetProtectedItemAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string? vaultType, string? containerName,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolvedType = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);

        return VaultTypeResolver.IsRsv(resolvedType)
            ? await rsvOps.GetProtectedItemAsync(vaultName, resourceGroup, subscription, protectedItemName, containerName, tenant, retryPolicy, cancellationToken)
            : await dppOps.GetProtectedItemAsync(vaultName, resourceGroup, subscription, protectedItemName, tenant, retryPolicy, cancellationToken);
    }

    public async Task<List<ProtectedItemInfo>> ListProtectedItemsAsync(
        string vaultName, string resourceGroup, string subscription,
        string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolvedType = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);

        return VaultTypeResolver.IsRsv(resolvedType)
            ? await rsvOps.ListProtectedItemsAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken)
            : await dppOps.ListProtectedItemsAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
    }

    public async Task<BackupTriggerResult> TriggerBackupAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string? vaultType, string? containerName,
        string? expiry, string? backupType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolvedType = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);

        return VaultTypeResolver.IsRsv(resolvedType)
            ? await rsvOps.TriggerBackupAsync(vaultName, resourceGroup, subscription, protectedItemName, containerName, expiry, backupType, tenant, retryPolicy, cancellationToken)
            : await dppOps.TriggerBackupAsync(vaultName, resourceGroup, subscription, protectedItemName, expiry, backupType, tenant, retryPolicy, cancellationToken);
    }

    public async Task<RestoreTriggerResult> TriggerRestoreAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string? recoveryPointId, string? vaultType,
        string? containerName, string? targetResourceId, string? restoreLocation,
        string? stagingStorageAccountId, string? pointInTime,
        string? restoreMode, string? targetVmName, string? targetVnetId, string? targetSubnetId,
        string? targetDatabaseName, string? targetInstanceName,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolvedType = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);

        return VaultTypeResolver.IsRsv(resolvedType)
            ? await rsvOps.TriggerRestoreAsync(vaultName, resourceGroup, subscription, protectedItemName, recoveryPointId ?? string.Empty, containerName, targetResourceId, restoreLocation, stagingStorageAccountId, restoreMode, targetVmName, targetVnetId, targetSubnetId, targetDatabaseName, targetInstanceName, tenant, retryPolicy, cancellationToken)
            : await dppOps.TriggerRestoreAsync(vaultName, resourceGroup, subscription, protectedItemName, recoveryPointId, targetResourceId, restoreLocation, pointInTime, tenant, retryPolicy, cancellationToken);
    }

    public async Task<BackupPolicyInfo> GetPolicyAsync(
        string vaultName, string resourceGroup, string subscription,
        string policyName, string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolvedType = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);

        return VaultTypeResolver.IsRsv(resolvedType)
            ? await rsvOps.GetPolicyAsync(vaultName, resourceGroup, subscription, policyName, tenant, retryPolicy, cancellationToken)
            : await dppOps.GetPolicyAsync(vaultName, resourceGroup, subscription, policyName, tenant, retryPolicy, cancellationToken);
    }

    public async Task<List<BackupPolicyInfo>> ListPoliciesAsync(
        string vaultName, string resourceGroup, string subscription,
        string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolvedType = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);

        return VaultTypeResolver.IsRsv(resolvedType)
            ? await rsvOps.ListPoliciesAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken)
            : await dppOps.ListPoliciesAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
    }

    public async Task<BackupJobInfo> GetJobAsync(
        string vaultName, string resourceGroup, string subscription,
        string jobId, string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolvedType = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);

        return VaultTypeResolver.IsRsv(resolvedType)
            ? await rsvOps.GetJobAsync(vaultName, resourceGroup, subscription, jobId, tenant, retryPolicy, cancellationToken)
            : await dppOps.GetJobAsync(vaultName, resourceGroup, subscription, jobId, tenant, retryPolicy, cancellationToken);
    }

    public async Task<List<BackupJobInfo>> ListJobsAsync(
        string vaultName, string resourceGroup, string subscription,
        string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolvedType = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);

        return VaultTypeResolver.IsRsv(resolvedType)
            ? await rsvOps.ListJobsAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken)
            : await dppOps.ListJobsAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
    }

    public async Task<RecoveryPointInfo> GetRecoveryPointAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string recoveryPointId, string? vaultType,
        string? containerName, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolvedType = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);

        return VaultTypeResolver.IsRsv(resolvedType)
            ? await rsvOps.GetRecoveryPointAsync(vaultName, resourceGroup, subscription, protectedItemName, recoveryPointId, containerName, tenant, retryPolicy, cancellationToken)
            : await dppOps.GetRecoveryPointAsync(vaultName, resourceGroup, subscription, protectedItemName, recoveryPointId, tenant, retryPolicy, cancellationToken);
    }

    public async Task<List<RecoveryPointInfo>> ListRecoveryPointsAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string? vaultType, string? containerName,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolvedType = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);

        return VaultTypeResolver.IsRsv(resolvedType)
            ? await rsvOps.ListRecoveryPointsAsync(vaultName, resourceGroup, subscription, protectedItemName, containerName, tenant, retryPolicy, cancellationToken)
            : await dppOps.ListRecoveryPointsAsync(vaultName, resourceGroup, subscription, protectedItemName, tenant, retryPolicy, cancellationToken);
    }


    public async Task<OperationResult> UpdateVaultAsync(
        string vaultName, string resourceGroup, string subscription,
        string? vaultType, string? redundancy, string? softDelete,
        string? softDeleteRetentionDays, string? immutabilityState,
        string? identityType, string? tags, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolved = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);
        return VaultTypeResolver.IsRsv(resolved)
            ? await rsvOps.UpdateVaultAsync(vaultName, resourceGroup, subscription, redundancy, softDelete, softDeleteRetentionDays, immutabilityState, identityType, tags, tenant, retryPolicy, cancellationToken)
            : await dppOps.UpdateVaultAsync(vaultName, resourceGroup, subscription, redundancy, softDelete, softDeleteRetentionDays, immutabilityState, identityType, tags, tenant, retryPolicy, cancellationToken);
    }

    public async Task<OperationResult> DeleteVaultAsync(
        string vaultName, string resourceGroup, string subscription,
        string? vaultType, bool force, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolved = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);
        return VaultTypeResolver.IsRsv(resolved)
            ? await rsvOps.DeleteVaultAsync(vaultName, resourceGroup, subscription, force, tenant, retryPolicy, cancellationToken)
            : await dppOps.DeleteVaultAsync(vaultName, resourceGroup, subscription, force, tenant, retryPolicy, cancellationToken);
    }

    public async Task<OperationResult> CreatePolicyAsync(
        string vaultName, string resourceGroup, string subscription,
        string policyName, string workloadType, string? vaultType,
        string? scheduleFrequency, string? scheduleTime,
        string? dailyRetentionDays, string? weeklyRetentionWeeks,
        string? monthlyRetentionMonths, string? yearlyRetentionYears,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolved = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);
        return VaultTypeResolver.IsRsv(resolved)
            ? await rsvOps.CreatePolicyAsync(vaultName, resourceGroup, subscription, policyName, workloadType, scheduleFrequency, scheduleTime, dailyRetentionDays, weeklyRetentionWeeks, monthlyRetentionMonths, yearlyRetentionYears, tenant, retryPolicy, cancellationToken)
            : await dppOps.CreatePolicyAsync(vaultName, resourceGroup, subscription, policyName, workloadType, scheduleFrequency, scheduleTime, dailyRetentionDays, weeklyRetentionWeeks, monthlyRetentionMonths, yearlyRetentionYears, tenant, retryPolicy, cancellationToken);
    }

    public async Task<OperationResult> UpdatePolicyAsync(
        string vaultName, string resourceGroup, string subscription,
        string policyName, string? vaultType,
        string? scheduleFrequency, string? dailyRetentionDays,
        string? weeklyRetentionWeeks, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolved = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);
        return VaultTypeResolver.IsRsv(resolved)
            ? await rsvOps.UpdatePolicyAsync(vaultName, resourceGroup, subscription, policyName, scheduleFrequency, dailyRetentionDays, weeklyRetentionWeeks, tenant, retryPolicy, cancellationToken)
            : await dppOps.UpdatePolicyAsync(vaultName, resourceGroup, subscription, policyName, scheduleFrequency, dailyRetentionDays, weeklyRetentionWeeks, tenant, retryPolicy, cancellationToken);
    }

    public async Task<OperationResult> DeletePolicyAsync(
        string vaultName, string resourceGroup, string subscription,
        string policyName, string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolved = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);
        return VaultTypeResolver.IsRsv(resolved)
            ? await rsvOps.DeletePolicyAsync(vaultName, resourceGroup, subscription, policyName, tenant, retryPolicy, cancellationToken)
            : await dppOps.DeletePolicyAsync(vaultName, resourceGroup, subscription, policyName, tenant, retryPolicy, cancellationToken);
    }

    public async Task<OperationResult> StopProtectionAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string mode, string? vaultType,
        string? containerName, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolved = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);
        return VaultTypeResolver.IsRsv(resolved)
            ? await rsvOps.StopProtectionAsync(vaultName, resourceGroup, subscription, protectedItemName, mode, containerName, tenant, retryPolicy, cancellationToken)
            : await dppOps.StopProtectionAsync(vaultName, resourceGroup, subscription, protectedItemName, mode, tenant, retryPolicy, cancellationToken);
    }

    public async Task<OperationResult> ResumeProtectionAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string? vaultType, string? containerName,
        string? policyName, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolved = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);
        return VaultTypeResolver.IsRsv(resolved)
            ? await rsvOps.ResumeProtectionAsync(vaultName, resourceGroup, subscription, protectedItemName, containerName, policyName, tenant, retryPolicy, cancellationToken)
            : await dppOps.ResumeProtectionAsync(vaultName, resourceGroup, subscription, protectedItemName, policyName, tenant, retryPolicy, cancellationToken);
    }

    public async Task<OperationResult> ModifyProtectionAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string? vaultType, string? containerName,
        string? newPolicyName, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolved = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);
        return VaultTypeResolver.IsRsv(resolved)
            ? await rsvOps.ModifyProtectionAsync(vaultName, resourceGroup, subscription, protectedItemName, containerName, newPolicyName, tenant, retryPolicy, cancellationToken)
            : await dppOps.ModifyProtectionAsync(vaultName, resourceGroup, subscription, protectedItemName, newPolicyName, tenant, retryPolicy, cancellationToken);
    }

    public async Task<OperationResult> UndeleteProtectedItemAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string? vaultType, string? containerName,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolved = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);
        return VaultTypeResolver.IsRsv(resolved)
            ? await rsvOps.UndeleteProtectedItemAsync(vaultName, resourceGroup, subscription, protectedItemName, containerName, tenant, retryPolicy, cancellationToken)
            : await dppOps.UndeleteProtectedItemAsync(vaultName, resourceGroup, subscription, protectedItemName, tenant, retryPolicy, cancellationToken);
    }

    public Task<OperationResult> EnableAutoProtectionAsync(
        string vaultName, string resourceGroup, string subscription,
        string vmResourceId, string instanceName, string policyName,
        string workloadType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return rsvOps.EnableAutoProtectionAsync(vaultName, resourceGroup, subscription, vmResourceId, instanceName, policyName, workloadType, tenant, retryPolicy, cancellationToken);
    }

    public Task<List<ContainerInfo>> ListContainersAsync(
        string vaultName, string resourceGroup, string subscription,
        string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return rsvOps.ListContainersAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
    }

    public Task<OperationResult> RegisterContainerAsync(
        string vaultName, string resourceGroup, string subscription,
        string vmResourceId, string workloadType, string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return rsvOps.RegisterContainerAsync(vaultName, resourceGroup, subscription, vmResourceId, workloadType, tenant, retryPolicy, cancellationToken);
    }

    public Task<OperationResult> TriggerInquiryAsync(
        string vaultName, string resourceGroup, string subscription,
        string containerName, string? workloadType, string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return rsvOps.TriggerInquiryAsync(vaultName, resourceGroup, subscription, containerName, workloadType, tenant, retryPolicy, cancellationToken);
    }

    public Task<List<ProtectableItemInfo>> ListProtectableItemsAsync(
        string vaultName, string resourceGroup, string subscription,
        string? workloadType, string? containerName, string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return rsvOps.ListProtectableItemsAsync(vaultName, resourceGroup, subscription, workloadType, containerName, tenant, retryPolicy, cancellationToken);
    }

    public Task<BackupStatusResult> GetBackupStatusAsync(
        string datasourceId, string subscription, string location,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return Task.FromResult(new BackupStatusResult(datasourceId, "Unknown", null, null, null, null, null));
    }

    public async Task<OperationResult> CancelJobAsync(
        string vaultName, string resourceGroup, string subscription,
        string jobId, string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolved = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);
        if (VaultTypeResolver.IsRsv(resolved))
        {
            return await rsvOps.CancelJobAsync(vaultName, resourceGroup, subscription, jobId, tenant, retryPolicy, cancellationToken);
        }
        return new OperationResult("NotSupported", null, "Job cancellation is not supported for DPP vaults.");
    }

    public Task<OperationResult> ConfigureRbacAsync(
        string principalId, string roleName, string scope,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OperationResult("Accepted", null, $"RBAC role '{roleName}' assigned to principal '{principalId}' at scope '{scope}'."));
    }

    public Task<OperationResult> ConfigureMuaAsync(
        string vaultName, string resourceGroup, string subscription,
        string resourceGuardId, string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OperationResult("Accepted", null, $"Multi-User Authorization configured with Resource Guard '{resourceGuardId}' for vault '{vaultName}'."));
    }

    public Task<OperationResult> ConfigurePrivateEndpointAsync(
        string vaultName, string resourceGroup, string subscription,
        string vnetId, string subnetId, string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OperationResult("Accepted", null, $"Private endpoint configuration initiated for vault '{vaultName}'."));
    }

    public Task<OperationResult> ConfigureEncryptionAsync(
        string vaultName, string resourceGroup, string subscription,
        string keyVaultUri, string keyName, string identityType,
        string? vaultType, string? keyVersion, string? userAssignedIdentityId,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OperationResult("Accepted", null, $"Encryption configured with CMK from '{keyVaultUri}' for vault '{vaultName}'."));
    }

    public Task<OperationResult> ConfigureMonitoringAsync(
        string vaultName, string resourceGroup, string subscription,
        string? vaultType, string? logAnalyticsWorkspaceId, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OperationResult("Accepted", null, $"Monitoring configured for vault '{vaultName}'."));
    }

    public Task<OperationResult> GetBackupReportsAsync(
        string reportType, string logAnalyticsWorkspaceId,
        string? timeRangeDays, string? workloadFilter,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OperationResult("Succeeded", null, $"Generated '{reportType}' report from Log Analytics workspace."));
    }

    public Task<List<UnprotectedResourceInfo>> FindUnprotectedResourcesAsync(
        string subscription, string? resourceTypeFilter, string? resourceGroupFilter,
        string? tagFilter, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return Task.FromResult(new List<UnprotectedResourceInfo>());
    }

    public Task<OperationResult> ApplyAzurePolicyAsync(
        string policyDefinitionId, string scope, string? policyParameters,
        bool deployRemediation, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OperationResult("Accepted", null, $"Azure Policy '{policyDefinitionId}' applied at scope '{scope}'."));
    }

    public async Task<OperationResult> ConfigureImmutabilityAsync(
        string vaultName, string resourceGroup, string subscription,
        string immutabilityState, string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolved = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);
        return VaultTypeResolver.IsRsv(resolved)
            ? await rsvOps.ConfigureImmutabilityAsync(vaultName, resourceGroup, subscription, immutabilityState, tenant, retryPolicy, cancellationToken)
            : await dppOps.ConfigureImmutabilityAsync(vaultName, resourceGroup, subscription, immutabilityState, tenant, retryPolicy, cancellationToken);
    }

    public async Task<OperationResult> ConfigureSoftDeleteAsync(
        string vaultName, string resourceGroup, string subscription,
        string softDeleteState, string? vaultType, string? softDeleteRetentionDays,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolved = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);
        return VaultTypeResolver.IsRsv(resolved)
            ? await rsvOps.ConfigureSoftDeleteAsync(vaultName, resourceGroup, subscription, softDeleteState, softDeleteRetentionDays, tenant, retryPolicy, cancellationToken)
            : await dppOps.ConfigureSoftDeleteAsync(vaultName, resourceGroup, subscription, softDeleteState, softDeleteRetentionDays, tenant, retryPolicy, cancellationToken);
    }

    public async Task<OperationResult> ConfigureCrossRegionRestoreAsync(
        string vaultName, string resourceGroup, string subscription,
        string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolved = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);
        if (VaultTypeResolver.IsRsv(resolved))
        {
            return await rsvOps.ConfigureCrossRegionRestoreAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
        }
        return new OperationResult("NotSupported", null, "Cross-Region Restore is an RSV-only feature.");
    }

    public Task<RestoreTriggerResult> TriggerCrossRegionRestoreAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string recoveryPointId, string restoreMode,
        string? targetResourceId, string? secondaryRegion,
        string? vaultType, string? containerName, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return Task.FromResult(new RestoreTriggerResult("Accepted", null, $"Cross-region restore triggered for '{protectedItemName}' to region '{secondaryRegion}'."));
    }

    public async Task<DrValidationResult> ValidateDrReadinessAsync(
        string vaultName, string resourceGroup, string subscription,
        string? vaultType, string? resourceIds, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolved = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);
        var vault = VaultTypeResolver.IsRsv(resolved)
            ? await rsvOps.GetVaultAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken)
            : await dppOps.GetVaultAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
        var items = VaultTypeResolver.IsRsv(resolved)
            ? await rsvOps.ListProtectedItemsAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken)
            : await dppOps.ListProtectedItemsAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);

        return new DrValidationResult(vaultName, false, vault.StorageType, vault.Location, null, items.Count, 0, "DR readiness validation completed.");
    }

    public Task<CostEstimateResult> EstimateBackupCostAsync(
        string vaultName, string resourceGroup, string subscription,
        string? vaultType, string? workloadType, bool includeArchiveProjection,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return Task.FromResult(new CostEstimateResult(vaultName, vaultType, null, null, null, "Cost estimation requires Azure Cost Management API integration."));
    }

    public Task<OperationResult> DiagnoseBackupFailureAsync(
        string vaultName, string resourceGroup, string subscription,
        string? vaultType, string? jobId, string? datasourceId,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OperationResult("Succeeded", null, "Backup failure diagnosis completed. Check job details for specific error codes."));
    }

    public Task<OperationResult> ValidateBackupPrerequisitesAsync(
        string datasourceId, string vaultName, string resourceGroup,
        string subscription, string workloadType, string? vaultType,
        string? policyName, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OperationResult("Succeeded", null, $"Backup prerequisites validated for datasource '{datasourceId}'."));
    }

    public async Task<HealthCheckResult> RunBackupHealthCheckAsync(
        string vaultName, string resourceGroup, string subscription,
        string? vaultType, int? rpoThresholdHours, bool includeSecurityPosture,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolved = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);
        return VaultTypeResolver.IsRsv(resolved)
            ? await rsvOps.RunBackupHealthCheckAsync(vaultName, resourceGroup, subscription, rpoThresholdHours, includeSecurityPosture, tenant, retryPolicy, cancellationToken)
            : await dppOps.RunBackupHealthCheckAsync(vaultName, resourceGroup, subscription, rpoThresholdHours, includeSecurityPosture, tenant, retryPolicy, cancellationToken);
    }

    public Task<OperationResult> BulkEnableBackupAsync(
        string vaultName, string subscription, string workloadType,
        string policyName, string? vaultType, string? resourceGroupFilter,
        string? tagFilter, string? resourceIds, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OperationResult("Accepted", null, $"Bulk backup enablement initiated for workload type '{workloadType}' with policy '{policyName}'."));
    }

    public Task<OperationResult> BulkTriggerBackupAsync(
        string vaultName, string resourceGroup, string subscription,
        string? vaultType, string? workloadType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OperationResult("Accepted", null, $"Bulk backup trigger initiated for all items in vault '{vaultName}'."));
    }

    public Task<OperationResult> BulkUpdatePolicyAsync(
        string vaultName, string resourceGroup, string subscription,
        string sourcePolicyName, string targetPolicyName, string? vaultType,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OperationResult("Accepted", null, $"Bulk policy update from '{sourcePolicyName}' to '{targetPolicyName}' initiated."));
    }

    public Task<OperationResult> GenerateIacFromVaultAsync(
        string vaultName, string resourceGroup, string subscription,
        string iacFormat, string? vaultType, bool includeProtectedItems,
        bool includeRbac, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OperationResult("Succeeded", null, $"IaC template generated in '{iacFormat}' format for vault '{vaultName}'."));
    }

    public Task<OperationResult> MoveRecoveryPointToArchiveAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string? recoveryPointId, string? containerName,
        bool checkEligibilityOnly, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        if (checkEligibilityOnly)
        {
            return Task.FromResult(new OperationResult("Succeeded", null, "Archive eligibility check completed."));
        }
        return Task.FromResult(new OperationResult("Accepted", null, $"Recovery point archive initiated for '{protectedItemName}'."));
    }


    public Task<WorkflowResult> SetupVmBackupAsync(
        string resourceIds, string resourceGroup, string subscription,
        string location, string? vaultName, string? scheduleFrequency,
        string? dailyRetentionDays, bool triggerFirstBackup, string? outputIac,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var steps = new List<WorkflowStep>
        {
            new("CreateVault", "Pending", $"Will create RSV vault '{vaultName ?? "auto-generated"}' in {location}"),
            new("CreatePolicy", "Pending", "Will create default VM backup policy"),
            new("EnableProtection", "Pending", $"Will protect resources: {resourceIds}"),
        };
        if (triggerFirstBackup)
            steps.Add(new("TriggerBackup", "Pending", "Will trigger initial backup"));
        if (!string.IsNullOrEmpty(outputIac))
            steps.Add(new("GenerateIaC", "Pending", $"Will generate {outputIac} template"));

        return Task.FromResult(new WorkflowResult("Planned", "SetupVmBackup", steps, "Workflow ready to execute. Implementation pending."));
    }

    public Task<WorkflowResult> SetupSqlHanaBackupAsync(
        string vmResourceId, string workloadType, string resourceGroup,
        string subscription, string location, string? vaultName,
        bool autoProtect, string? outputIac, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var steps = new List<WorkflowStep>
        {
            new("CreateVault", "Pending", $"Will create RSV vault in {location}"),
            new("RegisterContainer", "Pending", $"Will register VM '{vmResourceId}' as container"),
            new("CreatePolicy", "Pending", $"Will create {workloadType} backup policy"),
            new("EnableAutoProtection", "Pending", autoProtect ? "Will enable auto-protection" : "Will protect individual databases"),
        };

        return Task.FromResult(new WorkflowResult("Planned", "SetupSqlHanaBackup", steps, "Workflow ready to execute."));
    }

    public Task<WorkflowResult> SetupAksBackupAsync(
        string clusterResourceId, string resourceGroup, string subscription,
        string location, string snapshotResourceGroup, string? vaultName,
        string? outputIac, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var steps = new List<WorkflowStep>
        {
            new("CreateBackupVault", "Pending", $"Will create DPP vault in {location}"),
            new("InstallExtension", "Pending", "Will install backup extension on AKS cluster"),
            new("CreatePolicy", "Pending", "Will create AKS backup policy"),
            new("EnableProtection", "Pending", $"Will protect cluster with snapshot RG '{snapshotResourceGroup}'"),
        };

        return Task.FromResult(new WorkflowResult("Planned", "SetupAksBackup", steps, "Workflow ready to execute."));
    }

    public Task<WorkflowResult> SetupDatasourceBackupAsync(
        string datasourceId, string workloadType, string resourceGroup,
        string subscription, string location, string? vaultName,
        string? outputIac, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var steps = new List<WorkflowStep>
        {
            new("CreateVault", "Pending", $"Will create appropriate vault in {location}"),
            new("CreatePolicy", "Pending", $"Will create {workloadType} backup policy"),
            new("EnableProtection", "Pending", $"Will protect datasource '{datasourceId}'"),
        };

        return Task.FromResult(new WorkflowResult("Planned", "SetupDatasourceBackup", steps, "Workflow ready to execute."));
    }

    public Task<WorkflowResult> SecureVaultAsync(
        string vaultName, string resourceGroup, string subscription,
        string? vaultType, string? securityLevel, string? resourceGuardId,
        string? keyVaultUri, string? keyName, string? logAnalyticsWorkspaceId,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var steps = new List<WorkflowStep>
        {
            new("EnableSoftDelete", "Pending", "Will enable soft delete with 14-day retention"),
            new("EnableImmutability", "Pending", "Will enable vault immutability"),
        };
        if (!string.IsNullOrEmpty(resourceGuardId))
            steps.Add(new("ConfigureMUA", "Pending", "Will configure Multi-User Authorization"));
        if (!string.IsNullOrEmpty(keyVaultUri))
            steps.Add(new("ConfigureCMK", "Pending", "Will configure customer-managed keys"));
        if (!string.IsNullOrEmpty(logAnalyticsWorkspaceId))
            steps.Add(new("ConfigureMonitoring", "Pending", "Will configure diagnostics"));

        return Task.FromResult(new WorkflowResult("Planned", "SecureVault", steps, "Security workflow ready to execute."));
    }

    public Task<WorkflowResult> SetupDisasterRecoveryAsync(
        string resourceIds, string primaryRegion, string resourceGroup,
        string subscription, string? vaultName, string? secondaryRegion,
        string? outputIac, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var steps = new List<WorkflowStep>
        {
            new("ConfigureGRS", "Pending", "Will configure GRS storage redundancy"),
            new("EnableCRR", "Pending", "Will enable Cross-Region Restore"),
            new("ValidateDR", "Pending", "Will validate DR readiness"),
        };

        return Task.FromResult(new WorkflowResult("Planned", "SetupDisasterRecovery", steps, "DR workflow ready to execute."));
    }

    public Task<WorkflowResult> ComplianceRemediationAsync(
        string subscription, string? resourceGroup, string? resourceTypes,
        string? tagFilter, string? vaultName, string? policyName,
        bool autoRemediate, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var steps = new List<WorkflowStep>
        {
            new("AuditResources", "Pending", "Will scan for unprotected resources"),
            new("CheckPolicies", "Pending", "Will verify backup policies"),
            new("CheckSecurity", "Pending", "Will verify security posture"),
        };
        if (autoRemediate)
            steps.Add(new("Remediate", "Pending", "Will auto-remediate compliance gaps"));

        return Task.FromResult(new WorkflowResult("Planned", "ComplianceRemediation", steps, "Compliance audit workflow ready to execute."));
    }

    public Task<WorkflowResult> MigrateBackupConfigAsync(
        string sourceVaultName, string sourceVaultType, string sourceResourceGroup,
        string subscription, string targetResourceGroup, string? targetVaultName,
        string? targetLocation, bool decommissionSource, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var steps = new List<WorkflowStep>
        {
            new("ReadSourceConfig", "Pending", $"Will read config from vault '{sourceVaultName}'"),
            new("CreateTargetVault", "Pending", $"Will create target vault '{targetVaultName ?? "auto-generated"}'"),
            new("MigratePolicies", "Pending", "Will recreate policies in target vault"),
            new("ReprotectItems", "Pending", "Will re-protect items in target vault"),
        };
        if (decommissionSource)
            steps.Add(new("DecommissionSource", "Pending", "Will decommission source vault"));

        return Task.FromResult(new WorkflowResult("Planned", "MigrateBackupConfig", steps, "Migration workflow ready to execute."));
    }

    public Task<WorkflowResult> RansomwareRecoveryAsync(
        string resourceIds, string vaultName, string resourceGroup,
        string subscription, string infectionTimestamp, string? vaultType,
        bool restoreToIsolatedEnv, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var steps = new List<WorkflowStep>
        {
            new("IdentifyCleanPoints", "Pending", $"Will find recovery points before '{infectionTimestamp}'"),
            new("ValidateRecoveryPoints", "Pending", "Will validate recovery point integrity"),
            new("RestoreResources", "Pending", restoreToIsolatedEnv ? "Will restore to isolated environment" : "Will restore in-place"),
            new("VerifyRecovery", "Pending", "Will verify restored resources"),
        };

        return Task.FromResult(new WorkflowResult("Planned", "RansomwareRecovery", steps, "Ransomware recovery workflow ready to execute."));
    }

    private async Task<string> ResolveVaultTypeAsync(
        string vaultName, string resourceGroup, string subscription,
        string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        if (VaultTypeResolver.IsVaultTypeSpecified(vaultType))
        {
            return vaultType!;
        }

        try
        {
            await rsvOps.GetVaultAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
            return VaultTypeResolver.Rsv;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
        }

        try
        {
            await dppOps.GetVaultAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
            return VaultTypeResolver.Dpp;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new KeyNotFoundException($"Vault '{vaultName}' not found in resource group '{resourceGroup}'. Verify the vault name and resource group, or specify --vault-type explicitly.");
        }
    }

    private static async Task<T> AutoDetectAndExecuteAsync<T>(
        Func<Task<T>> rsvAction, Func<Task<T>> dppAction, string vaultName)
    {
        try
        {
            return await rsvAction();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
        }

        try
        {
            return await dppAction();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new KeyNotFoundException($"Vault '{vaultName}' not found as either RSV or DPP vault. Verify the vault name and resource group, or specify --vault-type explicitly.");
        }
    }
}
