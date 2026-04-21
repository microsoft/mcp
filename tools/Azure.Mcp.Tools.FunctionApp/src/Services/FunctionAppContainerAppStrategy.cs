// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FunctionApp.Models;
using Azure.ResourceManager.AppContainers;
using Azure.ResourceManager.AppContainers.Models;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Storage;

namespace Azure.Mcp.Tools.FunctionApp.Services;

internal static class FunctionAppContainerAppStrategy
{
    private static class ContainerAppsDefaults
    {
        public const double CpuCores = 0.25;
        public const string Memory = "0.5Gi";
        public const int IngressPort = 80;
        public const bool ExternalIngress = true;
    }

    internal static string GetContainerImage(string runtime) => runtime switch
    {
        "dotnet" => "mcr.microsoft.com/azure-functions/dotnet:4",
        "dotnet-isolated" => "mcr.microsoft.com/azure-functions/dotnet-isolated:4",
        "node" => "mcr.microsoft.com/azure-functions/node:4",
        "python" => "mcr.microsoft.com/azure-functions/python:4",
        "java" => "mcr.microsoft.com/azure-functions/java:4",
        "powershell" => "mcr.microsoft.com/azure-functions/powershell:4",
        _ => "mcr.microsoft.com/azure-functions/dotnet-isolated:4"
    };

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
        var containerApp = await EnsureMinimalContainerApp(rg, functionAppName, location, options.Runtime, storage, options.UseManagedIdentityStorage, containerAppsEnvironmentName, cancellationToken);
        var host = containerApp.Data.Configuration?.Ingress?.Fqdn ?? containerApp.Data.LatestRevisionName ?? containerApp.Data.Name;
        return new FunctionAppInfo(
            containerApp.Data.Name,
            rg.Data.Name,
            location,
            "containerapp",
            containerApp.Data.ProvisioningState.ToString(),
            host,
            "linux",
            containerApp.Data.Tags?.ToDictionary(k => k.Key, v => v.Value));
    }

    private static async Task<ContainerAppResource> EnsureMinimalContainerApp(
        ResourceGroupResource rg,
        string name,
        string location,
        string runtime,
        StorageProvisioningResult storage,
        bool useManagedIdentity,
        string? containerAppsEnvironmentName,
        CancellationToken cancellationToken)
    {
        var envs = rg.GetContainerAppManagedEnvironments();
        var envName = containerAppsEnvironmentName ?? $"{name}-env";

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

        var apps = rg.GetContainerApps();
        var image = GetContainerImage(runtime);
        var container = new ContainerAppContainer
        {
            Name = name,
            Image = image,
            Resources = new AppContainerResources
            {
                Cpu = ContainerAppsDefaults.CpuCores,
                Memory = ContainerAppsDefaults.Memory
            },
            Env =
            {
                new ContainerAppEnvironmentVariable { Name = "FUNCTIONS_WORKER_RUNTIME", Value = runtime },
                new ContainerAppEnvironmentVariable { Name = "FUNCTIONS_EXTENSION_VERSION", Value = "~4" }
            }
        };
        if (useManagedIdentity)
        {
            container.Env.Add(new ContainerAppEnvironmentVariable { Name = "AzureWebJobsStorage__accountName", Value = storage.AccountName });
            container.Env.Add(new ContainerAppEnvironmentVariable { Name = "AzureWebJobsStorage__credential", Value = "managedidentity" });
        }
        else
        {
            container.Env.Add(new ContainerAppEnvironmentVariable { Name = "AzureWebJobsStorage", Value = storage.ConnectionString });
        }

        var data = new ContainerAppData(location)
        {
            ManagedEnvironmentId = env.Id,
            Configuration = new ContainerAppConfiguration
            {
                Ingress = new ContainerAppIngressConfiguration
                {
                    External = ContainerAppsDefaults.ExternalIngress,
                    TargetPort = ContainerAppsDefaults.IngressPort
                }
            },
            Template = new ContainerAppTemplate
            {
                Containers = { container }
            }
        };
        if (useManagedIdentity)
            data.Identity = new ManagedServiceIdentity(ManagedServiceIdentityType.SystemAssigned);

        return (await apps.CreateOrUpdateAsync(WaitUntil.Completed, name, data, cancellationToken)).Value;
    }
}
