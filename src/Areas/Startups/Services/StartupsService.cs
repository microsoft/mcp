// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using AzureMcp.Options;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;
using Azure; // WaitUntil
using Azure.ResourceManager;
using AzureMcp.Services.Azure.Subscription;
using AzureMcp.Services.Azure.Tenant;
using Azure.Storage.Blobs;
using AzureMcp.Services.Azure;
using Azure.Identity;
using Azure.ResourceManager.Resources;
using Azure.Storage.Blobs.Models;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Areas.Startups.Services
{
    public sealed class StartupsService(ISubscriptionService subscriptionService, ITenantService tenantService) : BaseAzureService(tenantService), IStartupsService
    {
        private const string PREFERRED_REGION = "westus";
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
            bool overwrite = false,
            IProgress<string>? progress = null
        )
        {
            progress?.Report("Validating source path");
            progress?.Report("Authenticating with Azure");
            progress?.Report("Creating/updating storage account");
            progress?.Report("Enabling static website hosting");
            var fileCount = Directory.Exists(sourcePath)
                ? Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories).Length
                : 0;
            progress?.Report($"Uploading {fileCount} files");
            progress?.Report("Deployment completed successfully");

            ValidateRequiredParameters(subscription, storageAccount, resourceGroup, sourcePath);

            // Storage account name validation
            if (!IsValidStorageAccountName(storageAccount))
            {
                throw new ArgumentException("Storage account name must be between 3-24 characters long and only contain letters and numbers");
            }

            // Source path validation
            if (!Directory.Exists(sourcePath))
            {
                throw new DirectoryNotFoundException($"Source directory '{sourcePath}' does not exist");
            }

            if (!Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories).Any())
            {
                throw new ArgumentException($"Source directory '{sourcePath}' is empty");
            }

            // Auto-detect React build folder
            if (IsReactProject(sourcePath))
            {
                var buildPath = Path.Combine(sourcePath, "build");
                if (Directory.Exists(buildPath))
                {
                    progress?.Report("Detected React project - using build folder");
                    sourcePath = buildPath; // Redirect to build folder automatically
                }
                else
                {
                    progress?.Report("React project detected but no build folder found. Run 'npm run build' first.");
                    throw new DirectoryNotFoundException($"React build folder not found at '{buildPath}'. Please run 'npm run build' first.");
                }
            }

            if (!Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories).Any())
            {
                throw new ArgumentException($"Source directory '{sourcePath}' is empty");
            }

            // Get subscription and resource group
            var armClient = await CreateArmClientAsync(tenantId, retryPolicy);
            var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
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
                var availability = await subscriptionResource.CheckStorageAccountNameAvailabilityAsync(new StorageAccountNameAvailabilityContent(storageAccount));

                if (availability.Value.IsNameAvailable != true)
                {
                    throw new ArgumentException($"Storage account name '{storageAccount}' is not available globally due to {availability.Value.Reason}. Please choose a different name");
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

            if (IsReactProject(sourcePath))
            {
                await EnableSpaRoutingAsync(connectionString, default);
                progress?.Report("Enabled SPA routing for React app");
            }

            var websiteUrl = storageAccountResource.Data.PrimaryEndpoints.WebUri?.ToString() ?? 
                 $"https://{storageAccount}.web.core.windows.net/";
            var portalUrl = $"https://portal.azure.com/#@{tenantId}/resource/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Storage/storageAccounts/{storageAccount}/staticwebsite";
            var containerUrl = $"https://portal.azure.com/#view/Microsoft_Azure_Storage/ContainerMenuBlade/~/overview/storageAccountId/%2Fsubscriptions%2F{subscription}%2FresourceGroups%2F{resourceGroup}%2Fproviders%2FMicrosoft.Storage%2FstorageAccounts%2F{storageAccount}/path/%24web";

            return new StartupsDeployResources(
                StorageAccount: storageAccount,
                Container: "$web",
                Status: "Success",
                WebsiteUrl: websiteUrl,
                PortalUrl: portalUrl,
                ContainerUrl: containerUrl);
        }

        private static bool IsReactProject(string path)
        {
            var packageJsonPath = Path.Combine(path, "package.json");
            if (!File.Exists(packageJsonPath))
                return false;

            try
            {
                var packageJson = File.ReadAllText(packageJsonPath);
                // Check for React-specific indicators
                return packageJson.Contains("\"react\"") ||
                    packageJson.Contains("react-scripts") ||
                    packageJson.Contains("@vitejs/plugin-react");
            }
            catch
            {
                return false;
            }
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

        private static async Task EnableSpaRoutingAsync(string connectionString, CancellationToken cancellationToken)
        {
            var blobServiceClient = new BlobServiceClient(connectionString);
            var properties = blobServiceClient.GetProperties(cancellationToken).Value;

            properties.StaticWebsite.Enabled = true;
            properties.StaticWebsite.IndexDocument = "index.html";
            properties.StaticWebsite.ErrorDocument404Path = "index.html";

            await blobServiceClient.SetPropertiesAsync(properties, cancellationToken);
        }

        // Returns the region code used in Azure static website URLs
        private static string GetRegionCode(string location)
        {
            // Azure region codes for static website URLs
            return location.ToLowerInvariant() switch
            {
                "westus2" => "2",
                "eastus2" => "2",
                "centralus" => "1",
                "westus" => "1",
                "eastus" => "1",
                "northcentralus" => "1",
                "southcentralus" => "1",
                "westeurope" => "1",
                "northeurope" => "1",
                "southeastasia" => "1",
                "australiaeast" => "1",
                "australiasoutheast" => "1",
                "uksouth" => "1",
                "ukwest" => "1",
                "japaneast" => "1",
                "japanwest" => "1",
                "canadacentral" => "1",
                "canadaeast" => "1",
                "francecentral" => "1",
                "francesouth" => "1",
                "germanywestcentral" => "1",
                "norwayeast" => "1",
                "switzerlandnorth" => "1",
                "uaenorth" => "1",
                "brazilsouth" => "1",
                _ => "1"
            };
        }
    }
}