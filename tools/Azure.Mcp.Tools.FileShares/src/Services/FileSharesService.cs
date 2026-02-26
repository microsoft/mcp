// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.FileShares.Models;
using Azure.ResourceManager.FileShares;
using Azure.ResourceManager.Resources;
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
    public const string HttpClientName = "AzureMcpFileSharesService";

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
                SubscriptionResource.CreateResourceIdentifier(subscription));

            var fileShares = new List<FileShareInfo>();

            if (!string.IsNullOrEmpty(resourceGroup))
            {
                ResourceGroupResource resourceGroupResource;
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
                await foreach (var fileShareResource in collection.WithCancellation(cancellationToken))
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

    public async Task<FileShareInfo> PatchFileShareAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
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
            (nameof(fileShareName), fileShareName));

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

            // Create a patch object with only the properties to update
            var patch = new Azure.ResourceManager.FileShares.Models.FileSharePatch();

            // Set properties that are explicitly provided
            if (provisionedStorageInGiB.HasValue || provisionedIOPerSec.HasValue || provisionedThroughputMiBPerSec.HasValue ||
                !string.IsNullOrEmpty(publicNetworkAccess) || !string.IsNullOrEmpty(nfsRootSquash) || allowedSubnets?.Length > 0)
            {
                patch.Properties = new Azure.ResourceManager.FileShares.Models.FileSharePatchProperties();

                if (provisionedStorageInGiB.HasValue)
                {
                    patch.Properties.ProvisionedStorageInGiB = provisionedStorageInGiB.Value;
                }

                if (provisionedIOPerSec.HasValue)
                {
                    patch.Properties.ProvisionedIOPerSec = provisionedIOPerSec.Value;
                }

                if (provisionedThroughputMiBPerSec.HasValue)
                {
                    patch.Properties.ProvisionedThroughputMiBPerSec = provisionedThroughputMiBPerSec.Value;
                }

                if (!string.IsNullOrEmpty(publicNetworkAccess))
                {
                    patch.Properties.PublicNetworkAccess = new Azure.ResourceManager.FileShares.Models.FileSharePublicNetworkAccess(publicNetworkAccess);
                }

                if (!string.IsNullOrEmpty(nfsRootSquash))
                {
                    patch.Properties.NfsProtocolRootSquash = new Azure.ResourceManager.FileShares.Models.ShareRootSquash(nfsRootSquash);
                }

                if (allowedSubnets != null && allowedSubnets.Length > 0)
                {
                    foreach (var subnet in allowedSubnets)
                    {
                        patch.Properties.PublicAccessAllowedSubnets.Add(subnet);
                    }
                }
            }

            if (tags is { Count: > 0 })
            {
                foreach (var tag in tags)
                {
                    patch.Tags.Add(tag.Key, tag.Value);
                }
            }

            // Get the file share resource to update
            var fileShareResource = await resourceGroupResource.Value.GetFileShares().GetAsync(fileShareName, cancellationToken);

            // Use UpdateAsync to patch the file share
            var operation = await fileShareResource.Value.UpdateAsync(WaitUntil.Completed, patch, cancellationToken);

            _logger.LogInformation(
                "Successfully patched file share. FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                fileShareName, resourceGroup);

            return FileShareInfo.FromResource(operation.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error patching file share. FileShare: {FileShare}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
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
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
            var azureLocation = new Azure.Core.AzureLocation(location);

            var content = new Azure.ResourceManager.FileShares.Models.FileShareNameAvailabilityContent
            {
                Name = fileShareName,
                Type = "Microsoft.FileShares/fileShares"
            };
            var response = await subscriptionResource.CheckFileShareNameAvailabilityAsync(azureLocation, content, cancellationToken);

            var result = response.Value;

            _logger.LogInformation(
                "File share name availability checked. FileShare: {FileShareName}, IsAvailable: {IsAvailable}",
                fileShareName, result.IsNameAvailable);

            return new FileShareNameAvailabilityResult(
                result.IsNameAvailable ?? false,
                result.Reason?.ToString(),
                result.Message);
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

            await foreach (var snapshotResource in snapshotCollection.WithCancellation(cancellationToken))
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
            await foreach (var snapshotResource in snapshotCollection.WithCancellation(cancellationToken))
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

    public async Task<FileShareSnapshotInfo> PatchSnapshotAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string snapshotId,
        Dictionary<string, string>? metadata = null,
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

            // Get the existing snapshot
            var existingSnapshot = await snapshotCollection.GetAsync(snapshotId, cancellationToken);

            // Create a patch object with only the properties to update
            var patch = new Azure.ResourceManager.FileShares.Models.FileShareSnapshotPatch();

            if (metadata is { Count: > 0 })
            {
                foreach (var kvp in metadata)
                {
                    patch.FileShareSnapshotUpdateMetadata.Add(kvp.Key, kvp.Value);
                }
            }

            // Use UpdateAsync to patch the snapshot
            var operation = await existingSnapshot.Value.UpdateAsync(
                WaitUntil.Completed,
                patch,
                cancellationToken);

            _logger.LogInformation(
                "Successfully updated snapshot. Snapshot: {SnapshotId}, FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                snapshotId, fileShareName, resourceGroup);

            return FileShareSnapshotInfo.FromResource(operation.Value);
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
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

            var fileShareResource = await resourceGroupResource.Value.GetFileShares().GetAsync(fileShareName, cancellationToken);
            var snapshotCollection = fileShareResource.Value.GetFileShareSnapshots();

            // Get the snapshot and delete it
            var snapshotResource = await snapshotCollection.GetAsync(snapshotId, cancellationToken);
            await snapshotResource.Value.DeleteAsync(WaitUntil.Completed, cancellationToken);

            _logger.LogInformation(
                "Successfully deleted snapshot. Snapshot: {SnapshotId}, FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                snapshotId, fileShareName, resourceGroup);
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

    public async Task<FileShareLimitsResult> GetLimitsAsync(
        string subscription,
        string location,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(location), location));

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
            var azureLocation = new Azure.Core.AzureLocation(location);

            var response = await subscriptionResource.GetLimitsAsync(azureLocation, cancellationToken);

            var output = response.Value.Properties;

            _logger.LogInformation(
                "Retrieved limits. MaxFileShares: {MaxFileShares}, Subscription: {Subscription}, Location: {Location}",
                output.Limits.MaxFileShares, subscription, location);

            return new FileShareLimitsResult
            {
                Limits = new FileShareLimits
                {
                    MaxFileShares = output.Limits.MaxFileShares,
                    MaxFileShareSnapshots = output.Limits.MaxFileShareSnapshots,
                    MaxFileShareSubnets = output.Limits.MaxFileShareSubnets,
                    MaxFileSharePrivateEndpointConnections = output.Limits.MaxFileSharePrivateEndpointConnections,
                    MinProvisionedStorageGiB = output.Limits.MinProvisionedStorageGiB,
                    MaxProvisionedStorageGiB = output.Limits.MaxProvisionedStorageGiB,
                    MinProvisionedIOPerSec = output.Limits.MinProvisionedIOPerSec,
                    MaxProvisionedIOPerSec = output.Limits.MaxProvisionedIOPerSec,
                    MinProvisionedThroughputMiBPerSec = output.Limits.MinProvisionedThroughputMiBPerSec,
                    MaxProvisionedThroughputMiBPerSec = output.Limits.MaxProvisionedThroughputMiBPerSec
                },
                ProvisioningConstants = new FileShareProvisioningConstants
                {
                    BaseIOPerSec = output.ProvisioningConstants.BaseIOPerSec,
                    ScalarIOPerSec = output.ProvisioningConstants.ScalarIOPerSec,
                    BaseThroughputMiBPerSec = output.ProvisioningConstants.BaseThroughputMiBPerSec,
                    ScalarThroughputMiBPerSec = output.ProvisioningConstants.ScalarThroughputMiBPerSec
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting limits. Subscription: {Subscription}, Location: {Location}",
                subscription, location);
            throw;
        }
    }

    public async Task<FileShareUsageDataResult> GetUsageDataAsync(
        string subscription,
        string location,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(location), location));

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
            var azureLocation = new Azure.Core.AzureLocation(location);

            var response = await subscriptionResource.GetUsageDataAsync(azureLocation, cancellationToken);

            var result = response.Value;

            _logger.LogInformation(
                "Retrieved usage data. FileShareCount: {Count}, Subscription: {Subscription}, Location: {Location}",
                result.LiveSharesFileShareCount, subscription, location);

            return new FileShareUsageDataResult
            {
                LiveShares = new LiveSharesUsageData
                {
                    FileShareCount = result.LiveSharesFileShareCount ?? 0
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
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(location), location),
            (nameof(provisionedStorageGiB), provisionedStorageGiB.ToString()));

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
            var azureLocation = new Azure.Core.AzureLocation(location);

            var content = new Azure.ResourceManager.FileShares.Models.FileShareProvisioningRecommendationContent(provisionedStorageGiB);
            var response = await subscriptionResource.GetProvisioningRecommendationAsync(azureLocation, content, cancellationToken);

            var output = response.Value.Properties;

            _logger.LogInformation(
                "Retrieved provisioning recommendation. StorageGiB: {Storage}, IOPerSec: {IO}, ThroughputMiBPerSec: {Throughput}, Location: {Location}",
                provisionedStorageGiB, output.ProvisionedIOPerSec, output.ProvisionedThroughputMiBPerSec, location);

            return new Models.FileShareProvisioningRecommendationResult
            {
                ProvisionedIOPerSec = output.ProvisionedIOPerSec,
                ProvisionedThroughputMiBPerSec = output.ProvisionedThroughputMiBPerSec,
                AvailableRedundancyOptions = output.AvailableRedundancyOptions?.Select(r => r.ToString()).ToList() ?? []
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
                SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var fileShareResource = await resourceGroupResource.Value.GetFileShares().GetAsync(fileShareName, cancellationToken);

            var connection = await fileShareResource.Value.GetFileSharePrivateEndpointConnections()
                .GetAsync(connectionName, cancellationToken);

            _logger.LogInformation(
                "Successfully retrieved private endpoint connection. Connection: {ConnectionName}, FileShare: {FileShareName}",
                connectionName, fileShareName);

            return PrivateEndpointConnectionInfo.FromResource(connection.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting private endpoint connection. Connection: {ConnectionName}, FileShare: {FileShareName}",
                connectionName, fileShareName);
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
                SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var fileShareResource = await resourceGroupResource.Value.GetFileShares().GetAsync(fileShareName, cancellationToken);

            var connections = new List<PrivateEndpointConnectionInfo>();
            await foreach (var connection in fileShareResource.Value.GetFileSharePrivateEndpointConnections()
                .GetAllAsync(cancellationToken))
            {
                connections.Add(PrivateEndpointConnectionInfo.FromResource(connection));
            }

            _logger.LogInformation(
                "Successfully listed private endpoint connections. Count: {Count}, FileShare: {FileShareName}",
                connections.Count, fileShareName);

            return connections;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing private endpoint connections. FileShare: {FileShareName}",
                fileShareName);
            throw;
        }
    }

    public async Task<PrivateEndpointConnectionInfo> UpdatePrivateEndpointConnectionAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string connectionName,
        string status,
        string? description = null,
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

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var fileShareResource = await resourceGroupResource.Value.GetFileShares().GetAsync(fileShareName, cancellationToken);

            // Get existing connection
            var existingConnection = await fileShareResource.Value.GetFileSharePrivateEndpointConnections()
                .GetAsync(connectionName, cancellationToken);

            // Create updated connection data
            var connectionData = new FileSharePrivateEndpointConnectionData
            {
                Properties = new Azure.ResourceManager.FileShares.Models.FileSharePrivateEndpointConnectionProperties(
                    new Azure.ResourceManager.FileShares.Models.FileSharePrivateLinkServiceConnectionState
                    {
                        Status = new Azure.ResourceManager.FileShares.Models.FileSharesPrivateEndpointServiceConnectionStatus(status),
                        Description = description
                    })
            };

            var operation = await fileShareResource.Value.GetFileSharePrivateEndpointConnections()
                .CreateOrUpdateAsync(WaitUntil.Completed, connectionName, connectionData, cancellationToken);

            _logger.LogInformation(
                "Successfully updated private endpoint connection. Connection: {ConnectionName}, FileShare: {FileShareName}, Status: {Status}",
                connectionName, fileShareName, status);

            return PrivateEndpointConnectionInfo.FromResource(operation.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating private endpoint connection. Connection: {ConnectionName}, FileShare: {FileShareName}",
                connectionName, fileShareName);
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
