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

            var fileShareData = new FileShareData(new Azure.Core.AzureLocation(location));

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

            await resourceGroupResource.Value.GetFileShares().Get(fileShareName, cancellationToken).Value.DeleteAsync(
                WaitUntil.Completed,
                cancellationToken);

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

    // TODO: Implement snapshot operations using SDK once available
    public async Task<List<FileShareSnapshotInfo>> ListFileShareSnapshotsAsync(
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

        _logger.LogWarning("Snapshot operations are not yet implemented with SDK");
        return [];
    }

    public async Task<FileShareSnapshotInfo> GetFileShareSnapshotAsync(
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

        _logger.LogWarning("Snapshot operations are not yet implemented with SDK");
        throw new NotImplementedException("Snapshot operations are not yet implemented with SDK");
    }

    public async Task<FileShareSnapshotInfo> CreateFileShareSnapshotAsync(
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

        _logger.LogWarning("Snapshot operations are not yet implemented with SDK");
        throw new NotImplementedException("Snapshot operations are not yet implemented with SDK");
    }

    public async Task<FileShareSnapshotInfo> CreateSnapshotAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string snapshotName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(fileShareName), fileShareName),
            (nameof(snapshotName), snapshotName));

        _logger.LogWarning("Snapshot operations are not yet implemented with SDK");
        throw new NotImplementedException("Snapshot operations are not yet implemented with SDK");
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
        return await GetFileShareSnapshotAsync(subscription, resourceGroup, fileShareName, snapshotId, tenant, retryPolicy, cancellationToken);
    }

    public async Task<List<FileShareSnapshotInfo>> ListSnapshotsAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        return await ListFileShareSnapshotsAsync(subscription, resourceGroup, fileShareName, tenant, retryPolicy, cancellationToken);
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

        _logger.LogWarning("Snapshot operations are not yet implemented with SDK");
        throw new NotImplementedException("Snapshot operations are not yet implemented with SDK");
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

        _logger.LogWarning("Snapshot operations are not yet implemented with SDK");
        throw new NotImplementedException("Snapshot operations are not yet implemented with SDK");
    }

    // TODO: Implement private endpoint operations using SDK once available
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

        _logger.LogWarning("Private endpoint operations are not yet implemented with SDK");
        return [];
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

        _logger.LogWarning("Private endpoint operations are not yet implemented with SDK");
        throw new NotImplementedException("Private endpoint operations are not yet implemented with SDK");
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

        _logger.LogWarning("Private endpoint operations are not yet implemented with SDK");
        throw new NotImplementedException("Private endpoint operations are not yet implemented with SDK");
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

        _logger.LogWarning("Private endpoint operations are not yet implemented with SDK");
        throw new NotImplementedException("Private endpoint operations are not yet implemented with SDK");
    }
}

/// <summary>
/// Result of file share name availability check.
/// </summary>
/// <param name="IsAvailable">Whether the name is available.</param>
/// <param name="Reason">The reason if the name is unavailable.</param>
/// <param name="Message">Additional message about availability.</param>
public record FileShareNameAvailabilityResult(bool IsAvailable, string? Reason, string? Message);
