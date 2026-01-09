// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.FileShares.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.FileShares.Services;

/// <summary>
/// Service for Azure File Shares operations using Azure Resource Manager.
/// </summary>
public sealed class FileSharesService(ISubscriptionService subscriptionService, ITenantService tenantService, ILogger<FileSharesService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), IFileSharesService
{
    private readonly ILogger<FileSharesService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
            var fileShares = await ExecuteResourceQueryAsync(
                "Microsoft.FileShares/fileShares",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToFileShareInfo,
                cancellationToken: cancellationToken);

            return fileShares ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing file shares in subscription '{Subscription}'", subscription);
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
            var fileShare = await ExecuteSingleResourceQueryAsync(
                "Microsoft.FileShares/fileShares",
                resourceGroup: resourceGroup,
                subscription: subscription,
                retryPolicy: retryPolicy,
                converter: ConvertToFileShareInfo,
                additionalFilter: $"name =~ '{EscapeKqlString(fileShareName)}'",
                cancellationToken: cancellationToken);

            if (fileShare == null)
            {
                throw new KeyNotFoundException($"File share '{fileShareName}' not found in resource group '{resourceGroup}'.");
            }

            return fileShare;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file share '{FileShareName}' in resource group '{ResourceGroup}'", fileShareName, resourceGroup);
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
                ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

            var fileShareId = ResourceIdentifier.Parse(
                $"{resourceGroupResource.Value.Id}/providers/Microsoft.FileShares/fileShares/{fileShareName}");

            var fileShareData = new GenericResourceData(new AzureLocation(location));

            var operation = await armClient.GetGenericResources()
                .CreateOrUpdateAsync(WaitUntil.Completed, fileShareId, fileShareData, cancellationToken);

            var createdResource = operation.Value;

            // Convert the created resource back to FileShareInfo
            var fileShareInfo = await GetFileShareAsync(subscription, resourceGroup, fileShareName, tenant, retryPolicy, cancellationToken);

            _logger.LogInformation(
                "Successfully created or updated file share '{FileShareName}' in resource group '{ResourceGroup}'",
                fileShareName, resourceGroup);

