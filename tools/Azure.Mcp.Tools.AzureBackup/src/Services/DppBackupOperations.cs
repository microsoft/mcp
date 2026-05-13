// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.DataProtectionBackup;
using Azure.ResourceManager.DataProtectionBackup.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Services;

public sealed class DppBackupOperations(ITenantService tenantService) : BaseAzureService(tenantService), IDppBackupOperations
{
    private const string VaultType = VaultTypeResolver.Dpp;

    /// <summary>
    /// Resolves the DPP datasource profile from a user-supplied or auto-detected type string.
    /// Handles auto-detection (e.g. "Microsoft.Storage/storageAccounts" -> Blob profile)
    /// and friendly name mapping (e.g. "aks" -> AKS profile).
    /// </summary>
    internal static DppDatasourceProfile ResolveProfile(string datasourceTypeOrArm)
    {
        var autoDetected = DppDatasourceRegistry.TryAutoDetect(datasourceTypeOrArm);
        if (autoDetected != null)
        {
            return autoDetected;
        }

        return DppDatasourceRegistry.Resolve(datasourceTypeOrArm);
    }

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
        var collection = rgResource.GetDataProtectionBackupVaults();

        var storageSettings = new List<DataProtectionBackupStorageSetting>
        {
            new()
            {
                DataStoreType = StorageSettingStoreType.VaultStore,
                StorageSettingType = storageType?.ToLowerInvariant() switch
                {
                    "locallyredundant" => StorageSettingType.LocallyRedundant,
                    "zoneredundant" => StorageSettingType.ZoneRedundant,
                    "georedundant" or null => StorageSettingType.GeoRedundant,
                    _ => throw new ArgumentException($"Invalid storage type: '{storageType}'.")
                }
            }
        };

        var vaultData = new DataProtectionBackupVaultData(new AzureLocation(location), new DataProtectionBackupVaultProperties(storageSettings))
        {
            // DPP (Backup Vault) requires a Managed Identity to authenticate to protected
            // datasources (storage accounts, disks, PG Flex, etc.). Without it every
            // 'protecteditem protect' call would fail server-side with VaultMSIUnauthorized.
            // Default to SystemAssigned so the vault is usable out of the box; callers can
            // change this later via 'vault update --identity-type ...'.
            Identity = new Azure.ResourceManager.Models.ManagedServiceIdentity(
                Azure.ResourceManager.Models.ManagedServiceIdentityType.SystemAssigned)
        };

        var result = await collection.CreateOrUpdateAsync(WaitUntil.Completed, vaultName, vaultData, cancellationToken);

        return new VaultCreateResult(
            result.Value.Id?.ToString(),
            result.Value.Data.Name,
            VaultType,
            result.Value.Data.Location.Name,
            result.Value.Data.Properties?.ProvisioningState?.ToString());
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
        var vaultId = DataProtectionBackupVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetDataProtectionBackupVaultResource(vaultId);
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
        await foreach (var vault in subResource.GetDataProtectionBackupVaultsAsync(cancellationToken))
        {
            var rg = vault.Id?.ResourceGroupName;
            vaults.Add(MapToVaultInfo(vault.Data, rg));
        }

