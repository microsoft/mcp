// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.RecoveryServices;
using Azure.ResourceManager.RecoveryServices.Models;
using Azure.ResourceManager.RecoveryServicesBackup;
using Azure.ResourceManager.RecoveryServicesBackup.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Services;

public sealed class RsvBackupOperations(ITenantService tenantService) : BaseAzureService(tenantService), IRsvBackupOperations
{
    private const string VaultType = VaultTypeResolver.Rsv;
    private const string FabricName = "Azure";

    public async Task<VaultCreateResult> CreateVaultAsync(
        string vaultName, string resourceGroup, string subscription, string location,
        string? sku, string? storageType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(location), location));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var rgId = ResourceGroupResource.CreateResourceIdentifier(subscription, resourceGroup);
        var rgResource = armClient.GetResourceGroupResource(rgId);
        var collection = rgResource.GetRecoveryServicesVaults();

        var vaultSku = new RecoveryServicesSku(RecoveryServicesSkuName.Standard);
        var vaultData = new RecoveryServicesVaultData(new AzureLocation(location))
        {
            Sku = vaultSku,
            Properties = new RecoveryServicesVaultProperties
            {
                PublicNetworkAccess = VaultPublicNetworkAccess.Enabled
            }
        };

        var result = await collection.CreateOrUpdateAsync(WaitUntil.Completed, vaultName, vaultData, cancellationToken);

        return new VaultCreateResult(
            result.Value.Id?.ToString(),
            result.Value.Data.Name,
            VaultType,
            result.Value.Data.Location.Name,
            result.Value.Data.Properties?.ProvisioningState);
    }

    public async Task<BackupVaultInfo> GetVaultAsync(
        string vaultName, string resourceGroup, string subscription,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var vaultId = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultId);
        var vault = await vaultResource.GetAsync(cancellationToken);

        return MapToVaultInfo(vault.Value.Data, resourceGroup);
    }

    public async Task<List<BackupVaultInfo>> ListVaultsAsync(
        string subscription, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var subId = SubscriptionResource.CreateResourceIdentifier(subscription);
        var subResource = armClient.GetSubscriptionResource(subId);

        var vaults = new List<BackupVaultInfo>();
        await foreach (var vault in subResource.GetRecoveryServicesVaultsAsync(cancellationToken))
        {
            var rg = vault.Id?.ResourceGroupName;
            vaults.Add(MapToVaultInfo(vault.Data, rg));
        }

        return vaults;
    }

    public async Task<ProtectResult> ProtectItemAsync(
        string vaultName, string resourceGroup, string subscription,
        string datasourceId, string policyName, string? containerName,
        string? datasourceType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(datasourceId), datasourceId),
            (nameof(policyName), policyName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

        var vaultId = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultId);
        var vault = await vaultResource.GetAsync(cancellationToken: cancellationToken);
        var vaultLocation = vault.Value.Data.Location;

        var policyArmId = BackupProtectionPolicyResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, policyName);

        var profile = RsvDatasourceRegistry.ResolveOrDefault(datasourceType);

        if (profile.IsWorkloadType)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException($"The --container parameter is required for {profile.FriendlyName} workload protection. Use 'azurebackup protectableitem list' to discover containers and items.");
            }

            if (datasourceId.StartsWith("/subscriptions/", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"For {profile.FriendlyName} workload protection, --datasource-id must be the protectable item name " +
                    $"(e.g., 'SAPHanaDatabase;instance;dbname'), not an ARM resource ID. " +
                    $"Use 'azurebackup protectableitem list' to discover protectable item names.");
            }

            var protectedItemName = datasourceId; // For workloads, datasourceId is the protectable item name
            var protectedItemId = BackupProtectedItemResource.CreateResourceIdentifier(
                subscription, resourceGroup, vaultName, FabricName, containerName, protectedItemName);

            BackupGenericProtectedItem protectedItemProperties = profile.ProtectedItemType switch
            {
                RsvProtectedItemType.SapHanaDatabase => new VmWorkloadSapHanaDatabaseProtectedItem { PolicyId = policyArmId },
                _ => new VmWorkloadSqlDatabaseProtectedItem { PolicyId = policyArmId }, // SQL, ASE use the same type
            };

            var protectedItemData = new BackupProtectedItemData(vaultLocation) { Properties = protectedItemProperties };
            var protectedItemResource = armClient.GetBackupProtectedItemResource(protectedItemId);
            var result = await protectedItemResource.UpdateAsync(WaitUntil.Started, protectedItemData, cancellationToken);

            var jobId = await FindLatestJobIdAsync(armClient, subscription, resourceGroup, vaultName, "ConfigureBackup", cancellationToken);
            jobId ??= ExtractOperationIdFromResponse(result.GetRawResponse());

            return new ProtectResult("Accepted", protectedItemName, jobId,
                jobId != null ? $"Workload protection initiated. Use 'azurebackup job get --job {jobId}' to monitor progress." : "Workload protection initiated.");
        }

        if (profile.ProtectedItemType == RsvProtectedItemType.AzureFileShare)
        {
            var fsContainer = containerName ?? RsvNamingHelper.DeriveContainerName(datasourceId, datasourceType);
            var fsProtectedItemName = RsvNamingHelper.DeriveProtectedItemName(datasourceId, datasourceType);

            var containerId = BackupProtectionContainerResource.CreateResourceIdentifier(
                subscription, resourceGroup, vaultName, FabricName, fsContainer);
            var containerResource = armClient.GetBackupProtectionContainerResource(containerId);
            try
            {
                await containerResource.InquireAsync(filter: null, cancellationToken);
                // The container inquiry API is asynchronous on the server side. A brief delay
                // allows the service to register the container before we attempt to configure
                // protection on the file share. Without this, protection requests may fail with 404.
                await Task.Delay(5000, cancellationToken);
            }
            catch (RequestFailedException ex) when (ex.Status is 404 or 409)
            {
                // Inquiry may fail if container isn't registered yet (404) or is already being processed (409) - expected during protection setup
            }

            var fsProtectedItemId = BackupProtectedItemResource.CreateResourceIdentifier(
                subscription, resourceGroup, vaultName, FabricName, fsContainer, fsProtectedItemName);

            var parsedDatasourceId = new ResourceIdentifier(datasourceId);
            var storageAccountId = RsvNamingHelper.GetStorageAccountId(parsedDatasourceId);

            var fsProtectedItemData = new BackupProtectedItemData(vaultLocation)
            {
                Properties = new FileshareProtectedItem
                {
                    PolicyId = policyArmId,
                    SourceResourceId = new ResourceIdentifier(storageAccountId)
                }
            };

            var fsProtectedItemResource = armClient.GetBackupProtectedItemResource(fsProtectedItemId);
            var fsResult = await fsProtectedItemResource.UpdateAsync(WaitUntil.Started, fsProtectedItemData, cancellationToken);

            var fsJobId = await FindLatestJobIdAsync(armClient, subscription, resourceGroup, vaultName, "ConfigureBackup", cancellationToken);
            fsJobId ??= ExtractOperationIdFromResponse(fsResult.GetRawResponse());

            return new ProtectResult("Accepted", fsProtectedItemName, fsJobId,
                fsJobId != null ? $"File share protection initiated. Use 'azurebackup job get --job {fsJobId}' to monitor progress." : "File share protection initiated.");
        }

        var rgId = ResourceGroupResource.CreateResourceIdentifier(subscription, resourceGroup);
        var rgResource = armClient.GetResourceGroupResource(rgId);
        await rgResource.RefreshProtectionContainerAsync(vaultName, FabricName, filter: null, cancellationToken);

        var container = containerName ?? RsvNamingHelper.DeriveContainerName(datasourceId);

        // Poll for container visibility after refresh (up to 180s with 5s intervals).
        // The RSV RefreshProtectionContainerAsync API does not return a pollable LRO,
        // so we must manually poll for the container to become visible.
        // Container discovery can take 2-3 minutes for some workloads.
        const int maxRetries = 36;
        const int delayMs = 5000;
        for (int i = 0; i < maxRetries; i++)
        {
            await Task.Delay(delayMs, cancellationToken);
            try
            {
                var checkContainerId = BackupProtectionContainerResource.CreateResourceIdentifier(
                    subscription, resourceGroup, vaultName, FabricName, container);
                var checkContainer = armClient.GetBackupProtectionContainerResource(checkContainerId);
                await checkContainer.GetAsync(cancellationToken: cancellationToken);
                break; // Container is visible
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                if (i == maxRetries - 1)
                {
                    throw new InvalidOperationException(
                        $"Container '{container}' was not discovered after {maxRetries * delayMs / 1000}s. " +
                        "Container discovery can take several minutes for some workloads. " +
                        "Retry later or verify the VM resource ID is correct.", ex);
                }
            }
        }

        var vmProtectedItemName = RsvNamingHelper.DeriveProtectedItemName(datasourceId);

        var vmProtectedItemId = BackupProtectedItemResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, FabricName, container, vmProtectedItemName);

        var vmProtectedItemData = new BackupProtectedItemData(vaultLocation)
        {
            Properties = new IaasComputeVmProtectedItem
            {
                PolicyId = policyArmId,
                SourceResourceId = new ResourceIdentifier(datasourceId)
            }
        };

        var vmProtectedItemResource = armClient.GetBackupProtectedItemResource(vmProtectedItemId);
        var vmResult = await vmProtectedItemResource.UpdateAsync(WaitUntil.Started, vmProtectedItemData, cancellationToken);

        var vmJobId = await FindLatestJobIdAsync(armClient, subscription, resourceGroup, vaultName, "ConfigureBackup", cancellationToken);
        vmJobId ??= ExtractOperationIdFromResponse(vmResult.GetRawResponse()); // Fallback to operation ID

        return new ProtectResult(
            "Accepted",
            vmProtectedItemName,
            vmJobId,
            vmJobId != null ? $"Protection initiated. Use 'azurebackup job get --job {vmJobId}' to monitor progress." : "Protection initiated.");
    }

    public async Task<ProtectedItemInfo> GetProtectedItemAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string? containerName, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(protectedItemName), protectedItemName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

        if (string.IsNullOrEmpty(containerName))
        {
            // Search by both internal RSV name and friendly/datasource name
            var items = await ListProtectedItemsAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
            var found = items.FirstOrDefault(i =>
                i.Name.Equals(protectedItemName, StringComparison.OrdinalIgnoreCase) ||
                MatchesFriendlyName(i, protectedItemName));
            return found ?? throw new KeyNotFoundException(
                $"Protected item '{protectedItemName}' not found in vault '{vaultName}'. " +
                "Use the full internal name from 'azurebackup protecteditem get' list output, " +
                "or provide --container to look up by container/item path.");
        }

        var itemId = BackupProtectedItemResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, FabricName, containerName, protectedItemName);
        var itemResource = armClient.GetBackupProtectedItemResource(itemId);
        var item = await itemResource.GetAsync(cancellationToken: cancellationToken);

        return MapToProtectedItemInfo(item.Value.Data);
    }

    /// <summary>
    /// Checks whether a protected item matches a user-provided friendly name.
    /// A friendly name can be the VM name, file share name, or database name extracted
    /// from the full RSV internal name or the datasource resource ID.
    /// </summary>
    private static bool MatchesFriendlyName(ProtectedItemInfo item, string friendlyName)
    {
        // Check datasource ID ends with the friendly name (e.g., /virtualMachines/mcp-test-vm)
        if (!string.IsNullOrEmpty(item.DatasourceId))
        {
            var datasourceResourceName = item.DatasourceId.Split('/').LastOrDefault();
            if (string.Equals(datasourceResourceName, friendlyName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        // Check if the RSV internal name contains the friendly name as the last segment
        // RSV names follow patterns like: VM;iaasvmcontainerv2;rg;vmname
        var nameParts = item.Name.Split(';');
        if (nameParts.Length > 0 &&
            string.Equals(nameParts[^1], friendlyName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    public async Task<List<ProtectedItemInfo>> ListProtectedItemsAsync(
        string vaultName, string resourceGroup, string subscription,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var rgId = ResourceGroupResource.CreateResourceIdentifier(subscription, resourceGroup);
        var rgResource = armClient.GetResourceGroupResource(rgId);

        var items = new List<ProtectedItemInfo>();
        await foreach (var item in rgResource.GetBackupProtectedItemsAsync(vaultName, cancellationToken: cancellationToken))
        {
            items.Add(MapToProtectedItemInfo(item.Data));
        }

        return items;
    }

    public async Task<BackupPolicyInfo> GetPolicyAsync(
        string vaultName, string resourceGroup, string subscription,
        string policyName, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(policyName), policyName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var policyId = BackupProtectionPolicyResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, policyName);
        var policyResource = armClient.GetBackupProtectionPolicyResource(policyId);
        var policy = await policyResource.GetAsync(cancellationToken);

        return MapToPolicyInfo(policy.Value.Data);
    }

    public async Task<List<BackupPolicyInfo>> ListPoliciesAsync(
        string vaultName, string resourceGroup, string subscription,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var rgId = ResourceGroupResource.CreateResourceIdentifier(subscription, resourceGroup);
        var rgResource = armClient.GetResourceGroupResource(rgId);

        var policies = new List<BackupPolicyInfo>();
        await foreach (var policy in rgResource.GetBackupProtectionPolicies(vaultName).GetAllAsync(cancellationToken: cancellationToken))
        {
            policies.Add(MapToPolicyInfo(policy.Data));
        }

        return policies;
    }

    public async Task<BackupJobInfo> GetJobAsync(
        string vaultName, string resourceGroup, string subscription,
        string jobId, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(jobId), jobId));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var jobResourceId = BackupJobResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, jobId);
        var jobResource = armClient.GetBackupJobResource(jobResourceId);
        var job = await jobResource.GetAsync(cancellationToken);

        return MapToJobInfo(job.Value.Data);
    }

    public async Task<List<BackupJobInfo>> ListJobsAsync(
        string vaultName, string resourceGroup, string subscription,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var rgId = ResourceGroupResource.CreateResourceIdentifier(subscription, resourceGroup);
        var rgResource = armClient.GetResourceGroupResource(rgId);

        var jobs = new List<BackupJobInfo>();
        await foreach (var job in rgResource.GetBackupJobs(vaultName).GetAllAsync(cancellationToken: cancellationToken))
        {
            jobs.Add(MapToJobInfo(job.Data));
        }

        return jobs;
    }

    public async Task<RecoveryPointInfo> GetRecoveryPointAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string recoveryPointId, string? containerName,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(protectedItemName), protectedItemName),
            (nameof(recoveryPointId), recoveryPointId));

        if (string.IsNullOrEmpty(containerName))
        {
            // Auto-discover container from protected items list
            var resolvedItem = await ResolveProtectedItemContainerAsync(
                vaultName, resourceGroup, subscription, protectedItemName, tenant, retryPolicy, cancellationToken);
            containerName = resolvedItem.ContainerName;
            protectedItemName = resolvedItem.Name;
        }

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var rpId = BackupRecoveryPointResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, FabricName, containerName!, protectedItemName, recoveryPointId);
        var rpResource = armClient.GetBackupRecoveryPointResource(rpId);
        var rp = await rpResource.GetAsync(cancellationToken);

        return MapToRecoveryPointInfo(rp.Value.Data);
    }

    public async Task<List<RecoveryPointInfo>> ListRecoveryPointsAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string? containerName,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(protectedItemName), protectedItemName));

        if (string.IsNullOrEmpty(containerName))
        {
            // Auto-discover container from protected items list
            var resolvedItem = await ResolveProtectedItemContainerAsync(
                vaultName, resourceGroup, subscription, protectedItemName, tenant, retryPolicy, cancellationToken);
            containerName = resolvedItem.ContainerName;
            protectedItemName = resolvedItem.Name;
        }

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var itemId = BackupProtectedItemResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, FabricName, containerName!, protectedItemName);
        var itemResource = armClient.GetBackupProtectedItemResource(itemId);
        var collection = itemResource.GetBackupRecoveryPoints();

        var points = new List<RecoveryPointInfo>();
        await foreach (var rp in collection.GetAllAsync(cancellationToken: cancellationToken))
        {
            points.Add(MapToRecoveryPointInfo(rp.Data));
        }

        return points;
    }

    /// <summary>
    /// Resolves the container name and internal protected item name for an RSV protected item.
    /// When the user provides a friendly name (e.g., "mcp-test-vm"), this searches the protected
    /// items list to find the matching item with its container information.
    /// </summary>
    private async Task<ProtectedItemInfo> ResolveProtectedItemContainerAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        var items = await ListProtectedItemsAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
        var found = items.FirstOrDefault(i =>
            i.Name.Equals(protectedItemName, StringComparison.OrdinalIgnoreCase) ||
            MatchesFriendlyName(i, protectedItemName));

        if (found is null || string.IsNullOrEmpty(found.ContainerName))
        {
            throw new ArgumentException(
                $"Could not resolve container for protected item '{protectedItemName}' in vault '{vaultName}'. " +
                "Provide --container explicitly (format: IaasVMContainer;iaasvmcontainerv2;{rg};{name}), " +
                "or use the full internal name from 'azurebackup protecteditem get' list output.");
        }

        return found;
    }


    public async Task<OperationResult> UpdateVaultAsync(
        string vaultName, string resourceGroup, string subscription,
        string? redundancy, string? softDelete, string? softDeleteRetentionDays,
        string? immutabilityState, string? identityType, string? tags,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var vaultId = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultId);
        var vault = await vaultResource.GetAsync(cancellationToken);

        var patchData = new RecoveryServicesVaultPatch(vault.Value.Data.Location);

        if (!string.IsNullOrEmpty(identityType))
        {
            patchData.Identity = new Azure.ResourceManager.Models.ManagedServiceIdentity(
                ParseIdentityType(identityType));
        }

        if (!string.IsNullOrEmpty(tags))
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(tags);
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    patchData.Tags[prop.Name] = prop.Value.GetString() ?? string.Empty;
                }
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw new ArgumentException($"Invalid JSON format for --tags. Expected a JSON object like '{{\"key\":\"value\"}}'. Details: {ex.Message}", ex);
            }
        }

        await vaultResource.UpdateAsync(WaitUntil.Completed, patchData, cancellationToken);

        // RSV storage redundancy is managed via the BackupResourceStorageConfig API,
        // not the vault patch endpoint.
        if (!string.IsNullOrEmpty(redundancy))
        {
            await ConfigureStorageRedundancyAsync(armClient, vaultName, resourceGroup, subscription, redundancy, cancellationToken);
        }

        // Delegate soft delete and immutability to their dedicated methods for RSV vaults,
        // since RSV vault patch only supports identity and tag updates.
        if (!string.IsNullOrEmpty(softDelete))
        {
            await ConfigureSoftDeleteAsync(vaultName, resourceGroup, subscription, softDelete, softDeleteRetentionDays, tenant, retryPolicy, cancellationToken);
        }

        if (!string.IsNullOrEmpty(immutabilityState))
        {
            await ConfigureImmutabilityAsync(vaultName, resourceGroup, subscription, immutabilityState, tenant, retryPolicy, cancellationToken);
        }

        return new OperationResult("Succeeded", null, $"Vault '{vaultName}' updated successfully.");
    }

    private static async Task ConfigureStorageRedundancyAsync(
        ArmClient armClient, string vaultName, string resourceGroup,
        string subscription, string redundancy, CancellationToken cancellationToken)
    {
        var configResourceId = BackupResourceConfigResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var configResource = armClient.GetBackupResourceConfigResource(configResourceId);
        var currentConfig = await configResource.GetAsync(cancellationToken);

        var data = currentConfig.Value.Data;
        data.Properties.StorageModelType = new BackupStorageType(redundancy);

        var rgId = ResourceGroupResource.CreateResourceIdentifier(subscription, resourceGroup);
        var rgResource = armClient.GetResourceGroupResource(rgId);
        var collection = rgResource.GetBackupResourceConfigs();
        await collection.CreateOrUpdateAsync(WaitUntil.Completed, vaultName, data, cancellationToken);
    }

    public async Task<OperationResult> CreatePolicyAsync(
        string vaultName, string resourceGroup, string subscription,
        string policyName, string workloadType,
        string? scheduleFrequency, string? scheduleTime,
        string? dailyRetentionDays, string? weeklyRetentionWeeks,
        string? monthlyRetentionMonths, string? yearlyRetentionYears,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(policyName), policyName),
            (nameof(workloadType), workloadType));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var vaultResourceId = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultResourceId);
        var vault = await vaultResource.GetAsync(cancellationToken);
        var vaultLocation = vault.Value.Data.Location;

        var rgId = ResourceGroupResource.CreateResourceIdentifier(subscription, resourceGroup);
        var rgResource = armClient.GetResourceGroupResource(rgId);
        var policyCollection = rgResource.GetBackupProtectionPolicies(vaultName);

        var retentionDays = int.TryParse(dailyRetentionDays, out var dd) ? dd : 30;

        // Stage 2 TODO: Multi-tier retention (--weekly-retention-weeks, --monthly-retention-months, --yearly-retention-years)
        // Replace the current DailyRetentionSchedule-only approach with a full LongTermRetentionPolicy:
        //   - DailySchedule: always present, using retentionDays.
        //   - WeeklySchedule: if weeklyRetentionWeeks > 0, create WeeklyRetentionSchedule
        //     with DaysOfTheWeek=[Sunday], RetentionDuration={Count=N, DurationType=Weeks}.
        //   - MonthlySchedule: if monthlyRetentionMonths > 0, create MonthlyRetentionSchedule
        //     with RetentionScheduleFormatType=Weekly, WeeksOfTheMonth=[First], DaysOfTheWeek=[Sunday].
        //   - YearlySchedule: if yearlyRetentionYears > 0, create YearlyRetentionSchedule
        //     with MonthsOfYear=[January], same weekly format as monthly.
        // For VmWorkload: multi-tier retention goes on the Full sub-policy only.
        // The weeklyRetentionWeeks/monthlyRetentionMonths/yearlyRetentionYears params are
        // accepted but not yet wired up.

        var scheduleDateTime = DateTimeOffset.TryParse(scheduleTime, out var st) ? st : new DateTimeOffset(DateTime.UtcNow.Date.AddHours(2), TimeSpan.Zero);
        var scheduleRunTime = new DateTimeOffset(scheduleDateTime.Year, scheduleDateTime.Month, scheduleDateTime.Day,
            scheduleDateTime.Hour, scheduleDateTime.Minute, 0, TimeSpan.Zero);

        BackupGenericProtectionPolicy policyProperties;

        var profile = RsvDatasourceRegistry.ResolveOrDefault(workloadType);

        if (profile.PolicyType == RsvPolicyType.VmWorkload)
        {
            var fullSchedule = new SimpleSchedulePolicy { ScheduleRunFrequency = ScheduleRunType.Daily };
            fullSchedule.ScheduleRunTimes.Add(scheduleRunTime);

            var fullRetention = new DailyRetentionSchedule { RetentionDuration = new RetentionDuration { Count = retentionDays, DurationType = RetentionDurationType.Days } };
            fullRetention.RetentionTimes.Add(scheduleRunTime);

            var fullSubPolicy = new SubProtectionPolicy
            {
                PolicyType = new SubProtectionPolicyType("Full"),
                SchedulePolicy = fullSchedule,
                RetentionPolicy = new LongTermRetentionPolicy { DailySchedule = fullRetention }
            };

            var logSubPolicy = new SubProtectionPolicy
            {
                PolicyType = new SubProtectionPolicyType("Log"),
                SchedulePolicy = new LogSchedulePolicy { ScheduleFrequencyInMins = 60 },
                RetentionPolicy = new SimpleRetentionPolicy { RetentionDuration = new RetentionDuration { Count = 15, DurationType = RetentionDurationType.Days } }
            };

            var wlPolicy = new VmWorkloadProtectionPolicy
            {
                WorkLoadType = new BackupWorkloadType(profile.ApiWorkloadType ?? "SQLDataBase"),
                Settings = new BackupCommonSettings
                {
                    TimeZone = "UTC",
                    IsCompression = false,
                    IsSqlCompression = false
                }
            };
            wlPolicy.SubProtectionPolicy.Add(fullSubPolicy);
            wlPolicy.SubProtectionPolicy.Add(logSubPolicy);

            policyProperties = wlPolicy;
        }
        else if (profile.PolicyType == RsvPolicyType.AzureFileShare)
        {
            var schedulePolicy = new SimpleSchedulePolicy { ScheduleRunFrequency = ScheduleRunType.Daily };
            schedulePolicy.ScheduleRunTimes.Add(scheduleRunTime);

            var dailyRetention = new DailyRetentionSchedule { RetentionDuration = new RetentionDuration { Count = retentionDays, DurationType = RetentionDurationType.Days } };
            dailyRetention.RetentionTimes.Add(scheduleRunTime);

            policyProperties = new FileShareProtectionPolicy
            {
                WorkLoadType = new BackupWorkloadType("AzureFileShare"),
                SchedulePolicy = schedulePolicy,
                RetentionPolicy = new LongTermRetentionPolicy { DailySchedule = dailyRetention }
            };
        }
        else
        {
            var schedulePolicy = new SimpleSchedulePolicy { ScheduleRunFrequency = ScheduleRunType.Daily };
            schedulePolicy.ScheduleRunTimes.Add(scheduleRunTime);

            var dailyRetention = new DailyRetentionSchedule { RetentionDuration = new RetentionDuration { Count = retentionDays, DurationType = RetentionDurationType.Days } };
            dailyRetention.RetentionTimes.Add(scheduleRunTime);

            policyProperties = new IaasVmProtectionPolicy
            {
                SchedulePolicy = schedulePolicy,
                RetentionPolicy = new LongTermRetentionPolicy { DailySchedule = dailyRetention }
            };
        }

        var policyData = new BackupProtectionPolicyData(vaultLocation)
        {
            Properties = policyProperties
        };

        await policyCollection.CreateOrUpdateAsync(WaitUntil.Completed, policyName, policyData, cancellationToken);

        return new OperationResult("Succeeded", null, $"Policy '{policyName}' created in vault '{vaultName}'.");
    }

    public async Task<OperationResult> ConfigureImmutabilityAsync(
        string vaultName, string resourceGroup, string subscription,
        string immutabilityState, string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(immutabilityState), immutabilityState));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var vaultId = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultId);
        var vault = await vaultResource.GetAsync(cancellationToken);

        var patchData = new RecoveryServicesVaultPatch(vault.Value.Data.Location)
        {
            Properties = new RecoveryServicesVaultProperties
            {
                SecuritySettings = new RecoveryServicesSecuritySettings
                {
                    ImmutabilityState = new ImmutabilityState(immutabilityState)
                }
            }
        };
        await vaultResource.UpdateAsync(WaitUntil.Completed, patchData, cancellationToken);

        return new OperationResult("Succeeded", null, $"Immutability set to '{immutabilityState}' for vault '{vaultName}'");
    }

    public async Task<OperationResult> ConfigureSoftDeleteAsync(
        string vaultName, string resourceGroup, string subscription,
        string softDeleteState, string? softDeleteRetentionDays,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(softDeleteState), softDeleteState));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

        // RSV soft-delete must be configured via the BackupResourceVaultConfig API,
        // not the vault PATCH endpoint. Using vault PATCH returns 500 CloudInternalError.
        var configResourceId = BackupResourceVaultConfigResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName);
        var configResource = armClient.GetBackupResourceVaultConfigResource(configResourceId);
        var currentConfig = await configResource.GetAsync(cancellationToken);

        var configData = currentConfig.Value.Data;
        configData.Properties.EnhancedSecurityState = EnhancedSecurityState.Enabled;

        // Map user-facing values (On/Off/AlwaysOn) to RSV API values (Enabled/Disabled/AlwaysON)
        var rsvSoftDeleteState = softDeleteState.ToUpperInvariant() switch
        {
            "ON" => SoftDeleteFeatureState.Enabled,
            "OFF" => SoftDeleteFeatureState.Disabled,
            "ALWAYSON" => new SoftDeleteFeatureState("AlwaysON"),
            _ => new SoftDeleteFeatureState(softDeleteState)
        };
        configData.Properties.SoftDeleteFeatureState = rsvSoftDeleteState;

        if (int.TryParse(softDeleteRetentionDays, out var retentionDays))
        {
            configData.Properties.SoftDeleteRetentionPeriodInDays = retentionDays;
        }

        var rgId = ResourceGroupResource.CreateResourceIdentifier(subscription, resourceGroup);
        var rgResource = armClient.GetResourceGroupResource(rgId);
        var collection = rgResource.GetBackupResourceVaultConfigs();
        await collection.CreateOrUpdateAsync(WaitUntil.Completed, vaultName, configData, cancellationToken);

        return new OperationResult("Succeeded", null, $"Soft delete set to '{softDeleteState}' for vault '{vaultName}'");
    }

    public async Task<OperationResult> ConfigureCrossRegionRestoreAsync(
        string vaultName, string resourceGroup, string subscription,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

        var configResourceId = BackupResourceConfigResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var configResource = armClient.GetBackupResourceConfigResource(configResourceId);
        var currentConfig = await configResource.GetAsync(cancellationToken);

        var data = currentConfig.Value.Data;
        data.Properties.EnableCrossRegionRestore = true;

        var rgId = ResourceGroupResource.CreateResourceIdentifier(subscription, resourceGroup);
        var rgResource = armClient.GetResourceGroupResource(rgId);
        var collection = rgResource.GetBackupResourceConfigs();
        await collection.CreateOrUpdateAsync(WaitUntil.Completed, vaultName, data, cancellationToken);

        return new OperationResult("Succeeded", null, $"Cross-Region Restore enabled for vault '{vaultName}'.");
    }

    private static Azure.ResourceManager.Models.ManagedServiceIdentityType ParseIdentityType(string identityType) =>
        identityType.ToUpperInvariant() switch
        {
            "SYSTEMASSIGNED" => Azure.ResourceManager.Models.ManagedServiceIdentityType.SystemAssigned,
            "USERASSIGNED" => Azure.ResourceManager.Models.ManagedServiceIdentityType.UserAssigned,
            "SYSTEMASSIGNED,USERASSIGNED" or "SYSTEMASSIGNEDUSERASSIGNED"
                => Azure.ResourceManager.Models.ManagedServiceIdentityType.SystemAssignedUserAssigned,
            "NONE" => Azure.ResourceManager.Models.ManagedServiceIdentityType.None,
            _ => throw new ArgumentException(
                $"Invalid identity type '{identityType}'. Supported values: 'SystemAssigned', 'UserAssigned', 'SystemAssigned,UserAssigned', 'None'.")
        };

    private static BackupVaultInfo MapToVaultInfo(RecoveryServicesVaultData data, string? resourceGroup)
    {
        var securitySettings = data.Properties?.SecuritySettings;
        var softDeleteSettings = securitySettings?.SoftDeleteSettings;
        var immutabilityState = securitySettings?.ImmutabilityState?.ToString();
        var identityType = data.Identity?.ManagedServiceIdentityType.ToString();

        return new BackupVaultInfo(
            data.Id?.ToString(),
            data.Name,
            VaultType,
            data.Location.Name,
            resourceGroup,
            data.Properties?.ProvisioningState,
            data.Sku?.Name.ToString(),
            null,
            data.Properties?.RedundancySettings?.StandardTierStorageRedundancy?.ToString(),
            softDeleteSettings?.SoftDeleteState?.ToString(),
            softDeleteSettings?.SoftDeleteRetentionPeriodInDays,
            immutabilityState,
            identityType,
            data.Tags?.ToDictionary(t => t.Key, t => t.Value));
    }

    private static ProtectedItemInfo MapToProtectedItemInfo(BackupProtectedItemData data)
    {
        string? protectionStatus = null;
        string? datasourceType = null;
        string? datasourceId = null;
        string? policyName = null;
        DateTimeOffset? lastBackupTime = null;
        string? container = null;

        if (data.Properties is BackupGenericProtectedItem genericItem)
        {
            datasourceType = genericItem.WorkloadType?.ToString();
            datasourceId = genericItem.SourceResourceId?.ToString();
            policyName = genericItem.PolicyId?.Name;
            container = genericItem.ContainerName;

            if (genericItem is IaasVmProtectedItem vmItem)
            {
                protectionStatus = vmItem.ProtectionState?.ToString();
                lastBackupTime = vmItem.LastBackupOn;
            }
            else if (genericItem is VmWorkloadProtectedItem workloadItem)
            {
                protectionStatus = workloadItem.ProtectionState?.ToString();
                lastBackupTime = workloadItem.LastBackupOn;
                datasourceType = workloadItem.WorkloadType?.ToString();
            }
        }

        return new ProtectedItemInfo(
            data.Id?.ToString(),
            data.Name,
            VaultType,
            protectionStatus,
            datasourceType,
            datasourceId,
            policyName,
            lastBackupTime,
            container);
    }

    private static BackupPolicyInfo MapToPolicyInfo(BackupProtectionPolicyData data)
    {
        string? workloadType = null;
        int? protectedItemsCount = null;
        string? scheduleFrequency = null;
        string? scheduleTime = null;
        int? dailyRetentionDays = null;

        if (data.Properties is BackupGenericProtectionPolicy genericPolicy)
        {
            protectedItemsCount = genericPolicy.ProtectedItemsCount;

            if (genericPolicy is IaasVmProtectionPolicy vmPolicy)
            {
                workloadType = "AzureIaasVM";
                if (vmPolicy.SchedulePolicy is SimpleSchedulePolicy simpleSchedule)
                {
                    scheduleFrequency = simpleSchedule.ScheduleRunFrequency?.ToString();
                    var firstRunTime = simpleSchedule.ScheduleRunTimes?.Count > 0 ? simpleSchedule.ScheduleRunTimes[0] : (DateTimeOffset?)null;
                    scheduleTime = firstRunTime?.ToString("HH:mm");
                }

                if (vmPolicy.RetentionPolicy is LongTermRetentionPolicy longTermRetention)
                {
                    dailyRetentionDays = longTermRetention.DailySchedule?.RetentionDuration?.Count;
                }
            }
            else if (genericPolicy is FileShareProtectionPolicy fsPolicy)
            {
                workloadType = "AzureFileShare";
                if (fsPolicy.SchedulePolicy is SimpleSchedulePolicy fsSchedule)
                {
                    scheduleFrequency = fsSchedule.ScheduleRunFrequency?.ToString();
                    var firstRunTime = fsSchedule.ScheduleRunTimes?.Count > 0 ? fsSchedule.ScheduleRunTimes[0] : (DateTimeOffset?)null;
                    scheduleTime = firstRunTime?.ToString("HH:mm");
                }

                if (fsPolicy.RetentionPolicy is LongTermRetentionPolicy fsRetention)
                {
                    dailyRetentionDays = fsRetention.DailySchedule?.RetentionDuration?.Count;
                }
            }
            else if (genericPolicy is VmWorkloadProtectionPolicy wlPolicy)
            {
                workloadType = wlPolicy.WorkLoadType?.ToString();
                var fullSubPolicy = wlPolicy.SubProtectionPolicy?.FirstOrDefault(
                    s => string.Equals(s.PolicyType?.ToString(), "Full", StringComparison.OrdinalIgnoreCase));
                if (fullSubPolicy?.SchedulePolicy is SimpleSchedulePolicy wlSchedule)
                {
                    scheduleFrequency = wlSchedule.ScheduleRunFrequency?.ToString();
                    var firstRunTime = wlSchedule.ScheduleRunTimes?.Count > 0 ? wlSchedule.ScheduleRunTimes[0] : (DateTimeOffset?)null;
                    scheduleTime = firstRunTime?.ToString("HH:mm");
                }

                if (fullSubPolicy?.RetentionPolicy is LongTermRetentionPolicy wlRetention)
                {
                    dailyRetentionDays = wlRetention.DailySchedule?.RetentionDuration?.Count;
                }
            }
        }

        return new BackupPolicyInfo(
            data.Id?.ToString(),
            data.Name,
            VaultType,
            workloadType != null ? [workloadType] : null,
            protectedItemsCount,
            scheduleFrequency,
            scheduleTime,
            dailyRetentionDays);
    }

    private static BackupJobInfo MapToJobInfo(BackupJobData data)
    {
        string? operation = null;
        string? status = null;
        DateTimeOffset? startTime = null;
        DateTimeOffset? endTime = null;
        string? entityFriendlyName = null;

        if (data.Properties is BackupGenericJob genericJob)
        {
            operation = genericJob.Operation;
            status = genericJob.Status;
            startTime = genericJob.StartOn;
            endTime = genericJob.EndOn;
            entityFriendlyName = genericJob.EntityFriendlyName;
        }

        return new BackupJobInfo(
            data.Id?.ToString(),
            data.Name,
            VaultType,
            operation,
            status,
            startTime,
            endTime,
            null,
            entityFriendlyName);
    }

    private static RecoveryPointInfo MapToRecoveryPointInfo(BackupRecoveryPointData data)
    {
        DateTimeOffset? rpTime = null;
        string? rpType = null;

        if (data.Properties is IaasVmRecoveryPoint vmRp)
        {
            rpType = vmRp.RecoveryPointType;
            rpTime = vmRp.RecoveryPointOn;
        }
        else if (data.Properties is WorkloadRecoveryPoint workloadRp)
        {
            rpType = workloadRp.RestorePointType?.ToString();
            rpTime = workloadRp.RecoveryPointCreatedOn;
        }
        else if (data.Properties is GenericRecoveryPoint genRp)
        {
            rpType = genRp.RecoveryPointType;
            rpTime = genRp.RecoveryPointOn;
        }

        return new RecoveryPointInfo(
            data.Id?.ToString(),
            data.Name,
            VaultType,
            rpTime,
            rpType);
    }

    private static string? ExtractOperationIdFromResponse(Response response)
    {
        if (response.Headers.TryGetValue("Azure-AsyncOperation", out var asyncOpUrl) && !string.IsNullOrEmpty(asyncOpUrl))
        {
            var uri = new Uri(asyncOpUrl);
            var segments = uri.AbsolutePath.Split('/');
            return segments.Length > 0 ? segments[^1] : null;
        }

        return null;
    }

    private static async Task<string?> FindLatestJobIdAsync(
        ArmClient armClient, string subscription, string resourceGroup,
        string vaultName, string operationType, CancellationToken cancellationToken)
    {
        var rgId = ResourceGroupResource.CreateResourceIdentifier(subscription, resourceGroup);
        var rgResource = armClient.GetResourceGroupResource(rgId);
        var jobCollection = rgResource.GetBackupJobs(vaultName);

        await foreach (var job in jobCollection.GetAllAsync(cancellationToken: cancellationToken))
        {
            if (job.Data.Properties is BackupGenericJob genericJob)
            {
                if (genericJob.StartOn.HasValue &&
                    genericJob.StartOn.Value > DateTimeOffset.UtcNow.AddMinutes(-2) &&
                    string.Equals(genericJob.Operation, operationType, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(genericJob.Status, "InProgress", StringComparison.OrdinalIgnoreCase))
                {
                    return job.Data.Name;
                }
            }
        }

        return null;
    }

    public async Task<List<ProtectableItemInfo>> ListProtectableItemsAsync(
        string vaultName, string resourceGroup, string subscription,
        string? workloadType, string? containerName, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

        var rgId = ResourceGroupResource.CreateResourceIdentifier(subscription, resourceGroup);
        var rgResource = armClient.GetResourceGroupResource(rgId);

        string filter;
        if (!string.IsNullOrEmpty(workloadType))
        {
            var normalizedType = NormalizeWorkloadTypeForFilter(workloadType);
            filter = $"backupManagementType eq 'AzureWorkload' and workloadType eq '{normalizedType}'";
        }
        else
        {
            filter = "backupManagementType eq 'AzureWorkload'";
        }

        var items = new List<ProtectableItemInfo>();
        await foreach (var item in rgResource.GetBackupProtectableItemsAsync(vaultName, filter: filter, cancellationToken: cancellationToken))
        {
            items.Add(MapToProtectableItemInfo(item));
        }

        return items;
    }

    /// <summary>
    /// Normalizes user-provided workload type values to the API filter format.
    /// The REST API filter expects specific types like "SAPHanaDatabase" but users
    /// commonly pass "SAPHana" (which is what the API returns in workloadType fields).
    /// Validates input against known workload types to prevent OData injection.
    /// </summary>
    private static string NormalizeWorkloadTypeForFilter(string workloadType)
    {
        var normalized = workloadType.ToUpperInvariant() switch
        {
            "SAPHANA" => "SAPHanaDatabase",
            "SAPHANASYSTEM" => "SAPHanaSystem",
            "SAPHANADBINSTANCE" or "SAPHANADBI" => "SAPHanaDBInstance",
            "SQL" => "SQLDataBase",
            "SQLINSTANCE" => "SQLInstance",
            "SAPHANADATABASE" => "SAPHanaDatabase",
            "SQLDATABASE" => "SQLDataBase",
            _ => (string?)null
        };

        if (normalized is null)
        {
            throw new ArgumentException(
                $"Unknown workload type '{workloadType}'. Supported values: SQL, SQLInstance, SAPHana, SAPHanaSystem, SAPHanaDBInstance, SQLDataBase, SAPHanaDatabase.");
        }

        return normalized;
    }

    private static ProtectableItemInfo MapToProtectableItemInfo(WorkloadProtectableItemResource data)
    {
        string? protectableItemType = null;
        string? workloadType = null;
        string? friendlyName = null;
        string? serverName = null;
        string? parentName = null;
        string? protectionState = null;
        string? containerName = null;

        if (data.Properties is WorkloadProtectableItem workloadItem)
        {
            protectableItemType = workloadItem switch
            {
                VmWorkloadSqlDatabaseProtectableItem => "SQLDataBase",
                VmWorkloadSapHanaDatabaseProtectableItem => "SAPHanaDatabase",
                VmWorkloadSqlInstanceProtectableItem => "SQLInstance",
                VmWorkloadSapHanaSystemProtectableItem => "SAPHanaSystem",
                _ => workloadItem.GetType().Name
            };
            workloadType = workloadItem.WorkloadType;
            friendlyName = workloadItem.FriendlyName;
            protectionState = workloadItem.ProtectionState?.ToString();

            if (workloadItem is VmWorkloadSqlDatabaseProtectableItem sqlDb)
            {
                serverName = sqlDb.ServerName;
                parentName = sqlDb.ParentName;
            }
            else if (workloadItem is VmWorkloadSapHanaDatabaseProtectableItem hanaDb)
            {
                serverName = hanaDb.ServerName;
                parentName = hanaDb.ParentName;
            }
        }

        return new ProtectableItemInfo(
            data.Id?.ToString(),
            data.Name,
            protectableItemType,
            workloadType,
            friendlyName,
            serverName,
            parentName,
            protectionState,
            containerName);
    }
}

