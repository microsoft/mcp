// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using AzureMcp.Options;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;
using Azure; // WaitUntil
using AzureMcp.Services.Azure.Subscription;
using AzureMcp.Services.Azure.Tenant;
using Azure.Storage.Blobs;
using AzureMcp.Services.Azure;

namespace AzureMcp.Areas.Startups.Services
{
    public sealed class StartupsService(ISubscriptionService subscriptionService, ITenantService tenantService) : BaseAzureService(tenantService), IStartupsService
    {
        private readonly ISubscriptionService _subscriptionService = subscriptionService;
        // Guidance command
        public Task<StartupsGuidanceInfo> GetGuidanceAsync()
        {
            var info = new StartupsGuidanceInfo(
                Title: "Microsoft for Startups Guidance",
                Description: "Microsoft for Startups is a global program that helps startups succeed with access to technology, coaching, and support. Startups receive free Azure credits, technical resources, expert guidance, and opportunities to connect with Microsoft partners and customers. Learn more and apply at the website.",
                Link: "https://startups.microsoft.com/"
            );
            return Task.FromResult(info);
        }

        // Deploy command
        public async Task<StartupsDeployResources> DeployStaticWebAsync(
            string subscription,
            string storageAccount,
            string resourceGroup,
            string sourcePath)
        {

            ValidateRequiredParameters(subscription);

            // Validate service-specific validation
            if (string.IsNullOrEmpty(resourceGroup))
                throw new ArgumentException("Resource group is required", nameof(resourceGroup));
            if (string.IsNullOrEmpty(storageAccount))
                throw new ArgumentException("Storage account name is required", nameof(storageAccount));
            if (string.IsNullOrEmpty(sourcePath))
                throw new ArgumentException("Source path is required", nameof(sourcePath));

            // Validate source path exists
            if (!Directory.Exists(sourcePath))
            {
                throw new ArgumentException($"Source path '{sourcePath}' does not exist");
            }

            // Get subscription and resource group
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription);
            var resource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

            // Get or create storage account
            var storageAccounts = resource.Value.GetStorageAccounts();
            StorageAccountResource storageAccountResource;
            if (await storageAccounts.ExistsAsync(storageAccount))
            {
                storageAccountResource = await storageAccounts.GetAsync(storageAccount, null);
            }
            else
            {
                // Create storage account with static website support
                var data = new StorageAccountCreateOrUpdateContent(
                    new StorageSku(StorageSkuName.StandardLrs),
                    StorageKind.StorageV2,
                    resource.Value.Data.Location
                )
                {
                    EnableHttpsTrafficOnly = true,
                    AllowBlobPublicAccess = true
                };
                storageAccountResource = (await storageAccounts.CreateOrUpdateAsync(
                    WaitUntil.Completed, storageAccount, data)).Value;
            }
            // Get storage account connection string
            var keys = new List<StorageAccountKey>();
            await foreach (var key in storageAccountResource.GetKeysAsync())
            {
                keys.Add(key);
            }
            var connectionString = $"DefaultEndpointsProtocol=https;AccountName={storageAccount};AccountKey={keys.First().Value};EndpointSuffix=core.windows.net";

            // Enable static website hosting and upload files
            await EnableStaticWebsiteAsync(connectionString, cancellationToken: default);
            await UploadFilesAsync(connectionString, sourcePath, cancellationToken: default);

            // Get the website URL
            var websiteUrl = $"https://{storageAccount}.z13.web.core.windows.net";

            return new StartupsDeployResources(
                StorageAccount: storageAccount,
                Container: "$web",
                Status: "Success");
        }

        private static async Task EnableStaticWebsiteAsync(string connectionString, CancellationToken cancellationToken)
        {
            var blobServiceClient = new BlobServiceClient(connectionString);
            var properties = blobServiceClient.GetProperties(cancellationToken).Value;

            properties.StaticWebsite.Enabled = true;
            properties.StaticWebsite.IndexDocument = "index.html";
            properties.StaticWebsite.ErrorDocument404Path = "404.html";

            await blobServiceClient.SetPropertiesAsync(properties, cancellationToken);
        }

        // files are uploaded to the web container in Azure Blob storage, used for static website hosting
        private static async Task UploadFilesAsync(string connectionString, string sourcePath, CancellationToken cancellationToken)
        {
            // creates client to connect to Azure Blob storage
            var blobServiceClient = new BlobServiceClient(connectionString);
            // gets reference to "$web" container used for static website hosting in Azure storage
            var containerClient = blobServiceClient.GetBlobContainerClient("$web");
            // creates the container if it does not exist
            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
            foreach (var file in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
            {
                // converts full file paths to relative paths and formatting for web URLs
                var blobName = Path.GetRelativePath(sourcePath, file).Replace("\\", "/");
                // uploads file to Azure storage
                await containerClient.UploadBlobAsync(blobName, File.OpenRead(file), cancellationToken);
            }
        }
    }
}