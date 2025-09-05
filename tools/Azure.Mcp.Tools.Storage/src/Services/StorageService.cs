// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Data.Tables;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tools.Storage.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Azure.Storage.Queues;

namespace Azure.Mcp.Tools.Storage.Services;

public class StorageService(ISubscriptionService subscriptionService, ITenantService tenantService, ICacheService cacheService) : BaseAzureService(tenantService), IStorageService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private const string CacheGroup = "storage";
    private const string StorageAccountsCacheKey = "accounts";
    private static readonly TimeSpan s_cacheDuration = TimeSpan.FromHours(1);

    public async Task<List<StorageAccountInfo>> GetStorageAccounts(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);

        // Create cache key using the resolved subscription ID for consistency
        var cacheKey = string.IsNullOrEmpty(tenant)
            ? $"{StorageAccountsCacheKey}_{subscriptionResource.Data.SubscriptionId}"
            : $"{StorageAccountsCacheKey}_{subscriptionResource.Data.SubscriptionId}_{tenant}";

        // Try to get from cache first
        var cachedAccounts = await _cacheService.GetAsync<List<StorageAccountInfo>>(CacheGroup, cacheKey, s_cacheDuration);
        if (cachedAccounts != null)
        {
            return cachedAccounts;
        }

        var accounts = new List<StorageAccountInfo>();
        try
        {
            await foreach (var account in subscriptionResource.GetStorageAccountsAsync())
            {
                var data = account?.Data;
                if (data?.Name == null)
                    continue;

                accounts.Add(new StorageAccountInfo(
                    data.Name,
                    data.Location.ToString(),
                    data.Kind?.ToString(),
                    data.Sku?.Name.ToString(),
                    data.Sku?.Tier.ToString(),
                    data.IsHnsEnabled,
                    data.AllowBlobPublicAccess,
                    data.EnableHttpsTrafficOnly));
            }

            // Cache the results
            await _cacheService.SetAsync(CacheGroup, cacheKey, accounts, s_cacheDuration);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Storage accounts: {ex.Message}", ex);
        }

        return accounts;
    }

    public async Task<StorageAccountInfo> GetStorageAccountDetails(
        string account,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(account, subscription);

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);

        try
        {
            var storageAccount = await GetStorageAccount(subscriptionResource, account);
            if (storageAccount == null)
            {
                throw new Exception($"Storage account '{account}' not found in subscription '{subscription}'");
            }

            var data = storageAccount.Data;
            return new StorageAccountInfo(
                data.Name,
                data.Location.ToString(),
                data.Kind?.ToString(),
                data.Sku?.Name.ToString(),
                data.Sku?.Tier.ToString(),
                data.IsHnsEnabled,
                data.AllowBlobPublicAccess,
                data.EnableHttpsTrafficOnly);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Storage account details for '{account}': {ex.Message}", ex);
        }
    }

    public async Task<StorageAccountInfo> CreateStorageAccount(
        string account,
        string resourceGroup,
        string location,
        string subscription,
        string? sku = null,
        string? kind = null,
        string? accessTier = null,
        bool? enableHttpsTrafficOnly = null,
        bool? allowBlobPublicAccess = null,
        bool? enableHierarchicalNamespace = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(account, resourceGroup, location, subscription);

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);

        try
        {
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

            if (!resourceGroupResource.HasValue)
            {
                throw new Exception($"Resource group '{resourceGroup}' not found in subscription '{subscription}'");
            }

            // Set default values
            var storageKind = string.IsNullOrEmpty(kind) ? StorageKind.StorageV2 : ParseStorageKind(kind);
            var storageSku = new StorageSku(string.IsNullOrEmpty(sku) ? StorageSkuName.StandardLrs : ParseStorageSkuName(sku));
            var defaultAccessTier = string.IsNullOrEmpty(accessTier) ? StorageAccountAccessTier.Hot : ParseAccessTier(accessTier);

            var createOptions = new StorageAccountCreateOrUpdateContent(
                storageSku,
                storageKind,
                location)
            {
                AccessTier = defaultAccessTier,
                EnableHttpsTrafficOnly = enableHttpsTrafficOnly ?? true,
                AllowBlobPublicAccess = allowBlobPublicAccess ?? false,
                IsHnsEnabled = enableHierarchicalNamespace ?? false
            };

            var operation = await resourceGroupResource.Value
                .GetStorageAccounts()
                .CreateOrUpdateAsync(WaitUntil.Completed, account, createOptions);

            var result = operation.Value;
            var data = result.Data;

            return new StorageAccountInfo(
                data.Name,
                data.Location.ToString(),
                data.Kind?.ToString(),
                data.Sku?.Name.ToString(),
                data.Sku?.Tier.ToString(),
                data.IsHnsEnabled,
                data.AllowBlobPublicAccess,
                data.EnableHttpsTrafficOnly);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating Storage account '{account}': {ex.Message}", ex);
        }
    }

    public async Task<List<string>> ListContainers(
        string account,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(account, subscription);

        var blobServiceClient = await CreateBlobServiceClient(account, tenant, retryPolicy);
        var containers = new List<string>();

        try
        {
            await foreach (var container in blobServiceClient.GetBlobContainersAsync())
            {
                containers.Add(container.Name);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error listing containers: {ex.Message}", ex);
        }

        return containers;
    }

    public async Task<List<string>> ListTables(
        string account,
        string subscription,
        AuthMethod authMethod = AuthMethod.Credential,
        string? connectionString = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(account, subscription);

        var tables = new List<string>();

        try
        {
            // First attempt with requested auth method
            var tableServiceClient = await CreateTableServiceClientWithAuth(
                account,
                subscription,
                authMethod,
                connectionString,
                tenant,
                retryPolicy);

            await foreach (var table in tableServiceClient.QueryAsync())
            {
                tables.Add(table.Name);
            }
            return tables;
        }
        catch (Exception ex) when (
            authMethod == AuthMethod.Credential &&
            ex is RequestFailedException rfEx &&
            (rfEx.Status == 403 || rfEx.Status == 401))
        {
            try
            {
                // If credential auth fails with 403/401, try key auth
                var keyClient = await CreateTableServiceClientWithAuth(
                    account, subscription, AuthMethod.Key, connectionString, tenant, retryPolicy);

                tables.Clear(); // Reset the list for reuse
                await foreach (var table in keyClient.QueryAsync())
                {
                    tables.Add(table.Name);
                }
                return tables;
            }
            catch (Exception keyEx) when (keyEx is RequestFailedException keyRfEx && keyRfEx.Status == 403)
            {
                // If key auth fails with 403, try connection string
                var connStringClient = await CreateTableServiceClientWithAuth(
                    account, subscription, AuthMethod.ConnectionString, connectionString, tenant, retryPolicy);

                tables.Clear(); // Reset the list for reuse
                await foreach (var table in connStringClient.QueryAsync())
                {
                    tables.Add(table.Name);
                }
                return tables;
            }
            catch (Exception keyEx)
            {
                throw new Exception($"Error listing tables with key auth: {keyEx.Message}", keyEx);
            }
        }
        catch (Exception ex) when (
            authMethod == AuthMethod.Key &&
            ex is RequestFailedException rfEx && rfEx.Status == 403)
        {
            try
            {
                // If key auth fails with 403, try connection string
                var connStringClient = await CreateTableServiceClientWithAuth(
                    account, subscription, AuthMethod.ConnectionString, connectionString, tenant, retryPolicy);

                tables.Clear(); // Reset the list for reuse
                await foreach (var table in connStringClient.QueryAsync())
                {
                    tables.Add(table.Name);
                }
                return tables;
            }
            catch (Exception connStringEx)
            {
                throw new Exception($"Error listing tables with connection string: {connStringEx.Message}", connStringEx);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error listing tables: {ex.Message}", ex);
        }
    }

    public async Task<List<string>> ListBlobs(
        string account,
        string container,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(account, container, subscription);

        var blobServiceClient = await CreateBlobServiceClient(account, tenant, retryPolicy);
        var containerClient = blobServiceClient.GetBlobContainerClient(container);
        var blobs = new List<string>();

        try
        {
            await foreach (var blob in containerClient.GetBlobsAsync())
            {
                blobs.Add(blob.Name);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error listing blobs: {ex.Message}", ex);
        }

        return blobs;
    }

    public async Task<BlobProperties> GetBlobDetails(
        string account,
        string container,
        string blob,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(account, container, blob, subscription);

        var blobServiceClient = await CreateBlobServiceClient(account, tenant, retryPolicy);
        var containerClient = blobServiceClient.GetBlobContainerClient(container);
        var blobClient = containerClient.GetBlobClient(blob);

        try
        {
            var properties = await blobClient.GetPropertiesAsync();
            return properties.Value;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting blob details: {ex.Message}", ex);
        }
    }

    public async Task<BlobContainerProperties> GetContainerDetails(
        string account,
        string container,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(account, container, subscription);

        var blobServiceClient = await CreateBlobServiceClient(account, tenant, retryPolicy);
        var containerClient = blobServiceClient.GetBlobContainerClient(container);

        try
        {
            var properties = await containerClient.GetPropertiesAsync();
            return properties.Value;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting container details: {ex.Message}", ex);
        }
    }

    public async Task<BlobContainerProperties> CreateContainer(
        string account,
        string container,
        string subscription,
        string? blobContainerPublicAccess = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(account, container, subscription);

        var blobServiceClient = await CreateBlobServiceClient(account, tenant, retryPolicy);
        var containerClient = blobServiceClient.GetBlobContainerClient(container);

        try
        {
            PublicAccessType publicAccessType = PublicAccessType.None;

            if (!string.IsNullOrEmpty(blobContainerPublicAccess))
            {
                publicAccessType = blobContainerPublicAccess.ToLowerInvariant() switch
                {
                    "blob" => PublicAccessType.Blob,
                    "container" => PublicAccessType.BlobContainer,
                    _ => throw new Exception($"Unknown blob-container-public-access {blobContainerPublicAccess}, only 'blob' or 'container' are allowed.")
                };
            }

            await containerClient.CreateAsync(publicAccessType);
            var properties = await containerClient.GetPropertiesAsync();
            return properties.Value;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating container: {ex.Message}", ex);
        }
    }

    private async Task<string> GetStorageAccountKey(
        string account,
        string subscription,
        string? tenant = null)
    {
        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant);
        var storageAccount = await GetStorageAccount(subscriptionResource, account) ??
            throw new Exception($"Storage account '{account}' not found in subscription '{subscription}'");

        var keys = new List<StorageAccountKey>();
        await foreach (var key in storageAccount.GetKeysAsync())
        {
            keys.Add(key);
        }

        var firstKey = keys.FirstOrDefault() ?? throw new Exception($"No keys found for storage account '{account}'");
        return firstKey.Value;
    }

    private async Task<string> GetStorageAccountConnectionString(
        string account,
        string subscription,
        string? tenant = null)
    {
        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant);
        var storageAccount = await GetStorageAccount(subscriptionResource, account) ??
            throw new Exception($"Storage account '{account}' not found in subscription '{subscription}'");

        var keys = new List<StorageAccountKey>();
        await foreach (var key in storageAccount.GetKeysAsync())
        {
            keys.Add(key);
        }

        var firstKey = keys.FirstOrDefault() ?? throw new Exception($"No keys found for storage account '{account}'");
        return $"DefaultEndpointsProtocol=https;AccountName={account};AccountKey={firstKey.Value};EndpointSuffix=core.windows.net";
    }

    // Helper method to get storage account
    private static async Task<StorageAccountResource?> GetStorageAccount(SubscriptionResource subscription, string account)
    {
        await foreach (var storageAccount in subscription.GetStorageAccountsAsync())
        {
            if (storageAccount.Data.Name == account)
            {
                return storageAccount;
            }
        }
        return null;
    }

    protected async Task<TableServiceClient> CreateTableServiceClientWithAuth(
        string account,
        string subscription,
        AuthMethod authMethod,
        string? connectionString = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        var options = ConfigureRetryPolicy(AddDefaultPolicies(new TableClientOptions()), retryPolicy);

        switch (authMethod)
        {
            case AuthMethod.Key:
                var key = await GetStorageAccountKey(account, subscription, tenant);
                var uri = $"https://{account}.table.core.windows.net";
                return new TableServiceClient(new Uri(uri), new TableSharedKeyCredential(account, key), options);

            case AuthMethod.ConnectionString:
                var connString = await GetStorageAccountConnectionString(account, subscription, tenant);
                return new TableServiceClient(connString, options);

            case AuthMethod.Credential:
            default:
                var defaultUri = $"https://{account}.table.core.windows.net";
                return new TableServiceClient(new Uri(defaultUri), await GetCredential(tenant), options);
        }
    }

    private async Task<BlobServiceClient> CreateBlobServiceClient(
        string account,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        var uri = $"https://{account}.blob.core.windows.net";
        var options = ConfigureRetryPolicy(AddDefaultPolicies(new BlobClientOptions()), retryPolicy);
        return new BlobServiceClient(new Uri(uri), await GetCredential(tenant), options);
    }

    private async Task<DataLakeServiceClient> CreateDataLakeServiceClient(
        string account,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        var uri = $"https://{account}.dfs.core.windows.net";
        var options = ConfigureRetryPolicy(AddDefaultPolicies(new DataLakeClientOptions()), retryPolicy);
        return new DataLakeServiceClient(new Uri(uri), await GetCredential(tenant), options);
    }

    private async Task<ShareServiceClient> CreateShareServiceClient(
        string account,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        var uri = $"https://{account}.file.core.windows.net";
        var options = ConfigureRetryPolicy(AddDefaultPolicies(new ShareClientOptions()), retryPolicy);
        options.ShareTokenIntent = ShareTokenIntent.Backup; // Set the intent for file backup, needed for Manged Identity
        return new ShareServiceClient(new Uri(uri), await GetCredential(tenant), options);
    }

    private async Task<QueueServiceClient> CreateQueueServiceClient(
        string account,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        var uri = $"https://{account}.queue.core.windows.net";
        var options = ConfigureRetryPolicy(AddDefaultPolicies(new QueueClientOptions()), retryPolicy);
        return new QueueServiceClient(new Uri(uri), await GetCredential(tenant), options);
    }

    public async Task<List<DataLakePathInfo>> ListDataLakePaths(
        string account,
        string fileSystem,
        bool recursive,
        string subscription,
        string? filterPath = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(account, fileSystem, subscription);

        var dataLakeServiceClient = await CreateDataLakeServiceClient(account, tenant, retryPolicy);
        var fileSystemClient = dataLakeServiceClient.GetFileSystemClient(fileSystem);
        var paths = new List<DataLakePathInfo>();

        try
        {
            await foreach (var pathItem in fileSystemClient.GetPathsAsync(filterPath, recursive))
            {
                var pathInfo = new DataLakePathInfo(
                    pathItem.Name,
                    pathItem.IsDirectory == true ? "directory" : "file",
                    pathItem.ContentLength,
                    pathItem.LastModified,
                    pathItem.ETag.ToString());

                paths.Add(pathInfo);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error listing Data Lake paths: {ex.Message}", ex);
        }

        return paths;
    }

    public async Task<DataLakePathInfo> CreateDirectory(
        string account,
        string directoryPath,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(account, directoryPath, subscription);

        var dataLakeServiceClient = await CreateDataLakeServiceClient(account, tenant, retryPolicy);

        try
        {
            // Parse the directory path to extract file system name and directory path
            var pathParts = directoryPath.Split('/', 2);
            if (pathParts.Length < 2)
            {
                throw new ArgumentException("DirectoryPath must include file system name (e.g., 'myfilesystem/path/to/directory')");
            }

            var fileSystemName = pathParts[0];
            var directoryPathWithinFileSystem = pathParts[1];

            var fileSystemClient = dataLakeServiceClient.GetFileSystemClient(fileSystemName);
            var directoryClient = fileSystemClient.GetDirectoryClient(directoryPathWithinFileSystem);
            var response = await directoryClient.CreateIfNotExistsAsync();

            if (response?.Value == null)
            {
                // Directory already exists, get its properties
                var properties = await directoryClient.GetPropertiesAsync();
                return new DataLakePathInfo(
                    directoryPath,
                    "directory",
                    null, // Directories don't have content length
                    properties.Value.LastModified,
                    properties.Value.ETag.ToString());
            }
            else
            {
                // Directory was created, return new directory info
                return new DataLakePathInfo(
                    directoryPath,
                    "directory",
                    null, // Directories don't have content length
                    response.Value.LastModified,
                    response.Value.ETag.ToString());
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating directory: {ex.Message}", ex);
        }
    }

    public async Task<(List<string> SuccessfulBlobs, List<string> FailedBlobs)> SetBlobTierBatch(
        string account,
        string container,
        string tier,
        string[] blobs,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(account, container, tier, subscription);

        if (blobs == null || blobs.Length == 0)
        {
            throw new ArgumentException("At least one blob name must be provided.", nameof(blobs));
        }

        var blobServiceClient = await CreateBlobServiceClient(account, tenant, retryPolicy);
        var containerClient = blobServiceClient.GetBlobContainerClient(container);
        var batchClient = blobServiceClient.GetBlobBatchClient();
        var accessTier = new AccessTier(tier);

        try
        {
            // Use Azure.Storage.Blobs.Batch for true batch operations
            var batch = batchClient.CreateBatch();

            // Add all blob tier operations to the batch
            var batchOperations = new List<(string blobName, Response batchOperationResponse)>();
            foreach (var blobName in blobs)
            {
                var blobClient = containerClient.GetBlobClient(blobName);
                var batchOperationResponse = batch.SetBlobAccessTier(blobClient.Uri, accessTier);
                batchOperations.Add((blobName, batchOperationResponse));
            }

            // Submit the batch operation
            var batchResponse = await batchClient.SubmitBatchAsync(batch);

            // Process results
            var successfulBlobs = new List<string>();
            var failedBlobs = new List<string>();

            for (int i = 0; i < batchOperations.Count; i++)
            {
                var (blobName, batchOperationResponse) = batchOperations[i];
                try
                {
                    // Check if the individual operation succeeded
                    if (batchOperationResponse.Status >= 200 && batchOperationResponse.Status < 300)
                    {
                        successfulBlobs.Add(blobName);
                    }
                    else
                    {
                        failedBlobs.Add($"{blobName}: HTTP {batchOperationResponse.Status}");
                    }
                }
                catch (Exception ex)
                {
                    failedBlobs.Add($"{blobName}: {ex.Message}");
                }
            }

            return (successfulBlobs, failedBlobs);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error setting blob tier batch: {ex.Message}", ex);
        }
    }

    public async Task<List<FileShareItemInfo>> ListFilesAndDirectories(
        string account,
        string share,
        string directoryPath,
        string? prefix,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(account, share, directoryPath, subscription);

        var shareServiceClient = await CreateShareServiceClient(account, tenant, retryPolicy);

        try
        {
            var shareClient = shareServiceClient.GetShareClient(share);
            var directoryClient = shareClient.GetDirectoryClient(directoryPath);

            var items = new List<FileShareItemInfo>();

            await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync(prefix: prefix))
            {
                items.Add(new FileShareItemInfo(
                    item.Name,
                    item.IsDirectory,
                    item.FileSize,
                    item.Properties.LastModified,
                    item.Properties.ETag.ToString()));
            }

            return items;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error listing files and directories: {ex.Message}", ex);
        }
    }

    public async Task<QueueMessageSendResult> SendQueueMessage(
        string account,
        string queue,
        string message,
        int? timeToLiveInSeconds,
        int? visibilityTimeoutInSeconds,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(account, queue, message, subscription);

        // Create queue service client
        var queueServiceClient = await CreateQueueServiceClient(account, tenant, retryPolicy);
        var queueClient = queueServiceClient.GetQueueClient(queue);

        try
        {
            // Send message with optional parameters
            TimeSpan? timeToLive = timeToLiveInSeconds.HasValue ? TimeSpan.FromSeconds(timeToLiveInSeconds.Value) : null;
            TimeSpan? visibilityTimeout = visibilityTimeoutInSeconds.HasValue ? TimeSpan.FromSeconds(visibilityTimeoutInSeconds.Value) : null;

            var response = await queueClient.SendMessageAsync(message, visibilityTimeout, timeToLive);

            return new QueueMessageSendResult(
                response.Value.MessageId,
                response.Value.InsertionTime,
                response.Value.ExpirationTime,
                response.Value.PopReceipt,
                response.Value.TimeNextVisible,
                message);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error sending queue message: {ex.Message}", ex);
        }
    }

    private static StorageKind ParseStorageKind(string kind)
    {
        return kind?.ToLowerInvariant() switch
        {
            "storage" => StorageKind.Storage,
            "storagev2" => StorageKind.StorageV2,
            "blobstorage" => StorageKind.BlobStorage,
            "filestorage" => StorageKind.FileStorage,
            "blockstorage" => StorageKind.BlockBlobStorage,
            _ => throw new ArgumentException($"Invalid storage kind '{kind}'. Valid values are: Storage, StorageV2, BlobStorage, FileStorage, BlockBlobStorage")
        };
    }

    private static StorageSkuName ParseStorageSkuName(string sku)
    {
        return sku?.ToUpperInvariant() switch
        {
            "STANDARD_LRS" => StorageSkuName.StandardLrs,
            "STANDARD_GRS" => StorageSkuName.StandardGrs,
            "STANDARD_RAGRS" => StorageSkuName.StandardRagrs,
            "STANDARD_ZRS" => StorageSkuName.StandardZrs,
            "PREMIUM_LRS" => StorageSkuName.PremiumLrs,
            "PREMIUM_ZRS" => StorageSkuName.PremiumZrs,
            "STANDARD_GZRS" => StorageSkuName.StandardGzrs,
            "STANDARD_RAGZRS" => StorageSkuName.StandardRagzrs,
            _ => throw new ArgumentException($"Invalid storage SKU '{sku}'. Valid values are: Standard_LRS, Standard_GRS, Standard_RAGRS, Standard_ZRS, Premium_LRS, Premium_ZRS, Standard_GZRS, Standard_RAGZRS")
        };
    }

    private static StorageAccountAccessTier? ParseAccessTier(string? accessTier)
    {
        if (string.IsNullOrEmpty(accessTier))
            return null;

        return accessTier.ToLowerInvariant() switch
        {
            "hot" => StorageAccountAccessTier.Hot,
            "cool" => StorageAccountAccessTier.Cool,
            _ => throw new ArgumentException($"Invalid access tier '{accessTier}'. Valid values are: Hot, Cool")
        };
    }

    public async Task<BlobUploadResult> UploadBlob(
        string account,
        string container,
        string blob,
        string localFilePath,
        bool overwrite,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(account, container, blob, localFilePath, subscription);

        if (!File.Exists(localFilePath))
        {
            throw new FileNotFoundException($"Local file not found: {localFilePath}");
        }

        var blobServiceClient = await CreateBlobServiceClient(account, tenant, retryPolicy);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(container);
        var blobClient = blobContainerClient.GetBlobClient(blob);

        // Upload the file
        using var fileStream = File.OpenRead(localFilePath);
        var response = await blobClient.UploadAsync(fileStream, overwrite);

        return new BlobUploadResult(
            Blob: blob,
            Container: container,
            UploadedFile: localFilePath,
            LastModified: response.Value.LastModified,
            ETag: response.Value.ETag.ToString(),
            MD5Hash: response.Value.ContentHash != null ? Convert.ToBase64String(response.Value.ContentHash) : null
        );
    }
}
