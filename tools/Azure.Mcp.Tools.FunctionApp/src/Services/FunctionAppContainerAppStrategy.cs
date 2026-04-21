// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FunctionApp.Models;
using Azure.ResourceManager.AppContainers;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.FunctionApp.Services;

internal static class FunctionAppContainerAppStrategy
{
    private const string ContainerAppsFunctionKind = "functionapp,linux,container,azurecontainerapps";

    internal static string GetContainerImage(string runtime, string? runtimeVersion)
    {
        var effectiveVersion = string.IsNullOrWhiteSpace(runtimeVersion)
            ? FunctionAppValidation.GetDefaultRuntimeVersion(runtime)
            : runtimeVersion;
        var normalized = NormalizeVersionForImageTag(runtime, effectiveVersion);
        return runtime switch
        {
            "dotnet" => $"mcr.microsoft.com/azure-functions/dotnet:4-dotnet{normalized}",
            "dotnet-isolated" => $"mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated{normalized}",
            "node" => $"mcr.microsoft.com/azure-functions/node:4-node{normalized}",
            "python" => $"mcr.microsoft.com/azure-functions/python:4-python{normalized}",
            "java" => $"mcr.microsoft.com/azure-functions/java:4-java{normalized}",
            "powershell" => $"mcr.microsoft.com/azure-functions/powershell:4-powershell{normalized}",
            _ => $"mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated{normalized}"
        };
    }

    private static string NormalizeVersionForImageTag(string runtime, string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return string.Empty;
        var trimmed = version.Trim();
        if ((runtime == "java" || runtime == "node") && trimmed.EndsWith(".0", StringComparison.Ordinal))
            trimmed = trimmed[..^2];
        return trimmed;
    }

    public static async Task<FunctionAppInfo> CreateFunctionAppAsync(
        SubscriptionResource subscription,
        ResourceGroupResource rg,
        string functionAppName,
        string location,
        CreateOptions options,
        string? storageAccountName,
        string? containerAppsEnvironmentName,
        CancellationToken cancellationToken)
    {
        var storage = await FunctionAppStorageProvisioner.EnsureStorageForFunctionApp(subscription, rg, functionAppName, location, storageAccountName, options.UseManagedIdentityStorage, cancellationToken);
        var site = await EnsureContainerAppHostedFunctionAppAsync(rg, functionAppName, location, options.Runtime, options.RuntimeVersion, storage, options.UseManagedIdentityStorage, containerAppsEnvironmentName, cancellationToken);
        var data = site.Data;
        return new FunctionAppInfo(
            data.Name,
            rg.Data.Name,
            location,
            "containerapp",
            data.State,
            data.DefaultHostName,
            "linux",
            data.Tags?.ToDictionary(k => k.Key, v => v.Value));
    }

    private static async Task<WebSiteResource> EnsureContainerAppHostedFunctionAppAsync(
        ResourceGroupResource rg,
        string functionAppName,
        string location,
        string runtime,
        string? runtimeVersion,
        StorageProvisioningResult storage,
        bool useManagedIdentity,
        string? containerAppsEnvironmentName,
        CancellationToken cancellationToken)
    {
        var envs = rg.GetContainerAppManagedEnvironments();
        var envName = containerAppsEnvironmentName ?? $"{functionAppName}-env";

        ContainerAppManagedEnvironmentResource env;
        if (await envs.ExistsAsync(envName, cancellationToken))
        {
            env = await envs.GetAsync(envName, cancellationToken);
        }
        else
        {
            var envData = new ContainerAppManagedEnvironmentData(location);
            env = (await envs.CreateOrUpdateAsync(WaitUntil.Completed, envName, envData, cancellationToken)).Value;
        }

        var image = GetContainerImage(runtime, runtimeVersion);
        var siteConfig = new SiteConfigProperties
        {
            LinuxFxVersion = $"DOCKER|{image}"
        };
        siteConfig.AppSettings.Add(new AppServiceNameValuePair { Name = "FUNCTIONS_EXTENSION_VERSION", Value = "~4" });
        siteConfig.AppSettings.Add(new AppServiceNameValuePair { Name = "FUNCTIONS_WORKER_RUNTIME", Value = runtime });
        if (useManagedIdentity)
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
            ManagedEnvironmentId = env.Id.ToString(),
            SiteConfig = siteConfig
        };
        if (useManagedIdentity)
            data.Identity = new ManagedServiceIdentity(ManagedServiceIdentityType.SystemAssigned);

        var op = await rg.GetWebSites().CreateOrUpdateAsync(WaitUntil.Completed, functionAppName, data, cancellationToken);
        return op.Value;
    }
}
