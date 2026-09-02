// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.ResourceManager.RecoveryServicesBackup.Models;

namespace Azure.Mcp.Tools.AzureBackup.Services;

public interface IRsvBackupOperations
{
    Task<VaultCreateResult> CreateVaultAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string location,
        string? sku,
        string? storageType,
        string? tenant,
        CancellationToken cancellationToken);

    Task<BackupVaultInfo> GetVaultAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string? tenant,
        CancellationToken cancellationToken,
        VaultExpand expand = VaultExpand.None);

    Task<List<BackupVaultInfo>> ListVaultsAsync(
        string subscription,
        string? tenant,
        CancellationToken cancellationToken,
        VaultExpand expand = VaultExpand.None);

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
        CancellationToken cancellationToken);

    Task<BackupPolicyInfo> GetPolicyAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string policyName,
        string? tenant,
        CancellationToken cancellationToken);

    Task<List<BackupPolicyInfo>> ListPoliciesAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string? tenant,
        CancellationToken cancellationToken);

    Task<OperationResult> CreatePolicyAsync(
        Policy.PolicyCreateRequest request,
        string vaultName,
        string resourceGroup,
        string subscription,
        string? tenant,
        CancellationToken cancellationToken);

    Task<OperationResult> UpdatePolicyAsync(
        Policy.PolicyUpdateRequest request,
        string vaultName,
        string resourceGroup,
        string subscription,
        string? tenant,
        CancellationToken cancellationToken);

    Task<ProtectResult> ProtectItemAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string datasourceId,
        string policyName,
        string? containerName,
        string? datasourceType,
        DiskExclusionSpec? diskExclusion,
        string? tenant,
        CancellationToken cancellationToken);

    Task<ProtectResult> UpdateProtectionAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string datasourceId,
        string? policyName,
        DiskExclusionSpec? diskExclusion,
        string? containerName,
        string? tenant,
        CancellationToken cancellationToken);

    Task<ProtectedItemInfo> GetProtectedItemAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string protectedItemName,
        string? containerName,
        string? tenant,
        CancellationToken cancellationToken);

    Task<List<ProtectedItemInfo>> ListProtectedItemsAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string? tenant,
        CancellationToken cancellationToken);

    Task<List<ProtectableItemInfo>> ListProtectableItemsAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string? workloadType,
        string? containerName,
        string? tenant,
        CancellationToken cancellationToken);

    Task<OperationResult> UndeleteProtectedItemAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string datasourceId,
        string? containerName,
        string? tenant,
        CancellationToken cancellationToken);

    Task<BackupJobInfo> GetJobAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string jobId,
        string? tenant,
        CancellationToken cancellationToken);

    Task<List<BackupJobInfo>> ListJobsAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string? tenant,
        CancellationToken cancellationToken);

    Task<RecoveryPointInfo> GetRecoveryPointAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string protectedItemName,
        string recoveryPointId,
        string? containerName,
        string? tenant,
        CancellationToken cancellationToken);

    Task<List<RecoveryPointInfo>> ListRecoveryPointsAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string protectedItemName,
        string? containerName,
        string? tenant,
        CancellationToken cancellationToken);

    Task<OperationResult> ConfigureImmutabilityAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string immutabilityState,
        string? tenant,
        CancellationToken cancellationToken);

    Task<OperationResult> ConfigureSoftDeleteAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string softDeleteState,
        string? softDeleteRetentionDays,
        string? tenant,
        CancellationToken cancellationToken);

    Task<OperationResult> ConfigureCrossRegionRestoreAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string? tenant,
        CancellationToken cancellationToken);

    Task<OperationResult> ConfigureMultiUserAuthorizationAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string resourceGuardId,
        string? tenant,
        CancellationToken cancellationToken);

    Task<OperationResult> DisableMultiUserAuthorizationAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string? tenant,
        CancellationToken cancellationToken);

    Task<List<ProtectableItemInfo>> ListDiscoveredProtectableItemsAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string? tenant,
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
        CancellationToken cancellationToken);

    // Private endpoint operations
    Task<PrivateEndpointConnectionInfo> CreatePrivateEndpointAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string privateEndpointName,
        string vnetSubnetId,
        string groupId,
        string? location,
        bool autoApprove,
        string? tenant,
        CancellationToken cancellationToken);

    Task<List<PrivateEndpointConnectionInfo>> ListPrivateEndpointsAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string? tenant,
        CancellationToken cancellationToken);

    Task<PrivateEndpointConnectionInfo> GetPrivateEndpointAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string privateEndpointConnectionName,
        string? tenant,
        CancellationToken cancellationToken);

    Task<OperationResult> DeletePrivateEndpointAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string privateEndpointConnectionName,
        string? tenant,
        CancellationToken cancellationToken);

    Task<PrivateEndpointConnectionInfo> SetPrivateEndpointConnectionStateAsync(
        string vaultName,
        string resourceGroup,
        string subscription,
        string privateEndpointConnectionName,
        PrivateEndpointConnectionStatus targetStatus,
        string? description,
        string? tenant,
        CancellationToken cancellationToken);
}
