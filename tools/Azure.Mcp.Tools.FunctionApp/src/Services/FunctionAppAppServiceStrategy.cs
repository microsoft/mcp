// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.FunctionApp.Services;

/// <summary>
/// Provisions a Function App on an App Service plan (Consumption, Flex Consumption, Premium, or App Service).
/// </summary>
internal static class FunctionAppAppServiceStrategy
{
    private const int FlexConsumptionInstanceMemoryMB = 2048;
    private const int FlexConsumptionMaximumInstanceCount = 100;

    public static async Task<WebSiteResource> CreateFunctionAppAsync(
        ResourceGroupResource resourceGroup,
        string functionAppName,
        string location,
        string? planName,
        CreateOptions options,
        string? storageAccountName,
        string endpointSuffix,
        CancellationToken cancellationToken)
    {
        var plan = await FunctionAppPlanProvisioner.EnsureAppServicePlan(resourceGroup, planName, functionAppName, location, options, cancellationToken);
        var storage = await FunctionAppStorageProvisioner.EnsureStorageForFunctionApp(resourceGroup, functionAppName, location, storageAccountName, options.UseManagedIdentityStorage, endpointSuffix, cancellationToken);
        var isLinux = plan.Data.IsReserved == true;

        var data = BuildSiteData(location, plan.Id, isLinux, options, storage, endpointSuffix);
        var operation = await resourceGroup.GetWebSites().CreateOrUpdateAsync(WaitUntil.Completed, functionAppName, data, cancellationToken);
        var site = operation.Value;

        await site.UpdateApplicationSettingsAsync(BuildAppSettings(options, storage, isLinux), cancellationToken);
        return site;
    }

    internal static WebSiteData BuildSiteData(
        string location,
        ResourceIdentifier planId,
        bool isLinux,
        CreateOptions options,
        StorageProvisioningResult storage,
        string endpointSuffix)
    {
        var data = new WebSiteData(location)
        {
            Kind = BuildKind(isLinux),
            AppServicePlanId = planId,
            SiteConfig = BuildSiteConfig(isLinux, options)
        };

        if (options.UseManagedIdentityStorage)
        {
            data.Identity = new ManagedServiceIdentity(Azure.ResourceManager.Models.ManagedServiceIdentityType.SystemAssigned);
        }

        if (options.HostingKind == HostingKind.FlexConsumption)
        {
            // Flex Consumption describes the runtime through functionAppConfig rather than LinuxFxVersion.
            if (data.SiteConfig is not null)
            {
                data.SiteConfig.LinuxFxVersion = null;
            }

            data.FunctionAppConfig = new FunctionAppConfig
            {
                Runtime = new FunctionAppRuntime
                {
                    Name = MapToFunctionAppRuntimeName(options.Runtime),
                    Version = NormalizeRuntimeVersionForConfig(options.Runtime, options.RuntimeVersion)
                },
                DeploymentStorage = FunctionAppStorageProvisioner.BuildDeploymentStorage(storage.AccountName, options.UseManagedIdentityStorage, endpointSuffix),
                ScaleAndConcurrency = new FunctionAppScaleAndConcurrency
                {
                    InstanceMemoryMB = FlexConsumptionInstanceMemoryMB,
                    MaximumInstanceCount = FlexConsumptionMaximumInstanceCount
                }
            };
        }

        return data;
    }

    internal static AppServiceConfigurationDictionary BuildAppSettings(CreateOptions options, StorageProvisioningResult storage, bool isLinux)
    {
        var settings = new AppServiceConfigurationDictionary
        {
            Properties = { ["FUNCTIONS_EXTENSION_VERSION"] = "~4" }
        };

        if (options.UseManagedIdentityStorage)
        {
            settings.Properties["AzureWebJobsStorage__accountName"] = storage.AccountName;
            settings.Properties["AzureWebJobsStorage__credential"] = "managedidentity";
        }
        else
        {
            settings.Properties["AzureWebJobsStorage"] = storage.ConnectionString;
        }

        // Flex Consumption derives the worker runtime from functionAppConfig and rejects the legacy settings.
        if (options.HostingKind == HostingKind.FlexConsumption)
        {
            return settings;
        }

        settings.Properties["FUNCTIONS_WORKER_RUNTIME"] = options.Runtime;

        if (!isLinux && options.Runtime == "node")
        {
            var version = options.RuntimeVersion ?? FunctionAppValidation.GetDefaultRuntimeVersion(options.Runtime);
            var major = FunctionAppValidation.ExtractMajorVersion(version);
            if (major is not null)
            {
                settings.Properties["WEBSITE_NODE_DEFAULT_VERSION"] = $"~{major}";
            }
        }

        return settings;
    }

    internal static string BuildKind(bool isLinux) => isLinux ? "functionapp,linux" : "functionapp";

    internal static SiteConfigProperties? BuildSiteConfig(bool isLinux, CreateOptions options)
    {
        if (isLinux)
        {
            return CreateLinuxSiteConfig(options.Runtime, options.RuntimeVersion);
        }

        return options.Runtime == "powershell" ? CreateWindowsPowerShellSiteConfig(options.RuntimeVersion) : null;
    }

    internal static SiteConfigProperties CreateLinuxSiteConfig(string runtime, string? runtimeVersion)
    {
        var version = NormalizeRuntimeVersionForConfig(runtime, runtimeVersion);
        return new SiteConfigProperties
        {
            LinuxFxVersion = runtime switch
            {
                "python" => $"Python|{version}",
                "node" => $"Node|{version}",
                "dotnet" => $"DOTNET|{version}",
                "dotnet-isolated" => $"DOTNET-ISOLATED|{version}",
                "java" => $"Java|{version}",
                "powershell" => $"PowerShell|{version}",
                _ => null
            }
        };
    }

    internal static SiteConfigProperties? CreateWindowsPowerShellSiteConfig(string? runtimeVersion)
    {
        var version = NormalizeRuntimeVersionForConfig("powershell", runtimeVersion);
        return version.Length == 0 ? null : new SiteConfigProperties { PowerShellVersion = version };
    }

    internal static FunctionAppRuntimeName MapToFunctionAppRuntimeName(string runtime) => runtime switch
    {
        "dotnet" or "dotnet-isolated" => FunctionAppRuntimeName.DotnetIsolated,
        "node" => FunctionAppRuntimeName.Node,
        "java" => FunctionAppRuntimeName.Java,
        "powershell" => FunctionAppRuntimeName.Powershell,
        "python" => FunctionAppRuntimeName.Python,
        _ => FunctionAppRuntimeName.Custom
    };

    internal static string NormalizeRuntimeVersionForConfig(string runtime, string? runtimeVersion)
    {
        var version = string.IsNullOrWhiteSpace(runtimeVersion) ? FunctionAppValidation.GetDefaultRuntimeVersion(runtime) : runtimeVersion.Trim();
        if (string.IsNullOrWhiteSpace(version))
        {
            return string.Empty;
        }

        // App Service expects Java major versions without a trailing ".0" (e.g. "17", not "17.0").
        if (runtime == "java" && version.EndsWith(".0", StringComparison.Ordinal))
        {
            version = version[..^2];
        }

        return version;
    }
}
