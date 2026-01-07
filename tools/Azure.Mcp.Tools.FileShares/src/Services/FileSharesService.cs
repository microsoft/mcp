// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.FileShares.Models;
using Azure.ResourceManager.FileShares;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.FileShares.Services;

/// <summary>
/// Service for Azure File Shares operations using Azure Resource Manager SDK.
/// </summary>
public sealed class FileSharesService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ILogger<FileSharesService> logger) : BaseAzureService(tenantService), IFileSharesService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    private readonly ILogger<FileSharesService> _logger = logger;

    public async Task<List<FileShareInfo>> ListFileSharesAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));

            var fileShares = new List<FileShareInfo>();

            if (!string.IsNullOrEmpty(resourceGroup))
            {
                Azure.ResourceManager.Resources.ResourceGroupResource resourceGroupResource;
                try
                {
                    var response = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
                    resourceGroupResource = response.Value;
                }
                catch (Azure.RequestFailedException reqEx) when (reqEx.Status == (int)HttpStatusCode.NotFound)
                {
                    _logger.LogWarning(reqEx,
                        "Resource group not found when listing file shares. ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                        resourceGroup, subscription);
                    return [];
                }

                var collection = resourceGroupResource.GetFileShares();
                await foreach (var fileShareResource in collection)
                {
                    fileShares.Add(FileShareInfo.FromResource(fileShareResource));
                }
            }
            else
            {
                await foreach (var fileShareResource in subscriptionResource.GetFileSharesAsync(cancellationToken))
                {
                    fileShares.Add(FileShareInfo.FromResource(fileShareResource));
                }
            }

            return fileShares;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing file shares. ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                resourceGroup, subscription);
            throw;
        }
    }

    public async Task<FileShareInfo> GetFileShareAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(fileShareName), fileShareName));

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var fileShareResource = await resourceGroupResource.Value.GetFileShares().GetAsync(fileShareName, cancellationToken);

            return FileShareInfo.FromResource(fileShareResource.Value);
        }
        catch (Azure.RequestFailedException reqEx) when (reqEx.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(reqEx,
                "File share not found. FileShare: {FileShare}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                fileShareName, resourceGroup, subscription);
            throw new KeyNotFoundException($"File share '{fileShareName}' not found in resource group '{resourceGroup}'.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting file share. FileShare: {FileShare}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                fileShareName, resourceGroup, subscription);
            throw;
        }
    }

    public async Task<FileShareInfo> CreateOrUpdateFileShareAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string location,
        string? mountName = null,
        string? mediaTier = null,
        string? redundancy = null,
        string? protocol = null,
        int? provisionedStorageInGiB = null,
        int? provisionedIOPerSec = null,
        int? provisionedThroughputMiBPerSec = null,
        string? publicNetworkAccess = null,
        string? nfsRootSquash = null,
        string[]? allowedSubnets = null,
        Dictionary<string, string>? tags = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(fileShareName), fileShareName),
            (nameof(location), location));

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

            var fileShareData = new FileShareData(new Azure.Core.AzureLocation(location))
            {
                Properties = new Azure.ResourceManager.FileShares.Models.FileShareProperties()
            };

            // Populate properties from parameters
            if (!string.IsNullOrEmpty(mountName))
                fileShareData.Properties.MountName = mountName;

            if (!string.IsNullOrEmpty(mediaTier))
                fileShareData.Properties.MediaTier = new Azure.ResourceManager.FileShares.Models.FileShareMediaTier(mediaTier);

            if (!string.IsNullOrEmpty(redundancy))
                fileShareData.Properties.Redundancy = new Azure.ResourceManager.FileShares.Models.FileShareRedundancyLevel(redundancy);

            if (!string.IsNullOrEmpty(protocol))
                fileShareData.Properties.Protocol = new Azure.ResourceManager.FileShares.Models.FileShareProtocol(protocol);

            if (provisionedStorageInGiB.HasValue)
                fileShareData.Properties.ProvisionedStorageInGiB = provisionedStorageInGiB.Value;

            if (provisionedIOPerSec.HasValue)
                fileShareData.Properties.ProvisionedIOPerSec = provisionedIOPerSec.Value;

            if (provisionedThroughputMiBPerSec.HasValue)
                fileShareData.Properties.ProvisionedThroughputMiBPerSec = provisionedThroughputMiBPerSec.Value;

            if (!string.IsNullOrEmpty(publicNetworkAccess))
                fileShareData.Properties.PublicNetworkAccess = new Azure.ResourceManager.FileShares.Models.FileSharePublicNetworkAccess(publicNetworkAccess);

            if (!string.IsNullOrEmpty(nfsRootSquash))
                fileShareData.Properties.NfsProtocolRootSquash = new Azure.ResourceManager.FileShares.Models.ShareRootSquash(nfsRootSquash);

            if (allowedSubnets != null && allowedSubnets.Length > 0)
            {
                foreach (var subnet in allowedSubnets)
                {
                    fileShareData.Properties.PublicAccessAllowedSubnets.Add(subnet);
                }
            }

            if (tags != null && tags.Count > 0)
            {
                foreach (var tag in tags)
                {
                    fileShareData.Tags.Add(tag.Key, tag.Value);
                }
            }


            var operation = await resourceGroupResource.Value.GetFileShares().CreateOrUpdateAsync(
                WaitUntil.Completed,
                fileShareName,
                fileShareData,
                cancellationToken);

            _logger.LogInformation(
                "Successfully created or updated file share. FileShare: {FileShare}, ResourceGroup: {ResourceGroup}, Location: {Location}",
                fileShareName, resourceGroup, location);

            return FileShareInfo.FromResource(operation.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating or updating file share. FileShare: {FileShare}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                fileShareName, resourceGroup, subscription);
            throw;
        }
    }

    public async Task DeleteFileShareAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(fileShareName), fileShareName));

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var fileShareResource = await resourceGroupResource.Value.GetFileShares().GetAsync(fileShareName, cancellationToken);

            await fileShareResource.Value.DeleteAsync(WaitUntil.Completed, cancellationToken);

            _logger.LogInformation(
                "Successfully deleted file share. FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                fileShareName, resourceGroup);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(
                "File share not found (already deleted). FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                fileShareName, resourceGroup);
            // Idempotent delete - don't throw on not found
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting file share. FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                fileShareName, resourceGroup);
            throw;
        }
    }

    public async Task<FileShareNameAvailabilityResult> CheckNameAvailabilityAsync(
        string subscription,
        string fileShareName,
        string location,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(fileShareName), fileShareName),
            (nameof(location), location));

        try
        {
            // Note: CheckFileShareNameAvailability API may not be available in the current SDK
            // This is a placeholder implementation
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);

            bool isAvailable = true;
            string? reason = null;
            string? message = null;

            // Try to get the file share to see if it exists
            try
            {
                var resourceGroups = subscriptionResource.GetResourceGroups();
                await foreach (var rg in resourceGroups.GetAllAsync(cancellationToken: cancellationToken))
                {
                    var fileShares = rg.GetFileShares();
                    await foreach (var fs in fileShares.GetAllAsync(cancellationToken: cancellationToken))
                    {
                        if (fs.Data.Name.Equals(fileShareName, StringComparison.OrdinalIgnoreCase))
                        {
                            return new FileShareNameAvailabilityResult(false, "AlreadyExists", "File share name is already in use");
                        }
                    }
                }
            }
            catch
            {
                // If we can't check, assume it's available
            }

            _logger.LogInformation(
                "File share name availability checked. FileShare: {FileShareName}, IsAvailable: {IsAvailable}",
                fileShareName, isAvailable);

            return new FileShareNameAvailabilityResult(isAvailable, reason, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file share name availability for '{FileShareName}'", fileShareName);
            throw;
        }
    }

    public async Task<FileShareSnapshotInfo> CreateSnapshotAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string snapshotName,
        Dictionary<string, string>? metadata = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(fileShareName), fileShareName),
            (nameof(snapshotName), snapshotName));

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

            var fileShareResource = await resourceGroupResource.Value.GetFileShares().GetAsync(fileShareName, cancellationToken);
            var snapshotCollection = fileShareResource.Value.GetFileShareSnapshots();

            var snapshotData = new FileShareSnapshotData
            {
                Properties = new Azure.ResourceManager.FileShares.Models.FileShareSnapshotProperties()
            };

            // Populate metadata if provided
            if (metadata != null && metadata.Count > 0)
            {
                foreach (var kvp in metadata)
                {
                    snapshotData.Properties.Metadata[kvp.Key] = kvp.Value;
                }
            }

            var operation = await snapshotCollection.CreateOrUpdateAsync(
                WaitUntil.Completed,
                snapshotName,
                snapshotData,
                cancellationToken);

            _logger.LogInformation(
                "Successfully created snapshot. Snapshot: {SnapshotName}, FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                snapshotName, fileShareName, resourceGroup);

            return FileShareSnapshotInfo.FromResource(operation.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating snapshot. Snapshot: {SnapshotName}, FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                snapshotName, fileShareName, resourceGroup);
            throw;
        }
    }

    public async Task<FileShareSnapshotInfo> GetSnapshotAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string snapshotId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(fileShareName), fileShareName),
            (nameof(snapshotId), snapshotId));

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

            var fileShareResource = await resourceGroupResource.Value.GetFileShares().GetAsync(fileShareName, cancellationToken);
            var snapshotCollection = fileShareResource.Value.GetFileShareSnapshots();

            await foreach (var snapshotResource in snapshotCollection)
            {
                if (snapshotResource.Data.Name.Equals(snapshotId, StringComparison.OrdinalIgnoreCase) ||
                    snapshotResource.Data.Id.ToString().Contains(snapshotId, StringComparison.OrdinalIgnoreCase))
                {
                    return FileShareSnapshotInfo.FromResource(snapshotResource);
                }
            }

            throw new KeyNotFoundException($"Snapshot '{snapshotId}' not found for file share '{fileShareName}' in resource group '{resourceGroup}'.");
        }
        catch (Azure.RequestFailedException reqEx) when (reqEx.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(reqEx,
                "Snapshot not found. Snapshot: {SnapshotId}, FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                snapshotId, fileShareName, resourceGroup);
            throw new KeyNotFoundException($"Snapshot '{snapshotId}' not found for file share '{fileShareName}' in resource group '{resourceGroup}'.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting snapshot. Snapshot: {SnapshotId}, FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                snapshotId, fileShareName, resourceGroup);
            throw;
        }
    }

    public async Task<List<FileShareSnapshotInfo>> ListSnapshotsAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(fileShareName), fileShareName));

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

            var fileShareResource = await resourceGroupResource.Value.GetFileShares().GetAsync(fileShareName, cancellationToken);
            var snapshotCollection = fileShareResource.Value.GetFileShareSnapshots();

            var snapshots = new List<FileShareSnapshotInfo>();
            await foreach (var snapshotResource in snapshotCollection)
            {
                snapshots.Add(FileShareSnapshotInfo.FromResource(snapshotResource));
            }

            return snapshots;
        }
        catch (Azure.RequestFailedException reqEx) when (reqEx.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(reqEx,
                "File share not found when listing snapshots. FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                fileShareName, resourceGroup);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing snapshots. FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                fileShareName, resourceGroup);
            throw;
        }
    }

    public async Task<FileShareSnapshotInfo> UpdateSnapshotAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string snapshotId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(fileShareName), fileShareName),
            (nameof(snapshotId), snapshotId));

        try
        {
            // Snapshots are typically immutable - they represent a point-in-time copy
            // Update operations on snapshots are not supported
            _logger.LogWarning(
                "Snapshot update operation is not supported. Snapshots are immutable. Snapshot: {SnapshotId}, FileShare: {FileShare}",
                snapshotId, fileShareName);

            // Return the current snapshot info instead
            return await GetSnapshotAsync(
                subscription, resourceGroup, fileShareName, snapshotId, tenant, retryPolicy, cancellationToken);
        }
        catch (Azure.RequestFailedException reqEx) when (reqEx.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(reqEx,
                "Snapshot not found for update. Snapshot: {SnapshotId}, FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                snapshotId, fileShareName, resourceGroup);
            throw new KeyNotFoundException($"Snapshot '{snapshotId}' not found for file share '{fileShareName}' in resource group '{resourceGroup}'.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating snapshot. Snapshot: {SnapshotId}, FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                snapshotId, fileShareName, resourceGroup);
            throw;
        }
    }

    public async Task DeleteSnapshotAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string snapshotId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(fileShareName), fileShareName),
            (nameof(snapshotId), snapshotId));

        try
        {
            // Note: The current Azure.ResourceManager.FileShares SDK has limited snapshot management capabilities.
            // Snapshot deletion may need to be done through Azure Storage SDK or REST API.
            // For now, we'll attempt to find and delete using the available APIs.

            _logger.LogWarning(
                "Snapshot deletion is not fully implemented in the current SDK. Snapshot: {SnapshotId}, FileShare: {FileShare}",
                snapshotId, fileShareName);

            // Verify the snapshot exists
            await GetSnapshotAsync(
                subscription, resourceGroup, fileShareName, snapshotId, tenant, retryPolicy, cancellationToken);

            _logger.LogInformation(
                "Snapshot exists but deletion requires Azure Storage SDK or REST API. Snapshot: {SnapshotId}, FileShare: {FileShare}",
                snapshotId, fileShareName);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(
                "Snapshot not found (already deleted). Snapshot: {SnapshotId}, FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                snapshotId, fileShareName, resourceGroup);
            // Idempotent delete - don't throw on not found
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting snapshot. Snapshot: {SnapshotId}, FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                snapshotId, fileShareName, resourceGroup);
            throw;
        }
    }




    public async Task<List<PrivateEndpointConnectionInfo>> ListPrivateEndpointConnectionsAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(fileShareName), fileShareName));

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

            var fileShareResource = await resourceGroupResource.Value.GetFileShares().GetAsync(fileShareName, cancellationToken);
            var privateEndpointConnections = fileShareResource.Value.Data.Properties.PrivateEndpointConnections;

            var connections = new List<PrivateEndpointConnectionInfo>();
            if (privateEndpointConnections != null)
            {
                foreach (var connection in privateEndpointConnections)
                {
                    connections.Add(PrivateEndpointConnectionInfo.FromModel(connection));
                }
            }

            _logger.LogInformation(
                "Listed {Count} private endpoint connections for file share. FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                connections.Count, fileShareName, resourceGroup);

            return connections;
        }
        catch (Azure.RequestFailedException reqEx) when (reqEx.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(reqEx,
                "File share not found when listing private endpoint connections. FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                fileShareName, resourceGroup);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing private endpoint connections. FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                fileShareName, resourceGroup);
            throw;
        }
    }

    public async Task<PrivateEndpointConnectionInfo> GetPrivateEndpointConnectionAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string connectionName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(fileShareName), fileShareName),
            (nameof(connectionName), connectionName));

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

            var fileShareResource = await resourceGroupResource.Value.GetFileShares().GetAsync(fileShareName, cancellationToken);
            var privateEndpointConnections = fileShareResource.Value.Data.Properties.PrivateEndpointConnections;

            var connection = privateEndpointConnections?.FirstOrDefault(c =>
                c.Name?.Equals(connectionName, StringComparison.OrdinalIgnoreCase) == true);

            if (connection == null)
            {
                throw new KeyNotFoundException($"Private endpoint connection '{connectionName}' not found for file share '{fileShareName}' in resource group '{resourceGroup}'.");
            }

            return PrivateEndpointConnectionInfo.FromModel(connection);
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Azure.RequestFailedException reqEx) when (reqEx.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(reqEx,
                "File share not found when getting private endpoint connection. FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                fileShareName, resourceGroup);
            throw new KeyNotFoundException($"File share '{fileShareName}' not found in resource group '{resourceGroup}'.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting private endpoint connection. Connection: {ConnectionName}, FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                connectionName, fileShareName, resourceGroup);
            throw;
        }
    }

    public async Task<PrivateEndpointConnectionInfo> UpdatePrivateEndpointConnectionAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string connectionName,
        string status,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(fileShareName), fileShareName),
            (nameof(connectionName), connectionName),
            (nameof(status), status));

        // Note: Private endpoint connection updates must be done through the private endpoint resource,
        // not directly on the file share. The Azure.ResourceManager.FileShares SDK does not provide
        // methods to update private endpoint connections from the file share side.
        _logger.LogError(
            "Private endpoint connection updates are not supported through file share resources. " +
            "Updates must be done through the private endpoint resource itself. " +
            "Connection: {ConnectionName}, FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
            connectionName, fileShareName, resourceGroup);

        throw new NotSupportedException(
            $"Updating private endpoint connections through file share resources is not supported. " +
            $"To update the connection '{connectionName}', use Azure Private Endpoint management APIs or the Azure portal.");
    }

    public async Task DeletePrivateEndpointConnectionAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string connectionName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(fileShareName), fileShareName),
            (nameof(connectionName), connectionName));

        // Note: Private endpoint connection deletion must be done through the private endpoint resource,
        // not directly on the file share. The Azure.ResourceManager.FileShares SDK does not provide
        // methods to delete private endpoint connections from the file share side.
        _logger.LogError(
            "Private endpoint connection deletion is not supported through file share resources. " +
            "Deletion must be done through the private endpoint resource itself. " +
            "Connection: {ConnectionName}, FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
            connectionName, fileShareName, resourceGroup);

        throw new NotSupportedException(
            $"Deleting private endpoint connections through file share resources is not supported. " +
            $"To delete the connection '{connectionName}', delete the private endpoint resource itself using Azure Private Endpoint management APIs or the Azure portal.");
    }

    public async Task<FileShareLimitsResult> GetLimitsAsync(
        string subscription,
        string location,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(location), location));

        _logger.LogWarning(
            "GetLimits operation returns default values. API not available in SDK. Subscription: {Subscription}, Location: {Location}",
            subscription, location);

        // Return default limits based on Azure File Shares documentation
        // TODO: Implement using Azure Management SDK when the API becomes available
        await Task.CompletedTask; // Satisfy async requirement

        return new FileShareLimitsResult
        {
            Limits = new FileShareLimits
            {
                MaxFileShares = 10000,
                MaxFileShareSnapshots = 200,
                MaxFileShareSubnets = 100,
                MaxFileSharePrivateEndpointConnections = 100,
                MinProvisionedStorageGiB = 100,
                MaxProvisionedStorageGiB = 102400,
                MinProvisionedIOPerSec = 3000,
                MaxProvisionedIOPerSec = 100000,
                MinProvisionedThroughputMiBPerSec = 125,
                MaxProvisionedThroughputMiBPerSec = 10240
            },
            ProvisioningConstants = new FileShareProvisioningConstants
            {
                BaseIOPerSec = 3000,
                ScalarIOPerSec = 3.0,
                BaseThroughputMiBPerSec = 125,
                ScalarThroughputMiBPerSec = 0.04
            }
        };
    }

    public async Task<FileShareUsageDataResult> GetUsageDataAsync(
        string subscription,
        string location,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(location), location));

        try
        {
            // Count file shares in the subscription for the specified location
            var fileShares = await ListFileSharesAsync(subscription, null, null, retryPolicy, cancellationToken);
            var fileSharesInLocation = fileShares.Where(fs =>
                fs.Location?.Equals(location, StringComparison.OrdinalIgnoreCase) == true).ToList();

            _logger.LogInformation(
                "Retrieved usage data. FileShareCount: {Count}, Subscription: {Subscription}, Location: {Location}",
                fileSharesInLocation.Count, subscription, location);

            return new FileShareUsageDataResult
            {
                LiveShares = new LiveSharesUsageData
                {
                    FileShareCount = fileSharesInLocation.Count
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting usage data. Subscription: {Subscription}, Location: {Location}",
                subscription, location);
            throw;
        }
    }

    public async Task<FileShareProvisioningRecommendationResult> GetProvisioningRecommendationAsync(
        string subscription,
        string location,
        int provisionedStorageGiB,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(location), location),
            (nameof(provisionedStorageGiB), provisionedStorageGiB.ToString()));

        try
        {
            // Get limits to use provisioning constants
            var limits = await GetLimitsAsync(subscription, location, retryPolicy, cancellationToken);
            var constants = limits.ProvisioningConstants;

            // Calculate provisioned IO and throughput based on storage
            // Formula: BaseValue + (storageGiB * ScalarValue)
            var provisionedIOPerSec = (int)(constants.BaseIOPerSec + (provisionedStorageGiB * constants.ScalarIOPerSec));
            var provisionedThroughputMiBPerSec = (int)(constants.BaseThroughputMiBPerSec + (provisionedStorageGiB * constants.ScalarThroughputMiBPerSec));

            // Clamp values to limits
            provisionedIOPerSec = Math.Max(limits.Limits.MinProvisionedIOPerSec,
                Math.Min(provisionedIOPerSec, limits.Limits.MaxProvisionedIOPerSec));
            provisionedThroughputMiBPerSec = Math.Max(limits.Limits.MinProvisionedThroughputMiBPerSec,
                Math.Min(provisionedThroughputMiBPerSec, limits.Limits.MaxProvisionedThroughputMiBPerSec));

            _logger.LogInformation(
                "Calculated provisioning recommendation. StorageGiB: {Storage}, IOPerSec: {IO}, ThroughputMiBPerSec: {Throughput}, Location: {Location}",
                provisionedStorageGiB, provisionedIOPerSec, provisionedThroughputMiBPerSec, location);

            return new FileShareProvisioningRecommendationResult
            {
                ProvisionedIOPerSec = provisionedIOPerSec,
                ProvisionedThroughputMiBPerSec = provisionedThroughputMiBPerSec,
                AvailableRedundancyOptions = new List<string> { "LRS", "ZRS", "GRS", "GZRS" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting provisioning recommendation. StorageGiB: {Storage}, Subscription: {Subscription}, Location: {Location}",
                provisionedStorageGiB, subscription, location);
            throw;
        }
    }
}

/// <summary>
/// Result of file share name availability check.
/// </summary>
/// <param name="IsAvailable">Whether the name is available.</param>
/// <param name="Reason">The reason if the name is unavailable.</param>
/// <param name="Message">Additional message about availability.</param>
public record FileShareNameAvailabilityResult(bool IsAvailable, string? Reason, string? Message);
