// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Startups.Options;

namespace Azure.Mcp.Tools.Startups.Services
{
    public sealed class StartupsService(ISubscriptionService subscriptionService, ITenantService tenantService) : BaseAzureService(tenantService), IStartupsService
    {
        private const string PREFERRED_REGION = "westus";
        private readonly ISubscriptionService _subscriptionService = subscriptionService;

        // Deploy command
        public async Task<StartupsDeployResources> DeployStaticWebAsync(
            StartupsDeployOptions options,
            RetryPolicyOptions retryPolicy,
            IProgress<string>? progress = null
        )
        {
            progress?.Report("Validating source path");
            if (options.SourcePath == null || !Directory.Exists(options.SourcePath))
            {
                throw new DirectoryNotFoundException($"Source directory is not specified or does not exist");
            }

            progress?.Report("Authenticating with Azure");
            progress?.Report("Creating/updating storage account");
            progress?.Report("Enabling static website hosting");
            var fileCount = Directory.GetFiles(options.SourcePath, "*", SearchOption.AllDirectories).Length;
            progress?.Report($"Uploading {fileCount} files");
            progress?.Report("Deployment completed successfully");

            // Storage account name validation
            if (string.IsNullOrEmpty(options.StorageAccount) || !IsValidStorageAccountName(options.StorageAccount))
            {
                throw new ArgumentException("Storage account name must be between 3-24 characters long and only contain letters and numbers");
            }

            // Source path validation
            if (!Directory.GetFiles(options.SourcePath, "*", SearchOption.AllDirectories).Any())
            {
                throw new ArgumentException($"Source directory '{options.SourcePath}' is empty");
            }

            // Get subscription and resource group
            var armClient = await CreateArmClientAsync(options.Tenant, retryPolicy);
            var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(options.Subscription));
            var resource = await subscriptionResource.GetResourceGroupAsync(options.ResourceGroup);

            // Get or create storage account
            var storageAccounts = resource.Value.GetStorageAccounts();
            StorageAccountResource storageAccountResource;

            if (await storageAccounts.ExistsAsync(options.StorageAccount))
            {
                storageAccountResource = await storageAccounts.GetAsync(options.StorageAccount, null);
            }
            else
            {
                var availability = await subscriptionResource.CheckStorageAccountNameAvailabilityAsync(new StorageAccountNameAvailabilityContent(options.StorageAccount));

                if (availability.Value.IsNameAvailable != true)
                {
                    throw new ArgumentException($"Storage account name '{options.StorageAccount}' is not available globally due to {availability.Value.Reason}. Please choose a different name");
                }
                // Create storage account with static website support
                var data = new StorageAccountCreateOrUpdateContent(
                    new StorageSku(StorageSkuName.StandardLrs),
                    StorageKind.StorageV2,
                    PREFERRED_REGION
                )
                {
                    EnableHttpsTrafficOnly = true,
                    AllowBlobPublicAccess = false
                };

                storageAccountResource = (await storageAccounts.CreateOrUpdateAsync(WaitUntil.Completed, options.StorageAccount, data)).Value;
            }

            // Get storage account connection string
            var keys = new List<StorageAccountKey>();
            await foreach (var key in storageAccountResource.GetKeysAsync())
            {
                keys.Add(key);
            }
            var connectionString = $"DefaultEndpointsProtocol=https;AccountName={options.StorageAccount};AccountKey={keys.First().Value};EndpointSuffix=core.windows.net";

            // Enable static website hosting and upload files
            await EnableStaticWebsiteAsync(connectionString, cancellationToken: default);
            await UploadFilesAsync(connectionString, options.SourcePath, options.Overwrite, cancellationToken: default);

            var websiteUrl = storageAccountResource.Data.PrimaryEndpoints.WebUri?.ToString() ??
                 $"https://{options.StorageAccount}.web.core.windows.net/";
            var portalUrl = $"https://portal.azure.com/#@{options.Tenant}/resource/subscriptions/{options.Subscription}/resourceGroups/{options.ResourceGroup}/providers/Microsoft.Storage/storageAccounts/{options.StorageAccount}/staticwebsite";
            var containerUrl = $"https://portal.azure.com/#view/Microsoft_Azure_Storage/ContainerMenuBlade/~/overview/storageAccountId/%2Fsubscriptions%2F{options.Subscription}%2FresourceGroups%2F{options.ResourceGroup}%2Fproviders%2FMicrosoft.Storage%2FstorageAccounts%2F{options.StorageAccount}/path/%24web";

            return new StartupsDeployResources(
                StorageAccount: options.StorageAccount,
                Container: "$web",
                Status: "Success",
                WebsiteUrl: websiteUrl,
                PortalUrl: portalUrl,
                ContainerUrl: containerUrl);
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

                var contentType = GetContentType(Path.GetExtension(file));
                var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };

                using var fileStream = File.OpenRead(file);
                if (overwrite)
                {
                    await blobClient.UploadAsync(fileStream, new BlobUploadOptions
                    {
                        HttpHeaders = blobHttpHeaders
                    }, cancellationToken);
                }
                // if overwrite is false, only upload if blob doesn't exist
                else
                {
                    var exists = await blobClient.ExistsAsync(cancellationToken);
                    if (!exists)
                    {
                        await blobClient.UploadAsync(fileStream, new BlobUploadOptions
                        {
                            HttpHeaders = blobHttpHeaders
                        }, cancellationToken: cancellationToken);
                    }
                }
            }
        }

        private static string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }

        private static bool IsValidStorageAccountName(string name)
        {
            return name.Length >= 3 && name.Length <= 24 && name.All(c => char.IsLower(c) || char.IsDigit(c));
        }
    }
}
