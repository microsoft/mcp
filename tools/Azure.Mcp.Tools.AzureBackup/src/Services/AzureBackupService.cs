// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.RecoveryServicesBackup;
using Azure.ResourceManager.RecoveryServicesBackup.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;

using SdkBackupStatusResult = Azure.ResourceManager.RecoveryServicesBackup.Models.BackupStatusResult;

namespace Azure.Mcp.Tools.AzureBackup.Services;

public sealed partial class AzureBackupService(IRsvBackupOperations rsvOps, IDppBackupOperations dppOps, ITenantService tenantService, ILogger<AzureBackupService> logger)
    : BaseAzureService(tenantService), IAzureBackupService
{
    /// <summary>
    /// Resource types that Azure Backup can protect.
    /// RSV: IaasVM, SQL-in-IaasVM (workload on VM), SAP HANA (workload on VM), SAP ASE (workload on VM), Azure FileShare.
    /// DPP: Disk, Blob, AKS, ElasticSAN, ADLS, PostgreSQL Flexible, CosmosDB.
    /// Note: SQL/SAP HANA/SAP ASE are in-guest workloads on VMs, so VMs covers them.
    /// Blob and ADLS share the storageAccounts ARM type.
    /// </summary>
    private static readonly string[] s_protectableResourceTypes =
    [
        "Microsoft.Compute/virtualMachines",
        "Microsoft.Storage/storageAccounts",
        "Microsoft.DBforPostgreSQL/flexibleServers",
        "Microsoft.ContainerService/managedClusters",
        "Microsoft.Compute/disks",
        "Microsoft.ElasticSan/elasticSans",
        "Microsoft.DocumentDB/databaseAccounts"
    ];
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
        string subscription, string? resourceGroup, string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        List<BackupVaultInfo> FilterByResourceGroup(List<BackupVaultInfo> vaults) =>
            string.IsNullOrEmpty(resourceGroup)
                ? vaults
                : vaults.Where(v => string.Equals(v.ResourceGroup, resourceGroup, StringComparison.OrdinalIgnoreCase)).ToList();

        if (VaultTypeResolver.IsRsv(vaultType))
        {
            return FilterByResourceGroup(await rsvOps.ListVaultsAsync(subscription, tenant, retryPolicy, cancellationToken));
        }

        if (VaultTypeResolver.IsDpp(vaultType))
        {
            return FilterByResourceGroup(await dppOps.ListVaultsAsync(subscription, tenant, retryPolicy, cancellationToken));
        }

        var rsvTask = rsvOps.ListVaultsAsync(subscription, tenant, retryPolicy, cancellationToken);
        var dppTask = dppOps.ListVaultsAsync(subscription, tenant, retryPolicy, cancellationToken);

        try
        {
            await Task.WhenAll(rsvTask, dppTask);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            // Individual task results are inspected below
        }

        var merged = new List<BackupVaultInfo>();

        if (rsvTask.IsCompletedSuccessfully)
        {
            merged.AddRange(rsvTask.Result);
        }
        else if (rsvTask.IsFaulted)
        {
            logger.LogWarning(rsvTask.Exception, "Failed to list Recovery Services vaults. DPP results will still be returned.");
        }

        if (dppTask.IsCompletedSuccessfully)
        {
            merged.AddRange(dppTask.Result);
        }
        else if (dppTask.IsFaulted)
        {
            logger.LogWarning(dppTask.Exception, "Failed to list Data Protection vaults. RSV results will still be returned.");
        }

        if (rsvTask.IsFaulted && dppTask.IsFaulted)
        {
            throw new AggregateException("Both RSV and DPP vault listing failed.", rsvTask.Exception!, dppTask.Exception!);
        }

        return FilterByResourceGroup(merged);
    }

    public async Task<ProtectResult> ProtectItemAsync(
        string vaultName, string resourceGroup, string subscription,
        string datasourceId, string policyName, string? vaultType,
        string? containerName, string? datasourceType,
        string? aksIncludedNamespaces, string? aksExcludedNamespaces,
        string? aksLabelSelectors, string? aksIncludeClusterScopeResources,
        string? aksSnapshotResourceGroup,
        string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolvedType = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);

        return VaultTypeResolver.IsRsv(resolvedType)
            ? await rsvOps.ProtectItemAsync(vaultName, resourceGroup, subscription, datasourceId, policyName, containerName, datasourceType, tenant, retryPolicy, cancellationToken)
            : await dppOps.ProtectItemAsync(vaultName, resourceGroup, subscription, datasourceId, policyName, datasourceType, aksIncludedNamespaces, aksExcludedNamespaces, aksLabelSelectors, aksIncludeClusterScopeResources, aksSnapshotResourceGroup, tenant, retryPolicy, cancellationToken);
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

    public async Task<OperationResult> UndeleteProtectedItemAsync(
        string vaultName, string resourceGroup, string subscription,
        string datasourceId, string? vaultType, string? containerName,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolvedType = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);

        return VaultTypeResolver.IsRsv(resolvedType)
            ? await rsvOps.UndeleteProtectedItemAsync(vaultName, resourceGroup, subscription, datasourceId, containerName, tenant, retryPolicy, cancellationToken)
            : await dppOps.UndeleteProtectedItemAsync(vaultName, resourceGroup, subscription, datasourceId, tenant, retryPolicy, cancellationToken);
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

    public async Task<OperationResult> CreatePolicyAsync(
        Policy.PolicyCreateRequest request,
        string vaultName, string resourceGroup, string subscription,
        string? vaultType,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolved = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);
        return VaultTypeResolver.IsRsv(resolved)
            ? await rsvOps.CreatePolicyAsync(request, vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken)
            : await dppOps.CreatePolicyAsync(request, vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
    }

    public async Task<OperationResult> UpdatePolicyAsync(
        string vaultName, string resourceGroup, string subscription,
        string policyName, string? vaultType,
        string? scheduleTime, string? dailyRetentionDays,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolved = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);
        if (!VaultTypeResolver.IsRsv(resolved))
        {
            throw new InvalidOperationException("Update is only supported for RSV (Recovery Services vault) policies. DPP policies do not support update.");
        }

        return await rsvOps.UpdatePolicyAsync(vaultName, resourceGroup, subscription, policyName, scheduleTime, dailyRetentionDays, tenant, retryPolicy, cancellationToken);
    }

    public async Task<List<ProtectableItemInfo>> ListProtectableItemsAsync(
        string vaultName, string resourceGroup, string subscription,
        string? workloadType, string? containerName, string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        if (VaultTypeResolver.IsDpp(vaultType))
        {
            throw new ArgumentException("Protectable item discovery is only supported for Recovery Services (RSV) vaults. DPP datasources are protected by their ARM resource ID directly.");
        }

        // Auto-detect vault type when not explicitly specified to avoid routing DPP vaults to RSV
        if (!VaultTypeResolver.IsVaultTypeSpecified(vaultType))
        {
            var resolved = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);
            if (VaultTypeResolver.IsDpp(resolved))
            {
                throw new ArgumentException(
                    $"Vault '{vaultName}' is a Data Protection (DPP) vault. Protectable item discovery is only supported for Recovery Services (RSV) vaults. " +
                    "DPP datasources (disks, blobs, AKS, etc.) are protected by their ARM resource ID directly using 'azurebackup protecteditem protect'.");
            }
        }

        return await rsvOps.ListProtectableItemsAsync(vaultName, resourceGroup, subscription, workloadType, containerName, tenant, retryPolicy, cancellationToken);
    }

    public async Task<Models.BackupStatusResult> GetBackupStatusAsync(
        string datasourceId, string subscription, string location,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ResourceIdentifier resourceId;
        try
        {
            resourceId = new ResourceIdentifier(datasourceId);
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException or UriFormatException)
        {
            throw new ArgumentException(
                $"Invalid datasource ID '{datasourceId}'. Expected a fully-qualified ARM resource ID " +
                "(e.g., /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.Compute/virtualMachines/{name}).", ex);
        }

        string? armResourceType = null;
        try
        {
            armResourceType = resourceId.ResourceType.ToString().ToLowerInvariant();
        }
        catch (Exception)
        {
            // ResourceType can throw for malformed IDs
        }

        var datasourceType = string.IsNullOrEmpty(armResourceType)
            ? null
            : MapArmResourceTypeToBackupDataSourceType(armResourceType);

        if (datasourceType != null)
        {
            // RSV-supported resource types use the BackupStatus API
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
            var subId = SubscriptionResource.CreateResourceIdentifier(subscription);
            var subResource = armClient.GetSubscriptionResource(subId);

            var content = new BackupStatusContent
            {
                ResourceId = resourceId,
                ResourceType = datasourceType
            };

            Response<SdkBackupStatusResult> response = await subResource.GetBackupStatusAsync(new AzureLocation(location), content, cancellationToken);
            SdkBackupStatusResult status = response.Value;

            return new Models.BackupStatusResult(
                datasourceId,
                status.ProtectionStatus?.ToString(),
                status.VaultId?.ToString(),
                status.PolicyName,
                null,
                null,
                null);
        }

        // DPP-only resource types (disks, blobs, AKS, etc.) - search across DPP vaults
        return await GetDppBackupStatusAsync(datasourceId, subscription, tenant, retryPolicy, cancellationToken);
    }

    /// <summary>
    /// For DPP-managed resources (disks, blobs, AKS, etc.), searches across all DPP vaults
    /// to find the backup instance matching the datasource ID.
    /// </summary>
    private async Task<Models.BackupStatusResult> GetDppBackupStatusAsync(
        string datasourceId, string subscription, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var dppVaults = await dppOps.ListVaultsAsync(subscription, tenant, retryPolicy, cancellationToken);

        foreach (var vault in dppVaults.Where(v => v.Name is not null && v.ResourceGroup is not null))
        {
            try
            {
                var items = await dppOps.ListProtectedItemsAsync(vault.Name!, vault.ResourceGroup!, subscription, tenant, retryPolicy, cancellationToken);
                var match = items.FirstOrDefault(i =>
                    string.Equals(i.DatasourceId, datasourceId, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    return new Models.BackupStatusResult(
                        datasourceId,
                        match.ProtectionStatus ?? "Protected",
                        vault.Id,
                        match.PolicyName,
                        match.LastBackupTime,
                        null,
                        null);
                }
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to check backup status in DPP vault '{VaultName}'. Skipping.", vault.Name);
            }
        }

        return new Models.BackupStatusResult(datasourceId, "NotProtected", null, null, null, null, null);
    }

    /// <summary>
    /// Maps ARM resource type strings to the BackupDataSourceType expected by the RSV
    /// Backup Status API. Returns null for DPP-only resource types (disks, blobs, etc.)
    /// that are not supported by the RSV BackupStatus API.
    /// </summary>
    private static BackupDataSourceType? MapArmResourceTypeToBackupDataSourceType(string armResourceType) =>
        armResourceType switch
        {
            "microsoft.compute/virtualmachines" => BackupDataSourceType.Vm,
            "microsoft.storage/storageaccounts" => BackupDataSourceType.AzureFileShare,
            "microsoft.sql/servers/databases" => BackupDataSourceType.SqlDatabase,
            _ => null // DPP-only types handled via DPP vault lookup
        };

    public async Task<List<UnprotectedResourceInfo>> FindUnprotectedResourcesAsync(
        string subscription, string? resourceTypeFilter, string? resourceGroup,
        string? tagFilter, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        // Step 1: List all vaults (RSV + DPP) in the subscription (parallelized)
        var rsvVaultsTask = rsvOps.ListVaultsAsync(subscription, tenant, retryPolicy, cancellationToken);
        var dppVaultsTask = dppOps.ListVaultsAsync(subscription, tenant, retryPolicy, cancellationToken);

        try
        {
            await Task.WhenAll(rsvVaultsTask, dppVaultsTask);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            // Individual task results are inspected below
        }

        var rsvVaults = rsvVaultsTask.IsCompletedSuccessfully ? rsvVaultsTask.Result : [];
        var dppVaults = dppVaultsTask.IsCompletedSuccessfully ? dppVaultsTask.Result : [];

        if (rsvVaultsTask.IsFaulted)
        {
            logger.LogWarning(rsvVaultsTask.Exception, "Failed to list RSV vaults for unprotected resource scan. DPP vaults will still be checked.");
        }

        if (dppVaultsTask.IsFaulted)
        {
            logger.LogWarning(dppVaultsTask.Exception, "Failed to list DPP vaults for unprotected resource scan. RSV vaults will still be checked.");
        }

        if (rsvVaultsTask.IsFaulted && dppVaultsTask.IsFaulted)
        {
            throw new AggregateException(
                "Both RSV and DPP vault listing failed during unprotected resource scan.",
                rsvVaultsTask.Exception!,
                dppVaultsTask.Exception!);
        }

        // Step 2: Collect all protected datasource ARM IDs from every vault
        var protectedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var rsvTasks = rsvVaults
            .Where(v => v.Name is not null && v.ResourceGroup is not null)
            .Select(async v =>
            {
                try
                {
                    return await rsvOps.ListProtectedItemsAsync(
                        v.Name!, v.ResourceGroup!, subscription, tenant, retryPolicy, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to list protected items for RSV vault '{VaultName}' in resource group '{ResourceGroup}'. Skipping vault.", v.Name, v.ResourceGroup);
                    return new List<ProtectedItemInfo>();
                }
            });

        var rsvResults = await Task.WhenAll(rsvTasks);
        foreach (var items in rsvResults)
        {
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.DatasourceId))
                {
                    protectedIds.Add(item.DatasourceId);
                }
            }
        }

        var dppTasks = dppVaults
            .Where(v => v.Name is not null && v.ResourceGroup is not null)
            .Select(async v =>
            {
                try
                {
                    return await dppOps.ListProtectedItemsAsync(
                        v.Name!, v.ResourceGroup!, subscription, tenant, retryPolicy, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to list protected items for DPP vault '{VaultName}' in resource group '{ResourceGroup}'. Skipping vault.", v.Name, v.ResourceGroup);
                    return new List<ProtectedItemInfo>();
                }
            });

        var dppResults = await Task.WhenAll(dppTasks);
        foreach (var items in dppResults)
        {
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.DatasourceId))
                {
                    protectedIds.Add(item.DatasourceId);
                }
            }
        }

        // Step 3: List all resources of protectable types in the subscription
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var subId = SubscriptionResource.CreateResourceIdentifier(subscription);
        var subResource = armClient.GetSubscriptionResource(subId);

        var targetTypes = !string.IsNullOrEmpty(resourceTypeFilter)
            ? ValidateAndParseResourceTypeFilter(resourceTypeFilter)
            : s_protectableResourceTypes;

        var unprotected = new List<UnprotectedResourceInfo>();

        foreach (var resourceType in targetTypes)
        {
            var filter = $"resourceType eq '{resourceType}'";

            await foreach (var resource in subResource.GetGenericResourcesAsync(filter: filter, cancellationToken: cancellationToken))
            {
                var resourceId = resource.Id?.ToString();
                if (string.IsNullOrEmpty(resourceId))
                {
                    continue;
                }

                // Apply optional resource group filter
                if (!string.IsNullOrEmpty(resourceGroup) &&
                    !string.Equals(resource.Id?.ResourceGroupName, resourceGroup, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Apply optional tag filter (format: "key=value")
                if (!string.IsNullOrEmpty(tagFilter) && tagFilter.Contains('=', StringComparison.Ordinal))
                {
                    var parts = tagFilter.Split('=', 2);
                    var tagKey = parts[0];
                    var tagValue = parts.Length > 1 ? parts[1] : string.Empty;

                    if (resource.Data.Tags is null ||
                        !resource.Data.Tags.TryGetValue(tagKey, out var actualValue) ||
                        !string.Equals(actualValue, tagValue, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                // Skip if already protected
                if (protectedIds.Contains(resourceId))
                {
                    continue;
                }

                unprotected.Add(new UnprotectedResourceInfo(
                    resourceId,
                    resource.Data.Name,
                    resource.Data.ResourceType.ToString(),
                    resource.Id?.ResourceGroupName,
                    resource.Data.Location.ToString(),
                    resource.Data.Tags?.ToDictionary(t => t.Key, t => t.Value)));
            }
        }

        return unprotected;
    }

    public async Task<OperationResult> ConfigureImmutabilityAsync(
        string vaultName, string resourceGroup, string subscription,
        string immutabilityState, string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(immutabilityState, nameof(immutabilityState));

        var normalizedState = NormalizeImmutabilityState(immutabilityState);
        var resolved = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);
        return VaultTypeResolver.IsRsv(resolved)
            ? await rsvOps.ConfigureImmutabilityAsync(vaultName, resourceGroup, subscription, normalizedState, tenant, retryPolicy, cancellationToken)
            : await dppOps.ConfigureImmutabilityAsync(vaultName, resourceGroup, subscription, normalizedState, tenant, retryPolicy, cancellationToken);
    }

    /// <summary>
    /// Normalizes user-friendly immutability state values to the API-expected values.
    /// Both RSV and DPP APIs expect 'Unlocked' (not 'Enabled') for the active-but-unlocked state.
    /// Valid API values are: Disabled, Unlocked, Locked.
    /// </summary>
    private static string NormalizeImmutabilityState(string immutabilityState) =>
        immutabilityState.ToUpperInvariant() switch
        {
            "ENABLED" => "Unlocked",
            "UNLOCKED" => "Unlocked",
            "DISABLED" => "Disabled",
            "LOCKED" => "Locked",
            _ => throw new ArgumentException(
                $"Invalid immutability state '{immutabilityState}'. Valid values are: Enabled, Disabled, Unlocked, Locked.",
                nameof(immutabilityState))
        };

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
        return await dppOps.ConfigureCrossRegionRestoreAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
    }

    public async Task<OperationResult> ConfigureMultiUserAuthorizationAsync(
        string vaultName, string resourceGroup, string subscription,
        string resourceGuardId, string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolved = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);
        return VaultTypeResolver.IsRsv(resolved)
            ? await rsvOps.ConfigureMultiUserAuthorizationAsync(vaultName, resourceGroup, subscription, resourceGuardId, tenant, retryPolicy, cancellationToken)
            : await dppOps.ConfigureMultiUserAuthorizationAsync(vaultName, resourceGroup, subscription, resourceGuardId, tenant, retryPolicy, cancellationToken);
    }

    public async Task<OperationResult> DisableMultiUserAuthorizationAsync(
        string vaultName, string resourceGroup, string subscription,
        string? vaultType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var resolved = await ResolveVaultTypeAsync(vaultName, resourceGroup, subscription, vaultType, tenant, retryPolicy, cancellationToken);
        return VaultTypeResolver.IsRsv(resolved)
            ? await rsvOps.DisableMultiUserAuthorizationAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken)
            : await dppOps.DisableMultiUserAuthorizationAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
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
        catch (RequestFailedException ex) when (ex.Status is 401 or 403)
        {
            logger.LogDebug(ex, "RSV probe for vault '{VaultName}' returned {Status}. Trying DPP.", vaultName, ex.Status);
        }
        catch (RequestFailedException ex) when (ex.Status is 404)
        {
            logger.LogDebug(ex, "RSV probe for vault '{VaultName}' returned 404. Trying DPP.", vaultName);
        }

        try
        {
            await dppOps.GetVaultAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
            return VaultTypeResolver.Dpp;
        }
        catch (RequestFailedException ex) when (ex.Status is 401 or 403)
        {
            throw new UnauthorizedAccessException($"Authorization failed for vault '{vaultName}'. Verify your RBAC permissions on the vault, or specify --vault-type to skip auto-detection. Details: {ex.Message}", ex);
        }
        catch (RequestFailedException ex) when (ex.Status is 404)
        {
            throw new KeyNotFoundException($"Vault '{vaultName}' not found in resource group '{resourceGroup}'. Verify the vault name and resource group, or specify --vault-type to skip auto-detection.");
        }
    }

    private static async Task<T> AutoDetectAndExecuteAsync<T>(
        Func<Task<T>> rsvAction, Func<Task<T>> dppAction, string vaultName)
    {
        bool rsvAuthFailed = false;

        try
        {
            return await rsvAction();
        }
        catch (RequestFailedException ex) when (ex.Status is 401 or 403)
        {
            // RSV auth failure  -  try DPP before giving up
            rsvAuthFailed = true;
        }
        catch (RequestFailedException ex) when (ex.Status is 404)
        {
            // RSV not found  -  try DPP
        }

        try
        {
            return await dppAction();
        }
        catch (RequestFailedException ex) when (ex.Status is 401 or 403)
        {
            throw new UnauthorizedAccessException($"Authorization failed for vault '{vaultName}'. Verify your RBAC permissions on the vault, or specify --vault-type to skip auto-detection. Details: {ex.Message}", ex);
        }
        catch (RequestFailedException ex) when (ex.Status is 404)
        {
            var message = rsvAuthFailed
                ? $"Vault '{vaultName}' not found as DPP vault, and RSV access was denied (authorization failure). Verify your RBAC permissions or specify --vault-type to skip auto-detection."
                : $"Vault '{vaultName}' not found as either RSV or DPP vault. Verify the vault name and resource group, or specify --vault-type to skip auto-detection.";
            throw new KeyNotFoundException(message);
        }
    }

    /// <summary>
    /// Validates that each resource type in the filter matches the expected ARM resource type format
    /// (e.g., "Microsoft.Compute/virtualMachines") to prevent OData injection.
    /// </summary>
    private static string[] ValidateAndParseResourceTypeFilter(string resourceTypeFilter)
    {
        var types = resourceTypeFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var type in types)
        {
            if (!ArmResourceTypeRegex().IsMatch(type))
            {
                throw new ArgumentException(
                    $"Invalid resource type format '{type}'. Expected format: 'Microsoft.Provider/resourceType' (e.g., 'Microsoft.Compute/virtualMachines').");
            }
        }

        return types;
    }

    [GeneratedRegex(@"^[A-Za-z0-9]+\.[A-Za-z0-9]+/[A-Za-z0-9]+$")]
    private static partial Regex ArmResourceTypeRegex();
}
