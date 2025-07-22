// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using AzureMcp.Areas.Startups.Options;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;
using Azure; // WaitUntil
using AzureMcp.Services.Azure.Subscription;
using AzureMcp.Services.Azure.Tenant;
using Azure.Storage.Blobs;
using AzureMcp.Services.Azure;

namespace AzureMcp.Areas.Startups.Services
{
    public class StartupsService(ISubscriptionService subscriptionService, ITenantService tenantService) : BaseAzureService(tenantService), IStartupsService
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
        public async Task<StartupsDeployResources> DeployStaticWebAsync(StartupsDeployOptions options, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(options);

            // Validate individual parameters first for better error messages
            if (string.IsNullOrEmpty(options.Subscription))
                throw new ArgumentException("Subscription ID is required", nameof(options.Subscription));
            if (string.IsNullOrEmpty(options.ResourceGroup))
                throw new ArgumentException("Resource group is required", nameof(options.ResourceGroup));
            if (string.IsNullOrEmpty(options.StorageAccount))
                throw new ArgumentException("Storage account name is required", nameof(options.StorageAccount));
            if (string.IsNullOrEmpty(options.SourcePath))
                throw new ArgumentException("Source path is required", nameof(options.SourcePath));

            // Validate required parameters
            ValidateRequiredParameters(
                options.Subscription,
                options.ResourceGroup,
                options.StorageAccount,
                options.SourcePath
            );

            // Validate source path exists
            if (!Directory.Exists(options.SourcePath))
            {
                throw new ArgumentException($"Source path '{options.SourcePath}' does not exist");
            }

            // Get subscription and resource group
            var subscription = await _subscriptionService.GetSubscription(options.Subscription, options.Tenant, new AzureMcp.Options.RetryPolicyOptions());
            var resourceGroup = await subscription.GetResourceGroupAsync(options.ResourceGroup, cancellationToken);

            // Get or create storage account
            var storageAccounts = resourceGroup.Value.GetStorageAccounts();
            StorageAccountResource storageAccount;
            if (await storageAccounts.ExistsAsync(options.StorageAccount))
            {
                storageAccount = await storageAccounts.GetAsync(options.StorageAccount, null, cancellationToken);
            }
            else
            {
                // Create storage account with static website support
                var data = new StorageAccountCreateOrUpdateContent(
                    new StorageSku(StorageSkuName.StandardLrs),
                    StorageKind.StorageV2,
                    resourceGroup.Value.Data.Location
                )
                {
                    EnableHttpsTrafficOnly = true,
                    AllowBlobPublicAccess = true
                };
                storageAccount = (await storageAccounts.CreateOrUpdateAsync(
                    WaitUntil.Completed, options.StorageAccount, data, cancellationToken)).Value;
            }
            // Get storage account connection string
            var keys = new List<StorageAccountKey>();
            await foreach (var key in storageAccount.GetKeysAsync())
            {
                keys.Add(key);
            }
            var connectionString = $"DefaultEndpointsProtocol=https;AccountName={options.StorageAccount};AccountKey={keys.First().Value};EndpointSuffix=core.windows.net";

            // Enable static website hosting and upload files
            await EnableStaticWebsiteAsync(connectionString, cancellationToken);
            await UploadFilesAsync(connectionString, options.SourcePath, cancellationToken);

            // Get the website URL
            var websiteUrl = $"https://{options.StorageAccount}.z13.web.core.windows.net";

            return new StartupsDeployResources(
                StorageAccount: options.StorageAccount,
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