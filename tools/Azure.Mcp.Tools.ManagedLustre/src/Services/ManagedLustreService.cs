// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.ManagedLustre.Models;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Resources.Models;
using Azure.ResourceManager.StorageCache;
using Azure.ResourceManager.StorageCache.Models;


namespace Azure.Mcp.Tools.ManagedLustre.Services;

public sealed class ManagedLustreService(ISubscriptionService subscriptionService, IResourceGroupService resourceGroupService, ITenantService tenantService) : BaseAzureService(tenantService), IManagedLustreService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly IResourceGroupService _resourceGroupService = resourceGroupService;

    public async Task<List<LustreFileSystem>> ListFileSystemsAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var results = new List<LustreFileSystem>();

        try
        {
            if (!string.IsNullOrWhiteSpace(resourceGroup))
            {
                var rg = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy, cancellationToken) ?? throw new Exception($"Resource group '{resourceGroup}' not found");
                foreach (var fs in rg.GetAmlFileSystems())
                {
                    results.Add(Map(fs));
                }
                return results;
            }
            else
            {
                var sub = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken) ?? throw new Exception($"Subscription '{subscription}' not found");
                await foreach (var fs in sub.GetAmlFileSystemsAsync(cancellationToken))
                {
                    results.Add(Map(fs));
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error listing AMLFS file systems in subscription '{subscription}': {ex.Message}", ex);
        }

        return results;
    }

    private static LustreFileSystem Map(AmlFileSystemResource fs)
    {
        var data = fs.Data;
        return new LustreFileSystem(
            data.Name,
            fs.Id.ToString(),
            fs.Id.ResourceGroupName,
            fs.Id.SubscriptionId,
            data.Location,
            data.ProvisioningState?.ToString(),
            data.Health?.ToString(),
            data.ClientInfo?.MgsAddress,
            data.SkuName,
            data.StorageCapacityTiB.HasValue ? Convert.ToInt64(Math.Round(data.StorageCapacityTiB.Value)) : null,
            data.MaintenanceWindow?.DayOfWeek?.ToString(),
            data.MaintenanceWindow?.TimeOfDayUTC?.ToString(),
            data.FilesystemSubnet,
            data.Hsm?.Settings?.Container,
            data.Hsm?.Settings?.LoggingContainer,
            data.RootSquashSettings?.Mode?.ToString(),
            data.RootSquashSettings?.NoSquashNidLists,
            data.RootSquashSettings?.SquashUID,
            data.RootSquashSettings?.SquashGID
        );
    }

    private static List<ManagedLustreSkuCapability> MapCapabilities(IEnumerable<StorageCacheSkuCapability>? caps)
    {
        var list = new List<ManagedLustreSkuCapability>();
        if (caps is null)
            return list;
        foreach (var cap in caps)
        {
            var name = cap?.Name;
            if (string.IsNullOrWhiteSpace(name))
                continue;
            list.Add(new ManagedLustreSkuCapability(name!, cap?.Value ?? string.Empty));
        }
        return list;
    }

    private static AmlFileSystemPropertiesMaintenanceWindow GenerateMaintenanceWindow(string maintenanceDay, string maintenanceTime)
    {
        MaintenanceDayOfWeekType dayEnum;

        if (!Enum.TryParse<MaintenanceDayOfWeekType>(maintenanceDay, true, out dayEnum))
        {
            throw new ArgumentException($"Invalid maintenance day '{maintenanceDay}'. Allowed values: Monday..Sunday");
        }

        return new AmlFileSystemPropertiesMaintenanceWindow
        {
            DayOfWeek = dayEnum,
            TimeOfDayUTC = maintenanceTime
        };
    }

    private static AmlFileSystemRootSquashSettings GenerateRootSquashSettings(string rootSquashMode, string? noSquashNidLists, long? squashUid, long? squashGid)
    {
        // Root squash: default to None if not provided; when not None, ensure required squash parameters are provided
        var rootSquashSettings = new AmlFileSystemRootSquashSettings
        {
            Mode = AmlFileSystemSquashMode.None
        };

        if (!string.IsNullOrWhiteSpace(rootSquashMode))
        {
            AmlFileSystemSquashMode modeParsed = rootSquashMode;

            // When a squash mode other than None is specified, UID and GID must be provided
            if (modeParsed != AmlFileSystemSquashMode.None)
            {
                if (!squashUid.HasValue)
                {
                    throw new ArgumentException("squash-uid must be provided when root-squash-mode is not None.");
                }
                if (!squashGid.HasValue)
                {
                    throw new ArgumentException("squash-gid must be provided when root-squash-mode is not None.");
                }
                if (string.IsNullOrWhiteSpace(noSquashNidLists))
                {
                    throw new ArgumentException("no-squash-nid-list must be provided when root-squash-mode is not None.");
                }
                if (squashUid.Value < 0)
                {
                    throw new ArgumentException("squash-uid must be a non-negative integer.");
                }
                if (squashGid.Value < 0)
                {
                    throw new ArgumentException("squash-gid must be a non-negative integer.");
                }

                rootSquashSettings = new AmlFileSystemRootSquashSettings
                {
                    Mode = modeParsed,
                    NoSquashNidLists = noSquashNidLists,
                    SquashUID = squashUid,
                    SquashGID = squashGid
                };
            }
        }

        return rootSquashSettings;
    }

    private static AmlFileSystemPropertiesHsm GenerateHsmSettings(string? hsmContainer, string? hsmLogContainer, string? importPrefix)
    {
        // HSM settings if provided
        if (!string.IsNullOrWhiteSpace(hsmContainer) || !string.IsNullOrWhiteSpace(hsmLogContainer) || !string.IsNullOrWhiteSpace(importPrefix))
        {
            if (string.IsNullOrWhiteSpace(hsmContainer) || string.IsNullOrWhiteSpace(hsmLogContainer))
            {
                throw new ArgumentException("Both hsm-container and hsm-log-container must be provided when specifying HSM settings.");
            }

            var hsmSettings = new AmlFileSystemHsmSettings(hsmContainer, hsmLogContainer);
            if (!string.IsNullOrWhiteSpace(importPrefix))
            {
                hsmSettings.ImportPrefix = importPrefix;
            }

            return new AmlFileSystemPropertiesHsm
            {
                Settings = hsmSettings
            };
        }
        else
        {
            return new AmlFileSystemPropertiesHsm
            {
                Settings = null
            };
        }
    }
    public async Task<int> GetRequiredAmlFSSubnetsSize(string subscription,
    string sku, int size,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var sub = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken) ?? throw new Exception($"Subscription '{subscription}' not found");
        var fileSystemSizeContent = new RequiredAmlFileSystemSubnetsSizeContent
        {
            SkuName = sku,
            StorageCapacityTiB = size
        };

        try
        {
            var sdkResult = await sub.GetRequiredAmlFSSubnetsSizeAsync(fileSystemSizeContent, cancellationToken);
            var numberOfRequiredIPs = sdkResult.Value.FilesystemSubnetSize ?? throw new Exception($"Failed to retrieve the number of IPs");
            return numberOfRequiredIPs;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving required subnet size: {ex.Message}", ex);
        }
    }

    public async Task<List<ManagedLustreSkuInfo>> SkuGetInfoAsync(
        string subscription,
        string? tenant = null,
        string? location = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var sub = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken) ?? throw new Exception($"Subscription '{subscription}' not found");

        try
        {
            var results = new List<ManagedLustreSkuInfo>();

            await foreach (var sku in sub.GetStorageCacheSkusAsync(cancellationToken))
            {

                if (sku is null ||
                    !string.Equals(sku.ResourceType, "amlFilesystems", StringComparison.OrdinalIgnoreCase) ||
                    sku.LocationInfo is null ||
                    string.IsNullOrEmpty(sku.Name))
                    continue;

                var name = sku.Name;
                var capabilities = MapCapabilities(sku.Capabilities);

                foreach (var locationInfo in sku.LocationInfo)
                {
                    var foundLocation = locationInfo?.Location;
                    if (string.IsNullOrWhiteSpace(foundLocation) || (!string.IsNullOrWhiteSpace(location) && !string.Equals(foundLocation, location, StringComparison.OrdinalIgnoreCase)))
                        continue;
                    var supportsZones = (locationInfo?.Zones?.Count ?? 0) > 1;

                    results.Add(new ManagedLustreSkuInfo(name, foundLocation, supportsZones, [.. capabilities]));
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Azure Managed Lustre SKUs for subscription '{subscription}': {ex.Message}", ex);
        }
    }


    public async Task<LustreFileSystem> CreateFileSystemAsync(
        string subscription,
        string resourceGroup,
        string name,
        string location,
        string sku,
        int sizeTiB,
        string subnetId,
        string zone,
        string maintenanceDay,
        string maintenanceTime,
        string? hsmContainer = null,
        string? hsmLogContainer = null,
        string? importPrefix = null,
        string? rootSquashMode = null,
        string? noSquashNidLists = null,
        long? squashUid = null,
        long? squashGid = null,
        bool enableCustomEncryption = false,
        string? keyUrl = null,
        string? sourceVaultId = null,
        string? userAssignedIdentityId = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription),
        (nameof(resourceGroup), resourceGroup),
        (nameof(name), name),
        (nameof(location), location),
        (nameof(sku), sku),
        (nameof(subnetId), subnetId)
        );

        var rg = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy, cancellationToken)
            ?? throw new Exception($"Resource group '{resourceGroup}' not found");
        var sub = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken)
            ?? throw new Exception($"Subscription '{subscription}' not found");

        var data = new AmlFileSystemData(new AzureLocation(location))
        {
            SkuName = sku,
            StorageCapacityTiB = sizeTiB,
            FilesystemSubnet = subnetId
        };

        // Validate zone support for the specified location before adding
        try
        {
            bool? supportsZones = null;

            await foreach (var loc in sub.GetLocationsAsync(cancellationToken: cancellationToken))
            {
                if (loc.Name.Equals(location, StringComparison.OrdinalIgnoreCase) ||
                    loc.DisplayName.Equals(location, StringComparison.OrdinalIgnoreCase))
                {
                    supportsZones = (loc.AvailabilityZoneMappings?.Count ?? 0) > 0;
                    break;
                }
            }

            if (supportsZones == false && !string.Equals(zone, "1", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Location '{location}' does not support availability zones; only zone '1' is allowed.");
            }
            if (supportsZones == true)
            {
                // Zone is required by command; add to zones
                data.Zones.Add(zone);
            }

        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to validate availability zones for location '{location}': {ex.Message}", ex);
        }

        data.RootSquashSettings = GenerateRootSquashSettings(rootSquashMode ?? "None", noSquashNidLists, squashUid, squashGid);
        data.MaintenanceWindow = GenerateMaintenanceWindow(maintenanceDay, maintenanceTime);
        data.Hsm = GenerateHsmSettings(hsmContainer, hsmLogContainer, importPrefix);

        // Encryption
        if (enableCustomEncryption)
        {
            if (string.IsNullOrWhiteSpace(keyUrl) || string.IsNullOrWhiteSpace(sourceVaultId))
            {
                throw new Exception("Both key-url and source-vault must be provided when custom-encryption is enabled.");
            }
            data.KeyEncryptionKey = new StorageCacheEncryptionKeyVaultKeyReference(
                new Uri(keyUrl!),
                new WritableSubResource { Id = new ResourceIdentifier(sourceVaultId!) });

            // Assign user-assigned managed identity for Key Vault access
            if (!string.IsNullOrWhiteSpace(userAssignedIdentityId))
            {
                data.Identity = new ManagedServiceIdentity(ManagedServiceIdentityType.UserAssigned)
                {
                    UserAssignedIdentities =
                    {
                        [new ResourceIdentifier(userAssignedIdentityId)] = new UserAssignedIdentity()
                    }
                };

            }
        }

        try
        {
            var collection = rg.GetAmlFileSystems();
            var createOperationResult = await collection.CreateOrUpdateAsync(WaitUntil.Completed, name, data, cancellationToken);
            var fileSystemResource = createOperationResult.Value;
            return Map(fileSystemResource);
        }
        catch (RequestFailedException rfe)
        {
            throw new Exception($"Failed to create AML file system '{name}': {rfe.Message}", rfe);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create AML file system '{name}': {ex.Message}", ex);
        }
    }

    public async Task<LustreFileSystem> UpdateFileSystemAsync(
        string subscription,
        string resourceGroup,
        string name,
        string? maintenanceDay = null,
        string? maintenanceTime = null,
        string? rootSquashMode = null,
        string? noSquashNidLists = null,
        long? squashUid = null,
        long? squashGid = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceGroup), resourceGroup), (nameof(name), name));

        var rg = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy, cancellationToken)
            ?? throw new Exception($"Resource group '{resourceGroup}' not found");

        try
        {
            var fs = await rg.GetAmlFileSystemAsync(name, cancellationToken);

            var patch = new AmlFileSystemPatch();

            // Maintenance window update if any value provided
            if (!string.IsNullOrWhiteSpace(maintenanceDay) && !string.IsNullOrWhiteSpace(maintenanceTime))
            {
                var maintenanceWindowConfiguration = GenerateMaintenanceWindow(maintenanceDay, maintenanceTime);

                patch.MaintenanceWindow = new AmlFileSystemUpdatePropertiesMaintenanceWindow
                {
                    DayOfWeek = maintenanceWindowConfiguration.DayOfWeek,
                    TimeOfDayUTC = maintenanceWindowConfiguration.TimeOfDayUTC
                };
            }

            // Root squash updates: if any related field provided, set RootSquashSettings accordingly
            if (!string.IsNullOrWhiteSpace(rootSquashMode))
            {
                patch.RootSquashSettings = GenerateRootSquashSettings(rootSquashMode ?? "None", noSquashNidLists, squashUid, squashGid);
            }

            var updateOperation = await fs.Value.UpdateAsync(WaitUntil.Completed, patch, cancellationToken);
            return Map(updateOperation.Value);
        }
        catch (RequestFailedException rfe)
        {
            throw new Exception($"Failed to update AML file system '{name}': {rfe.Message}", rfe);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to update AML file system '{name}': {ex.Message}", ex);
        }
    }

    public async Task<bool> CheckAmlFSSubnetAsync(
        string subscription,
        string sku,
        int size,
        string subnetId,
        string location,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(sku), sku), (nameof(subnetId), subnetId), (nameof(location), location));

        var sub = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken) ?? throw new Exception($"Subscription '{subscription}' not found");
        var content = new AmlFileSystemSubnetContent
        {
            FilesystemSubnet = subnetId,
            SkuName = sku,
            StorageCapacityTiB = size,
            Location = location
        };

        try
        {
            var response = await sub.CheckAmlFSSubnetsAsync(content, cancellationToken);
            var status = response.Status;
            var sizeIsValid = (HttpStatusCode)status == HttpStatusCode.OK;
            if (!sizeIsValid)
            {
                throw new RequestFailedException(status, "Unexpected status code from validation.");
            }

            return sizeIsValid;
        }
        catch (Exception ex)
        {
            if (ex is RequestFailedException rfe &&
                ((HttpStatusCode)rfe.Status == HttpStatusCode.BadRequest &&
                rfe.Message.Contains("a subnet with a minimum size of", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            else
            {
                throw new Exception($"Error validating AMLFS subnet: {ex.Message}", ex);
            }

        }
    }

    public async Task<string> CreateAutoexportJobAsync(
        string subscription,
        string resourceGroup,
        string filesystemName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(filesystemName), filesystemName));

        var rg = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy)
            ?? throw new Exception($"Resource group '{resourceGroup}' not found");

        try
        {
            var fs = await rg.GetAmlFileSystemAsync(filesystemName, cancellationToken: cancellationToken);
            if (fs?.Value == null)
            {
                throw new Exception($"Filesystem '{filesystemName}' not found in resource group '{resourceGroup}'");
            }

            // Generate job name from timestamp
            var jobName = $"autoexport-{DateTime.UtcNow:yyyyMMddHHmmss}";

            // Create auto export job data with filesystem location
            var autoExportJobData = new AutoExportJobData(fs.Value.Data.Location);

            // Create the auto export job
            var createOperation = await fs.Value.GetAutoExportJobs().CreateOrUpdateAsync(
                WaitUntil.Completed,
                jobName,
                autoExportJobData,
                cancellationToken);

            return createOperation.Value.Data.Name;
        }
        catch (RequestFailedException rfe)
        {
            throw new Exception($"Failed to create auto export job for filesystem '{filesystemName}': {rfe.Message}", rfe);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create auto export job for filesystem '{filesystemName}': {ex.Message}", ex);
        }
    }

    public async Task CancelAutoexportJobAsync(
        string subscription,
        string resourceGroup,
        string filesystemName,
        string jobName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(filesystemName), filesystemName),
            (nameof(jobName), jobName));

        var rg = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy)
            ?? throw new Exception($"Resource group '{resourceGroup}' not found");

        try
        {
            var fs = await rg.GetAmlFileSystemAsync(filesystemName, cancellationToken: cancellationToken);
            if (fs?.Value == null)
            {
                throw new Exception($"Filesystem '{filesystemName}' not found in resource group '{resourceGroup}'");
            }

            // Delete (cancel) the auto export job
            await fs.Value.GetAutoExportJobs().GetAsync(jobName, cancellationToken: cancellationToken);
            await fs.Value.GetAutoExportJobs().Get(jobName, cancellationToken).Value.DeleteAsync(
                WaitUntil.Completed,
                cancellationToken);
        }
        catch (RequestFailedException rfe) when (rfe.Status == 404)
        {
            throw new Exception($"Auto export job '{jobName}' not found for filesystem '{filesystemName}'", rfe);
        }
        catch (RequestFailedException rfe)
        {
            throw new Exception($"Failed to cancel auto export job '{jobName}' for filesystem '{filesystemName}': {rfe.Message}", rfe);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to cancel auto export job '{jobName}' for filesystem '{filesystemName}': {ex.Message}", ex);
        }
    }

    public async Task<Models.AutoexportJob> GetAutoexportJobAsync(
        string subscription,
        string resourceGroup,
        string filesystemName,
        string jobName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription, nameof(subscription));
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceGroup, nameof(resourceGroup));
        ArgumentException.ThrowIfNullOrWhiteSpace(filesystemName, nameof(filesystemName));
        ArgumentException.ThrowIfNullOrWhiteSpace(jobName, nameof(jobName));

        try
        {
            // Get the resource group
            var rg = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy);

            // Get the filesystem
            var fs = await rg.GetAmlFileSystems().GetAsync(filesystemName, cancellationToken: cancellationToken);

            // Get the auto export job
            var job = await fs.Value.GetAutoExportJobs().GetAsync(jobName, cancellationToken: cancellationToken);

            return new Models.AutoexportJob
            {
                Name = job.Value.Data.Name,
                Id = job.Value.Data.Id.ToString(),
                ProvisioningState = job.Value.Data.ProvisioningState?.ToString() ?? "Unknown"
            };
        }
        catch (Azure.RequestFailedException rfe) when (rfe.Status == 404)
        {
            throw new Exception($"Autoexport job '{jobName}' not found for filesystem '{filesystemName}' in resource group '{resourceGroup}'.", rfe);
        }
        catch (Azure.RequestFailedException rfe)
        {
            throw new Exception($"Failed to get auto export job '{jobName}' for filesystem '{filesystemName}': {rfe.Message}", rfe);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get auto export job '{jobName}' for filesystem '{filesystemName}': {ex.Message}", ex);
        }
    }

    public async Task<List<Models.AutoexportJob>> ListAutoexportJobsAsync(
        string subscription,
        string resourceGroup,
        string filesystemName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription, nameof(subscription));
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceGroup, nameof(resourceGroup));
        ArgumentException.ThrowIfNullOrWhiteSpace(filesystemName, nameof(filesystemName));

        try
        {
            // Get the resource group
            var rg = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy);

            // Get the filesystem
            var fs = await rg.GetAmlFileSystems().GetAsync(filesystemName, cancellationToken: cancellationToken);

            // Get all auto export jobs
            var jobs = new List<Models.AutoexportJob>();
            await foreach (var job in fs.Value.GetAutoExportJobs().GetAllAsync(cancellationToken: cancellationToken))
            {
                jobs.Add(new Models.AutoexportJob
                {
                    Name = job.Data.Name,
                    Id = job.Data.Id.ToString(),
                    ProvisioningState = job.Data.ProvisioningState?.ToString() ?? "Unknown"
                });
            }

            return jobs;
        }
        catch (Azure.RequestFailedException rfe) when (rfe.Status == 404)
        {
            throw new Exception($"Filesystem '{filesystemName}' not found in resource group '{resourceGroup}'.", rfe);
        }
        catch (Azure.RequestFailedException rfe)
        {
            throw new Exception($"Failed to list auto export jobs for filesystem '{filesystemName}': {rfe.Message}", rfe);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to list auto export jobs for filesystem '{filesystemName}': {ex.Message}", ex);
        }
    }

    public async Task DeleteAutoexportJobAsync(
        string subscription,
        string resourceGroup,
        string filesystemName,
        string jobName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(filesystemName), filesystemName),
            (nameof(jobName), jobName));

        var rg = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy)
            ?? throw new Exception($"Resource group '{resourceGroup}' not found");

        try
        {
            var fs = await rg.GetAmlFileSystemAsync(filesystemName, cancellationToken: cancellationToken);
            if (fs?.Value == null)
            {
                throw new Exception($"Filesystem '{filesystemName}' not found in resource group '{resourceGroup}'");
            }

            // Delete the auto export job
            await fs.Value.GetAutoExportJobs().GetAsync(jobName, cancellationToken: cancellationToken);
            await fs.Value.GetAutoExportJobs().Get(jobName, cancellationToken).Value.DeleteAsync(
                WaitUntil.Completed,
                cancellationToken);
        }
        catch (RequestFailedException rfe) when (rfe.Status == 404)
        {
            throw new Exception($"Auto export job '{jobName}' not found for filesystem '{filesystemName}'", rfe);
        }
        catch (RequestFailedException rfe)
        {
            throw new Exception($"Failed to delete auto export job '{jobName}' for filesystem '{filesystemName}': {rfe.Message}", rfe);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to delete auto export job '{jobName}' for filesystem '{filesystemName}': {ex.Message}", ex);
        }
    }

    public async Task<string> CreateAutoimportJobAsync(
        string subscription,
        string resourceGroup,
        string filesystemName,
        string? jobName = null,
        string? conflictResolutionMode = null,
        List<string>? autoimportPrefixes = null,
        string? adminStatus = null,
        bool? enableDeletions = null,
        long? maximumErrors = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(filesystemName), filesystemName));

        var rg = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy)
            ?? throw new Exception($"Resource group '{resourceGroup}' not found");

        try
        {
            var fs = await rg.GetAmlFileSystemAsync(filesystemName, cancellationToken: cancellationToken);
            if (fs?.Value == null)
            {
                throw new Exception($"Filesystem '{filesystemName}' not found in resource group '{resourceGroup}'");
            }

            // Generate job name from timestamp if not provided
            var actualJobName = jobName ?? $"autoimport-{DateTime.UtcNow:yyyyMMddHHmmss}";

            // Create auto import job data with filesystem location
            var autoImportJobData = new AutoImportJobData(fs.Value.Data.Location);

            // Set optional properties
            if (!string.IsNullOrWhiteSpace(conflictResolutionMode))
            {
                autoImportJobData.ConflictResolutionMode = conflictResolutionMode switch
                {
                    "Fail" => ConflictResolutionMode.Fail,
                    "Skip" => ConflictResolutionMode.Skip,
                    "OverwriteIfDirty" => ConflictResolutionMode.OverwriteIfDirty,
                    "OverwriteAlways" => ConflictResolutionMode.OverwriteAlways,
                    _ => throw new ArgumentException($"Invalid conflict resolution mode: {conflictResolutionMode}. Allowed values: Fail, Skip, OverwriteIfDirty, OverwriteAlways")
                };
            }

            if (autoimportPrefixes != null && autoimportPrefixes.Count > 0)
            {
                if (autoimportPrefixes.Count > 100)
                {
                    throw new ArgumentException("Maximum of 100 autoimport prefixes allowed");
                }
                foreach (var prefix in autoimportPrefixes)
                {
                    autoImportJobData.AutoImportPrefixes.Add(prefix);
                }
            }

            if (!string.IsNullOrWhiteSpace(adminStatus))
            {
                autoImportJobData.AdminStatus = adminStatus switch
                {
                    "Enable" => AutoImportJobPropertiesAdminStatus.Enable,
                    "Disable" => AutoImportJobPropertiesAdminStatus.Disable,
                    _ => throw new ArgumentException($"Invalid admin status: {adminStatus}. Allowed values: Enable, Disable")
                };
            }

            if (enableDeletions.HasValue)
            {
                autoImportJobData.EnableDeletions = enableDeletions.Value;
            }

            if (maximumErrors.HasValue)
            {
                autoImportJobData.MaximumErrors = maximumErrors.Value;
            }

            // Create the auto import job
            var createOperation = await fs.Value.GetAutoImportJobs().CreateOrUpdateAsync(
                WaitUntil.Completed,
                actualJobName,
                autoImportJobData,
                cancellationToken);

            return createOperation.Value.Data.Name;
        }
        catch (RequestFailedException rfe)
        {
            throw new Exception($"Failed to create auto import job for filesystem '{filesystemName}': {rfe.Message}", rfe);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create auto import job for filesystem '{filesystemName}': {ex.Message}", ex);
        }
    }

    public async Task<Models.AutoimportJob> GetAutoimportJobAsync(
        string subscription,
        string resourceGroup,
        string filesystemName,
        string jobName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription, nameof(subscription));
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceGroup, nameof(resourceGroup));
        ArgumentException.ThrowIfNullOrWhiteSpace(filesystemName, nameof(filesystemName));
        ArgumentException.ThrowIfNullOrWhiteSpace(jobName, nameof(jobName));

        try
        {
            // Get the resource group
            var rg = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy);

            // Get the filesystem
            var fs = await rg.GetAmlFileSystems().GetAsync(filesystemName, cancellationToken: cancellationToken);

            // Get the auto import job
            var job = await fs.Value.GetAutoImportJobs().GetAsync(jobName, cancellationToken: cancellationToken);

            return new Models.AutoimportJob
            {
                Name = job.Value.Data.Name,
                Id = job.Value.Data.Id.ToString(),
                ProvisioningState = job.Value.Data.ProvisioningState?.ToString() ?? "Unknown"
            };
        }
        catch (Azure.RequestFailedException rfe) when (rfe.Status == 404)
        {
            throw new Exception($"Autoimport job '{jobName}' not found for filesystem '{filesystemName}' in resource group '{resourceGroup}'.", rfe);
        }
        catch (Azure.RequestFailedException rfe)
        {
            throw new Exception($"Failed to get auto import job '{jobName}' for filesystem '{filesystemName}': {rfe.Message}", rfe);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get auto import job '{jobName}' for filesystem '{filesystemName}': {ex.Message}", ex);
        }
    }
}

