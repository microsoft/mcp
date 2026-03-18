// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.RecoveryServices;
using Azure.ResourceManager.RecoveryServices.Models;
using Azure.ResourceManager.RecoveryServicesBackup;
using Azure.ResourceManager.RecoveryServicesBackup.Models;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.AzureBackup.Services;

public class RsvBackupOperations(ITenantService tenantService) : BaseAzureService(tenantService), IRsvBackupOperations
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

        // Get the vault to determine its location
        var vaultId = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultId);
        var vault = await vaultResource.GetAsync(cancellationToken: cancellationToken);
        var vaultLocation = vault.Value.Data.Location;

        var policyArmId = BackupProtectionPolicyResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, policyName);

        // Resolve the datasource profile
        var profile = RsvDatasourceRegistry.ResolveOrDefault(datasourceType);

        // Check if this is a workload (SQL/HANA/ASE) protection request
        if (profile.IsWorkloadType)
        {
            // For workload protection, containerName and datasourceId (protectable item name) are required
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException($"The --container parameter is required for {profile.FriendlyName} workload protection. Use 'azurebackup protectable_item_list' to discover containers and items.");
            }

            // Bug #47: Detect if user passed an ARM resource ID instead of the protectable item name.
            // For workloads, datasource-id must be the protectable item name (e.g., 'SAPHanaDatabase;instance;db')
            // from 'protectableitem list', NOT the VM ARM resource ID.
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

            // Construct the correct SDK protected item type based on profile
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

        // Azure File Share protection flow
        if (profile.ProtectedItemType == RsvProtectedItemType.AzureFileShare)
        {
            var fsContainer = containerName ?? RsvNamingHelper.DeriveContainerName(datasourceId, datasourceType);
            var fsProtectedItemName = RsvNamingHelper.DeriveProtectedItemName(datasourceId, datasourceType);

            // Trigger storage account inquiry so the vault discovers file shares
            var containerId = BackupProtectionContainerResource.CreateResourceIdentifier(
                subscription, resourceGroup, vaultName, FabricName, fsContainer);
            var containerResource = armClient.GetBackupProtectionContainerResource(containerId);
            try
            {
                await containerResource.InquireAsync(filter: null, cancellationToken);
                // Wait briefly for inquiry to complete
                await Task.Delay(5000, cancellationToken);
            }
            catch (RequestFailedException)
            {
                // Inquiry may fail if container is not yet registered; proceed anyway
            }

            var fsProtectedItemId = BackupProtectedItemResource.CreateResourceIdentifier(
                subscription, resourceGroup, vaultName, FabricName, fsContainer, fsProtectedItemName);

            // Derive the source resource ID (the storage account ARM ID)
            // We're already in the AzureFileShare block, so always extract the storage account ID
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

        // Standard VM protection flow
        // Trigger container discovery/refresh so the vault discovers the VM
        var rgId = ResourceGroupResource.CreateResourceIdentifier(subscription, resourceGroup);
        var rgResource = armClient.GetResourceGroupResource(rgId);
        await rgResource.RefreshProtectionContainerAsync(vaultName, FabricName, filter: null, cancellationToken);

        // Wait for container discovery to complete (refresh is async on the server side)
        await Task.Delay(30000, cancellationToken);

        // Derive container name if not provided
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

        // The Azure-AsyncOperation header contains an operation ID, not a job ID.
        // We need to find the actual job by listing recent jobs.
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

        // If container name is not provided, we need to list and find the item
        if (string.IsNullOrEmpty(containerName))
        {
            var items = await ListProtectedItemsAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
            var found = items.FirstOrDefault(i => i.Name.Equals(protectedItemName, StringComparison.OrdinalIgnoreCase));
            return found ?? throw new KeyNotFoundException($"Protected item '{protectedItemName}' not found in vault '{vaultName}'.");
        }

        var itemId = BackupProtectedItemResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, FabricName, containerName, protectedItemName);
        var itemResource = armClient.GetBackupProtectedItemResource(itemId);
        var item = await itemResource.GetAsync(cancellationToken: cancellationToken);

        return MapToProtectedItemInfo(item.Value.Data);
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

    public async Task<BackupTriggerResult> TriggerBackupAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string? containerName, string? expiry,
        string? backupType, string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(protectedItemName), protectedItemName));

        if (string.IsNullOrEmpty(containerName))
        {
            throw new ArgumentException("The --container parameter is required for triggering backup on an RSV protected item.");
        }

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var itemId = BackupProtectedItemResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, FabricName, containerName, protectedItemName);
        var itemResource = armClient.GetBackupProtectedItemResource(itemId);

        DateTimeOffset? expiryTime = null;
        if (!string.IsNullOrEmpty(expiry) && DateTimeOffset.TryParse(expiry, out var parsed))
        {
            expiryTime = parsed;
        }

        var backupContent = new TriggerBackupContent(new AzureLocation(string.Empty))
        {
            Properties = CreateBackupRequestContent(backupType, expiryTime, protectedItemName, containerName)
        };

        var result = await itemResource.TriggerBackupAsync(backupContent, cancellationToken);

        // The Azure-AsyncOperation header contains an operation ID, not a job ID.
        // We need to find the actual backup job by listing recent jobs.
        var jobId = await FindLatestJobIdAsync(armClient, subscription, resourceGroup, vaultName, "Backup", cancellationToken);
        jobId ??= ExtractOperationIdFromResponse(result); // Fallback to operation ID if job not found yet

        return new BackupTriggerResult(
            "Accepted",
            jobId,
            jobId != null ? $"Backup triggered. Use 'azurebackup job get --job {jobId}' to monitor progress." : "Backup triggered.");
    }

    private static BackupContent CreateBackupRequestContent(string? backupType, DateTimeOffset? expiryTime, string? protectedItemName = null, string? containerName = null)
    {
        // If a specific backup type is provided (Full, Differential, Log), use workload backup content
        if (!string.IsNullOrEmpty(backupType) && !backupType.Equals("Default", StringComparison.OrdinalIgnoreCase))
        {
            return new WorkloadBackupContent
            {
                BackupType = new BackupType(backupType),
                RecoveryPointExpireOn = expiryTime,
                EnableCompression = true
            };
        }

        // Auto-detect workload type from item/container naming conventions using the registry
        var detectedProfile = RsvDatasourceRegistry.ResolveFromProtectedItemName(protectedItemName ?? string.Empty, containerName);
        var isWorkload = detectedProfile.IsWorkloadType;

        if (isWorkload)
        {
            return new WorkloadBackupContent
            {
                BackupType = new BackupType("Full"),
                RecoveryPointExpireOn = expiryTime,
                EnableCompression = true
            };
        }

        // Default: IaasVmBackupContent for VM workloads
        return new IaasVmBackupContent
        {
            RecoveryPointExpireOn = expiryTime
        };
    }

    public async Task<RestoreTriggerResult> TriggerRestoreAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string recoveryPointId, string? containerName,
        string? targetResourceId, string? restoreLocation, string? stagingStorageAccountId,
        string? restoreMode, string? targetVmName, string? targetVnetId, string? targetSubnetId,
        string? targetDatabaseName, string? targetInstanceName,
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
            throw new ArgumentException("The --container parameter is required for triggering restore on an RSV protected item.");
        }

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

        // Get the existing protected item to determine type and extract SourceResourceId
        var piId = BackupProtectedItemResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, FabricName, containerName, protectedItemName);
        var piResource = armClient.GetBackupProtectedItemResource(piId);
        var existingItem = await piResource.GetAsync(cancellationToken: cancellationToken);
        var existingProperties = existingItem.Value.Data.Properties;

        // Get vault location for restore region
        var vaultRes = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultRes);
        var vault = await vaultResource.GetAsync(cancellationToken);
        var vaultLocation = restoreLocation ?? vault.Value.Data.Location.ToString();

        var rpResourceId = BackupRecoveryPointResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, FabricName, containerName, protectedItemName, recoveryPointId);
        var rpResource = armClient.GetBackupRecoveryPointResource(rpResourceId);

        RestoreContent restoreProperties;

        // Check if this is a workload (SQL/HANA) protected item
        if (existingProperties is VmWorkloadSqlDatabaseProtectedItem)
        {
            // For SQL ALR, extract data directory paths from recovery point extended info
            IList<SqlDataDirectoryMapping>? sqlDataDirMappings = null;
            if (!string.IsNullOrEmpty(targetDatabaseName))
            {
                var rpData = await rpResource.GetAsync(cancellationToken: cancellationToken);
                if (rpData.Value.Data.Properties is WorkloadSqlRecoveryPoint sqlRp
                    && sqlRp.ExtendedInfo?.DataDirectoryPaths is { } dataDirs)
                {
                    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    sqlDataDirMappings = [];
                    foreach (var dir in dataDirs)
                    {
                        if (string.IsNullOrEmpty(dir.Path) || string.IsNullOrEmpty(dir.LogicalName))
                        {
                            continue;
                        }

                        var sourceFileName = Path.GetFileNameWithoutExtension(dir.Path);
                        var sourceExtension = Path.GetExtension(dir.Path);
                        var sourceDir = Path.GetDirectoryName(dir.Path) ?? string.Empty;
                        var targetPath = Path.Combine(sourceDir, $"{sourceFileName}_{timestamp}{sourceExtension}");

                        sqlDataDirMappings.Add(new SqlDataDirectoryMapping
                        {
                            MappingType = dir.DirectoryType,
                            SourceLogicalName = dir.LogicalName,
                            SourcePath = dir.Path,
                            TargetPath = targetPath
                        });
                    }
                }
            }

            restoreProperties = CreateWorkloadSqlRestoreContent(subscription, resourceGroup, vaultName, containerName, protectedItemName, targetDatabaseName, targetInstanceName, sqlDataDirMappings);
        }
        else if (existingProperties is VmWorkloadSapHanaDatabaseProtectedItem)
        {
            restoreProperties = CreateWorkloadSapHanaRestoreContent(targetResourceId, containerName, protectedItemName);
        }
        else
        {
            // Standard VM restore
            var sourceResourceId = (existingProperties as IaasComputeVmProtectedItem)?.SourceResourceId
                ?? (existingProperties as BackupGenericProtectedItem)?.SourceResourceId;

            // Determine restore mode: AlternateLocation, RestoreDisks, or OriginalLocation
            var resolvedMode = !string.IsNullOrEmpty(restoreMode)
                ? restoreMode
                : !string.IsNullOrEmpty(targetResourceId) ? "RestoreDisks" : "OriginalLocation";

            var recoveryType = resolvedMode switch
            {
                "AlternateLocation" => FileShareRecoveryType.AlternateLocation,
                "RestoreDisks" => FileShareRecoveryType.RestoreDisks,
                _ => FileShareRecoveryType.OriginalLocation
            };

            var vmRestoreContent = new IaasVmRestoreContent
            {
                RecoveryPointId = recoveryPointId,
                RecoveryType = recoveryType,
                SourceResourceId = sourceResourceId,
                Region = new AzureLocation(vaultLocation),
                OriginalStorageAccountOption = recoveryType == FileShareRecoveryType.OriginalLocation,
                DoesCreateNewCloudService = false
            };

            if (!string.IsNullOrEmpty(stagingStorageAccountId))
            {
                vmRestoreContent.StorageAccountId = new ResourceIdentifier(stagingStorageAccountId);
            }

            if (recoveryType == FileShareRecoveryType.AlternateLocation)
            {
                // Full ALR: create a new VM at alternate location
                if (string.IsNullOrEmpty(targetVmName) || string.IsNullOrEmpty(targetVnetId) || string.IsNullOrEmpty(targetSubnetId))
                {
                    throw new ArgumentException("AlternateLocation restore requires --target-vm-name, --target-vnet-id, and --target-subnet-id parameters.");
                }

                vmRestoreContent.TargetResourceGroupId = !string.IsNullOrEmpty(targetResourceId)
                    ? new ResourceIdentifier(targetResourceId)
                    : new ResourceIdentifier($"/subscriptions/{subscription}/resourceGroups/{resourceGroup}");
                vmRestoreContent.TargetVirtualMachineId = new ResourceIdentifier(
                    $"/subscriptions/{subscription}/resourceGroups/{(targetResourceId != null ? new ResourceIdentifier(targetResourceId).Name : resourceGroup)}/providers/Microsoft.Compute/virtualMachines/{targetVmName}");
                vmRestoreContent.VirtualNetworkId = new ResourceIdentifier(targetVnetId);
                vmRestoreContent.SubnetId = new ResourceIdentifier(targetSubnetId);
            }
            else if (recoveryType == FileShareRecoveryType.RestoreDisks && !string.IsNullOrEmpty(targetResourceId))
            {
                // RestoreDisks: restore managed disks to target RG
                vmRestoreContent.TargetResourceGroupId = new ResourceIdentifier(targetResourceId);
            }

            restoreProperties = vmRestoreContent;
        }

        var restoreContent = new TriggerRestoreContent(new AzureLocation(vaultLocation))
        {
            Properties = restoreProperties
        };

        var result = await rpResource.TriggerRestoreAsync(WaitUntil.Started, restoreContent, cancellationToken);

        // The Azure-AsyncOperation header contains an operation ID, not a job ID.
        var jobId = await FindLatestJobIdAsync(armClient, subscription, resourceGroup, vaultName, "Restore", cancellationToken);
        jobId ??= ExtractOperationIdFromResponse(result.GetRawResponse()); // Fallback to operation ID

        return new RestoreTriggerResult(
            "Accepted",
            jobId,
            jobId != null ? $"Restore triggered. Use 'azurebackup job get --job {jobId}' to monitor progress." : "Restore triggered.");
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
            throw new ArgumentException("The --container parameter is required for RSV recovery point operations.");
        }

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var rpId = BackupRecoveryPointResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, FabricName, containerName, protectedItemName, recoveryPointId);
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
            throw new ArgumentException("The --container parameter is required for RSV recovery point operations.");
        }

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var itemId = BackupProtectedItemResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, FabricName, containerName, protectedItemName);
        var itemResource = armClient.GetBackupProtectedItemResource(itemId);
        var collection = itemResource.GetBackupRecoveryPoints();

        var points = new List<RecoveryPointInfo>();
        await foreach (var rp in collection.GetAllAsync(cancellationToken: cancellationToken))
        {
            points.Add(MapToRecoveryPointInfo(rp.Data));
        }

        return points;
    }

    // ── New methods ──

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
                identityType.Equals("SystemAssigned", StringComparison.OrdinalIgnoreCase)
                    ? Azure.ResourceManager.Models.ManagedServiceIdentityType.SystemAssigned
                    : Azure.ResourceManager.Models.ManagedServiceIdentityType.None);
        }

        await vaultResource.UpdateAsync(WaitUntil.Completed, patchData, cancellationToken);

        return new OperationResult("Succeeded", null, $"Vault '{vaultName}' updated successfully.");
    }

    public async Task<OperationResult> DeleteVaultAsync(
        string vaultName, string resourceGroup, string subscription,
        bool force, string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var vaultId = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultId);
        await vaultResource.DeleteAsync(WaitUntil.Completed, cancellationToken);

        return new OperationResult("Succeeded", null, $"Vault '{vaultName}' deleted successfully.");
    }

    public async Task<OperationResult> UpdatePolicyAsync(
        string vaultName, string resourceGroup, string subscription,
        string policyName, string? scheduleFrequency,
        string? dailyRetentionDays, string? weeklyRetentionWeeks,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(policyName), policyName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

        // Fetch the existing policy — will throw RequestFailedException (404) if it doesn't exist
        var policyId = BackupProtectionPolicyResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, policyName);
        var policyResource = armClient.GetBackupProtectionPolicyResource(policyId);
        var existingPolicy = await policyResource.GetAsync(cancellationToken);
        var policyData = existingPolicy.Value.Data;

        // Modify only the requested fields on the existing policy
        if (policyData.Properties is FileShareProtectionPolicy fsPolicy)
        {
            UpdateFileSharePolicyRetention(fsPolicy, dailyRetentionDays, weeklyRetentionWeeks);
        }
        else if (policyData.Properties is IaasVmProtectionPolicy vmPolicy)
        {
            UpdateVmPolicyRetention(vmPolicy, dailyRetentionDays, weeklyRetentionWeeks);
            UpdateVmPolicySchedule(vmPolicy, scheduleFrequency);
        }
        else if (policyData.Properties is VmWorkloadProtectionPolicy workloadPolicy)
        {
            UpdateWorkloadPolicyRetention(workloadPolicy, dailyRetentionDays);
        }

        // Fetch vault location for PUT — the GET response data may lack location, causing NullRef in SDK constructor
        var vaultResourceId = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultResourceId);
        var vault = await vaultResource.GetAsync(cancellationToken);
        var vaultLocation = vault.Value.Data.Location;

        // PUT the modified policy back using fresh data with vault location
        var freshPolicyData = new BackupProtectionPolicyData(vaultLocation)
        {
            Properties = policyData.Properties
        };
        var rgId = ResourceGroupResource.CreateResourceIdentifier(subscription, resourceGroup);
        var rgResource = armClient.GetResourceGroupResource(rgId);
        var policyCollection = rgResource.GetBackupProtectionPolicies(vaultName);

        try
        {
            await policyCollection.CreateOrUpdateAsync(WaitUntil.Completed, policyName, freshPolicyData, cancellationToken);
        }
        catch (NullReferenceException)
        {
            // SDK v1.3.0 bug: BackupProtectionPolicyResource constructor throws NullRef
            // when deserializing certain policy types (e.g. VM policies with enhanced schedules).
            // The PUT REST call itself succeeds — verify by re-fetching the updated policy.
            var verifyPolicy = await policyResource.GetAsync(cancellationToken);
            if (verifyPolicy.Value?.Data?.Properties == null)
            {
                throw new InvalidOperationException($"Policy update for '{policyName}' could not be verified after an SDK deserialization error.");
            }
        }

        return new OperationResult("Succeeded", null, $"Policy '{policyName}' updated in vault '{vaultName}'.");
    }

    private static void UpdateVmPolicyRetention(IaasVmProtectionPolicy vmPolicy, string? dailyRetentionDays, string? weeklyRetentionWeeks)
    {
        if (vmPolicy.RetentionPolicy is LongTermRetentionPolicy longTermRetention)
        {
            if (int.TryParse(dailyRetentionDays, out var days) && longTermRetention.DailySchedule != null)
            {
                longTermRetention.DailySchedule.RetentionDuration = new RetentionDuration
                {
                    Count = days,
                    DurationType = RetentionDurationType.Days
                };
            }

            if (int.TryParse(weeklyRetentionWeeks, out var weeks) && longTermRetention.WeeklySchedule != null)
            {
                longTermRetention.WeeklySchedule.RetentionDuration = new RetentionDuration
                {
                    Count = weeks,
                    DurationType = RetentionDurationType.Weeks
                };
            }
        }
    }

    private static void UpdateVmPolicySchedule(IaasVmProtectionPolicy vmPolicy, string? scheduleFrequency)
    {
        if (!string.IsNullOrEmpty(scheduleFrequency) && vmPolicy.SchedulePolicy is SimpleSchedulePolicy simpleSchedule)
        {
            if (Enum.TryParse<ScheduleRunType>(scheduleFrequency, true, out var frequency))
            {
                simpleSchedule.ScheduleRunFrequency = frequency;
            }
        }
    }

    private static void UpdateFileSharePolicyRetention(FileShareProtectionPolicy fsPolicy, string? dailyRetentionDays, string? weeklyRetentionWeeks)
    {
        if (fsPolicy.RetentionPolicy is LongTermRetentionPolicy longTermRetention)
        {
            if (int.TryParse(dailyRetentionDays, out var days) && longTermRetention.DailySchedule != null)
            {
                longTermRetention.DailySchedule.RetentionDuration = new RetentionDuration
                {
                    Count = days,
                    DurationType = RetentionDurationType.Days
                };
            }

            if (int.TryParse(weeklyRetentionWeeks, out var weeks) && longTermRetention.WeeklySchedule != null)
            {
                longTermRetention.WeeklySchedule.RetentionDuration = new RetentionDuration
                {
                    Count = weeks,
                    DurationType = RetentionDurationType.Weeks
                };
            }
        }
    }

    private static void UpdateWorkloadPolicyRetention(VmWorkloadProtectionPolicy workloadPolicy, string? dailyRetentionDays)
    {
        if (!int.TryParse(dailyRetentionDays, out var days))
        {
            return;
        }

        foreach (var subPolicy in workloadPolicy.SubProtectionPolicy)
        {
            if (subPolicy.RetentionPolicy is LongTermRetentionPolicy longTermRetention && longTermRetention.DailySchedule != null)
            {
                longTermRetention.DailySchedule.RetentionDuration = new RetentionDuration
                {
                    Count = days,
                    DurationType = RetentionDurationType.Days
                };
            }
            else if (subPolicy.RetentionPolicy is SimpleRetentionPolicy simpleRetention)
            {
                simpleRetention.RetentionDuration = new RetentionDuration
                {
                    Count = days,
                    DurationType = RetentionDurationType.Days
                };
            }
        }
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

        // Parse schedule time or default to 2:00 AM UTC
        var scheduleDateTime = DateTimeOffset.TryParse(scheduleTime, out var st) ? st : new DateTimeOffset(DateTime.UtcNow.Date.AddHours(2), TimeSpan.Zero);
        var scheduleRunTime = new DateTimeOffset(scheduleDateTime.Year, scheduleDateTime.Month, scheduleDateTime.Day,
            scheduleDateTime.Hour, scheduleDateTime.Minute, 0, TimeSpan.Zero);

        BackupGenericProtectionPolicy policyProperties;

        // Resolve the profile to determine policy type
        var profile = RsvDatasourceRegistry.ResolveOrDefault(workloadType);

        if (profile.PolicyType == RsvPolicyType.VmWorkload)
        {
            // SQL/HANA/ASE workload policy
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
            // Azure File Share policy
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
            // Standard VM policy
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

    public async Task<OperationResult> DeletePolicyAsync(
        string vaultName, string resourceGroup, string subscription,
        string policyName, string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(policyName), policyName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var policyId = BackupProtectionPolicyResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName, policyName);
        var policyResource = armClient.GetBackupProtectionPolicyResource(policyId);
        await policyResource.DeleteAsync(WaitUntil.Completed, cancellationToken);

        return new OperationResult("Succeeded", null, $"Policy '{policyName}' deleted from vault '{vaultName}'.");
    }

    public async Task<OperationResult> StopProtectionAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string mode, string? containerName,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(protectedItemName), protectedItemName),
            (nameof(mode), mode));

        if (string.IsNullOrEmpty(containerName))
        {
            throw new ArgumentException("The --container parameter is required for RSV protection operations.");
        }

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

        if (mode.Equals("DeleteData", StringComparison.OrdinalIgnoreCase))
        {
            var itemId = BackupProtectedItemResource.CreateResourceIdentifier(
                subscription, resourceGroup, vaultName, FabricName, containerName, protectedItemName);
            var itemResource = armClient.GetBackupProtectedItemResource(itemId);
            await itemResource.DeleteAsync(WaitUntil.Started, cancellationToken);
            return new OperationResult("Accepted", null, "Protection stopped and data deletion initiated.");
        }

        // RetainData mode - update with ProtectionState = ProtectionStopped
        var vaultRes = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultRes);
        var vault = await vaultResource.GetAsync(cancellationToken);

        var piId = BackupProtectedItemResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, FabricName, containerName, protectedItemName);
        var piResource = armClient.GetBackupProtectedItemResource(piId);

        // Get existing item to determine type and construct the correct SDK type via profile matching
        var existingItem = await piResource.GetAsync(cancellationToken: cancellationToken);
        BackupGenericProtectedItem stopProps = existingItem.Value.Data.Properties switch
        {
            VmWorkloadSqlDatabaseProtectedItem => new VmWorkloadSqlDatabaseProtectedItem { ProtectionState = BackupProtectionState.ProtectionStopped },
            VmWorkloadSapHanaDatabaseProtectedItem => new VmWorkloadSapHanaDatabaseProtectedItem { ProtectionState = BackupProtectionState.ProtectionStopped },
            FileshareProtectedItem => new FileshareProtectedItem { ProtectionState = BackupProtectionState.ProtectionStopped },
            _ => new IaasComputeVmProtectedItem { ProtectionState = BackupProtectionState.ProtectionStopped }
        };

        var data = new BackupProtectedItemData(vault.Value.Data.Location) { Properties = stopProps };

        await piResource.UpdateAsync(WaitUntil.Started, data, cancellationToken);
        return new OperationResult("Accepted", null, "Protection stopped with data retained.");
    }

    public async Task<OperationResult> ResumeProtectionAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string? containerName, string? policyName,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(protectedItemName), protectedItemName));

        if (string.IsNullOrEmpty(containerName))
        {
            throw new ArgumentException("The --container parameter is required for RSV protection operations.");
        }

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var vaultRes = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultRes);
        var vault = await vaultResource.GetAsync(cancellationToken);

        var piId = BackupProtectedItemResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, FabricName, containerName, protectedItemName);
        var piResource = armClient.GetBackupProtectedItemResource(piId);

        // Get existing item to determine type and construct the correct SDK type via profile matching
        var existingItem = await piResource.GetAsync(cancellationToken: cancellationToken);
        ResourceIdentifier? policyArmId = null;
        if (!string.IsNullOrEmpty(policyName))
        {
            policyArmId = BackupProtectionPolicyResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName, policyName);
        }

        BackupGenericProtectedItem resumeProps = existingItem.Value.Data.Properties switch
        {
            VmWorkloadSqlDatabaseProtectedItem => new VmWorkloadSqlDatabaseProtectedItem { PolicyId = policyArmId },
            VmWorkloadSapHanaDatabaseProtectedItem => new VmWorkloadSapHanaDatabaseProtectedItem { PolicyId = policyArmId },
            FileshareProtectedItem => new FileshareProtectedItem { PolicyId = policyArmId },
            _ => new IaasComputeVmProtectedItem { PolicyId = policyArmId }
        };

        var data = new BackupProtectedItemData(vault.Value.Data.Location) { Properties = resumeProps };
        await piResource.UpdateAsync(WaitUntil.Started, data, cancellationToken);

        return new OperationResult("Accepted", null, "Protection resumed.");
    }

    public async Task<OperationResult> ModifyProtectionAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string? containerName, string? newPolicyName,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(protectedItemName), protectedItemName));

        if (string.IsNullOrEmpty(containerName))
        {
            throw new ArgumentException("The --container parameter is required for RSV protection operations.");
        }

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var vaultRes = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultRes);
        var vault = await vaultResource.GetAsync(cancellationToken);

        var piId = BackupProtectedItemResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, FabricName, containerName, protectedItemName);
        var piResource = armClient.GetBackupProtectedItemResource(piId);

        // Get existing protected item to determine type and extract SourceResourceId
        var existingItem = await piResource.GetAsync(cancellationToken: cancellationToken);
        var existingProperties = existingItem.Value.Data.Properties;

        // Bug #49: Check item state before attempting modify — IRPending items cannot be modified.
        // ProtectionState is on concrete types, not the base BackupGenericProtectedItem.
        var protectionState = GetProtectionState(existingProperties);
        if (string.Equals(protectionState, "IRPending", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Cannot modify protection for '{protectedItemName}' because it is in '{protectionState}' state. " +
                $"The initial replication (IR) must complete before the policy can be changed. " +
                $"Use 'azurebackup job list' to monitor the IR job, then retry after it completes.");
        }

        ResourceIdentifier? policyArmId = null;
        if (!string.IsNullOrEmpty(newPolicyName))
        {
            policyArmId = BackupProtectionPolicyResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName, newPolicyName);
        }

        BackupGenericProtectedItem modifyProps;
        if (existingProperties is VmWorkloadSqlDatabaseProtectedItem)
        {
            modifyProps = new VmWorkloadSqlDatabaseProtectedItem { PolicyId = policyArmId };
        }
        else if (existingProperties is VmWorkloadSapHanaDatabaseProtectedItem)
        {
            modifyProps = new VmWorkloadSapHanaDatabaseProtectedItem { PolicyId = policyArmId };
        }
        else if (existingProperties is FileshareProtectedItem)
        {
            modifyProps = new FileshareProtectedItem { PolicyId = policyArmId };
        }
        else
        {
            var sourceResourceId = (existingProperties as IaasComputeVmProtectedItem)?.SourceResourceId
                ?? (existingProperties as BackupGenericProtectedItem)?.SourceResourceId;

            // If SourceResourceId not available from cast, derive from container name
            if (sourceResourceId is null && !string.IsNullOrEmpty(containerName))
            {
                var parts = containerName.Split(';');
                var vmName = parts[^1];
                var vmRg = parts[^2];
                sourceResourceId = new ResourceIdentifier(
                    $"/subscriptions/{subscription}/resourceGroups/{vmRg}/providers/Microsoft.Compute/virtualMachines/{vmName}");
            }

            modifyProps = new IaasComputeVmProtectedItem
            {
                SourceResourceId = sourceResourceId,
                PolicyId = policyArmId
            };
        }

        var data = new BackupProtectedItemData(vault.Value.Data.Location) { Properties = modifyProps };
        await piResource.UpdateAsync(WaitUntil.Started, data, cancellationToken);

        return new OperationResult("Accepted", null, $"Protection modified. Policy changed to '{newPolicyName}'.");
    }

    public Task<OperationResult> UndeleteProtectedItemAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string? containerName,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        // RSV undelete is handled via support request or portal; SDK doesn't expose a direct undelete for RSV
        return Task.FromResult(new OperationResult("NotSupported", null, "Undelete for RSV protected items requires Azure portal or support request. Use soft-delete recovery instead."));
    }

    public async Task<OperationResult> CancelJobAsync(
        string vaultName, string resourceGroup, string subscription,
        string jobId, string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(jobId), jobId));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var jobResourceId = BackupJobResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName, jobId);
        var jobResource = armClient.GetBackupJobResource(jobResourceId);
        await jobResource.TriggerJobCancellationAsync(cancellationToken);

        return new OperationResult("Accepted", null, $"Job '{jobId}' cancellation triggered.");
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
        var vaultId = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultId);
        var vault = await vaultResource.GetAsync(cancellationToken);

        var softDeleteSettings = new RecoveryServicesSoftDeleteSettings
        {
            SoftDeleteState = new RecoveryServicesSoftDeleteState(softDeleteState)
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

        // CRR must be enabled via the BackupResourceConfig sub-resource (backupstorageconfig),
        // NOT via the vault-level PATCH which returns CloudInternalError.
        var configResourceId = BackupResourceConfigResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var configResource = armClient.GetBackupResourceConfigResource(configResourceId);
        var currentConfig = await configResource.GetAsync(cancellationToken);

        // Update the data and use CreateOrUpdate via the collection
        // (BackupResourceConfigResource.UpdateAsync has an SDK NullRef bug in its constructor)
        var data = currentConfig.Value.Data;
        data.Properties.EnableCrossRegionRestore = true;

        var rgId = ResourceGroupResource.CreateResourceIdentifier(subscription, resourceGroup);
        var rgResource = armClient.GetResourceGroupResource(rgId);
        var collection = rgResource.GetBackupResourceConfigs();
        await collection.CreateOrUpdateAsync(WaitUntil.Completed, vaultName, data, cancellationToken);

        return new OperationResult("Succeeded", null, $"Cross-Region Restore enabled for vault '{vaultName}'.");
    }

    public async Task<HealthCheckResult> RunBackupHealthCheckAsync(
        string vaultName, string resourceGroup, string subscription,
        int? rpoThresholdHours, bool includeSecurityPosture,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

        // Get vault info
        var vaultId = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultId);
        var vault = await vaultResource.GetAsync(cancellationToken);

        // List protected items and check health
        var items = await ListProtectedItemsAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
        var rpoThreshold = rpoThresholdHours ?? 24;
        var now = DateTimeOffset.UtcNow;

        var details = new List<HealthCheckItemDetail>();
        int healthy = 0, unhealthy = 0, breachingRpo = 0;

        foreach (var item in items)
        {
            var rpoBreached = item.LastBackupTime.HasValue && (now - item.LastBackupTime.Value).TotalHours > rpoThreshold;
            if (rpoBreached) breachingRpo++;

            var isHealthy = item.ProtectionStatus?.Contains("Protected", StringComparison.OrdinalIgnoreCase) == true && !rpoBreached;
            if (isHealthy) healthy++; else unhealthy++;

            details.Add(new HealthCheckItemDetail(
                item.Name, item.ProtectionStatus, isHealthy ? "Healthy" : "Unhealthy",
                item.LastBackupTime, rpoBreached));
        }

        return new HealthCheckResult(
            vaultName, VaultType, items.Count, healthy, unhealthy, breachingRpo,
            vault.Value.Data.Properties?.SecuritySettings?.SoftDeleteSettings?.SoftDeleteState?.ToString(),
            vault.Value.Data.Properties?.SecuritySettings?.ImmutabilityState?.ToString(),
            null, details);
    }

    private static BackupVaultInfo MapToVaultInfo(RecoveryServicesVaultData data, string? resourceGroup)
    {
        return new BackupVaultInfo(
            data.Id?.ToString(),
            data.Name,
            VaultType,
            data.Location.Name,
            resourceGroup,
            data.Properties?.ProvisioningState,
            data.Sku?.Name.ToString(),
            null,
            data.Tags?.ToDictionary(t => t.Key, t => t.Value));
    }

    private static ContainerInfo MapToContainerInfo(BackupProtectionContainerData data)
    {
        var properties = data.Properties;
        string? workloadType = null;
        string? sourceResourceId = null;
        DateTimeOffset? lastUpdatedTime = null;

        if (properties is VmAppContainerProtectionContainer vmAppContainer)
        {
            workloadType = vmAppContainer.WorkloadType?.ToString();
            sourceResourceId = vmAppContainer.SourceResourceId?.ToString();
            lastUpdatedTime = vmAppContainer.LastUpdatedOn;
        }
        else if (properties is IaasVmContainer iaasVmContainer)
        {
            workloadType = "AzureVM";
            sourceResourceId = iaasVmContainer.VirtualMachineId?.ToString();
        }

        return new ContainerInfo(
            data.Name,
            properties?.FriendlyName,
            properties?.RegistrationStatus,
            properties?.HealthStatus,
            properties?.ProtectableObjectType,
            properties?.BackupManagementType?.ToString(),
            sourceResourceId,
            workloadType,
            lastUpdatedTime);
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

        if (data.Properties is BackupGenericProtectionPolicy genericPolicy)
        {
            protectedItemsCount = genericPolicy.ProtectedItemsCount;
        }

        return new BackupPolicyInfo(
            data.Id?.ToString(),
            data.Name,
            VaultType,
            workloadType != null ? [workloadType] : null,
            protectedItemsCount);
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

        // Look for the most recent job of the given operation type started within the last minute
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

    private static bool IsWorkloadType(string? datasourceType)
    {
        var profile = RsvDatasourceRegistry.Resolve(datasourceType);
        return profile?.IsWorkloadType ?? false;
    }

    private static RestoreContent CreateWorkloadSqlRestoreContent(
        string subscription, string resourceGroup, string vaultName,
        string containerName, string protectedItemName, string? targetDatabaseName, string? targetInstanceName,
        IList<SqlDataDirectoryMapping>? dataDirectoryMappings)
    {
        if (!string.IsNullOrEmpty(targetDatabaseName))
        {
            // ALR: restore to a different database on the same or different SQL instance
            var resolvedInstanceName = targetInstanceName ?? "mssqlserver";

            // Derive VM ARM ID from container name (format: VMAppContainer;Compute;{RG};{VMName})
            var containerParts = containerName.Split(';');
            var vmResourceGroup = containerParts.Length > 2 ? containerParts[2] : resourceGroup;
            var vmName = containerParts.Length > 3 ? containerParts[3] : string.Empty;
            var vmId = new ResourceIdentifier(
                $"/subscriptions/{subscription}/resourceGroups/{vmResourceGroup}/providers/Microsoft.Compute/virtualMachines/{vmName}");

            // ContainerId must be the full ARM resource ID with lowercase container name
            var fullContainerId = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.RecoveryServices/vaults/{vaultName}/backupFabrics/{FabricName}/protectionContainers/{containerName.ToLowerInvariant()}";

            // DatabaseName format: {INSTANCE_UPPER}/SQLInstance;{instance_lower} (target SQL instance path)
            var databaseName = $"{resolvedInstanceName.ToUpperInvariant()}/SQLInstance;{resolvedInstanceName.ToLowerInvariant()}";

            var content = new WorkloadSqlRestoreContent
            {
                RecoveryType = FileShareRecoveryType.AlternateLocation,
                SourceResourceId = vmId,
                TargetVirtualMachineId = vmId,
                ShouldUseAlternateTargetLocation = true,
                IsNonRecoverable = false,
                TargetInfo = new TargetRestoreInfo
                {
                    OverwriteOption = RestoreOverwriteOption.Overwrite,
                    ContainerId = fullContainerId,
                    DatabaseName = databaseName
                }
            };

            // Add alternate directory paths for SQL data/log file mapping
            if (dataDirectoryMappings != null)
            {
                foreach (var mapping in dataDirectoryMappings)
                {
                    content.AlternateDirectoryPaths.Add(mapping);
                }
            }

            return content;
        }

        return new WorkloadSqlRestoreContent
        {
            RecoveryType = FileShareRecoveryType.OriginalLocation,
            ShouldUseAlternateTargetLocation = false
        };
    }

    private static RestoreContent CreateWorkloadSapHanaRestoreContent(
        string? targetResourceId, string containerName, string protectedItemName)
    {
        if (!string.IsNullOrEmpty(targetResourceId))
        {
            return new WorkloadSapHanaRestoreContent
            {
                RecoveryType = FileShareRecoveryType.AlternateLocation,
                TargetInfo = new TargetRestoreInfo
                {
                    OverwriteOption = RestoreOverwriteOption.FailOnConflict,
                    ContainerId = containerName,
                    DatabaseName = protectedItemName
                }
            };
        }

        return new WorkloadSapHanaRestoreContent
        {
            RecoveryType = FileShareRecoveryType.OriginalLocation
        };
    }

    public async Task<List<ContainerInfo>> ListContainersAsync(
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

        // The REST API requires a backupManagementType filter, so query the main types
        string[] backupManagementTypes = ["AzureWorkload", "AzureIaasVM", "AzureStorage", "MAB", "DPM"];
        var containers = new List<ContainerInfo>();

        foreach (var bmType in backupManagementTypes)
        {
            var filter = $"backupManagementType eq '{bmType}'";
            try
            {
                await foreach (var container in rgResource.GetBackupProtectionContainersAsync(vaultName, filter, cancellationToken))
                {
                    containers.Add(MapToContainerInfo(container.Data));
                }
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 400)
            {
                // Some backup management types may not be supported for all vault configurations; skip them
                continue;
            }
        }

        return containers;
    }

    public async Task<OperationResult> RegisterContainerAsync(
        string vaultName, string resourceGroup, string subscription,
        string vmResourceId, string workloadType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(vmResourceId), vmResourceId),
            (nameof(workloadType), workloadType));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

        var vmId = new ResourceIdentifier(vmResourceId);
        var containerName = $"VMAppContainer;Compute;{vmId.ResourceGroupName};{vmId.Name}";

        var containerId = BackupProtectionContainerResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, FabricName, containerName);

        var containerResource = armClient.GetBackupProtectionContainerResource(containerId);

        // Get vault location for the container data
        var vaultResId = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultResId);
        var vault = await vaultResource.GetAsync(cancellationToken: cancellationToken);

        var containerData = new BackupProtectionContainerData(vault.Value.Data.Location)
        {
            Properties = new VmAppContainerProtectionContainer
            {
                SourceResourceId = vmId,
                BackupManagementType = BackupManagementType.AzureWorkload,
                WorkloadType = new BackupWorkloadType(workloadType)
            }
        };

        var result = await containerResource.UpdateAsync(WaitUntil.Started, containerData, cancellationToken);
        var jobId = ExtractOperationIdFromResponse(result.GetRawResponse());

        return new OperationResult(
            "Accepted",
            jobId,
            $"Container registration initiated for VM '{vmId.Name}'. This may take several minutes. Use 'azurebackup job get --job {jobId}' to monitor progress.");
    }

    public async Task<OperationResult> TriggerInquiryAsync(
        string vaultName, string resourceGroup, string subscription,
        string containerName, string? workloadType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(containerName), containerName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

        var containerId = BackupProtectionContainerResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, FabricName, containerName);
        var containerResource = armClient.GetBackupProtectionContainerResource(containerId);

        string? filter = !string.IsNullOrEmpty(workloadType)
            ? $"backupManagementType eq 'AzureWorkload' and workloadType eq '{workloadType}'"
            : null;

        var result = await containerResource.InquireAsync(filter, cancellationToken);
        var operationId = ExtractOperationIdFromResponse(result);

        return new OperationResult(
            "Accepted",
            operationId,
            $"Inquiry triggered for container '{containerName}'. This discovers databases on the registered VM.");
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
            // Normalize workload-type values to what the REST API filter expects (Bug #46).
            // Users may pass common names like "SAPHana" but the API filter requires
            // specific values like "SAPHanaDatabase" or "SAPHanaDBInstance".
            var normalizedType = NormalizeWorkloadTypeForFilter(workloadType);
            filter = $"backupManagementType eq 'AzureWorkload' and workloadType eq '{normalizedType}'";
        }
        else
        {
            // Azure API requires at least a backupManagementType filter (Bug #38)
            filter = "backupManagementType eq 'AzureWorkload'";
        }

        var items = new List<ProtectableItemInfo>();
        await foreach (var item in rgResource.GetBackupProtectableItemsAsync(vaultName, filter: filter, cancellationToken: cancellationToken))
        {
            items.Add(MapToProtectableItemInfo(item));
        }

        return items;
    }

    public async Task<OperationResult> EnableAutoProtectionAsync(
        string vaultName, string resourceGroup, string subscription,
        string vmResourceId, string instanceName, string policyName,
        string workloadType, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(vmResourceId), vmResourceId),
            (nameof(instanceName), instanceName),
            (nameof(policyName), policyName),
            (nameof(workloadType), workloadType));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

        var policyArmId = BackupProtectionPolicyResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, policyName);

        var vmId = new ResourceIdentifier(vmResourceId);

        // Intent name is a GUID as used by Azure Resource Manager
        var intentName = Guid.NewGuid().ToString();

        var intentId = BackupProtectionIntentResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, FabricName, intentName);
        var intentResource = armClient.GetBackupProtectionIntentResource(intentId);

        var vaultResId = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultResId);
        var vault = await vaultResource.GetAsync(cancellationToken: cancellationToken);

        // Build container and protectable item references for the intent
        var containerName = $"VMAppContainer;Compute;{vmId.ResourceGroupName};{vmId.Name}";
        var itemId = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.RecoveryServices/vaults/{vaultName}/backupFabrics/Azure/protectionContainers/{containerName}/protectableItems/{instanceName}";

        // Bug #48: Use WorkloadSqlAutoProtectionIntent for all workload types (SQL and HANA).
        // Despite the "Sql" in the name, this is the only SDK intent type that supports
        // the WorkloadItemType property needed to distinguish SQL vs HANA auto-protection.
        // The REST API type is AzureWorkloadSQLAutoProtectionIntent for both SQL and HANA.
        var workloadItemType = workloadType?.ToUpperInvariant() switch
        {
            "SQLDATABASE" or "SQLINSTANCE" or "SQL" => WorkloadItemType.SqlInstance,
            "SAPHANA" or "SAPHANADATABASE" or "SAPHANASYSTEM" or "SAPHANADBINSTANCE" => WorkloadItemType.SapHanaSystem,
            _ => new WorkloadItemType(workloadType ?? "Invalid"),
        };

        var intentProperties = new WorkloadSqlAutoProtectionIntent
        {
            BackupManagementType = BackupManagementType.AzureWorkload,
            ItemId = new ResourceIdentifier(itemId),
            PolicyId = policyArmId,
            SourceResourceId = new ResourceIdentifier(vmResourceId),
            WorkloadItemType = workloadItemType,
        };

        var intentData = new BackupProtectionIntentData(vault.Value.Data.Location)
        {
            Properties = intentProperties
        };

        await intentResource.UpdateAsync(WaitUntil.Started, intentData, cancellationToken);

        return new OperationResult(
            "Succeeded",
            null,
            $"Auto-protection enabled for '{instanceName}' on VM '{vmId.Name}' with policy '{policyName}'.");
    }

    /// <summary>
    /// Normalizes user-provided workload type values to the API filter format.
    /// The REST API filter expects specific types like "SAPHanaDatabase" but users
    /// commonly pass "SAPHana" (which is what the API returns in workloadType fields).
    /// </summary>
    private static string NormalizeWorkloadTypeForFilter(string workloadType) => workloadType.ToUpperInvariant() switch
    {
        "SAPHANA" => "SAPHanaDatabase",
        "SAPHANASYSTEM" => "SAPHanaSystem",
        "SAPHANADBINSTANCE" or "SAPHANADBI" => "SAPHanaDBInstance",
        "SQL" => "SQLDataBase",
        "SQLINSTANCE" => "SQLInstance",
        _ => workloadType, // Pass through if already in correct format (e.g., "SAPHanaDatabase", "SQLDataBase")
    };

    /// <summary>
    /// Extracts the ProtectionState from concrete protected item types.
    /// The base BackupGenericProtectedItem does not expose ProtectionState directly.
    /// </summary>
    private static string? GetProtectionState(BackupGenericProtectedItem? properties) => properties switch
    {
        VmWorkloadSapHanaDatabaseProtectedItem hana => hana.ProtectionState?.ToString(),
        VmWorkloadSqlDatabaseProtectedItem sql => sql.ProtectionState?.ToString(),
        IaasComputeVmProtectedItem vm => vm.ProtectionState?.ToString(),
        FileshareProtectedItem fs => fs.ProtectionState?.ToString(),
        _ => null,
    };

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
            // ProtectableItemType is internal in the SDK, use type discrimination
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
        // Use the RSV datasource registry for workload detection
        var profile = RsvDatasourceRegistry.Resolve(datasourceType);
        if (profile?.IsWorkloadType == true)
        {
            var resourceId = new ResourceIdentifier(datasourceId);
            return $"{profile.ContainerNamePrefix};{resourceId.ResourceGroupName};{resourceId.Name}";
        }

        var vmResourceId = new ResourceIdentifier(datasourceId);

        // Use profile-based detection first (handles nested ARM IDs like file shares
        // where ResourceType.Type returns "storageAccounts/fileServices/shares" not "shares")
        if (profile?.ProtectedItemType == RsvProtectedItemType.AzureFileShare)
        {
            return $"StorageContainer;Storage;{vmResourceId.ResourceGroupName};{ExtractStorageAccountName(vmResourceId)}";
        }

        // Fallback to resource type detection for untyped calls
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
        // For workload types, the protectable item name is used directly (passed as datasourceId)
        var profile = RsvDatasourceRegistry.Resolve(datasourceType);
        if (profile?.IsWorkloadType == true)
        {
            return datasourceId;
        }

        var resourceId = new ResourceIdentifier(datasourceId);

        // Use profile-based detection first (handles nested ARM IDs like file shares
        // where ResourceType.Type returns "storageAccounts/fileServices/shares" not "shares")
        if (profile?.ProtectedItemType == RsvProtectedItemType.AzureFileShare)
        {
            return $"AzureFileShare;{resourceId.Name}";
        }

        // Fallback to resource type detection for untyped calls
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
        // For file share ARM IDs like .../storageAccounts/{sa}/fileServices/default/shares/{share}
        // Walk up the hierarchy to find the storage account name
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
        // For file share ARM IDs, walk up to return the storage account ARM ID
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
