// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.ResourceManager.AppContainers;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.FunctionApp.Services;

/// <summary>
/// Provisions a Function App hosted in an Azure Container Apps managed environment.
/// </summary>
internal static class FunctionAppContainerAppStrategy
{
    private const string ContainerAppsFunctionKind = "functionapp,linux,container,azurecontainerapps";

    public static async Task<WebSiteResource> CreateFunctionAppAsync(
        ResourceGroupResource resourceGroup,
        string functionAppName,
        string location,
        CreateOptions options,
        string? storageAccountName,
        string? containerAppsEnvironmentName,
        string endpointSuffix,
        CancellationToken cancellationToken)
    {
        var storage = await FunctionAppStorageProvisioner.EnsureStorageForFunctionApp(resourceGroup, functionAppName, location, storageAccountName, options.UseManagedIdentityStorage, endpointSuffix, cancellationToken);
        var environment = await EnsureManagedEnvironment(resourceGroup, containerAppsEnvironmentName ?? $"{functionAppName}-env", location, cancellationToken);

        var data = BuildSiteData(location, environment.Id, options, storage);
        var operation = await resourceGroup.GetWebSites().CreateOrUpdateAsync(WaitUntil.Completed, functionAppName, data, cancellationToken);
        return operation.Value;
    }

    internal static WebSiteData BuildSiteData(string location, ResourceIdentifier environmentId, CreateOptions options, StorageProvisioningResult storage)
    {
        var siteConfig = new SiteConfigProperties
        {
            LinuxFxVersion = $"DOCKER|{GetContainerImage(options.Runtime, options.RuntimeVersion)}"
        };

        siteConfig.AppSettings.Add(new AppServiceNameValuePair { Name = "FUNCTIONS_EXTENSION_VERSION", Value = "~4" });
        siteConfig.AppSettings.Add(new AppServiceNameValuePair { Name = "FUNCTIONS_WORKER_RUNTIME", Value = options.Runtime });

        if (options.UseManagedIdentityStorage)
        {
            siteConfig.AppSettings.Add(new AppServiceNameValuePair { Name = "AzureWebJobsStorage__accountName", Value = storage.AccountName });
            siteConfig.AppSettings.Add(new AppServiceNameValuePair { Name = "AzureWebJobsStorage__credential", Value = "managedidentity" });
        }
        else
        {
            siteConfig.AppSettings.Add(new AppServiceNameValuePair { Name = "AzureWebJobsStorage", Value = storage.ConnectionString });
        }

        var data = new WebSiteData(location)
        {
            Kind = ContainerAppsFunctionKind,
            ManagedEnvironmentId = environmentId.ToString(),
            SiteConfig = siteConfig
        };

        if (options.UseManagedIdentityStorage)
        {
            data.Identity = new ManagedServiceIdentity(Azure.ResourceManager.Models.ManagedServiceIdentityType.SystemAssigned);
        }

        return data;
    }

    internal static string GetContainerImage(string runtime, string? runtimeVersion)
    {
        var version = NormalizeVersionForImageTag(runtime, runtimeVersion ?? FunctionAppValidation.GetDefaultRuntimeVersion(runtime));
        var image = runtime switch
        {
            "dotnet" or "dotnet-isolated" or "node" or "python" or "java" or "powershell" => runtime,
            _ => "dotnet-isolated"
        };

        return $"mcr.microsoft.com/azure-functions/{image}:4-{image}{version}";
    }

    private static string NormalizeVersionForImageTag(string runtime, string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return string.Empty;
        }

        var trimmed = version.Trim();
        if ((runtime == "java" || runtime == "node") && trimmed.EndsWith(".0", StringComparison.Ordinal))
        {
            trimmed = trimmed[..^2];
        }

        return trimmed;
    }

    private static async Task<ContainerAppManagedEnvironmentResource> EnsureManagedEnvironment(
        ResourceGroupResource resourceGroup,
        string environmentName,
        string location,
        CancellationToken cancellationToken)
    {
        var environments = resourceGroup.GetContainerAppManagedEnvironments();
        if (await environments.ExistsAsync(environmentName, cancellationToken))
        {
            return (await environments.GetAsync(environmentName, cancellationToken)).Value;
        }

        var operation = await environments.CreateOrUpdateAsync(WaitUntil.Completed, environmentName, new ContainerAppManagedEnvironmentData(location), cancellationToken);
        return operation.Value;
    }
}
