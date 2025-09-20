// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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

namespace Azure.Mcp.Tools.Storage.Services;

public class StorageService(ISubscriptionService subscriptionService, ITenantService tenantService, ICacheService cacheService) : BaseAzureService(tenantService), IStorageService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private const string CacheGroup = "storage";
    private const string StorageAccountsCacheKey = "accounts";
    private static readonly TimeSpan s_cacheDuration = TimeSpan.FromHours(1);

    public async Task<List<Models.AccountInfo>> GetAccountDetails(
        string? account,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);

        var accounts = new List<Models.AccountInfo>();
        if (string.IsNullOrEmpty(account))
        {
            try
            {
                await foreach (var accountResource in subscriptionResource.GetStorageAccountsAsync())
                {
                    var data = accountResource?.Data;
                    if (data?.Name == null)
                        continue;

                    accounts.Add(new(
                        data.Name,
                        data.Location.ToString(),
                        data.Kind?.ToString(),
                        data.Sku?.Name.ToString(),
                        data.Sku?.Tier?.ToString(),
                        data.IsHnsEnabled,
                        data.AllowBlobPublicAccess,
                        data.EnableHttpsTrafficOnly));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error listing Storage accounts: {ex.Message}", ex);
            }
        }
        else
        {
            try
            {
                var storageAccount = await GetStorageAccount(subscriptionResource, account)
                    ?? throw new Exception($"Storage account '{account}' not found in subscription '{subscription}'");

                var data = storageAccount.Data;
                accounts.Add(new(
                    data.Name,
                    data.Location.ToString(),
                    data.Kind?.ToString(),
                    data.Sku.Name.ToString(),
                    data.Sku.Tier?.ToString(),
                    data.IsHnsEnabled,
                    data.AllowBlobPublicAccess,
                    data.EnableHttpsTrafficOnly));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Storage account details for '{account}': {ex.Message}", ex);
            }
        }

        return accounts;
    }

    public async Task<Models.AccountInfo> CreateStorageAccount(
        string account,
        string resourceGroup,
        string location,
        string subscription,
        string? sku = null,
        string? accessTier = null,
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
            var storageSku = new StorageSku(string.IsNullOrEmpty(sku) ? StorageSkuName.StandardLrs : ParseStorageSkuName(sku));
            var defaultAccessTier = string.IsNullOrEmpty(accessTier) ? StorageAccountAccessTier.Hot : ParseAccessTier(accessTier);

            var createOptions = new StorageAccountCreateOrUpdateContent(
                storageSku,
                StorageKind.StorageV2,
                location)
            {
                AccessTier = defaultAccessTier,
                EnableHttpsTrafficOnly = true,
                AllowBlobPublicAccess = false,
                IsHnsEnabled = enableHierarchicalNamespace ?? false
            };

            var operation = await resourceGroupResource.Value
                .GetStorageAccounts()
                .CreateOrUpdateAsync(WaitUntil.Completed, account, createOptions);

            var result = operation.Value;
            var data = result.Data;

            return new Models.AccountInfo(
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

    public async Task<List<BlobInfo>> GetBlobDetails(
        string account,
        string container,
        string? blob,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(account, container, subscription);

        var blobServiceClient = await CreateBlobServiceClient(account, tenant, retryPolicy);
        var containerClient = blobServiceClient.GetBlobContainerClient(container);

        var blobInfos = new List<BlobInfo>();
        if (string.IsNullOrEmpty(blob))
        {
            try
            {
                await foreach (var blobItem in containerClient.GetBlobsAsync())
                {
                    blobInfos.Add(new(
                        blobItem.Name,
                        blobItem.Properties.LastModified,
                        blobItem.Properties.ETag?.ToString(),
                        blobItem.Properties.ContentLength,
                        blobItem.Properties.ContentType,
                        blobItem.Properties.ContentHash,
                        blobItem.Properties.BlobType?.ToString(),
                        blobItem.Metadata,
                        blobItem.Properties.LeaseStatus?.ToString(),
                        blobItem.Properties.LeaseState?.ToString(),
                        blobItem.Properties.LeaseDuration?.ToString(),
                        blobItem.Properties.CopyStatus?.ToString(),
                        blobItem.Properties.CopySource,
                        blobItem.Properties.CopyCompletedOn,
                        blobItem.Properties.AccessTier?.ToString(),
                        blobItem.Properties.AccessTierChangedOn,
                        blobItem.Properties.HasLegalHold,
                        blobItem.Properties.CreatedOn,
                        blobItem.Properties.ArchiveStatus?.ToString(),
                        blobItem.VersionId));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error listing blobs: {ex.Message}", ex);
            }
        }
        else
        {
            var blobClient = containerClient.GetBlobClient(blob);

            try
            {
                var response = await blobClient.GetPropertiesAsync();
                var properties = response.Value;
                blobInfos.Add(new(
                    blob,
                    properties.LastModified,
                    properties.ETag.ToString(),
                    properties.ContentLength,
                    properties.ContentType,
                    properties.ContentHash,
                    properties.BlobType.ToString(),
                    properties.Metadata,
                    properties.LeaseStatus.ToString(),
                    properties.LeaseState.ToString(),
                    properties.LeaseDuration.ToString(),
                    properties.CopyStatus.ToString(),
                    properties.CopySource,
                    properties.CopyCompletedOn,
                    properties.AccessTier.ToString(),
                    properties.AccessTierChangedOn,
                    properties.HasLegalHold,
                    properties.CreatedOn,
                    properties.ArchiveStatus,
                    properties.VersionId));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting blob details: {ex.Message}", ex);
            }
        }

        return blobInfos;
    }

    public async Task<List<ContainerInfo>> GetContainerDetails(
        string account,
        string? container,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(account, subscription);

        var blobServiceClient = await CreateBlobServiceClient(account, tenant, retryPolicy);
        var containers = new List<ContainerInfo>();

        if (string.IsNullOrEmpty(container))
        {
            try
            {
                await foreach (var containerItem in blobServiceClient.GetBlobContainersAsync())
                {
                    var properties = containerItem.Properties;
                    containers.Add(new(
                        containerItem.Name,
                        properties.LastModified,
                        properties.ETag.ToString(),
                        properties.Metadata,
                        properties.LeaseStatus?.ToString(),
                        properties.LeaseState?.ToString(),
                        properties.LeaseDuration?.ToString(),
                        properties.PublicAccess?.ToString(),
                        properties.HasImmutabilityPolicy,
                        properties.HasLegalHold,
                        properties.DeletedOn,
                        properties.RemainingRetentionDays,
                        properties.HasImmutableStorageWithVersioning));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error listing containers: {ex.Message}", ex);
            }
        }
        else
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(container);

            try
            {
                var response = await containerClient.GetPropertiesAsync();
                var properties = response.Value;
                containers.Add(new(
                    container,
                    properties.LastModified,
                    properties.ETag.ToString(),
                    properties.Metadata,
                    properties.LeaseStatus?.ToString(),
                    properties.LeaseState?.ToString(),
                    properties.LeaseDuration?.ToString(),
                    properties.PublicAccess?.ToString(),
                    properties.HasImmutabilityPolicy,
                    properties.HasLegalHold,
                    properties.DeletedOn,
                    properties.RemainingRetentionDays,
                    properties.HasImmutableStorageWithVersioning));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting container details: {ex.Message}", ex);
            }
        }

        return containers;
    }

    public async Task<ContainerInfo> CreateContainer(
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
            await containerClient.CreateAsync(PublicAccessType.None);
            var response = await containerClient.GetPropertiesAsync();
            var properties = response.Value;
            return new(
                container,
                properties.LastModified,
                properties.ETag.ToString(),
                properties.Metadata,
                properties.LeaseStatus?.ToString(),
                properties.LeaseState?.ToString(),
                properties.LeaseDuration?.ToString(),
                properties.PublicAccess?.ToString(),
                properties.HasImmutabilityPolicy,
                properties.HasLegalHold,
                properties.DeletedOn,
                properties.RemainingRetentionDays,
                properties.HasImmutableStorageWithVersioning);
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

    private async Task<BlobServiceClient> CreateBlobServiceClient(
        string account,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        var uri = $"https://{account}.blob.core.windows.net";
        var options = ConfigureRetryPolicy(AddDefaultPolicies(new BlobClientOptions()), retryPolicy);
        return new BlobServiceClient(new Uri(uri), await GetCredential(tenant), options);
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
        var response = await blobClient.UploadAsync(fileStream, false);

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
