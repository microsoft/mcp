// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Models;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Services.Models;
using Azure.ResourceManager;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Storage.Services;

public class StorageService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ICacheService cacheService,
    ILogger<StorageService> logger) : BaseAzureResourceService(subscriptionService, tenantService), IStorageService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly ILogger<StorageService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private const string CacheGroup = "storage";
    private const string StorageAccountsCacheKey = "accounts";
    private static readonly TimeSpan s_cacheDuration = TimeSpan.FromHours(1);

    public async Task<List<StorageAccountInfo>> GetAccountDetails(
        string? account,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var accounts = new List<StorageAccountInfo>();

        if (string.IsNullOrEmpty(account))
        {
            // List all accounts
            try
            {
                return await ExecuteResourceQueryAsync(
                    "Microsoft.Storage/storageAccounts",
                    null,
                    subscription,
                    retryPolicy,
                    ConvertToAccountInfoModel,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing Storage Accounts in Subscription: {Subscription}", subscription);
                throw;
            }
        }
        else
        {
            try
            {
                var storageAccount = await ExecuteSingleResourceQueryAsync(
                    "Microsoft.Storage/storageAccounts",
                    resourceGroup: null,
                    subscription: subscription,
                    retryPolicy: retryPolicy,
                    converter: ConvertToAccountInfoModel,
                    additionalFilter: $"name =~ '{EscapeKqlString(account)}'");

                if (storageAccount == null)
                {
                    throw new KeyNotFoundException($"Storage account '{account}' not found in subscription '{subscription}'.");
                }

                accounts.Add(storageAccount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Storage account details for '{account}': {ex.Message}", ex);
            }
        }

        return accounts;
    }

    public async Task<StorageAccountResult> CreateStorageAccount(
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
        ValidateRequiredParameters(
            (nameof(account), account),
            (nameof(resourceGroup), resourceGroup),
            (nameof(location), location),
            (nameof(subscription), subscription));

        try
        {
            // Create ArmClient for deployments
            ArmClient armClient = await CreateArmClientWithApiVersionAsync("Microsoft.Storage/storageAccounts", "2024-01-01", null, retryPolicy);

            // Prepare data
            ResourceIdentifier accountId = new ResourceIdentifier($"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Storage/storageAccounts/{account}");
            var createContent = new StorageAccountCreateOrUpdateContent
            {
                Sku = new ResourceSku
                {
                    Name = string.IsNullOrEmpty(sku) ? "Standard_LRS" : ParseStorageSkuName(sku),
                    Tier = "Standard"
                },
                Kind = "StorageV2",
                Location = location,
                Properties = new StorageAccountProperties
                {
                    AccessTier = string.IsNullOrEmpty(accessTier) ? "Hot" : ParseAccessTier(accessTier),
                    EnableHttpsTrafficOnly = true,
                    AllowBlobPublicAccess = false,
                    IsHnsEnabled = enableHierarchicalNamespace ?? false
                }
            };

            var result = await CreateOrUpdateGenericResourceAsync(
                armClient,
                accountId,
                location,
                createContent,
                StorageJsonContext.Default.StorageAccountCreateOrUpdateContent);
            if (!result.HasData)
            {
                return new StorageAccountResult(
                    HasData: false,
                    Id: null,
                    Name: null,
                    Type: null,
                    Location: null,
                    SkuName: null,
                    SkuTier: null,
                    Kind: null,
                    Properties: null);
            }
            else
            {
                return new StorageAccountResult(
                    HasData: true,
                    Id: result.Data.Id.ToString(),
                    Name: result.Data.Name,
                    Type: result.Data.ResourceType.ToString(),
                    Location: result.Data.Location,
                    SkuName: result.Data.Sku?.Name,
                    SkuTier: result.Data.Sku?.Tier,
                    Kind: result.Data.Kind,
                    Properties: result.Data.Properties?.ToObjectFromJson(StorageJsonContext.Default.IDictionaryStringObject));
            }
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
        ValidateRequiredParameters(
            (nameof(account), account),
            (nameof(container), container),
            (nameof(subscription), subscription));

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
        ValidateRequiredParameters((nameof(account), account), (nameof(subscription), subscription));

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
        ValidateRequiredParameters(
            (nameof(account), account),
            (nameof(container), container),
            (nameof(subscription), subscription));

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

    private async Task<BlobServiceClient> CreateBlobServiceClient(
        string account,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        var uri = $"https://{account}.blob.core.windows.net";
        var options = ConfigureRetryPolicy(AddDefaultPolicies(new BlobClientOptions()), retryPolicy);
        return new BlobServiceClient(new Uri(uri), await GetCredential(tenant), options);
    }

    private static string ParseStorageSkuName(string sku)
    {
        if (string.IsNullOrEmpty(sku))
        {
            throw new ArgumentException("Storage SKU cannot be null or empty.");
        }

        var validSkus = new[]
        {
            "Standard_LRS", "Standard_GRS", "Standard_RAGRS", "Standard_ZRS",
            "Premium_LRS", "Premium_ZRS", "Standard_GZRS", "Standard_RAGZRS",
            "StandardV2_LRS", "StandardV2_GRS", "StandardV2_ZRS", "StandardV2_GZRS",
            "PremiumV2_LRS", "PremiumV2_ZRS"
        };

        if (!validSkus.Contains(sku, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Invalid storage SKU '{sku}'. Valid values are: {string.Join(", ", validSkus)}.");
        }

        return sku;
    }

    private static string ParseAccessTier(string accessTier)
    {
        var validTiers = new[] { "hot", "cool", "premium", "cold" };
        if (!validTiers.Contains(accessTier.ToLowerInvariant()))
        {
            throw new ArgumentException($"Invalid access tier '{accessTier}'. Valid values are: {string.Join(", ", validTiers)}.");
        }

        return accessTier;
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
        ValidateRequiredParameters(
            (nameof(account), account),
            (nameof(container), container),
            (nameof(blob), blob),
            (nameof(localFilePath), localFilePath),
            (nameof(subscription), subscription));

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

    private static StorageAccountInfo ConvertToAccountInfoModel(JsonElement item)
    {
        Models.StorageAccountData? storageAccount = Models.StorageAccountData.FromJson(item);
        if (storageAccount == null)
            throw new InvalidOperationException("Failed to parse storage account data");

        return new StorageAccountInfo(
            Name: storageAccount.ResourceName ?? "Unknown",
            Location: storageAccount.Location,
            Kind: storageAccount.Kind,
            SkuName: storageAccount.Sku?.Name,
            SkuTier: storageAccount.Sku?.Tier,
            IsHnsEnabled: storageAccount.Properties?.IsHnsEnabled,
            ProvisioningState: storageAccount.Properties?.StorageAccountProvisioningState,
            CreatedOn: storageAccount.Properties?.CreatedOn,
            AllowBlobPublicAccess: storageAccount.Properties?.AllowBlobPublicAccess,
            EnableHttpsTrafficOnly: storageAccount.Properties?.EnableHttpsTrafficOnly);
    }
}
