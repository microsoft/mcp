// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Services;

public interface IDppBackupOperations
{
    Task<VaultCreateResult> CreateVaultAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string location,
        string? sku,
        string? storageType,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);

    Task<BackupVaultInfo> GetVaultAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);

    Task<List<BackupVaultInfo>> ListVaultsAsync(
        string subscription,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);

    Task<OperationResult> UpdateVaultAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string? redundancy,
        string? softDelete,
        string? softDeleteRetentionDays,
        string? immutabilityState,
        string? identityType,
        string? tags,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);

    Task<BackupPolicyInfo> GetPolicyAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string policyName,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);

    Task<List<BackupPolicyInfo>> ListPoliciesAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);

    Task<OperationResult> CreatePolicyAsync(
        Policy.PolicyCreateRequest request,
        string vaultName,
        string resourceGroup,
        string subscription,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);

    Task<ProtectResult> ProtectItemAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string datasourceId,
        string policyName,
        string? datasourceType,
        string? aksIncludedNamespaces,
        string? aksExcludedNamespaces,
        string? aksLabelSelectors,
        string? aksIncludeClusterScopeResources,
        string? aksSnapshotResourceGroup,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);

    Task<ProtectedItemInfo> GetProtectedItemAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string protectedItemName,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);

    Task<List<ProtectedItemInfo>> ListProtectedItemsAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);

    Task<OperationResult> UndeleteProtectedItemAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string datasourceId,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);

    Task<BackupJobInfo> GetJobAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string jobId,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);

    Task<List<BackupJobInfo>> ListJobsAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);

    Task<RecoveryPointInfo> GetRecoveryPointAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string protectedItemName,
        string recoveryPointId,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);

    Task<List<RecoveryPointInfo>> ListRecoveryPointsAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string protectedItemName,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);

    Task<OperationResult> ConfigureImmutabilityAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string immutabilityState,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);

    Task<OperationResult> ConfigureSoftDeleteAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string softDeleteState,
        string? softDeleteRetentionDays,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);

    Task<OperationResult> ConfigureCrossRegionRestoreAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);

    Task<OperationResult> ConfigureMultiUserAuthorizationAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string resourceGuardId,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);

    Task<OperationResult> DisableMultiUserAuthorizationAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);

    Task<OperationResult> ConfigureEncryptionAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string keyVaultUri,
        string keyName,
        string identityType,
        string? keyVersion,
        string? userAssignedIdentityId,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);
}
