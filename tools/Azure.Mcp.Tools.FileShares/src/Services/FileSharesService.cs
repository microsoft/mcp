// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.ResourceManager.FileShares;
using Azure.ResourceManager.Resources;

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

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));

        var fileShares = new List<FileShareInfo>();

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
            var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var fileShareResource = await resourceGroupResource.Value.GetFileShares().GetAsync(fileShareName, cancellationToken);
            return FileShareInfo.FromResource(fileShareResource.Value);
        }
        catch (RequestFailedException reqEx) when (reqEx.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(reqEx,
                "File share not found. FileShare: {FileShare}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                fileShareName, resourceGroup, subscription);
            throw new KeyNotFoundException($"File share '{fileShareName}' not found in resource group '{resourceGroup}'.");
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

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

        var fileShareData = new FileShareData(new(location))
        {
            Properties = new()
        };

        // Populate properties from parameters
        if (!string.IsNullOrEmpty(mountName))
            fileShareData.Properties.MountName = mountName;

        if (!string.IsNullOrEmpty(mediaTier))
            fileShareData.Properties.MediaTier = new(mediaTier);

        if (!string.IsNullOrEmpty(redundancy))
            fileShareData.Properties.Redundancy = new(redundancy);

        if (!string.IsNullOrEmpty(protocol))
            fileShareData.Properties.Protocol = new(protocol);

        if (provisionedStorageInGiB.HasValue)
            fileShareData.Properties.ProvisionedStorageInGiB = provisionedStorageInGiB.Value;

        if (provisionedIOPerSec.HasValue)
            fileShareData.Properties.ProvisionedIOPerSec = provisionedIOPerSec.Value;

        if (provisionedThroughputMiBPerSec.HasValue)
            fileShareData.Properties.ProvisionedThroughputMiBPerSec = provisionedThroughputMiBPerSec.Value;

        if (!string.IsNullOrEmpty(publicNetworkAccess))
            fileShareData.Properties.PublicNetworkAccess = new(publicNetworkAccess);

        if (!string.IsNullOrEmpty(nfsRootSquash))
            fileShareData.Properties.NfsProtocolRootSquash = new(nfsRootSquash);

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

    public async Task<FileShareInfo> PatchFileShareAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string? mediaTier = null,
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

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

        // Get the file share resource to update
        var fileShareResource = await resourceGroupResource.Value.GetFileShares().GetAsync(fileShareName, cancellationToken);

        if (!string.IsNullOrEmpty(mediaTier))
        {
            // The SDK's FileSharePatchProperties does not expose MediaTier, so we use a full
            // PUT (CreateOrUpdate) to change it, preserving all other existing properties.
            var existingData = fileShareResource.Value.Data;
            var existingProps = existingData.Properties;

            var fileShareData = new FileShareData(existingData.Location)
            {
                Properties = new()
            };

            fileShareData.Properties.MediaTier = new(mediaTier);

            if (existingProps != null)
            {
                if (!string.IsNullOrEmpty(existingProps.MountName))
                    fileShareData.Properties.MountName = existingProps.MountName;
                fileShareData.Properties.Redundancy = existingProps.Redundancy;
                fileShareData.Properties.Protocol = existingProps.Protocol;
                fileShareData.Properties.ProvisionedStorageInGiB = provisionedStorageInGiB ?? existingProps.ProvisionedStorageInGiB;
                fileShareData.Properties.ProvisionedIOPerSec = provisionedIOPerSec ?? existingProps.ProvisionedIOPerSec;
                fileShareData.Properties.ProvisionedThroughputMiBPerSec = provisionedThroughputMiBPerSec ?? existingProps.ProvisionedThroughputMiBPerSec;
                fileShareData.Properties.PublicNetworkAccess = !string.IsNullOrEmpty(publicNetworkAccess)
                    ? new(publicNetworkAccess)
                    : existingProps.PublicNetworkAccess;
                fileShareData.Properties.NfsProtocolRootSquash = !string.IsNullOrEmpty(nfsRootSquash)
                    ? new(nfsRootSquash)
                    : existingProps.NfsProtocolRootSquash;

                var effectiveSubnets = allowedSubnets?.Length > 0 ? allowedSubnets : existingProps.PublicAccessAllowedSubnets?.ToArray();
                if (effectiveSubnets != null)
                    foreach (var subnet in effectiveSubnets)
                        fileShareData.Properties.PublicAccessAllowedSubnets.Add(subnet);
            }
            else
            {
                if (provisionedStorageInGiB.HasValue) fileShareData.Properties.ProvisionedStorageInGiB = provisionedStorageInGiB.Value;
                if (provisionedIOPerSec.HasValue) fileShareData.Properties.ProvisionedIOPerSec = provisionedIOPerSec.Value;
                if (provisionedThroughputMiBPerSec.HasValue) fileShareData.Properties.ProvisionedThroughputMiBPerSec = provisionedThroughputMiBPerSec.Value;
                if (!string.IsNullOrEmpty(publicNetworkAccess)) fileShareData.Properties.PublicNetworkAccess = new(publicNetworkAccess);
                if (!string.IsNullOrEmpty(nfsRootSquash)) fileShareData.Properties.NfsProtocolRootSquash = new(nfsRootSquash);
                if (allowedSubnets?.Length > 0)
                    foreach (var subnet in allowedSubnets)
                        fileShareData.Properties.PublicAccessAllowedSubnets.Add(subnet);
            }

            if (tags is { Count: > 0 })
                foreach (var tag in tags)
                    fileShareData.Tags.Add(tag.Key, tag.Value);
            else if (existingData.Tags?.Count > 0)
                foreach (var tag in existingData.Tags)
                    fileShareData.Tags.Add(tag.Key, tag.Value);

            var putOperation = await resourceGroupResource.Value.GetFileShares()
                .CreateOrUpdateAsync(WaitUntil.Completed, fileShareName, fileShareData, cancellationToken);

            _logger.LogInformation(
                "Successfully updated file share media tier. FileShare: {FileShare}, ResourceGroup: {ResourceGroup}, MediaTier: {MediaTier}",
                fileShareName, resourceGroup, mediaTier);

            return FileShareInfo.FromResource(putOperation.Value);
        }

        // Use PATCH for non-mediaTier property updates
        var patch = new ResourceManager.FileShares.Models.FileSharePatch();

        if (provisionedStorageInGiB.HasValue || provisionedIOPerSec.HasValue || provisionedThroughputMiBPerSec.HasValue ||
            !string.IsNullOrEmpty(publicNetworkAccess) || !string.IsNullOrEmpty(nfsRootSquash) || allowedSubnets?.Length > 0)
        {
            patch.Properties = new();

            if (provisionedStorageInGiB.HasValue)
                patch.Properties.ProvisionedStorageInGiB = provisionedStorageInGiB.Value;

            if (provisionedIOPerSec.HasValue)
                patch.Properties.ProvisionedIOPerSec = provisionedIOPerSec.Value;

            if (provisionedThroughputMiBPerSec.HasValue)
                patch.Properties.ProvisionedThroughputMiBPerSec = provisionedThroughputMiBPerSec.Value;
        }

        if (!string.IsNullOrEmpty(publicNetworkAccess) && patch.Properties != null)
            patch.Properties.PublicNetworkAccess = new(publicNetworkAccess);

        if (!string.IsNullOrEmpty(nfsRootSquash) && patch.Properties != null)
            patch.Properties.NfsProtocolRootSquash = new(nfsRootSquash);

        if (tags is { Count: > 0 })
            foreach (var tag in tags)
                patch.Tags.Add(tag.Key, tag.Value);

        var operation = await fileShareResource.Value.UpdateAsync(WaitUntil.Completed, patch, cancellationToken);

        _logger.LogInformation(
            "Successfully patched file share. FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
            fileShareName, resourceGroup);

        return FileShareInfo.FromResource(operation.Value);
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
            var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var fileShareResource = await resourceGroupResource.Value.GetFileShares().GetAsync(fileShareName, cancellationToken);

            await fileShareResource.Value.DeleteAsync(WaitUntil.Completed, cancellationToken);

            _logger.LogInformation(
                "Successfully deleted file share. FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                fileShareName, resourceGroup);
        }
        catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(
                "File share not found (already deleted). FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                fileShareName, resourceGroup);
            // Idempotent delete - don't throw on not found
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

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var content = new ResourceManager.FileShares.Models.FileShareNameAvailabilityContent
        {
            Name = fileShareName,
            Type = "Microsoft.FileShares/fileShares"
        };
        var response = await subscriptionResource.CheckFileShareNameAvailabilityAsync(new(location), content, cancellationToken);

        var result = response.Value;

        _logger.LogInformation(
            "File share name availability checked. FileShare: {FileShareName}, IsAvailable: {IsAvailable}",
            fileShareName, result.IsNameAvailable);

        return new(
            result.IsNameAvailable ?? false,
            result.Reason?.ToString(),
            result.Message);
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

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

        var fileShareResource = await resourceGroupResource.Value.GetFileShares().GetAsync(fileShareName, cancellationToken);
        var snapshotCollection = fileShareResource.Value.GetFileShareSnapshots();

        var snapshotData = new FileShareSnapshotData
        {
            Properties = new()
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

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
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
            var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
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
        catch (RequestFailedException reqEx) when (reqEx.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(reqEx,
                "File share not found when listing snapshots. FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                fileShareName, resourceGroup);
            return [];
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

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

        var fileShareResource = await resourceGroupResource.Value.GetFileShares().GetAsync(fileShareName, cancellationToken);
        var snapshotCollection = fileShareResource.Value.GetFileShareSnapshots();

        // Get the existing snapshot
        var existingSnapshot = await snapshotCollection.GetFileShareSnapshotAsync(snapshotId, cancellationToken);

        // Create a patch object with only the properties to update
        var patch = new ResourceManager.FileShares.Models.FileShareSnapshotPatch();

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
            var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

            var fileShareResource = await resourceGroupResource.Value.GetFileShares().GetAsync(fileShareName, cancellationToken);
            var snapshotCollection = fileShareResource.Value.GetFileShareSnapshots();

            // Get the snapshot and delete it
            var snapshotResource = await snapshotCollection.GetFileShareSnapshotAsync(snapshotId, cancellationToken);
            await snapshotResource.Value.DeleteFileShareSnapshotAsync(WaitUntil.Completed, cancellationToken);

            _logger.LogInformation(
                "Successfully deleted snapshot. Snapshot: {SnapshotId}, FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                snapshotId, fileShareName, resourceGroup);
        }
        catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(
                "Snapshot not found (already deleted). Snapshot: {SnapshotId}, FileShare: {FileShare}, ResourceGroup: {ResourceGroup}",
                snapshotId, fileShareName, resourceGroup);
            // Idempotent delete - don't throw on not found
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

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var response = await subscriptionResource.GetLimitsAsync(new(location), cancellationToken);

        var output = response.Value.Properties;

        _logger.LogInformation(
            "Retrieved limits. MaxFileShares: {MaxFileShares}, Subscription: {Subscription}, Location: {Location}",
            output.Limits.MaxFileShares, subscription, location);

        return new()
        {
            Limits = new()
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
            ProvisioningConstants = new()
            {
                BaseIOPerSec = output.ProvisioningConstants.BaseIOPerSec,
                ScalarIOPerSec = output.ProvisioningConstants.ScalarIOPerSec,
                BaseThroughputMiBPerSec = output.ProvisioningConstants.BaseThroughputMiBPerSec,
                ScalarThroughputMiBPerSec = output.ProvisioningConstants.ScalarThroughputMiBPerSec
            }
        };
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

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var response = await subscriptionResource.GetUsageDataAsync(new(location), cancellationToken);

        var output = response.Value.Properties;

        _logger.LogInformation(
            "Retrieved usage data. FileShareCount: {Count}, Subscription: {Subscription}, Location: {Location}",
            output.LiveSharesFileShareCount, subscription, location);

        return new()
        {
            LiveShares = new()
            {
                FileShareCount = output.LiveSharesFileShareCount ?? 0
            }
        };
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

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var response = await subscriptionResource.GetProvisioningRecommendationAsync(new(location), new(provisionedStorageGiB), cancellationToken);

        var output = response.Value.Properties;

        _logger.LogInformation(
            "Retrieved provisioning recommendation. StorageGiB: {Storage}, IOPerSec: {IO}, ThroughputMiBPerSec: {Throughput}, Location: {Location}",
            provisionedStorageGiB, output.ProvisionedIOPerSec, output.ProvisionedThroughputMiBPerSec, location);

        return new()
        {
            ProvisionedIOPerSec = output.ProvisionedIOPerSec,
            ProvisionedThroughputMiBPerSec = output.ProvisionedThroughputMiBPerSec,
            AvailableRedundancyOptions = output.AvailableRedundancyOptions?.Select(r => r.ToString()).ToList() ?? []
        };
    }
}

/// <summary>
/// Result of file share name availability check.
/// </summary>
/// <param name="IsAvailable">Whether the name is available.</param>
/// <param name="Reason">The reason if the name is unavailable.</param>
/// <param name="Message">Additional message about availability.</param>
public record FileShareNameAvailabilityResult(bool IsAvailable, string? Reason, string? Message);
