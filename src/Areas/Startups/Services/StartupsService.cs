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
            bool overwrite = false,
            IProgress<string>? progress = null // progress reporting
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

            // Get subscription and resource group
            var credential = new AzureCliCredential(new AzureCliCredentialOptions
            {
                TenantId = tenantId
            });

            var armClient = new ArmClient(credential);
            var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
            var resource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

            // Get or create storage account
            var storageAccounts = resource.Value.GetStorageAccounts();
            StorageAccountResource storageAccountResource;

            var availability = await subscriptionResource.CheckStorageAccountNameAvailabilityAsync(new StorageAccountNameAvailabilityContent(storageAccount));

            if (availability.Value.IsNameAvailable != true)
            {
                throw new ArgumentException($"Storage account name '{storageAccount}' is not available globally due to {availability.Value.Reason}. Please choose a different name");
            }
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

        public async Task<StartupsDeployResources> DeployReactAppAsync(
            string tenantId,
            string subscription,
            string storageAccount,
            string resourceGroup,
            string reactProject,
            RetryPolicyOptions retryPolicy,
            bool build = true,
            string? buildPath = null,
            bool overwrite = true
        )
        {
            // Validate React project directory
            if (!Directory.Exists(reactProject))
            {
                throw new DirectoryNotFoundException($"React project directory '{reactProject}' does not exist");
            }

            // Check for package.json to confirm it's a React project
            var packageJsonPath = Path.Combine(reactProject, "package.json");
            if (!File.Exists(packageJsonPath))
            {
                throw new ArgumentException($"No package.json found in '{reactProject}'. Please ensure this is a valid React project.");
            }

            string finalBuildPath = buildPath ?? Path.Combine(reactProject, "build");

            if (build)
            {
                await BuildReactAppAsync(reactProject);
            }

            // Validate build output exists
            if (!Directory.Exists(finalBuildPath))
            {
                throw new DirectoryNotFoundException($"Build output directory '{finalBuildPath}' does not exist. Please run 'npm run build' first or set build=true.");
            }

            // Deploy the built React app
            var result = await DeployStaticWebAsync(tenantId, subscription, storageAccount,
                resourceGroup, finalBuildPath, retryPolicy, overwrite);


            // Enable SPA routing for React apps
            var credential = new AzureCliCredential(new AzureCliCredentialOptions { TenantId = tenantId });
            var armClient = new ArmClient(credential);
            var subscriptionResource = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(subscription));
            var resource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);
            var storageAccountResource = await resource.Value.GetStorageAccounts().GetAsync(storageAccount);

            var keys = new List<StorageAccountKey>();
            await foreach (var key in storageAccountResource.Value.GetKeysAsync())
            {
                keys.Add(key);
            }
            var connectionString = $"DefaultEndpointsProtocol=https;AccountName={storageAccount};AccountKey={keys.First().Value};EndpointSuffix=core.windows.net";

            await EnableSpaRoutingAsync(connectionString, default);

            return result;
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

        private static async Task BuildReactAppAsync(string reactProject)
        {
            await Task.Run(async () =>
            {
                var installProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "npm",
                        Arguments = "install",
                        WorkingDirectory = reactProject,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                installProcess.Start();
                installProcess.WaitForExit();

                if (installProcess.ExitCode != 0)
                {
                    throw new InvalidOperationException($"npm install failed in '{reactProject}' with exit code {installProcess.ExitCode}");
                }

                // Run npm run build
                var buildProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "npm",
                        Arguments = "run build",
                        WorkingDirectory = reactProject,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                buildProcess.Start();
                await buildProcess.WaitForExitAsync();

                if (buildProcess.ExitCode != 0)
                {
                    var error = await buildProcess.StandardError.ReadToEndAsync();
                    throw new InvalidOperationException($"npm run build failed: {error}");
                }
            });
        }
    }
}
