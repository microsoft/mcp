// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Services;

public interface IAzureBackupService
{
    // Vault operations
    Task<VaultCreateResult> CreateVaultAsync(string vaultName, string resourceGroup, string subscription, string vaultType, string location, string? sku = null, string? storageType = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);
    Task<BackupVaultInfo> GetVaultAsync(string vaultName, string resourceGroup, string subscription, string? vaultType = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);
    Task<List<BackupVaultInfo>> ListVaultsAsync(string subscription, string? resourceGroup = null, string? vaultType = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);
    Task<OperationResult> UpdateVaultAsync(string vaultName, string resourceGroup, string subscription, string? vaultType = null, string? redundancy = null, string? softDelete = null, string? softDeleteRetentionDays = null, string? immutabilityState = null, string? identityType = null, string? tags = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);

    // Policy operations
    Task<BackupPolicyInfo> GetPolicyAsync(string vaultName, string resourceGroup, string subscription, string policyName, string? vaultType = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);
    Task<List<BackupPolicyInfo>> ListPoliciesAsync(string vaultName, string resourceGroup, string subscription, string? vaultType = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);
    Task<OperationResult> CreatePolicyAsync(string vaultName, string resourceGroup, string subscription, string policyName, string workloadType, string? vaultType = null, string? scheduleTime = null, string? dailyRetentionDays = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);

    // Protection operations
    Task<ProtectResult> ProtectItemAsync(string vaultName, string resourceGroup, string subscription, string datasourceId, string policyName, string? vaultType = null, string? containerName = null, string? datasourceType = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);
    Task<ProtectedItemInfo> GetProtectedItemAsync(string vaultName, string resourceGroup, string subscription, string protectedItemName, string? vaultType = null, string? containerName = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);
    Task<List<ProtectedItemInfo>> ListProtectedItemsAsync(string vaultName, string resourceGroup, string subscription, string? vaultType = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);
    Task<List<ProtectableItemInfo>> ListProtectableItemsAsync(string vaultName, string resourceGroup, string subscription, string? workloadType = null, string? containerName = null, string? vaultType = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);

    // Job operations
    Task<BackupJobInfo> GetJobAsync(string vaultName, string resourceGroup, string subscription, string jobId, string? vaultType = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);
    Task<List<BackupJobInfo>> ListJobsAsync(string vaultName, string resourceGroup, string subscription, string? vaultType = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);

    // Recovery point operations
    Task<RecoveryPointInfo> GetRecoveryPointAsync(string vaultName, string resourceGroup, string subscription, string protectedItemName, string recoveryPointId, string? vaultType = null, string? containerName = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);
    Task<List<RecoveryPointInfo>> ListRecoveryPointsAsync(string vaultName, string resourceGroup, string subscription, string protectedItemName, string? vaultType = null, string? containerName = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);

    // Backup status
    Task<BackupStatusResult> GetBackupStatusAsync(string datasourceId, string subscription, string location, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);

    // Governance
    Task<List<UnprotectedResourceInfo>> FindUnprotectedResourcesAsync(string subscription, string? resourceTypeFilter = null, string? resourceGroup = null, string? tagFilter = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);
    Task<OperationResult> ConfigureImmutabilityAsync(string vaultName, string resourceGroup, string subscription, string immutabilityState, string? vaultType = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);
    Task<OperationResult> ConfigureSoftDeleteAsync(string vaultName, string resourceGroup, string subscription, string softDeleteState, string? vaultType = null, string? softDeleteRetentionDays = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);

    // DR
    Task<OperationResult> ConfigureCrossRegionRestoreAsync(string vaultName, string resourceGroup, string subscription, string? vaultType = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);
}
