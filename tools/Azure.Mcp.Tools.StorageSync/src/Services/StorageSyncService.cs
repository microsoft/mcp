// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.StorageSync.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.StorageSync;
using Azure.ResourceManager.StorageSync.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.StorageSync.Services;

/// <summary>
/// Implementation of IStorageSyncService.
/// </summary>
public sealed class StorageSyncService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ILogger<StorageSyncService> logger) : BaseAzureResourceService(subscriptionService, tenantService), IStorageSyncService
{
    private readonly ILogger<StorageSyncService> _logger = logger;

    public async Task<List<StorageSyncServiceDataSchema>> ListStorageSyncServicesAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));

        var services = new List<StorageSyncServiceDataSchema>();

        if (!string.IsNullOrEmpty(resourceGroup))
        {
            ResourceGroupResource resourceGroupResource;
            try
            {
                var response = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
                resourceGroupResource = response.Value;
            }
            catch (RequestFailedException reqEx) when (reqEx.Status == (int)HttpStatusCode.NotFound)
            {
                _logger.LogWarning(reqEx,
                    "Resource group not found when listing Storage Sync services. ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                    resourceGroup, subscription);
                return [];
            }

            var collection = resourceGroupResource.GetStorageSyncServices();
            await foreach (var serviceResource in collection.WithCancellation(cancellationToken))
            {
                services.Add(StorageSyncServiceDataSchema.FromResource(serviceResource));
            }
        }
        else
        {
            await foreach (var serviceResource in subscriptionResource.GetStorageSyncServicesAsync(cancellationToken))
            {
                services.Add(StorageSyncServiceDataSchema.FromResource(serviceResource));
            }
        }

        return services;
    }

    public async Task<StorageSyncServiceDataSchema?> GetStorageSyncServiceAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName)
        );

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);

            return StorageSyncServiceDataSchema.FromResource(serviceResource.Value);
        }
        catch (RequestFailedException reqEx) when (reqEx.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(reqEx,
                "Storage Sync service not found. Service: {Service}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                storageSyncServiceName, resourceGroup, subscription);
            return null;
        }
    }

    public async Task<StorageSyncServiceDataSchema> CreateStorageSyncServiceAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string location,
        Dictionary<string, string>? tags = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(location), location));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

        var content = new StorageSyncServiceCreateOrUpdateContent(new(location));
        if (tags != null)
        {
            foreach (var tag in tags)
            {
                content.Tags.Add(tag.Key, tag.Value);
            }
        }

        var operation = await resourceGroupResource.Value.GetStorageSyncServices().CreateOrUpdateAsync(
            WaitUntil.Started,
            storageSyncServiceName,
            content,
            cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        _logger.LogInformation(
            "Successfully created Storage Sync service. Service: {Service}, ResourceGroup: {ResourceGroup}, Location: {Location}",
            storageSyncServiceName, resourceGroup, location);

        return StorageSyncServiceDataSchema.FromResource(operation.Value);
    }

    public async Task<StorageSyncServiceDataSchema> UpdateStorageSyncServiceAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string? incomingTrafficPolicy = null,
        Dictionary<string, object>? tags = null,
        string? identityType = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);

        var patch = new StorageSyncServicePatch();

        // Update incoming traffic policy
        if (!string.IsNullOrEmpty(incomingTrafficPolicy))
        {
            patch.IncomingTrafficPolicy = new(incomingTrafficPolicy);
        }

        // Update tags
        if (tags != null)
        {
            foreach (var tag in tags)
            {
                patch.Tags[tag.Key] = tag.Value?.ToString() ?? string.Empty;
            }
        }

        // Update identity
        if (!string.IsNullOrEmpty(identityType))
        {
            patch.Identity = new(new(identityType));
        }

        var operation = await serviceResource.Value.UpdateAsync(WaitUntil.Started, patch, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        _logger.LogInformation(
            "Successfully updated Storage Sync service. Service: {Service}, ResourceGroup: {ResourceGroup}",
            storageSyncServiceName, resourceGroup);

        return StorageSyncServiceDataSchema.FromResource(operation.Value);
    }

    public async Task DeleteStorageSyncServiceAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);

        var deleteOperation = await serviceResource.Value.DeleteAsync(WaitUntil.Started, cancellationToken);
        await WaitForLroCompletionAsync(deleteOperation, cancellationToken);

        _logger.LogInformation(
            "Successfully deleted Storage Sync service. Service: {Service}, ResourceGroup: {ResourceGroup}",
            storageSyncServiceName, resourceGroup);
    }

    // Sync Group Operations
    public async Task<List<SyncGroupDataSchema>> ListSyncGroupsAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);

        var syncGroups = new List<SyncGroupDataSchema>();
        await foreach (var syncGroupResource in serviceResource.Value.GetStorageSyncGroups().WithCancellation(cancellationToken))
        {
            syncGroups.Add(SyncGroupDataSchema.FromResource(syncGroupResource));
        }

        return syncGroups;
    }

    public async Task<SyncGroupDataSchema?> GetSyncGroupAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName));

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
            var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);

            return SyncGroupDataSchema.FromResource(syncGroupResource.Value);
        }
        catch (RequestFailedException reqEx) when (reqEx.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(reqEx, "Sync Group not found: {SyncGroup}", syncGroupName);
            return null;
        }
    }

    public async Task<SyncGroupDataSchema> CreateSyncGroupAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);

        var operation = await serviceResource.Value.GetStorageSyncGroups().CreateOrUpdateAsync(WaitUntil.Started, syncGroupName, new(), cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        _logger.LogInformation("Successfully created Sync Group: {SyncGroup}", syncGroupName);
        return SyncGroupDataSchema.FromResource(operation.Value);
    }

    public async Task DeleteSyncGroupAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
        var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);

        var deleteOperation = await syncGroupResource.Value.DeleteAsync(WaitUntil.Started, cancellationToken);
        await WaitForLroCompletionAsync(deleteOperation, cancellationToken);

        _logger.LogInformation("Successfully deleted Sync Group: {SyncGroup}", syncGroupName);
    }

    // Cloud Endpoint Operations
    public async Task<List<CloudEndpointDataSchema>> ListCloudEndpointsAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
        var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);

        var endpoints = new List<CloudEndpointDataSchema>();
        await foreach (var endpointResource in syncGroupResource.Value.GetCloudEndpoints().WithCancellation(cancellationToken))
        {
            endpoints.Add(CloudEndpointDataSchema.FromResource(endpointResource));
        }

        return endpoints;
    }

    public async Task<CloudEndpointDataSchema?> GetCloudEndpointAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string cloudEndpointName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName),
            (nameof(cloudEndpointName), cloudEndpointName));

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
            var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);
            var endpointResource = await syncGroupResource.Value.GetCloudEndpoints().GetAsync(cloudEndpointName, cancellationToken);

            return CloudEndpointDataSchema.FromResource(endpointResource.Value);
        }
        catch (RequestFailedException reqEx) when (reqEx.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(reqEx, "Cloud Endpoint not found: {Endpoint}", cloudEndpointName);
            return null;
        }
    }

    public async Task<CloudEndpointDataSchema> CreateCloudEndpointAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string cloudEndpointName,
        string storageAccountResourceId,
        string azureFileShareName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName),
            (nameof(cloudEndpointName), cloudEndpointName),
            (nameof(storageAccountResourceId), storageAccountResourceId),
            (nameof(azureFileShareName), azureFileShareName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));

        // Get subscription data to access tenant ID
        var subscriptionData = await subscriptionResource.GetAsync(cancellationToken);

        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
        var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);

        // Get the tenant ID - use provided tenant or get from subscription
        Guid? storageAccountTenantId = null;
        if (tenant != null && Guid.TryParse(tenant, out var parsedTenantId))
        {
            storageAccountTenantId = parsedTenantId;
        }
        else if (subscriptionData.Value.Data.TenantId.HasValue)
        {
            storageAccountTenantId = subscriptionData.Value.Data.TenantId.Value;
        }

        var content = new CloudEndpointCreateOrUpdateContent
        {
            StorageAccountResourceId = new(storageAccountResourceId),
            AzureFileShareName = azureFileShareName,
            StorageAccountTenantId = storageAccountTenantId
        };
        var operation = await syncGroupResource.Value.GetCloudEndpoints().CreateOrUpdateAsync(
            WaitUntil.Started, cloudEndpointName, content, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        _logger.LogInformation("Successfully created Cloud Endpoint: {Endpoint}", cloudEndpointName);
        return CloudEndpointDataSchema.FromResource(operation.Value);
    }

    public async Task DeleteCloudEndpointAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string cloudEndpointName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName),
            (nameof(cloudEndpointName), cloudEndpointName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
        var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);
        var endpointResource = await syncGroupResource.Value.GetCloudEndpoints().GetAsync(cloudEndpointName, cancellationToken);

        var deleteOperation = await endpointResource.Value.DeleteAsync(WaitUntil.Started, cancellationToken);
        await WaitForLroCompletionAsync(deleteOperation, cancellationToken);

        _logger.LogInformation("Successfully deleted Cloud Endpoint: {Endpoint}", cloudEndpointName);
    }

    public async Task TriggerChangeDetectionAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string cloudEndpointName,
        string directoryPath,
        string? changeDetectionMode = null,
        IList<string>? paths = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName),
            (nameof(cloudEndpointName), cloudEndpointName),
            (nameof(directoryPath), directoryPath));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
        var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);
        var endpointResource = await syncGroupResource.Value.GetCloudEndpoints().GetAsync(cloudEndpointName, cancellationToken);

        var content = new TriggerChangeDetectionContent
        {
            DirectoryPath = directoryPath
        };

        // Set change detection mode if provided
        if (!string.IsNullOrEmpty(changeDetectionMode))
        {
            content.ChangeDetectionMode = new(changeDetectionMode);
        }

        // Add paths if provided
        if (paths != null)
        {
            foreach (var path in paths)
            {
                content.Paths.Add(path);
            }
        }

        var changeDetectionOperation = await endpointResource.Value.TriggerChangeDetectionAsync(WaitUntil.Started, content, cancellationToken);
        await WaitForLroCompletionAsync(changeDetectionOperation, cancellationToken);

        _logger.LogInformation("Successfully triggered change detection for Cloud Endpoint: {Endpoint}", cloudEndpointName);
    }

    // Server Endpoint Operations
    public async Task<List<ServerEndpointDataSchema>> ListServerEndpointsAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
        var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);

        var endpoints = new List<ServerEndpointDataSchema>();
        await foreach (var endpointResource in syncGroupResource.Value.GetStorageSyncServerEndpoints().WithCancellation(cancellationToken))
        {
            endpoints.Add(ServerEndpointDataSchema.FromResource(endpointResource));
        }

        return endpoints;
    }

    public async Task<ServerEndpointDataSchema?> GetServerEndpointAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string serverEndpointName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName),
            (nameof(serverEndpointName), serverEndpointName));

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
            var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);
            var endpointResource = await syncGroupResource.Value.GetStorageSyncServerEndpoints().GetAsync(serverEndpointName, cancellationToken);

            return ServerEndpointDataSchema.FromResource(endpointResource.Value);
        }
        catch (RequestFailedException reqEx) when (reqEx.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(reqEx, "Server Endpoint not found: {Endpoint}", serverEndpointName);
            return null;
        }
    }

    public async Task<ServerEndpointDataSchema> CreateServerEndpointAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string serverEndpointName,
        string serverResourceId,
        string serverLocalPath,
        bool? enableCloudTiering = null,
        int? volumeFreeSpacePercent = null,
        int? tierFilesOlderThanDays = null,
        string? localCacheMode = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName),
            (nameof(serverEndpointName), serverEndpointName),
            (nameof(serverResourceId), serverResourceId),
            (nameof(serverLocalPath), serverLocalPath));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
        var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);

        var content = new StorageSyncServerEndpointCreateOrUpdateContent
        {
            ServerResourceId = new(serverResourceId),
            ServerLocalPath = serverLocalPath
        };

        if (enableCloudTiering.HasValue)
        {
            content.CloudTiering = enableCloudTiering.Value ? StorageSyncFeatureStatus.On : StorageSyncFeatureStatus.Off;
        }
        if (volumeFreeSpacePercent.HasValue)
        {
            content.VolumeFreeSpacePercent = volumeFreeSpacePercent;
        }
        if (tierFilesOlderThanDays.HasValue)
        {
            content.TierFilesOlderThanDays = tierFilesOlderThanDays;
        }
        if (!string.IsNullOrEmpty(localCacheMode))
        {
            content.LocalCacheMode = new(localCacheMode);
        }

        var operation = await syncGroupResource.Value.GetStorageSyncServerEndpoints().CreateOrUpdateAsync(
            WaitUntil.Started, serverEndpointName, content, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        _logger.LogInformation("Successfully created Server Endpoint: {Endpoint}", serverEndpointName);
        return ServerEndpointDataSchema.FromResource(operation.Value);
    }

    public async Task<ServerEndpointDataSchema> UpdateServerEndpointAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string serverEndpointName,
        bool? cloudTiering = null,
        int? volumeFreeSpacePercent = null,
        int? tierFilesOlderThanDays = null,
        string? localCacheMode = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName),
            (nameof(serverEndpointName), serverEndpointName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
        var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);
        var endpointResource = await syncGroupResource.Value.GetStorageSyncServerEndpoints().GetAsync(serverEndpointName, cancellationToken);

        var patch = new StorageSyncServerEndpointPatch();
        if (cloudTiering.HasValue)
        {
            patch.CloudTiering = cloudTiering.Value ? StorageSyncFeatureStatus.On : StorageSyncFeatureStatus.Off;
        }
        if (volumeFreeSpacePercent.HasValue)
        {
            patch.VolumeFreeSpacePercent = volumeFreeSpacePercent;
        }
        if (tierFilesOlderThanDays.HasValue)
        {
            patch.TierFilesOlderThanDays = tierFilesOlderThanDays;
        }
        if (!string.IsNullOrEmpty(localCacheMode))
        {
            patch.LocalCacheMode = new(localCacheMode);
        }

        var operation = await endpointResource.Value.UpdateAsync(WaitUntil.Started, patch, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        _logger.LogInformation("Successfully updated Server Endpoint: {Endpoint}", serverEndpointName);
        return ServerEndpointDataSchema.FromResource(operation.Value);
    }

    public async Task DeleteServerEndpointAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string serverEndpointName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName),
            (nameof(serverEndpointName), serverEndpointName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
        var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);
        var endpointResource = await syncGroupResource.Value.GetStorageSyncServerEndpoints().GetAsync(serverEndpointName, cancellationToken);

        var deleteOperation = await endpointResource.Value.DeleteAsync(WaitUntil.Started, cancellationToken);
        await WaitForLroCompletionAsync(deleteOperation, cancellationToken);

        _logger.LogInformation("Successfully deleted Server Endpoint: {Endpoint}", serverEndpointName);
    }

    // Registered Server Operations
    public async Task<List<RegisteredServerDataSchema>> ListRegisteredServersAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);

        var servers = new List<RegisteredServerDataSchema>();
        await foreach (var serverResource in serviceResource.Value.GetStorageSyncRegisteredServers().WithCancellation(cancellationToken))
        {
            servers.Add(RegisteredServerDataSchema.FromResource(serverResource));
        }

        return servers;
    }

    public async Task<RegisteredServerDataSchema?> GetRegisteredServerAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string registeredServerId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(registeredServerId), registeredServerId));

        // Validate registeredServerId is a valid GUID
        var serverGuid = CheckGuid(registeredServerId, nameof(registeredServerId));

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
            var serverResource = await serviceResource.Value.GetStorageSyncRegisteredServers().GetAsync(serverGuid, cancellationToken);

            return RegisteredServerDataSchema.FromResource(serverResource.Value);
        }
        catch (RequestFailedException reqEx) when (reqEx.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(reqEx, "Registered Server not found: {Server}", registeredServerId);
            return null;
        }
    }

    public async Task<RegisteredServerDataSchema> RegisterServerAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string registeredServerId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(registeredServerId), registeredServerId));

        // Validate registeredServerId is a valid GUID
        var serverGuid = CheckGuid(registeredServerId, nameof(registeredServerId));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);

        var content = new StorageSyncRegisteredServerCreateOrUpdateContent();
        var operation = await serviceResource.Value.GetStorageSyncRegisteredServers().CreateOrUpdateAsync(
            WaitUntil.Started, serverGuid, content, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        _logger.LogInformation("Successfully registered Server: {Server}", registeredServerId);
        return RegisteredServerDataSchema.FromResource(operation.Value);
    }

    public async Task<RegisteredServerDataSchema> UpdateServerAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string registeredServerId,
        Dictionary<string, object>? properties = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(registeredServerId), registeredServerId));

        // Validate registeredServerId is a valid GUID
        var serverGuid = CheckGuid(registeredServerId, nameof(registeredServerId));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
        var serverResource = await serviceResource.Value.GetStorageSyncRegisteredServers().GetAsync(serverGuid, cancellationToken);

        var patch = new StorageSyncRegisteredServerPatch();
        // Add any patch-specific logic here if needed

        var operation = await serverResource.Value.UpdateAsync(WaitUntil.Started, patch, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        _logger.LogInformation("Successfully updated Registered Server: {Server}", registeredServerId);
        return RegisteredServerDataSchema.FromResource(operation.Value);
    }

    public async Task UnregisterServerAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string registeredServerId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(registeredServerId), registeredServerId));

        // Validate registeredServerId is a valid GUID
        var serverGuid = CheckGuid(registeredServerId, nameof(registeredServerId));

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
        var serverResource = await serviceResource.Value.GetStorageSyncRegisteredServers().GetAsync(serverGuid, cancellationToken);

        var deleteOperation = await serverResource.Value.DeleteAsync(WaitUntil.Started, cancellationToken);
        await WaitForLroCompletionAsync(deleteOperation, cancellationToken);

        _logger.LogInformation("Successfully unregistered Server: {Server}", registeredServerId);
    }

    /// <summary>
    /// Validates and converts a string to a GUID.
    /// </summary>
    /// <param name="value">The string value to convert</param>
    /// <param name="paramName">The parameter name for error messages</param>
    /// <returns>The converted GUID</returns>
    /// <exception cref="ArgumentException">Thrown if the value is not a valid GUID</exception>
    private static Guid CheckGuid(string value, string paramName)
    {
        if (!Guid.TryParse(value, out var guid))
        {
            throw new ArgumentException($"'{paramName}' must be a valid GUID.", paramName);
        }

        return guid;
    }
}