internal static class RsvNamingHelper
{
    public static string DeriveContainerName(string datasourceId, string? datasourceType = null)
    {
        var profile = RsvDatasourceRegistry.Resolve(datasourceType);
        if (profile?.IsWorkloadType == true)
        {
            var resourceId = new ResourceIdentifier(datasourceId);
            return $"{profile.ContainerNamePrefix};{resourceId.ResourceGroupName};{resourceId.Name}";
        }

        var vmResourceId = new ResourceIdentifier(datasourceId);

        if (profile?.ProtectedItemType == RsvProtectedItemType.AzureFileShare)
        {
            return $"StorageContainer;Storage;{vmResourceId.ResourceGroupName};{ExtractStorageAccountName(vmResourceId)}";
        }

        var resourceType = vmResourceId.ResourceType.Type;

        return resourceType.ToLowerInvariant() switch
        {
            "virtualmachines" => $"IaasVMContainer;iaasvmcontainerv2;{vmResourceId.ResourceGroupName};{vmResourceId.Name}",
            "storageaccounts" => $"StorageContainer;Storage;{vmResourceId.ResourceGroupName};{vmResourceId.Name}",
            _ => $"GenericContainer;{vmResourceId.ResourceGroupName};{vmResourceId.Name}"
        };
    }

    public static string DeriveProtectedItemName(string datasourceId, string? datasourceType = null)
    {
        var profile = RsvDatasourceRegistry.Resolve(datasourceType);
        if (profile?.IsWorkloadType == true)
        {
            return datasourceId;
        }

        var resourceId = new ResourceIdentifier(datasourceId);

        if (profile?.ProtectedItemType == RsvProtectedItemType.AzureFileShare)
        {
            return $"AzureFileShare;{resourceId.Name}";
        }

        var resourceType = resourceId.ResourceType.Type;

        return resourceType.ToLowerInvariant() switch
        {
            "virtualmachines" => $"VM;iaasvmcontainerv2;{resourceId.ResourceGroupName};{resourceId.Name}",
            "storageaccounts" => $"AzureFileShare;{resourceId.Name}",
            _ => $"GenericProtectedItem;{resourceId.ResourceGroupName};{resourceId.Name}"
        };
    }

    private static string ExtractStorageAccountName(ResourceIdentifier resourceId)
    {
        ResourceIdentifier? current = resourceId;
        while (current is not null)
        {
            if (string.Equals(current.ResourceType.Type, "storageAccounts", StringComparison.OrdinalIgnoreCase))
            {
                return current.Name;
            }

            current = current.Parent;
        }

        return resourceId.Name;
    }

    public static string GetStorageAccountId(ResourceIdentifier resourceId)
    {
        ResourceIdentifier? current = resourceId;
        while (current is not null)
        {
            if (string.Equals(current.ResourceType.Type, "storageAccounts", StringComparison.OrdinalIgnoreCase))
            {
                return current.ToString();
            }

            current = current.Parent;
        }

        return resourceId.ToString();
    }
}
