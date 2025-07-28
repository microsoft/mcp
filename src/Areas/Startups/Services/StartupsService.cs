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
            string tenantId,
            string subscription,
            string storageAccount,
            string resourceGroup,
            string sourcePath,
            RetryPolicyOptions retryPolicy,
            bool overwrite = false
        )
        {

            ValidateRequiredParameters(subscription, storageAccount, resourceGroup, sourcePath);
            var uri = $"https://{storageAccount}.blob.core.windows.net";
            // Validate source path exists
            if (!Directory.Exists(sourcePath))
            {
                throw new ArgumentException($"Source path '{sourcePath}' does not exist");
            }

            // Get subscription and resource group
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenantId, retryPolicy);
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

                var options = ConfigureRetryPolicy(AddDefaultPolicies(new BlobClientOptions()), retryPolicy);
                var blobClient = new BlobServiceClient(new Uri(uri), await GetCredential(tenantId), options);

                // Create storage account with static website support
                var data = new StorageAccountCreateOrUpdateContent(
                    new StorageSku(StorageSkuName.StandardLrs),
                    StorageKind.StorageV2,
                    resource.Value.Data.Location
                )
                {
                    EnableHttpsTrafficOnly = true,
                    AllowBlobPublicAccess = false
                };

                storageAccountResource = (await storageAccounts.CreateOrUpdateAsync(WaitUntil.Completed, storageAccount, data)).Value;
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
            await UploadFilesAsync(connectionString, sourcePath, overwrite, cancellationToken: default);

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
        private static async Task UploadFilesAsync(string connectionString, string sourcePath, bool overwrite, CancellationToken cancellationToken)
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
                var blobClient = containerClient.GetBlobClient(blobName);
                using var fileStream = File.OpenRead(file);
                if (overwrite)
                {
                    await blobClient.UploadAsync(fileStream, overwrite: true, cancellationToken: cancellationToken);
                }
                // if overwrite is false, only upload if blob doesn't exist
                else
                {
                    var exists = await blobClient.ExistsAsync(cancellationToken);
                    if (!exists)
                    {
                        await blobClient.UploadAsync(fileStream, cancellationToken: cancellationToken);
                    }
                }
                // uploads file to Azure storage
                // Correct overload: stream, cancellationToken
                using var uploadStream = File.OpenRead(file);
            }
        }
    }
}