            return fileShareInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating or updating file share '{FileShareName}' in resource group '{ResourceGroup}'",
                fileShareName, resourceGroup);
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

            var fileShareId = ResourceIdentifier.Parse(
                $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.FileShares/fileShares/{fileShareName}");

            var fileShareResource = armClient.GetGenericResource(fileShareId);

            await fileShareResource.DeleteAsync(WaitUntil.Completed, cancellationToken);

            _logger.LogInformation(
                "Successfully deleted file share '{FileShareName}' from resource group '{ResourceGroup}'",
                fileShareName, resourceGroup);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning(
                "File share '{FileShareName}' not found in resource group '{ResourceGroup}'",
                fileShareName, resourceGroup);
            // Idempotent delete - don't throw on not found
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting file share '{FileShareName}' from resource group '{ResourceGroup}'",
                fileShareName, resourceGroup);
            throw;
        }
    }

    public async Task<bool> CheckNameAvailabilityAsync(
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
            // Query existing file shares to check if name is available
            var fileShares = await ListFileSharesAsync(subscription, tenant: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

            bool isAvailable = !fileShares.Any(fs => fs.Name?.Equals(fileShareName, StringComparison.OrdinalIgnoreCase) ?? false);

            _logger.LogInformation(
                "File share name availability check for '{FileShareName}': {IsAvailable}",
                fileShareName, isAvailable ? "Available" : "Not Available");

            return isAvailable;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file share name availability for '{FileShareName}'", fileShareName);
            throw;
        }
    }

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

        try
        {
            var snapshots = await ExecuteResourceQueryAsync(
                "Microsoft.FileShares/fileShares/snapshots",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToFileShareSnapshotInfo,
                cancellationToken: cancellationToken);

            return snapshots ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing snapshots for file share '{FileShareName}'", fileShareName);
            throw;
        }
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

        try
        {
            var snapshot = await ExecuteSingleResourceQueryAsync(
                "Microsoft.FileShares/fileShares/snapshots",
                resourceGroup: resourceGroup,
                subscription: subscription,
                retryPolicy: retryPolicy,
                converter: ConvertToFileShareSnapshotInfo,
                additionalFilter: $"name =~ '{EscapeKqlString(snapshotId)}'",
                cancellationToken: cancellationToken);

            if (snapshot == null)
            {
                throw new KeyNotFoundException($"File share snapshot '{snapshotId}' not found.");
            }

            return snapshot;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving snapshot '{SnapshotId}'", snapshotId);
            throw;
        }
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

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

            // Generate a snapshot ID with timestamp
            var snapshotId = $"snapshot-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}";

            var snapshotResourceId = ResourceIdentifier.Parse(
                $"{resourceGroupResource.Value.Id}/providers/Microsoft.FileShares/fileShares/{fileShareName}/snapshots/{snapshotId}");

            var snapshotData = new GenericResourceData(new AzureLocation("eastus"));

            var operation = await armClient.GetGenericResources()
                .CreateOrUpdateAsync(WaitUntil.Completed, snapshotResourceId, snapshotData, cancellationToken);

            // Return the newly created snapshot
            var createdSnapshot = await ListFileShareSnapshotsAsync(subscription, resourceGroup, fileShareName, tenant, retryPolicy, cancellationToken);

            var snapshotInfo = createdSnapshot?.FirstOrDefault(s => s.Name == snapshotId);
            if (snapshotInfo != null)
            {
                return snapshotInfo;
            }

            _logger.LogInformation(
                "Successfully created snapshot '{SnapshotId}' for file share '{FileShareName}'",
                snapshotId, fileShareName);

            return new FileShareSnapshotInfo(
                Id: snapshotResourceId.ToString(),
                Name: snapshotId,
                SnapshotTime: DateTimeOffset.UtcNow.ToString("O"),
                ResourceGroup: resourceGroup);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating snapshot for file share '{FileShareName}'", fileShareName);
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
            var connections = await ExecuteResourceQueryAsync(
                "Microsoft.FileShares/fileShares/privateEndpointConnections",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToPrivateEndpointConnectionInfo,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Retrieved {Count} private endpoint connections for file share '{FileShareName}'",
                connections?.Count ?? 0, fileShareName);

            return connections ?? new List<PrivateEndpointConnectionInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing private endpoint connections for file share '{FileShareName}'", fileShareName);
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
            var connection = await ExecuteSingleResourceQueryAsync(
                "Microsoft.FileShares/fileShares/privateEndpointConnections",
                resourceGroup: resourceGroup,
                subscription: subscription,
                retryPolicy: retryPolicy,
                converter: ConvertToPrivateEndpointConnectionInfo,
                additionalFilter: $"name =~ '{EscapeKqlString(connectionName)}'",
                cancellationToken: cancellationToken);

            if (connection == null)
            {
                throw new KeyNotFoundException($"Private endpoint connection '{connectionName}' not found.");
            }

            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving private endpoint connection '{ConnectionName}'", connectionName);
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

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

            var connectionResourceId = ResourceIdentifier.Parse(
                $"{resourceGroupResource.Value.Id}/providers/Microsoft.FileShares/fileShares/{fileShareName}/privateEndpointConnections/{connectionName}");

            var connectionData = new GenericResourceData(new AzureLocation("eastus"));

            await armClient.GetGenericResources()
                .CreateOrUpdateAsync(WaitUntil.Completed, connectionResourceId, connectionData, cancellationToken);

            var updatedConnection = await GetPrivateEndpointConnectionAsync(subscription, resourceGroup, fileShareName, connectionName, tenant, retryPolicy, cancellationToken);

            _logger.LogInformation(
                "Successfully updated private endpoint connection '{ConnectionName}' status to '{Status}'",
                connectionName, status);

            return updatedConnection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating private endpoint connection '{ConnectionName}' status to '{Status}'", connectionName, status);
            throw;
        }
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

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

            var connectionResourceId = ResourceIdentifier.Parse(
                $"{resourceGroupResource.Value.Id}/providers/Microsoft.FileShares/fileShares/{fileShareName}/privateEndpointConnections/{connectionName}");

            try
            {
                await armClient.GetGenericResource(connectionResourceId)
                    .DeleteAsync(WaitUntil.Completed, cancellationToken);

                _logger.LogInformation("Successfully deleted private endpoint connection '{ConnectionName}'", connectionName);
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogWarning("Private endpoint connection '{ConnectionName}' not found (already deleted)", connectionName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting private endpoint connection '{ConnectionName}'", connectionName);
            throw;
        }
    }

    /// <summary>
    /// Converts a JsonElement from Azure Resource Graph query to a FileShare model.
    /// </summary>
    private static FileShareInfo ConvertToFileShareInfo(System.Text.Json.JsonElement item)
    {
        var fileShareData = System.Text.Json.JsonSerializer.Deserialize<FileShareDataSchema>(
            item.GetRawText(),
            FileSharesJsonContext.Default.FileShareDataSchema);

        if (fileShareData == null)
        {
            throw new InvalidOperationException("Failed to parse File Share data");
        }

        var resourceGroup = ExtractResourceGroupFromId(fileShareData.Id ?? string.Empty);

        return new FileShareInfo(
            Id: fileShareData.Id ?? string.Empty,
            Name: fileShareData.Name ?? string.Empty,
            Location: fileShareData.Location,
            ResourceGroup: resourceGroup,
            Type: fileShareData.Type,
            ProvisioningState: fileShareData.Properties?.ProvisioningState);
    }

    private static FileShareSnapshotInfo ConvertToFileShareSnapshotInfo(JsonElement item)
    {
        var snapshotData = JsonSerializer.Deserialize<FileShareSnapshotSchema>(
            item.GetRawText(),
            FileSharesJsonContext.Default.FileShareSnapshotSchema);

        if (snapshotData == null)
        {
            throw new InvalidOperationException("Failed to deserialize snapshot data.");
        }

        var resourceGroup = ExtractResourceGroupFromId(snapshotData.Id ?? string.Empty) ?? string.Empty;

        return new FileShareSnapshotInfo(
            Id: snapshotData.Id ?? string.Empty,
            Name: snapshotData.Name ?? string.Empty,
            SnapshotTime: snapshotData.Properties?.SnapshotTime,
            ResourceGroup: resourceGroup);
    }

    private static PrivateEndpointConnectionInfo ConvertToPrivateEndpointConnectionInfo(JsonElement item)
    {
        var connectionData = JsonSerializer.Deserialize<PrivateEndpointConnectionDataSchema>(
            item.GetRawText(),
            FileSharesJsonContext.Default.PrivateEndpointConnectionDataSchema);

        if (connectionData == null)
        {
            throw new InvalidOperationException("Failed to deserialize private endpoint connection data.");
        }

        return new PrivateEndpointConnectionInfo(
            Id: connectionData.Id ?? string.Empty,
            Name: connectionData.Name ?? string.Empty,
            PrivateEndpointId: connectionData.Properties?.PrivateEndpoint?.Id,
            ConnectionState: connectionData.Properties?.PrivateLinkServiceConnectionState?.Status,
            ProvisioningState: connectionData.Properties?.ProvisioningState);
    }

    /// <summary>
    /// Extracts resource group name from Azure resource ID.
    /// </summary>
    private static string? ExtractResourceGroupFromId(string resourceId)
    {
        const string pattern = "/resourcegroups/";
        var index = resourceId.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return null;
        }

        var start = index + pattern.Length;
        var end = resourceId.IndexOf('/', start);
        if (end < 0)
        {
            end = resourceId.Length;
        }

        return resourceId.Substring(start, end - start);
    }
}
