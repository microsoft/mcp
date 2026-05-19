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

            return await BuildRsvProtectResultAsync(
                armClient, subscription, resourceGroup, vaultName, protectedItemName, jobId,
                "Workload protection", cancellationToken);
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

            return await BuildRsvProtectResultAsync(
                armClient, subscription, resourceGroup, vaultName, fsProtectedItemName, fsJobId,
                "File share protection", cancellationToken);
        }

        // For IaaS VM protection MCP follows the same approach as `az backup protection enable-for-vm`:
        // submit the protected-item PUT directly. The Recovery Services backend registers the
        // VM container as part of accepting the protect request, so a separate refresh +
        // discovery poll is unnecessary and was causing 180s timeouts on freshly created VMs.
        var container = containerName ?? RsvNamingHelper.DeriveContainerName(datasourceId);
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

        return await BuildRsvProtectResultAsync(
            armClient, subscription, resourceGroup, vaultName, vmProtectedItemName, vmJobId,
            "VM protection", cancellationToken);
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
                (!string.IsNullOrEmpty(i.Name) && i.Name.Equals(protectedItemName, StringComparison.OrdinalIgnoreCase)) ||
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
        if (!string.IsNullOrEmpty(item.Name))
        {
            var nameParts = item.Name.Split(';');
            if (nameParts.Length > 0 &&
                string.Equals(nameParts[^1], friendlyName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
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
            (!string.IsNullOrEmpty(i.Name) && i.Name.Equals(protectedItemName, StringComparison.OrdinalIgnoreCase)) ||
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
        Policy.PolicyCreateRequest request,
        string vaultName, string resourceGroup, string subscription,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var policyName = request.Policy;
        var workloadType = request.WorkloadType;

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

        var policyProperties = Policy.RsvPolicyBuilder.Build(request);

        var policyData = new BackupProtectionPolicyData(vaultLocation)
        {
            Properties = policyProperties
        };

        // --policy-tags maps to ARM resource tags on the policy (RSV only).
        ApplyPolicyTags(policyData.Tags, request.PolicyTags);

        await policyCollection.CreateOrUpdateAsync(WaitUntil.Completed, policyName, policyData, cancellationToken);

        return new OperationResult("Succeeded", null, $"Policy '{policyName}' created in vault '{vaultName}'.");
    }

    private static void ApplyPolicyTags(IDictionary<string, string> destination, string? tagsCsv)
    {
        if (string.IsNullOrWhiteSpace(tagsCsv))
        {
            return;
        }

        foreach (var pair in tagsCsv!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var idx = pair.IndexOf('=');
            if (idx <= 0 || idx == pair.Length - 1)
            {
                continue;
            }

            var key = pair[..idx].Trim();
            var value = pair[(idx + 1)..].Trim();
            if (key.Length > 0)
            {
                destination[key] = value;
            }
        }
    }

    public async Task<OperationResult> UpdatePolicyAsync(
        string vaultName, string resourceGroup, string subscription,
        string policyName,
        string? scheduleTime, string? dailyRetentionDays,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(policyName), policyName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var rgId = ResourceGroupResource.CreateResourceIdentifier(subscription, resourceGroup);
        var rgResource = armClient.GetResourceGroupResource(rgId);
        var policyCollection = rgResource.GetBackupProtectionPolicies(vaultName);

        var existingPolicy = await policyCollection.GetAsync(policyName, cancellationToken);
        var policyData = existingPolicy.Value.Data;
        var policyProperties = policyData.Properties as BackupGenericProtectionPolicy
            ?? throw new InvalidOperationException($"Policy '{policyName}' has an unsupported properties type.");

        DateTimeOffset? newScheduleTime = null;
        if (!string.IsNullOrWhiteSpace(scheduleTime))
        {
            if (!DateTimeOffset.TryParse(scheduleTime, out var st))
            {
                throw new ArgumentException($"Invalid schedule time '{scheduleTime}'. Provide a valid time in UTC HH:mm format (e.g., '04:00').");
            }
            newScheduleTime = st;
        }

        int? newRetentionDays = null;
        if (!string.IsNullOrWhiteSpace(dailyRetentionDays))
        {
            if (!int.TryParse(dailyRetentionDays, out var dd) || dd <= 0)
            {
                throw new ArgumentException($"Invalid daily retention days '{dailyRetentionDays}'. Provide a positive integer.");
            }
            newRetentionDays = dd;
        }

        if (newScheduleTime is null && newRetentionDays is null)
        {
            return new OperationResult("Succeeded", null, $"No changes specified for policy '{policyName}'. Policy remains unchanged.");
        }

        UpdatePolicyScheduleAndRetention(policyProperties, newScheduleTime, newRetentionDays);

        await policyCollection.CreateOrUpdateAsync(WaitUntil.Completed, policyName, policyData, cancellationToken);

        return new OperationResult("Succeeded", null, $"Policy '{policyName}' updated in vault '{vaultName}'.");
    }

    private static void UpdatePolicyScheduleAndRetention(BackupGenericProtectionPolicy policyProperties, DateTimeOffset? newScheduleTime, int? newRetentionDays)
    {
        bool scheduleApplied = newScheduleTime is null;
        bool retentionApplied = newRetentionDays is null;

        switch (policyProperties)
        {
            case VmWorkloadProtectionPolicy wlPolicy:
                foreach (var subPolicy in wlPolicy.SubProtectionPolicy)
                {
                    if (subPolicy.PolicyType?.ToString() == "Full")
                    {
                        if (newScheduleTime is not null && subPolicy.SchedulePolicy is SimpleSchedulePolicy fullSchedule)
                        {
                            var scheduleRunTime = NormalizeScheduleTime(newScheduleTime.Value);
                            fullSchedule.ScheduleRunTimes.Clear();
                            fullSchedule.ScheduleRunTimes.Add(scheduleRunTime);
                            scheduleApplied = true;
                        }

                        if (newRetentionDays is not null && subPolicy.RetentionPolicy is LongTermRetentionPolicy fullRetention && fullRetention.DailySchedule is not null)
                        {
                            fullRetention.DailySchedule.RetentionDuration = new RetentionDuration { Count = newRetentionDays.Value, DurationType = RetentionDurationType.Days };
                            if (newScheduleTime is not null)
                            {
                                var scheduleRunTime = NormalizeScheduleTime(newScheduleTime.Value);
                                fullRetention.DailySchedule.RetentionTimes.Clear();
                                fullRetention.DailySchedule.RetentionTimes.Add(scheduleRunTime);
                            }
                            retentionApplied = true;
                        }
                    }
                }
                break;

            case IaasVmProtectionPolicy vmPolicy:
                if (newScheduleTime is not null && vmPolicy.SchedulePolicy is SimpleSchedulePolicy vmSchedule)
                {
                    var scheduleRunTime = NormalizeScheduleTime(newScheduleTime.Value);
                    vmSchedule.ScheduleRunTimes.Clear();
                    vmSchedule.ScheduleRunTimes.Add(scheduleRunTime);
                    scheduleApplied = true;
                }

                if (vmPolicy.RetentionPolicy is LongTermRetentionPolicy vmRetention && vmRetention.DailySchedule is not null)
                {
                    if (newRetentionDays is not null)
                    {
                        vmRetention.DailySchedule.RetentionDuration = new RetentionDuration { Count = newRetentionDays.Value, DurationType = RetentionDurationType.Days };
                        retentionApplied = true;
                    }

                    if (newScheduleTime is not null)
                    {
                        var scheduleRunTime = NormalizeScheduleTime(newScheduleTime.Value);
                        vmRetention.DailySchedule.RetentionTimes.Clear();
                        vmRetention.DailySchedule.RetentionTimes.Add(scheduleRunTime);
                    }
                }
                else if (newRetentionDays is not null)
                {
                    // Retention policy type not supported for update
                }
                break;

            case FileShareProtectionPolicy fsPolicy:
                if (newScheduleTime is not null && fsPolicy.SchedulePolicy is SimpleSchedulePolicy fsSchedule)
                {
                    var scheduleRunTime = NormalizeScheduleTime(newScheduleTime.Value);
                    fsSchedule.ScheduleRunTimes.Clear();
                    fsSchedule.ScheduleRunTimes.Add(scheduleRunTime);
                    scheduleApplied = true;
                }

                if (fsPolicy.RetentionPolicy is LongTermRetentionPolicy fsRetention && fsRetention.DailySchedule is not null)
                {
                    if (newRetentionDays is not null)
                    {
                        fsRetention.DailySchedule.RetentionDuration = new RetentionDuration { Count = newRetentionDays.Value, DurationType = RetentionDurationType.Days };
                        retentionApplied = true;
                    }

                    if (newScheduleTime is not null)
                    {
                        var scheduleRunTime = NormalizeScheduleTime(newScheduleTime.Value);
                        fsRetention.DailySchedule.RetentionTimes.Clear();
                        fsRetention.DailySchedule.RetentionTimes.Add(scheduleRunTime);
                    }
                }
                else if (newRetentionDays is not null)
                {
                    // Retention policy type not supported for update
                }
                break;

            default:
                throw new InvalidOperationException($"Unsupported policy type '{policyProperties.GetType().Name}'. Only IaasVM, VmWorkload (SQL/HANA), and FileShare policies are supported for update.");
        }

        if (!scheduleApplied)
        {
            throw new InvalidOperationException(
                $"Schedule update could not be applied. Policy uses '{policyProperties.GetType().Name}' with a schedule type that is not supported for update. Only SimpleSchedulePolicy is supported.");
        }

        if (!retentionApplied)
        {
            throw new InvalidOperationException(
                $"Retention update could not be applied. Policy uses '{policyProperties.GetType().Name}' with a retention type that is not supported for update. Only LongTermRetentionPolicy with a daily schedule is supported.");
        }
    }

    private static DateTimeOffset NormalizeScheduleTime(DateTimeOffset input) =>
        new(input.Year, input.Month, input.Day, input.Hour, input.Minute, 0, TimeSpan.Zero);

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
        var vaultId = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultId);
        var vault = await vaultResource.GetAsync(cancellationToken);

        var rsvSoftDeleteState = softDeleteState.ToUpperInvariant() switch
        {
            "ON" => RecoveryServicesSoftDeleteState.Enabled,
            "OFF" => RecoveryServicesSoftDeleteState.Disabled,
            "ALWAYSON" => RecoveryServicesSoftDeleteState.AlwaysON,
            _ => new RecoveryServicesSoftDeleteState(softDeleteState)
        };

        var softDeleteSettings = new RecoveryServicesSoftDeleteSettings()
        {
            SoftDeleteState = rsvSoftDeleteState,
        };

        if (int.TryParse(softDeleteRetentionDays, out var retentionDays))
        {
            softDeleteSettings.SoftDeleteRetentionPeriodInDays = retentionDays;
        }

        var patchData = new RecoveryServicesVaultPatch(vault.Value.Data.Location)
        {
            Properties = new RecoveryServicesVaultProperties
            {
                SecuritySettings = new RecoveryServicesSecuritySettings
                {
                    SoftDeleteSettings = softDeleteSettings
                }
            }
        };

        await vaultResource.UpdateAsync(WaitUntil.Completed, patchData, cancellationToken);

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
        var vaultId = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultId);
        var vault = await vaultResource.GetAsync(cancellationToken);

        // Check if CRR is already enabled — re-enabling can cause CloudInternalError on some backends.
        if (vault.Value.Data.Properties?.RedundancySettings?.CrossRegionRestore == CrossRegionRestore.Enabled)
        {
            return new OperationResult("Succeeded", null, $"Cross-Region Restore is already enabled for vault '{vaultName}'.");
        }

        // Try legacy BackupResourceConfig API first (backward-compatible), fall back to Vault PATCH
        // if the legacy API returns BMSUserErrorRedundancySettingsUseVaultApi.
        try
        {
            var configResourceId = BackupResourceConfigResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
            var configResource = armClient.GetBackupResourceConfigResource(configResourceId);
            var currentConfig = await configResource.GetAsync(cancellationToken);

            var data = currentConfig.Value.Data;

            // The BackupResourceConfig GET reliably returns the CRR state even when the
            // Vault GET RedundancySettings.CrossRegionRestore property is not populated.
            if (data.Properties.EnableCrossRegionRestore == true)
            {
                return new OperationResult("Succeeded", null, $"Cross-Region Restore is already enabled for vault '{vaultName}'.");
            }
            data.Properties.EnableCrossRegionRestore = true;

            var rgId = ResourceGroupResource.CreateResourceIdentifier(subscription, resourceGroup);
            var rgResource = armClient.GetResourceGroupResource(rgId);
            var collection = rgResource.GetBackupResourceConfigs();
            await collection.CreateOrUpdateAsync(WaitUntil.Completed, vaultName, data, cancellationToken);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "BMSUserErrorRedundancySettingsUseVaultApi")
        {
            // Legacy API rejected — vault requires Vault PATCH API for redundancy settings.
            var patchData = new RecoveryServicesVaultPatch(vault.Value.Data.Location)
            {
                Properties = new RecoveryServicesVaultProperties
                {
                    RedundancySettings = new VaultPropertiesRedundancySettings
                    {
                        CrossRegionRestore = CrossRegionRestore.Enabled
                    }
                }
            };

            await vaultResource.UpdateAsync(WaitUntil.Completed, patchData, cancellationToken);
        }

        return new OperationResult("Succeeded", null, $"Cross-Region Restore enabled for vault '{vaultName}'.");
    }

    public async Task<OperationResult> ConfigureMultiUserAuthorizationAsync(
        string vaultName, string resourceGroup, string subscription,
        string resourceGuardId, string? tenant, RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(resourceGuardId), resourceGuardId));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

        var rgId = ResourceGroupResource.CreateResourceIdentifier(subscription, resourceGroup);
        var rgResource = armClient.GetResourceGroupResource(rgId);
        var proxyCollection = rgResource.GetResourceGuardProxies(vaultName);

        var proxyData = new ResourceGuardProxyData(default)
        {
            Properties = new ResourceGuardProxyProperties
            {
                ResourceGuardResourceId = new ResourceIdentifier(resourceGuardId)
            }
        };

        await proxyCollection.CreateOrUpdateAsync(
            WaitUntil.Completed,
            "VaultProxy",
            proxyData,
            cancellationToken);

        return new OperationResult("Succeeded", null, $"Multi-User Authorization enabled on vault '{vaultName}' with Resource Guard '{resourceGuardId}'.");
    }

    public async Task<OperationResult> DisableMultiUserAuthorizationAsync(
        string vaultName, string resourceGroup, string subscription,
        string? tenant, RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

        var rgId = ResourceGroupResource.CreateResourceIdentifier(subscription, resourceGroup);
        var rgResource = armClient.GetResourceGroupResource(rgId);

        var proxyResponse = await rgResource.GetResourceGuardProxyAsync(vaultName, "VaultProxy", cancellationToken);
        await proxyResponse.Value.DeleteAsync(WaitUntil.Completed, cancellationToken);

        return new OperationResult("Succeeded", null, $"Multi-User Authorization disabled on vault '{vaultName}'.");
    }


    public async Task<OperationResult> ConfigureEncryptionAsync(
        string vaultName, string resourceGroup, string subscription,
        string keyVaultUri, string keyName, string identityType,
        string? keyVersion, string? userAssignedIdentityId,
        string? tenant, RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(keyVaultUri), keyVaultUri),
            (nameof(keyName), keyName),
            (nameof(identityType), identityType));
        var isSystemAssigned = "SystemAssigned".Equals(identityType, StringComparison.OrdinalIgnoreCase);
        var isUserAssigned = "UserAssigned".Equals(identityType, StringComparison.OrdinalIgnoreCase);
        if (!isSystemAssigned && !isUserAssigned)
        {
            throw new ArgumentException(
                $"Invalid identity type '{identityType}' for CMK encryption. Supported values: 'SystemAssigned', 'UserAssigned'.");
        }

        if (isUserAssigned && string.IsNullOrWhiteSpace(userAssignedIdentityId))
        {
            throw new ArgumentException(
                "The --user-assigned-identity-id parameter is required when --identity-type is 'UserAssigned'.");
        }

        // Build the full key URI: {keyVaultUri}/keys/{keyName}[/{keyVersion}]
        var kvUri = keyVaultUri.TrimEnd('/');
        var keyUriString = string.IsNullOrEmpty(keyVersion)
            ? $"{kvUri}/keys/{keyName}"
            : $"{kvUri}/keys/{keyName}/{keyVersion}";

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var vaultId = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultId);
        var vault = await vaultResource.GetAsync(cancellationToken);

        var kekIdentity = new CmkKekIdentity();
        if (isSystemAssigned)
        {
            kekIdentity.UseSystemAssignedIdentity = true;
        }
        else
        {
            kekIdentity.UseSystemAssignedIdentity = false;
            kekIdentity.UserAssignedIdentity = new ResourceIdentifier(userAssignedIdentityId!);
        }

        var encryption = new VaultPropertiesEncryption
        {
            KeyUri = new Uri(keyUriString),
            KekIdentity = kekIdentity,
            InfrastructureEncryption = Azure.ResourceManager.RecoveryServices.Models.InfrastructureEncryptionState.Enabled
        };

        var patchData = new RecoveryServicesVaultPatch(vault.Value.Data.Location)
        {
            Properties = new RecoveryServicesVaultProperties
            {
                Encryption = encryption
            }
        };

        await vaultResource.UpdateAsync(WaitUntil.Completed, patchData, cancellationToken);

        return new OperationResult("Succeeded", null,
            $"Customer-Managed Key encryption configured on vault '{vaultName}' using key '{keyName}' from '{kvUri}'.");
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

    public async Task<OperationResult> UndeleteProtectedItemAsync(
        string vaultName, string resourceGroup, string subscription,
        string datasourceId, string? containerName, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(datasourceId), datasourceId));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

        // Find the soft-deleted protected item by datasource ID
        var rgId = ResourceGroupResource.CreateResourceIdentifier(subscription, resourceGroup);
        var rgResource = armClient.GetResourceGroupResource(rgId);

        BackupProtectedItemData? matchedItemData = null;

        // For RSV in-guest workloads (SQL/HANA), datasourceId is the protectable item name,
        // not an ARM ID. In that case, --container is required to build the item identifier directly.
        if (!datasourceId.StartsWith("/subscriptions/", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException(
                    $"The --container parameter is required when --datasource-id is a protectable item name ('{datasourceId}'). " +
                    "Use 'azurebackup protectableitem list' to discover containers and item names.");
            }

            // Build the protected item resource ID directly from container + item name
            var directItemId = BackupProtectedItemResource.CreateResourceIdentifier(
                subscription, resourceGroup, vaultName, FabricName, containerName, datasourceId);
            var directItemResource = armClient.GetBackupProtectedItemResource(directItemId);
            var directItem = await directItemResource.GetAsync(cancellationToken: cancellationToken);
            matchedItemData = directItem.Value.Data;
        }
        else if (!string.IsNullOrEmpty(containerName))
        {
            // When --container is provided with an ARM ID, use it for direct lookup
            // to avoid ambiguity (e.g., multiple file shares under one storage account).
            var derivedItemName = RsvNamingHelper.DeriveProtectedItemName(datasourceId);
            var directItemId = BackupProtectedItemResource.CreateResourceIdentifier(
                subscription, resourceGroup, vaultName, FabricName, containerName, derivedItemName);
            var directItemResource = armClient.GetBackupProtectedItemResource(directItemId);
            var directItem = await directItemResource.GetAsync(cancellationToken: cancellationToken);
            matchedItemData = directItem.Value.Data;
        }
        else
        {
            // ARM ID path without --container: list all protected items and match by SourceResourceId.
            // Prefer exact matches; only allow prefix matches when unambiguous.
            var exactMatches = new List<BackupProtectedItemData>();
            var prefixMatches = new List<BackupProtectedItemData>();
            await foreach (var item in rgResource.GetBackupProtectedItemsAsync(vaultName, cancellationToken: cancellationToken))
            {
                if (item.Data.Properties is BackupGenericProtectedItem genericItem)
                {
                    var sourceId = genericItem.SourceResourceId?.ToString();
                    if (sourceId is null)
                    {
                        continue;
                    }

                    if (string.Equals(sourceId, datasourceId, StringComparison.OrdinalIgnoreCase))
                    {
                        exactMatches.Add(item.Data);
                    }
                    else if (datasourceId.StartsWith(sourceId, StringComparison.OrdinalIgnoreCase))
                    {
                        prefixMatches.Add(item.Data);
                    }
                }
            }

            if (exactMatches.Count == 1)
            {
                matchedItemData = exactMatches[0];
            }
            else if (exactMatches.Count > 1)
            {
                throw new InvalidOperationException(
                    $"Multiple protected items found with datasource ID '{datasourceId}' in vault '{vaultName}'. " +
                    "Provide --container to disambiguate.");
            }
            else if (prefixMatches.Count == 1)
            {
                matchedItemData = prefixMatches[0];
            }
            else if (prefixMatches.Count > 1)
            {
                throw new InvalidOperationException(
                    $"Multiple protected items match datasource ID '{datasourceId}' in vault '{vaultName}' " +
                    "(shared storage account prefix). Provide a more specific datasource ID or --container to disambiguate.");
            }
        }

        if (matchedItemData is null)
        {
            throw new KeyNotFoundException(
                $"No protected item found with datasource ID '{datasourceId}' in vault '{vaultName}'. " +
                "Verify the datasource ID is correct and the item exists in this vault.");
        }

        var vaultResourceId = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultResourceId);
        var vault = await vaultResource.GetAsync(cancellationToken: cancellationToken);
        var vaultLocation = vault.Value.Data.Location;

        // Extract container and item name from the matched item's resource ID
        var matchedItemId = matchedItemData.Id!;
        var matchedContainerName = containerName ?? ExtractContainerName(matchedItemId.ToString());
        var matchedItemName = matchedItemId.Name;

        var protectedItemId = BackupProtectedItemResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, FabricName, matchedContainerName, matchedItemName);

        // Set IsRehydrate on the matched item's existing properties to preserve
        // the full protection definition (PolicyId, ContainerName, etc.).
        if (matchedItemData.Properties is not BackupGenericProtectedItem existingProperties)
        {
            throw new InvalidOperationException(
                "The matched protected item does not contain properties required to perform undelete.");
        }

        existingProperties.IsRehydrate = true;

        var protectedItemData = new BackupProtectedItemData(vaultLocation)
        {
            Properties = existingProperties
        };

        var protectedItemResource = armClient.GetBackupProtectedItemResource(protectedItemId);
        var operation = await protectedItemResource.UpdateAsync(WaitUntil.Started, protectedItemData, cancellationToken);
        var jobId = ExtractOperationIdFromResponse(operation.GetRawResponse());

        return new OperationResult("Accepted", jobId,
            jobId != null
                ? $"Restore of soft-deleted protected item for datasource '{datasourceId}' has been started in vault '{vaultName}'. Use 'azurebackup job get --job {jobId}' to monitor progress."
                : $"Restore of soft-deleted protected item for datasource '{datasourceId}' has been started in vault '{vaultName}'.");
    }

    /// <summary>
    /// Extracts the container name from a full RSV protected item resource ID.
    /// Format: .../protectionContainers/{containerName}/protectedItems/{itemName}
    /// </summary>
    private static string ExtractContainerName(string resourceId)
    {
        const string marker = "/protectionContainers/";
        var idx = resourceId.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
        {
            throw new ArgumentException($"Cannot extract container name from resource ID: {resourceId}");
        }

        var start = idx + marker.Length;
        var end = resourceId.IndexOf("/protectedItems/", start, StringComparison.OrdinalIgnoreCase);
        if (end < 0)
        {
            throw new ArgumentException($"Cannot extract container name from resource ID: {resourceId}");
        }

        return resourceId[start..end];
    }

    /// <summary>
    /// Polls the RSV ConfigureBackup job to a terminal state and builds a
    /// <see cref="ProtectResult"/> reflecting the actual job outcome. RSV protection is
    /// asynchronous; the protect PUT only accepts the request, so MCP must follow up by
    /// reading the job until it reports success or failure. If polling exceeds the timeout
    /// the result is returned with status <c>InProgress</c> and the job id, so the caller
    /// can continue monitoring with <c>azurebackup job get</c>.
    /// </summary>
    private static async Task<ProtectResult> BuildRsvProtectResultAsync(
        ArmClient armClient, string subscription, string resourceGroup, string vaultName,
        string protectedItemName, string? jobId, string operationDescription,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(jobId))
        {
            return new ProtectResult(
                "Accepted",
                protectedItemName,
                null,
                $"{operationDescription} initiated. Use 'azurebackup protecteditem get' to verify.");
        }

        var finalJob = await WaitForJobAsync(
            armClient, subscription, resourceGroup, vaultName, jobId, cancellationToken);

        if (finalJob == null)
        {
            return new ProtectResult(
                "InProgress",
                protectedItemName,
                jobId,
                $"{operationDescription} is still running after the polling budget elapsed. " +
                $"Use 'azurebackup job get --job {jobId}' to continue monitoring.");
        }

        var status = finalJob.Status ?? "Unknown";
        var errorMessage = ExtractJobErrorMessage(finalJob);
        var isFailure = status.Contains("Fail", StringComparison.OrdinalIgnoreCase) ||
                        status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase);

        var message = isFailure
            ? $"{operationDescription} failed: {errorMessage ?? status}. See 'azurebackup job get --job {jobId}' for details."
            : $"{operationDescription} status: {status}. Use 'azurebackup protecteditem get' to verify the protected item.";

        return new ProtectResult(
            status,
            protectedItemName,
            jobId,
            message,
            ProtectionStatus: null,
            ErrorMessage: isFailure ? errorMessage ?? status : null);
    }

    /// <summary>
    /// Polls a Recovery Services backup job until it reaches a terminal state. Returns the
    /// final <see cref="BackupGenericJob"/> on completion, or <c>null</c> if the job did not
    /// reach a terminal state within the polling budget. ConfigureBackup jobs typically
    /// finish in 2-10 minutes, so a 12-minute budget with 10-second intervals balances
    /// responsiveness and tolerance for slow operations.
    /// </summary>
    private static async Task<BackupGenericJob?> WaitForJobAsync(
        ArmClient armClient, string subscription, string resourceGroup, string vaultName,
        string jobId, CancellationToken cancellationToken)
    {
        const int maxAttempts = 72;          // 72 * 10s = 12 minutes
        var pollDelay = TimeSpan.FromSeconds(10);

        var jobResourceId = BackupJobResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, jobId);
        var jobResource = armClient.GetBackupJobResource(jobResourceId);

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                var jobResponse = await jobResource.GetAsync(cancellationToken);
                if (jobResponse.Value.Data.Properties is BackupGenericJob job &&
                    !string.IsNullOrEmpty(job.Status) &&
                    !job.Status.Equals("InProgress", StringComparison.OrdinalIgnoreCase))
                {
                    return job;
                }
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Job entry not yet visible; keep polling.
            }

            await Task.Delay(pollDelay, cancellationToken);
        }

        return null;
    }

    private static string? ExtractJobErrorMessage(BackupGenericJob job)
    {
        switch (job)
        {
            case IaasVmBackupJob vm when vm.ErrorDetails.Count > 0:
                return FirstNonEmpty(vm.ErrorDetails[0].ErrorString, vm.ErrorDetails[0].ErrorTitle);
            case IaasVmBackupJobV2 vm2 when vm2.ErrorDetails.Count > 0:
                return FirstNonEmpty(vm2.ErrorDetails[0].ErrorString, vm2.ErrorDetails[0].ErrorTitle);
            case StorageBackupJob storage when storage.ErrorDetails.Count > 0:
                return storage.ErrorDetails[0].ErrorString;
            case WorkloadBackupJob wl when wl.ErrorDetails.Count > 0:
                return FirstNonEmpty(wl.ErrorDetails[0].ErrorString, wl.ErrorDetails[0].ErrorTitle);
            default:
                return null;
        }
    }

    private static string? FirstNonEmpty(string? primary, string? fallback) =>
        string.IsNullOrEmpty(primary) ? fallback : primary;

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
            "SQL" or "SQLDATABASE" => "SQLDataBase",
            "SQLINSTANCE" => "SQLInstance",
            "SAPHANA" or "SAPHANADATABASE" => "SAPHanaDatabase",
            "SAPHANASYSTEM" => "SAPHanaSystem",
            "SAPHANADBINSTANCE" or "SAPHANADBI" => "SAPHanaDBInstance",
            "VM" or "IAASVM" or "VIRTUALMACHINE" => "VM",
            "FILESHARE" or "AZUREFILESHARE" or "AFS" => "AzureFileShare",
            "SAPASE" or "SAPASEDATABASE" or "ASE" or "SYBASE" => "SAPAseDatabase",
            _ => (string?)null
        };

        if (normalized is null)
        {
            throw new ArgumentException(
                $"Unknown workload type '{workloadType}'. Supported values: SQL (or SQLDatabase), SQLInstance, SAPHana (or SAPHanaDatabase), SAPHanaSystem, SAPHanaDBInstance (or SAPHanaDBI), VM (or IaaSVM, VirtualMachine), FileShare (or AzureFileShare, AFS), SAPAse (or SAPAseDatabase, ASE, Sybase).");
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