        return vaults;
    }

    public async Task<ProtectResult> ProtectItemAsync(
        string vaultName, string resourceGroup, string subscription,
        string datasourceId, string policyName, string? datasourceType,
        string? aksIncludedNamespaces, string? aksExcludedNamespaces,
        string? aksLabelSelectors, string? aksIncludeClusterScopeResources,
        string? aksSnapshotResourceGroup,
        string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(datasourceId), datasourceId),
            (nameof(policyName), policyName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var vaultId = DataProtectionBackupVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetDataProtectionBackupVaultResource(vaultId);
        var vaultData = await vaultResource.GetAsync(cancellationToken);
        var collection = vaultResource.GetDataProtectionBackupInstances();

        var policyId = DataProtectionBackupPolicyResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName, policyName);
        ResourceIdentifier datasourceResourceId;
        try
        {
            datasourceResourceId = new ResourceIdentifier(datasourceId);
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException or UriFormatException)
        {
            throw new ArgumentException(
                $"Invalid datasource ID '{datasourceId}'. Expected a fully-qualified ARM resource ID " +
                "(e.g., /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.Compute/disks/{name}).", ex);
        }

        string resolvedDatasourceType;
        if (!string.IsNullOrEmpty(datasourceType))
        {
            resolvedDatasourceType = datasourceType;
        }
        else
        {
            try
            {
                resolvedDatasourceType = datasourceResourceId.ResourceType.ToString();
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    $"Could not determine datasource type from '{datasourceId}'. " +
                    "The ARM resource ID may be malformed. Provide --datasource-type explicitly or fix the resource ID.", ex);
            }
        }
        var profile = ResolveProfile(resolvedDatasourceType);

        var instanceName = DppDatasourceRegistry.GenerateInstanceName(profile, datasourceResourceId);

        var policyInfo = new BackupInstancePolicyInfo(policyId);

        if (profile.RequiresSnapshotResourceGroup)
        {
            var snapshotRg = !string.IsNullOrWhiteSpace(aksSnapshotResourceGroup)
                ? aksSnapshotResourceGroup
                : datasourceResourceId.ResourceGroupName ?? resourceGroup;
            var snapshotRgId = ResourceGroupResource.CreateResourceIdentifier(subscription, snapshotRg);
            var opStoreSettings = new OperationalDataStoreSettings(DataStoreType.OperationalStore)
            {
                ResourceGroupId = snapshotRgId,
            };
            policyInfo.PolicyParameters = new BackupInstancePolicySettings();
            policyInfo.PolicyParameters.DataStoreParametersList.Add(opStoreSettings);
        }

        if (profile.BackupParametersMode == DppBackupParametersMode.KubernetesCluster)
        {
            policyInfo.PolicyParameters ??= new BackupInstancePolicySettings();

            var includeClusterScope = string.IsNullOrWhiteSpace(aksIncludeClusterScopeResources)
                || aksIncludeClusterScopeResources.Equals("true", StringComparison.OrdinalIgnoreCase);

            var aksSettings = new KubernetesClusterBackupDataSourceSettings(
                isSnapshotVolumesEnabled: true,
                isClusterScopeResourcesIncluded: includeClusterScope);

            // AKS namespace scoping (--aks-included-namespaces / --aks-excluded-namespaces):
            // NOT YET FUNCTIONAL due to Azure SDK serialization bug.
            //
            // Problem: KubernetesClusterBackupDataSourceSettings (Azure.ResourceManager.DataProtectionBackup
            // v1.7.1) initializes both IncludedNamespaces and ExcludedNamespaces as empty IList<string>.
            // The serializer always emits both as [] even when only one is populated. The DPP API rejects
            // with UserErrorInvalidIncludedExcludedNamespacesList: "Include and Exclude list for Namespaces
            // cannot be used together."
            //
            // How the CLI works around this: az dataprotection backup-instance initialize-backupconfig
            // outputs JSON with null for unused fields (e.g. "excluded_namespaces": null). The CLI never
            // uses the .NET SDK type for serialization — it constructs the JSON directly.
            //
            // Workarounds attempted and why they failed:
            //   1. Clear() on unused list → still serializes as []
            //   2. Reflection to null backing field → NullReferenceException in JsonModelWriteCore
            //   3. ModelReaderWriter.Read from JSON with null → re-initializes empty list on deserialize
            //
            // Fix required: Azure REST API specs (Azure/azure-rest-api-specs) should add x-nullable: true
            // to includedNamespaces and excludedNamespaces in KubernetesClusterBackupDatasourceParameters.
            // This would generate nullable IList<string>? properties that the serializer can omit when null.
            // Path: specification/dataprotection/resource-manager/Microsoft.DataProtection/stable/2023-11-01/dataprotection.json
            //
            // What works: --aks-label-selectors, --aks-include-cluster-scope-resources, --aks-snapshot-resource-group
            // What doesn't: --aks-included-namespaces, --aks-excluded-namespaces (params accepted but ignored)

            if (!string.IsNullOrWhiteSpace(aksLabelSelectors))
            {
                foreach (var label in aksLabelSelectors.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    aksSettings.LabelSelectors.Add(label);
                }
            }

            policyInfo.PolicyParameters.BackupDataSourceParametersList.Add(aksSettings);
        }

        var dataSourceInfo = new DataSourceInfo(datasourceResourceId)
        {
            DataSourceType = profile.ArmResourceType,
            ObjectType = "Datasource",
            ResourceType = datasourceResourceId.ResourceType,
            ResourceName = datasourceResourceId.Name,
            ResourceLocation = vaultData.Value.Data.Location,
        };
        var instanceProperties = new DataProtectionBackupInstanceProperties(
            dataSourceInfo,
            policyInfo,
            string.Empty)
        {
            ObjectType = "BackupInstance",
        };

        if (profile.DataSourceSetMode != DppDataSourceSetMode.None)
        {
            var setId = profile.DataSourceSetMode == DppDataSourceSetMode.Parent
                ? DppDatasourceRegistry.GetParentResourceId(datasourceResourceId)
                : datasourceResourceId;
            instanceProperties.DataSourceSetInfo = new DataSourceSetInfo(setId)
            {
                DataSourceType = profile.ArmResourceType,
                ObjectType = "DatasourceSet",
                ResourceType = setId.ResourceType,
                ResourceName = setId.Name,
                ResourceLocation = vaultData.Value.Data.Location,
            };
        }

        var instanceData = new DataProtectionBackupInstanceData
        {
            Properties = instanceProperties
        };

        // DPP protection is asynchronous on the server side and is NOT surfaced as a
        // backup job (only on-demand backup, restore, etc. are jobs). MCP must therefore
        // wait for the underlying operationStatus to reach a terminal state and then read
        // back the BackupInstance to confirm the protection actually configured. Using
        // WaitUntil.Completed lets the SDK poll the Azure-AsyncOperation header for us
        // and surface the real server-side error (e.g. VaultMSIUnauthorized) as a
        // RequestFailedException, instead of silently returning "Accepted".
        ArmOperation<DataProtectionBackupInstanceResource> operation;
        try
        {
            operation = await collection.CreateOrUpdateAsync(
                WaitUntil.Completed, instanceName, instanceData, cancellationToken);
        }
        catch (RequestFailedException ex)
        {
            return new ProtectResult(
                "Failed",
                instanceName,
                JobId: null,
                $"Protection failed for backup instance '{instanceName}': {ex.Message}",
                ProtectionStatus: null,
                ErrorMessage: ex.Message);
        }

        // Re-read the backup instance to capture the authoritative protection status.
        // The LRO can complete while the BI is still in ConfiguringProtection; both
        // outcomes are surfaced to the caller via ProtectionStatus. If the re-read
        // fails with a transient error, report success (protection did complete) and
        // let the caller verify with 'protecteditem get'.
        string? protectionStatus = null;
        try
        {
            var instanceResource = armClient.GetDataProtectionBackupInstanceResource(operation.Value.Id);
            var bi = await instanceResource.GetAsync(cancellationToken);
            protectionStatus = bi.Value.Data.Properties?.ProtectionStatus?.Status?.ToString();
        }
        catch (RequestFailedException)
        {
            // Transient re-read failure; protection itself succeeded.
        }

        return new ProtectResult(
            "Succeeded",
            instanceName,
            JobId: null,
            $"Protection configured for backup instance '{instanceName}' (status: {protectionStatus ?? "Unknown"}). " +
            $"Use 'azurebackup protecteditem get --protected-item {instanceName}' to view details.",
            ProtectionStatus: protectionStatus,
            ErrorMessage: null);
    }

    public async Task<ProtectedItemInfo> GetProtectedItemAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(protectedItemName), protectedItemName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

        // First try direct lookup by exact instance name
        try
        {
            var instanceId = DataProtectionBackupInstanceResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName, protectedItemName);
            var instanceResource = armClient.GetDataProtectionBackupInstanceResource(instanceId);
            var instance = await instanceResource.GetAsync(cancellationToken);
            return MapToProtectedItemInfo(instance.Value.Data);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // Direct lookup failed  -  search by friendly/datasource name
        }

        // Fall back to listing all items and searching by friendly name
        var items = await ListProtectedItemsAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
        var found = items.FirstOrDefault(i =>
            (!string.IsNullOrEmpty(i.Name) && i.Name.Equals(protectedItemName, StringComparison.OrdinalIgnoreCase)) ||
            MatchesDppFriendlyName(i, protectedItemName));
        return found ?? throw new KeyNotFoundException(
            $"Protected item '{protectedItemName}' not found in vault '{vaultName}'. " +
            "Use the full backup instance name from 'azurebackup protecteditem get' list output.");
    }

    /// <summary>
    /// Checks whether a DPP backup instance matches a user-provided friendly name.
    /// DPP instance names follow patterns like: rg-diskname-guid or parent-child-guid.
    /// This checks the datasource resource name from the datasource ID.
    /// </summary>
    private static bool MatchesDppFriendlyName(ProtectedItemInfo item, string friendlyName)
    {
        if (!string.IsNullOrEmpty(item.DatasourceId))
        {
            var datasourceResourceName = item.DatasourceId.Split('/').LastOrDefault();
            if (string.Equals(datasourceResourceName, friendlyName, StringComparison.OrdinalIgnoreCase))
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
        var vaultId = DataProtectionBackupVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetDataProtectionBackupVaultResource(vaultId);
        var collection = vaultResource.GetDataProtectionBackupInstances();

        var items = new List<ProtectedItemInfo>();
        await foreach (var instance in collection.GetAllAsync(cancellationToken))
        {
            items.Add(MapToProtectedItemInfo(instance.Data));
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
        var policyId = DataProtectionBackupPolicyResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName, policyName);
        var policyResource = armClient.GetDataProtectionBackupPolicyResource(policyId);
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
        var vaultId = DataProtectionBackupVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetDataProtectionBackupVaultResource(vaultId);
        var collection = vaultResource.GetDataProtectionBackupPolicies();

        var policies = new List<BackupPolicyInfo>();
        await foreach (var policy in collection.GetAllAsync(cancellationToken))
        {
            policies.Add(MapToPolicyInfo(policy.Data));
        }

        return policies;
    }

    public async Task<OperationResult> UndeleteProtectedItemAsync(
        string vaultName, string resourceGroup, string subscription,
        string datasourceId, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(datasourceId), datasourceId));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var vaultId = DataProtectionBackupVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetDataProtectionBackupVaultResource(vaultId);

        // List soft-deleted backup instances and find the one matching the datasource ID
        var deletedCollection = vaultResource.GetDeletedDataProtectionBackupInstances();

        DeletedDataProtectionBackupInstanceResource? matchedInstance = null;
        await foreach (var deletedInstance in deletedCollection.GetAllAsync(cancellationToken))
        {
            var deletedDatasourceId = deletedInstance.Data?.Properties?.DataSourceInfo?.ResourceId?.ToString();
            if (string.Equals(deletedDatasourceId, datasourceId, StringComparison.OrdinalIgnoreCase))
            {
                matchedInstance = deletedInstance;
                break;
            }
        }

        if (matchedInstance is null)
        {
            throw new KeyNotFoundException(
                $"No soft-deleted backup instance found with datasource ID '{datasourceId}' in vault '{vaultName}'. " +
                "Verify the datasource ID is correct and the item is in a soft-deleted state.");
        }

        var undeleteOperation = await matchedInstance.UndeleteAsync(WaitUntil.Started, cancellationToken);
        var jobId = ExtractJobIdFromOperation(undeleteOperation.GetRawResponse());
        var monitorMessage = string.IsNullOrWhiteSpace(jobId)
            ? $"Restore operation started, but no backup job ID was returned. Operation ID: '{undeleteOperation.Id}'."
            : $"Use 'azurebackup job get --job {jobId}' to monitor progress.";

        return new OperationResult("Accepted", jobId,
            $"Restore of soft-deleted backup instance for datasource '{datasourceId}' has been started in vault '{vaultName}'. {monitorMessage}");
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
        var jobResourceId = DataProtectionBackupJobResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName, jobId);
        var jobResource = armClient.GetDataProtectionBackupJobResource(jobResourceId);

        try
        {
            var job = await jobResource.GetAsync(cancellationToken);
            return MapToJobInfo(job.Value.Data);
        }
        catch (FormatException)
        {
            // The Azure SDK may throw FormatException when parsing the job's duration field
            // (e.g., non-standard ISO 8601 durations from the service). Fall back to listing
            // all jobs and matching by ID to work around this SDK limitation.
            var jobs = await ListJobsAsync(vaultName, resourceGroup, subscription, tenant, retryPolicy, cancellationToken);
            return jobs.FirstOrDefault(j => j.Name == jobId)
                ?? throw new InvalidOperationException($"Job '{jobId}' not found. The SDK cannot parse this job's duration field.");
        }
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
        var vaultId = DataProtectionBackupVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetDataProtectionBackupVaultResource(vaultId);
        var collection = vaultResource.GetDataProtectionBackupJobs();

        var jobs = new List<BackupJobInfo>();
        var enumerator = collection.GetAllAsync(cancellationToken).GetAsyncEnumerator(cancellationToken);
        try
        {
            while (true)
            {
                try
                {
                    if (!await enumerator.MoveNextAsync())
                    {
                        break;
                    }

                    jobs.Add(MapToJobInfo(enumerator.Current.Data));
                }
                catch (FormatException)
                {
                    // The Azure SDK may throw FormatException when deserializing jobs with
                    // non-standard ISO 8601 duration fields (XmlConvert.ToTimeSpan limitation).
                    // Return the jobs collected so far rather than failing the entire list.
                    break;
                }
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
        }

        return jobs;
    }

    public async Task<RecoveryPointInfo> GetRecoveryPointAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string recoveryPointId, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(protectedItemName), protectedItemName),
            (nameof(recoveryPointId), recoveryPointId));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var rpId = DataProtectionBackupRecoveryPointResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName, protectedItemName, recoveryPointId);
        var rpResource = armClient.GetDataProtectionBackupRecoveryPointResource(rpId);
        var rp = await rpResource.GetAsync(cancellationToken);

        return MapToRecoveryPointInfo(rp.Value.Data);
    }

    public async Task<List<RecoveryPointInfo>> ListRecoveryPointsAsync(
        string vaultName, string resourceGroup, string subscription,
        string protectedItemName, string? tenant,
        RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(protectedItemName), protectedItemName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var instanceId = DataProtectionBackupInstanceResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName, protectedItemName);
        var instanceResource = armClient.GetDataProtectionBackupInstanceResource(instanceId);
        var collection = instanceResource.GetDataProtectionBackupRecoveryPoints();

        var points = new List<RecoveryPointInfo>();
        await foreach (var rp in collection.GetAllAsync(cancellationToken: cancellationToken))
        {
            points.Add(MapToRecoveryPointInfo(rp.Data));
        }

        return points;
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

        if (!string.IsNullOrEmpty(redundancy))
        {
            throw new ArgumentException(
                "Storage redundancy cannot be changed after a Data Protection (DPP) vault is created. " +
                "Set --storage-type during vault creation instead.");
        }

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var vaultId = DataProtectionBackupVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetDataProtectionBackupVaultResource(vaultId);
        var vault = await vaultResource.GetAsync(cancellationToken);

        var patchData = new DataProtectionBackupVaultPatch();

        if (!string.IsNullOrEmpty(identityType))
        {
            patchData.Identity = new Azure.ResourceManager.Models.ManagedServiceIdentity(
                ParseIdentityType(identityType));
        }

        var securitySettings = new BackupVaultSecuritySettings();
        var hasSecurityUpdate = false;

        if (!string.IsNullOrEmpty(softDelete))
        {
            var softDeleteSettings = new BackupVaultSoftDeleteSettings
            {
                State = new BackupVaultSoftDeleteState(softDelete)
            };
            if (double.TryParse(softDeleteRetentionDays, out var retDays))
            {
                softDeleteSettings.RetentionDurationInDays = retDays;
            }
            securitySettings.SoftDeleteSettings = softDeleteSettings;
            hasSecurityUpdate = true;
        }

        if (!string.IsNullOrEmpty(immutabilityState))
        {
            securitySettings.ImmutabilityState = new BackupVaultImmutabilityState(immutabilityState);
            hasSecurityUpdate = true;
        }

        if (hasSecurityUpdate)
        {
            patchData.Properties ??= new DataProtectionBackupVaultPatchProperties();
            patchData.Properties.SecuritySettings = securitySettings;
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

        return new OperationResult("Succeeded", null, $"Vault '{vaultName}' updated successfully.");
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
        var vaultId = DataProtectionBackupVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetDataProtectionBackupVaultResource(vaultId);
        var collection = vaultResource.GetDataProtectionBackupPolicies();

        var profile = DppDatasourceRegistry.Resolve(workloadType);
        var policyProperties = Policy.DppPolicyBuilder.Build(request, profile);
        var policyData = new DataProtectionBackupPolicyData { Properties = policyProperties };

        try
        {
            await collection.CreateOrUpdateAsync(WaitUntil.Completed, policyName, policyData, cancellationToken);
        }
        catch (RequestFailedException ex) when (ex.Status == 400 && ex.ErrorCode == "UserErrorBMSUpdatePolicyNotSupported")
        {
            // DPP does not support updating an existing policy via CreateOrUpdate.
            // If the policy already exists, treat it as success (idempotent create).
            return new OperationResult("Succeeded", null, $"Policy '{policyName}' already exists in vault '{vaultName}'.");
        }

        return new OperationResult("Succeeded", null, $"Policy '{policyName}' created in vault '{vaultName}'.");
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
        var vaultId = DataProtectionBackupVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetDataProtectionBackupVaultResource(vaultId);

        // Pre-check current state. Re-enabling an already-enabled CRR returns a generic
        // CloudInternalError on the DPP backend, which is indistinguishable from a real
        // platform failure - so we avoid the call entirely when CRR is already enabled.
        var vault = await vaultResource.GetAsync(cancellationToken);
        if (vault.Value.Data.Properties?.FeatureSettings?.CrossRegionRestoreState == CrossRegionRestoreState.Enabled)
        {
            return new OperationResult("Succeeded", null, $"Cross-Region Restore is already enabled for vault '{vaultName}'.");
        }

        var patchData = new DataProtectionBackupVaultPatch
        {
            Properties = new DataProtectionBackupVaultPatchProperties
            {
                FeatureSettings = new BackupVaultFeatureSettings
                {
                    CrossRegionRestoreState = CrossRegionRestoreState.Enabled
                }
            }
        };
        await vaultResource.UpdateAsync(WaitUntil.Completed, patchData, cancellationToken);

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
        var vaultId = DataProtectionBackupVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetDataProtectionBackupVaultResource(vaultId);

        var patchData = new DataProtectionBackupVaultPatch
        {
            Properties = new DataProtectionBackupVaultPatchProperties
            {
                SecuritySettings = new BackupVaultSecuritySettings
                {
                    ImmutabilityState = new BackupVaultImmutabilityState(immutabilityState)
                }
            }
        };
        await vaultResource.UpdateAsync(WaitUntil.Completed, patchData, cancellationToken);

        return new OperationResult("Succeeded", null, $"Immutability set to '{immutabilityState}' for vault '{vaultName}'.");
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
        var vaultId = DataProtectionBackupVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetDataProtectionBackupVaultResource(vaultId);

        var softDeleteSettings = new BackupVaultSoftDeleteSettings
        {
            State = new BackupVaultSoftDeleteState(softDeleteState)
        };

        if (double.TryParse(softDeleteRetentionDays, out var retentionDays))
        {
            softDeleteSettings.RetentionDurationInDays = retentionDays;
        }

        var patchData = new DataProtectionBackupVaultPatch
        {
            Properties = new DataProtectionBackupVaultPatchProperties
            {
                SecuritySettings = new BackupVaultSecuritySettings
                {
                    SoftDeleteSettings = softDeleteSettings
                }
            }
        };
        await vaultResource.UpdateAsync(WaitUntil.Completed, patchData, cancellationToken);

        return new OperationResult("Succeeded", null, $"Soft delete set to '{softDeleteState}' for vault '{vaultName}'.");
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
        var vaultId = DataProtectionBackupVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetDataProtectionBackupVaultResource(vaultId);
        var proxyCollection = vaultResource.GetResourceGuardProxyBaseResources();

        var proxyData = new ResourceGuardProxyBaseResourceData
        {
            Properties = new ResourceGuardProxyBase
            {
                ResourceGuardResourceId = resourceGuardId
            }
        };

        await proxyCollection.CreateOrUpdateAsync(
            WaitUntil.Completed,
            "DppResourceGuardProxy",
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
        var vaultId = DataProtectionBackupVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetDataProtectionBackupVaultResource(vaultId);

        var proxyResponse = await vaultResource.GetResourceGuardProxyBaseResourceAsync("DppResourceGuardProxy", cancellationToken);
        await proxyResponse.Value.DeleteAsync(WaitUntil.Completed, cancellationToken);

        return new OperationResult("Succeeded", null, $"Multi-User Authorization disabled on vault '{vaultName}'.");
    }


    private static BackupVaultInfo MapToVaultInfo(DataProtectionBackupVaultData data, string? resourceGroup)
    {
        var securitySettings = data.Properties?.SecuritySettings;
        var softDeleteSettings = securitySettings?.SoftDeleteSettings;
        var identityType = data.Identity?.ManagedServiceIdentityType.ToString();

        return new BackupVaultInfo(
            data.Id?.ToString(),
            data.Name,
            VaultType,
            data.Location.Name,
            resourceGroup,
            data.Properties?.ProvisioningState?.ToString(),
            null,
            data.Properties?.StorageSettings?.FirstOrDefault()?.StorageSettingType?.ToString(),
            data.Properties?.StorageSettings?.FirstOrDefault()?.StorageSettingType?.ToString(),
            softDeleteSettings?.State?.ToString(),
            softDeleteSettings?.RetentionDurationInDays.HasValue == true ? (int)softDeleteSettings.RetentionDurationInDays.Value : null,
            securitySettings?.ImmutabilityState?.ToString(),
            identityType,
            data.Tags?.ToDictionary(t => t.Key, t => t.Value));
    }

    private static ProtectedItemInfo MapToProtectedItemInfo(DataProtectionBackupInstanceData data)
    {
        return new ProtectedItemInfo(
            data.Id?.ToString(),
            data.Name,
            VaultType,
            data.Properties?.ProtectionStatus?.Status?.ToString(),
            data.Properties?.DataSourceInfo?.DataSourceType,
            data.Properties?.DataSourceInfo?.ResourceId?.ToString(),
            data.Properties?.PolicyInfo?.PolicyId?.Name,
            null,
            null);
    }

    private static BackupPolicyInfo MapToPolicyInfo(DataProtectionBackupPolicyData data)
    {
        var datasourceTypes = data.Properties is DataProtectionBackupPolicyPropertiesBase props
            ? props.DataSourceTypes?.ToList() as IReadOnlyList<string>
            : null;

        string? scheduleFrequency = null;
        string? scheduleTime = null;
        int? dailyRetentionDays = null;

        if (data.Properties is RuleBasedBackupPolicy ruleBasedPolicy)
        {
            foreach (var rule in ruleBasedPolicy.PolicyRules)
            {
                if (rule is DataProtectionBackupRule backupRule &&
                    backupRule.Trigger is ScheduleBasedBackupTriggerContext scheduleTrigger)
                {
                    var repeatingInterval = scheduleTrigger.Schedule?.RepeatingTimeIntervals?.FirstOrDefault();
                    if (repeatingInterval != null)
                    {
                        // Parse repeating interval format: R/{startTime}/{interval}
                        var parts = repeatingInterval.Split('/');
                        if (parts.Length >= 3)
                        {
                            if (DateTimeOffset.TryParse(parts[1], out var startTime))
                            {
                                scheduleTime = startTime.ToString("HH:mm");
                            }

                            scheduleFrequency = parts[2]; // e.g. "PT4H", "P1D", "P1W"
                        }
                    }
                }
                else if (rule is DataProtectionRetentionRule retentionRule && retentionRule.IsDefault == true)
                {
                    var lifecycle = retentionRule.Lifecycles?.FirstOrDefault();
                    if (lifecycle?.DeleteAfter is DataProtectionBackupAbsoluteDeleteSetting deleteSetting)
                    {
                        dailyRetentionDays = (int)deleteSetting.Duration.TotalDays;
                    }
                }
            }
        }

        return new BackupPolicyInfo(
            data.Id?.ToString(),
            data.Name,
            VaultType,
            datasourceTypes,
            null,
            scheduleFrequency,
            scheduleTime,
            dailyRetentionDays);
    }

    private static BackupJobInfo MapToJobInfo(DataProtectionBackupJobData data)
    {
        return new BackupJobInfo(
            data.Id?.ToString(),
            data.Name,
            VaultType,
            data.Properties?.OperationCategory,
            data.Properties?.Status,
            data.Properties?.StartOn,
            data.Properties?.EndOn,
            data.Properties?.DataSourceType,
            data.Properties?.DataSourceName);
    }

    private static RecoveryPointInfo MapToRecoveryPointInfo(DataProtectionBackupRecoveryPointData data)
    {
        DateTimeOffset? rpTime = null;
        string? rpType = null;

        if (data.Properties is DataProtectionBackupDiscreteRecoveryPointProperties rpProps)
        {
            rpTime = rpProps.RecoverOn;
            rpType = rpProps.RecoveryPointType;
        }

        return new RecoveryPointInfo(
            data.Id?.ToString(),
            data.Name,
            VaultType,
            rpTime,
            rpType);
    }

    private static string? ExtractJobIdFromOperation(Response response)
    {
        if (response.Headers.TryGetValue("Azure-AsyncOperation", out var asyncOpUrl) && !string.IsNullOrEmpty(asyncOpUrl))
        {
            var uri = new Uri(asyncOpUrl);
            var segments = uri.AbsolutePath.Split('/');
            return segments.Length > 0 ? segments[^1] : null;
        }

        return null;
    }
}
