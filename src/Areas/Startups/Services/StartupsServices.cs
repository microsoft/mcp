// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using AzureMcp.Areas.Startups.Commands;
using AzureMcp.Areas.Startups.Options;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;
using Azure; // WaitUntil
using AzureMcp.Services.Azure.Subscription;
using AzureMcp.Services.Azure.Tenant;
using Azure.Storage.Blobs;

namespace AzureMcp.Areas.Startups.Services
{
    public sealed class StartupsServices(ISubscriptionService subscriptionService, ITenantService tenantService) : IStartupsServices
    {
        private readonly ISubscriptionService _subscriptionService = subscriptionService;
        private readonly ITenantService _tenantService = tenantService;

        public Task<StartupsGuidanceInfo> GetGuidanceAsync()
        {
            var info = new StartupsGuidanceInfo(
                Title: "Microsoft for Startups Guidance",
                Description: "Microsoft for Startups is a global program that helps startups succeed with access to technology, coaching, and support. Startups receive free Azure credits, technical resources, expert guidance, and opportunities to connect with Microsoft partners and customers. Learn more and apply at the website.",
                Link: "https://startups.microsoft.com/"
            );
            return Task.FromResult(info);
        }

        public async Task<StartupsDeployResources> DeployStaticWebAsync(StartupsDeployOptions options, CancellationToken cancellationToken)
        {
            // Resolve subscription and resource group
            var subscription = await _subscriptionService.GetSubscription(options.Subscription, options.Tenant, null);
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
                var data = new StorageAccountCreateOrUpdateContent(
                    new StorageSku(StorageSkuName.StandardLrs),
                    StorageKind.StorageV2,
                    resourceGroup.Value.Data.Location
                );
                storageAccount = (await storageAccounts.CreateOrUpdateAsync(
                    WaitUntil.Completed, options.StorageAccount, data, cancellationToken)).Value;
            }

            // Enable static website hosting if not enabled
            var blobService = await storageAccount.GetBlobService().GetAsync(cancellationToken);

            using (var process = new System.Diagnostics.Process())
            {
                process.StartInfo.FileName = "az";
                process.StartInfo.Arguments = $"storage blob service-properties update --account-name {storageAccount.Data.Name} --static-website --index-document index.html --404-document 404.html";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                await process.WaitForExitAsync(cancellationToken);

                if (process.ExitCode != 0)
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    throw new InvalidOperationException($"Failed to enable static website hosting: {error}");
                }
            }
            return new StartupsDeployResources(options.StorageAccount, options.Container, "Success");
        }
        // files are uploaded to the $web container in Azure Blob storage, which is used for static website hosting
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
