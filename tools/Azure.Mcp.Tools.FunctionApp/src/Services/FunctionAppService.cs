// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.FunctionApp.Models;
using Azure.ResourceManager.AppContainers;
using Azure.ResourceManager.AppContainers.Models;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Caching;

namespace Azure.Mcp.Tools.FunctionApp.Services;

public sealed class FunctionAppService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ICacheService cacheService,
    IResourceGroupService resourceGroupService,
    ILogger<FunctionAppService> logger) : BaseAzureService(tenantService), IFunctionAppService
{
    private const int MaxFunctionApps = 10_000;
    private const string CacheGroup = "functionapp";
    private static readonly TimeSpan s_cacheDuration = CacheDurations.ServiceData;

    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly IResourceGroupService _resourceGroupService = resourceGroupService ?? throw new ArgumentNullException(nameof(resourceGroupService));
    private readonly ILogger<FunctionAppService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private static class ContainerAppsDefaults
    {
        public const double CpuCores = 0.25;
        public const string Memory = "0.5Gi";
        public const int IngressPort = 80;
        public const bool ExternalIngress = true;
    }

    private static class FlexConsumptionDefaults
    {
        public const int InstanceMemoryMB = 2048;
        public const int MaximumInstanceCount = 100;
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

    public async Task<List<FunctionAppInfo>?> GetFunctionApp(
        string subscription,
        string? functionAppName,
        string? resourceGroup,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var functionApps = new List<FunctionAppInfo>();
        if (string.IsNullOrEmpty(functionAppName))
        {
            var cacheKey = (string.IsNullOrEmpty(tenant), string.IsNullOrEmpty(resourceGroup)) switch
            {
                (true, true) => subscription,
                (false, true) => CacheKeyBuilder.Build(subscription, tenant),
                (true, false) => CacheKeyBuilder.Build(subscription, resourceGroup),
                (false, false) => CacheKeyBuilder.Build(subscription, tenant, resourceGroup)
            };

            var cachedResults = await _cacheService.GetAsync<List<FunctionAppInfo>>(CacheGroup, cacheKey, s_cacheDuration, cancellationToken);
            if (cachedResults != null)
                return cachedResults;

            if (string.IsNullOrEmpty(resourceGroup))
            {
                await RetrieveAndAddFunctionApp(subscriptionResource.GetWebSitesAsync(cancellationToken), functionApps, _logger, cancellationToken);
            }
            else
            {
                var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
                if (!resourceGroupResource.HasValue)
                {
                    throw new Exception($"Resource group '{resourceGroup}' not found in subscription '{subscription}'");
                }

                await RetrieveAndAddFunctionApp(resourceGroupResource.Value.GetWebSites().GetAllAsync(cancellationToken: cancellationToken), functionApps, _logger, cancellationToken);
            }

            await _cacheService.SetAsync(CacheGroup, cacheKey, functionApps, s_cacheDuration, cancellationToken);
        }
        else
        {
            ValidateRequiredParameters(
                (nameof(functionAppName), functionAppName),
                (nameof(resourceGroup), resourceGroup));

            var cacheKey = string.IsNullOrEmpty(tenant)
                ? CacheKeyBuilder.Build(subscription, resourceGroup!, functionAppName)
                : CacheKeyBuilder.Build(subscription, tenant, resourceGroup!, functionAppName);

            var cachedResults = await _cacheService.GetAsync<List<FunctionAppInfo>>(CacheGroup, cacheKey, s_cacheDuration, cancellationToken);
            if (cachedResults != null)
                return cachedResults;

            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            if (!resourceGroupResource.HasValue)
            {
                throw new Exception($"Resource group '{resourceGroup}' not found in subscription '{subscription}'");
            }
            var site = await resourceGroupResource.Value.GetWebSites().GetAsync(functionAppName, cancellationToken);

            TryAddFunctionApp(site.Value, functionApps);
            await _cacheService.SetAsync(CacheGroup, cacheKey, functionApps, s_cacheDuration, cancellationToken);
        }

        return functionApps;
    }

    private static async Task RetrieveAndAddFunctionApp(
        AsyncPageable<WebSiteResource> sites,
        List<FunctionAppInfo> functionApps,
        ILogger<FunctionAppService> logger,
        CancellationToken cancellationToken)
    {
        await foreach (var site in sites.WithCancellation(cancellationToken))
        {
            TryAddFunctionApp(site, functionApps);
            if (functionApps.Count >= MaxFunctionApps)
            {
                logger.LogWarning("Warning: Reached maximum function app limit of {MaxFunctionApps}. Some function apps may not be included in the results.", MaxFunctionApps);
                break;
            }
        }
    }

    private static void TryAddFunctionApp(WebSiteResource site, List<FunctionAppInfo> functionApps)
    {
        if (site?.Data != null && site?.Data.Kind?.Contains("functionapp", StringComparison.OrdinalIgnoreCase) == true)
        {
            var data = site.Data;
            var os = data.Kind?.Contains("linux", StringComparison.OrdinalIgnoreCase) == true ? "linux" : "windows";
            functionApps.Add(new(data.Name, data.Id.ResourceGroupName, data.Location.ToString(), data.AppServicePlanId.Name,
                data.State, data.DefaultHostName, os, data.Tags));
        }
    }

    public async Task<FunctionAppInfo> CreateFunctionApp(
        string subscription,
        string resourceGroup,
        string functionAppName,
        string location,
        string? planName = null,
        string? hostingKind = null,
        string? sku = null,
        string? runtime = null,
        string? runtimeVersion = null,
        string? os = null,
        string? storageAccountName = null,
        string? storageAuthMode = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var requestedAuthMode = FunctionAppValidation.ParseStorageAuthMode(storageAuthMode);
        var inputs = FunctionAppValidation.ValidateAndNormalizeInputs(
            subscription, resourceGroup, functionAppName, location,
            runtime, runtimeVersion, hostingKind, sku, os,
            storageAccountName, containerAppsEnvironmentName: null);

        var resolvedHostingKind = FunctionAppValidation.ParseHostingKind(inputs.PlanType);
        if (resolvedHostingKind == HostingKind.ContainerApp)
            throw new ArgumentException("Use the 'functionapp create-containerapp' command to host a Function App in Azure Container Apps.");

        var useManagedIdentity = requestedAuthMode ?? true;
        var options = FunctionAppValidation.BuildCreateOptions(inputs, useManagedIdentity);

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var rg = await _resourceGroupService.CreateOrUpdateResourceGroup(subscription, resourceGroup, location, tenant, retryPolicy, cancellationToken);

        return await AppServiceStrategy.CreateFunctionAppAsync(subscriptionResource, rg, functionAppName, location, planName, options, inputs.StorageAccountName);
    }

    public async Task<FunctionAppInfo> CreateContainerAppFunctionApp(
        string subscription,
        string resourceGroup,
        string functionAppName,
        string location,
        string? runtime = null,
        string? runtimeVersion = null,
        string? storageAccountName = null,
        string? storageAuthMode = null,
        string? containerAppsEnvironmentName = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var requestedAuthMode = FunctionAppValidation.ParseStorageAuthMode(storageAuthMode);
        var useManagedIdentity = requestedAuthMode ?? true;

        var inputs = FunctionAppValidation.ValidateAndNormalizeInputs(
            subscription, resourceGroup, functionAppName, location,
            runtime, runtimeVersion, hostingKind: "containerapp", sku: null, os: null,
            storageAccountName, containerAppsEnvironmentName);
        var options = FunctionAppValidation.BuildCreateOptions(inputs, useManagedIdentity);

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var rg = await _resourceGroupService.CreateOrUpdateResourceGroup(subscription, resourceGroup, location, tenant, retryPolicy, cancellationToken);

        return await ContainerAppsStrategy.CreateFunctionAppAsync(subscriptionResource, rg, functionAppName, location, options, inputs.StorageAccountName, inputs.ContainerAppsEnvironmentName);
    }

    internal static SiteConfigProperties? BuildSiteConfig(bool isLinux, CreateOptions options)
    {
        if (isLinux)
            return CreateLinuxSiteConfig(options.Runtime, options.RuntimeVersion);
        return options.Runtime == "powershell" ? CreateWindowsPowerShellSiteConfig(options.RuntimeVersion) : null;
    }

    internal static string BuildKind(bool isLinux) => isLinux ? "functionapp,linux" : "functionapp";

    internal static SiteConfigProperties? CreateLinuxSiteConfig(string runtime, string? runtimeVersion)
    {
        var config = new SiteConfigProperties();
        var version = string.IsNullOrWhiteSpace(runtimeVersion) ? FunctionAppValidation.GetDefaultRuntimeVersion(runtime) : runtimeVersion;
        if (runtime == "java" && version != null && version.EndsWith(".0", StringComparison.Ordinal))
            version = version[..^2];
        config.LinuxFxVersion = runtime switch
        {
            "python" => $"Python|{version}",
            "node" => $"Node|{version}",
            "dotnet" => $"DOTNET|{version}",
            "dotnet-isolated" => $"DOTNET-ISOLATED|{version}",
            "java" => $"Java|{version}",
            "powershell" => $"PowerShell|{version}",
            _ => config.LinuxFxVersion
        };
        return config;
    }

    internal static SiteConfigProperties? CreateWindowsPowerShellSiteConfig(string? runtimeVersion)
    {
        var version = string.IsNullOrWhiteSpace(runtimeVersion) ? FunctionAppValidation.GetDefaultRuntimeVersion("powershell") : runtimeVersion;
        if (string.IsNullOrWhiteSpace(version))
            return null;
        return new SiteConfigProperties { PowerShellVersion = version };
    }

    internal static FunctionAppRuntimeName MapToFunctionAppRuntimeName(string runtime) => runtime switch
    {
        "dotnet-isolated" => FunctionAppRuntimeName.DotnetIsolated,
        "node" => FunctionAppRuntimeName.Node,
        "java" => FunctionAppRuntimeName.Java,
        "powershell" => FunctionAppRuntimeName.Powershell,
        "python" => FunctionAppRuntimeName.Python,
        "dotnet" => FunctionAppRuntimeName.DotnetIsolated,
        _ => FunctionAppRuntimeName.Custom
    };

    internal static string NormalizeRuntimeVersionForConfig(string runtime, string? runtimeVersion)
    {
        var version = string.IsNullOrWhiteSpace(runtimeVersion) ? FunctionAppValidation.GetDefaultRuntimeVersion(runtime) : runtimeVersion;
        if (string.IsNullOrWhiteSpace(version))
            return string.Empty;
        if (runtime == "java" && version.EndsWith(".0", StringComparison.Ordinal))
            version = version[..^2];
        return version;
    }

    internal static AppServiceConfigurationDictionary BuildAppSettings(string runtime, string? runtimeVersion, bool requiresLinux, string storageConnectionString, bool includeWorkerRuntime = true)
        => BuildAppSettingsInternal(runtime, runtimeVersion, requiresLinux, storageConnectionString, storageAccountName: null, useManagedIdentity: false, includeWorkerRuntime);

    private static AppServiceConfigurationDictionary BuildAppSettingsInternal(
        string runtime,
        string? runtimeVersion,
        bool requiresLinux,
        string? storageConnectionString,
        string? storageAccountName,
        bool useManagedIdentity,
        bool includeWorkerRuntime)
    {
        var settings = new AppServiceConfigurationDictionary
        {
            Properties = { ["FUNCTIONS_EXTENSION_VERSION"] = "~4" }
        };
        if (useManagedIdentity)
        {
            if (string.IsNullOrWhiteSpace(storageAccountName))
                throw new InvalidOperationException("Storage account name is required for managed-identity storage auth.");
            settings.Properties["AzureWebJobsStorage__accountName"] = storageAccountName!;
            settings.Properties["AzureWebJobsStorage__credential"] = "managedidentity";
        }
        else
        {
            settings.Properties["AzureWebJobsStorage"] = storageConnectionString ?? string.Empty;
        }
        if (includeWorkerRuntime)
            settings.Properties["FUNCTIONS_WORKER_RUNTIME"] = runtime;

        var effectiveVersion = string.IsNullOrWhiteSpace(runtimeVersion) ? FunctionAppValidation.GetDefaultRuntimeVersion(runtime) : runtimeVersion;
        if (!requiresLinux && runtime == "node" && !string.IsNullOrWhiteSpace(effectiveVersion))
        {
            var major = FunctionAppValidation.ExtractMajorVersion(effectiveVersion!);
            if (!string.IsNullOrEmpty(major))
                settings.Properties["WEBSITE_NODE_DEFAULT_VERSION"] = $"~{major}";
        }
        return settings;
    }

    private static void SanitizeAppSettingsForFlexConsumption(AppServiceConfigurationDictionary settings)
    {
        if (settings?.Properties is null)
            return;
        settings.Properties.Remove("WEBSITE_NODE_DEFAULT_VERSION");
        settings.Properties.Remove("FUNCTIONS_WORKER_RUNTIME");
    }

    private static async Task ApplyAppSettings(WebSiteResource site, CreateOptions options, StorageProvisioningResult storage)
    {
        var appSettings = BuildAppSettingsInternal(
            options.Runtime,
            options.RuntimeVersion,
            options.RequiresLinux,
            storage.ConnectionString,
            storage.AccountName,
            options.UseManagedIdentityStorage,
            includeWorkerRuntime: options.HostingKind != HostingKind.FlexConsumption);
        if (options.HostingKind == HostingKind.FlexConsumption)
            SanitizeAppSettingsForFlexConsumption(appSettings);
        await site.UpdateApplicationSettingsAsync(appSettings);
    }

    private static async Task<WebSiteResource> CreateAppServiceSiteAsync(ResourceGroupResource rg, string functionAppName, string location, AppServicePlanResource plan, CreateOptions options, StorageProvisioningResult storage)
    {
        var isLinux = plan.Data.IsReserved == true;
        var data = new WebSiteData(location)
        {
            Kind = BuildKind(isLinux),
            AppServicePlanId = plan.Id,
            SiteConfig = BuildSiteConfig(isLinux, options)
        };
        if (options.UseManagedIdentityStorage)
            data.Identity = new ManagedServiceIdentity(ManagedServiceIdentityType.SystemAssigned);
        if (options.HostingKind == HostingKind.FlexConsumption)
        {
            if (data.SiteConfig is not null)
                data.SiteConfig.LinuxFxVersion = null;
            data.FunctionAppConfig = new FunctionAppConfig
            {
                Runtime = new FunctionAppRuntime
                {
                    Name = MapToFunctionAppRuntimeName(options.Runtime),
                    Version = NormalizeRuntimeVersionForConfig(options.Runtime, options.RuntimeVersion)
                },
                DeploymentStorage = FunctionAppStorageProvisioner.BuildDeploymentStorageForAccount(storage.AccountName, options.UseManagedIdentityStorage),
                ScaleAndConcurrency = new FunctionAppScaleAndConcurrency
                {
                    InstanceMemoryMB = FlexConsumptionDefaults.InstanceMemoryMB,
                    MaximumInstanceCount = FlexConsumptionDefaults.MaximumInstanceCount
                }
            };
        }
        var op = await rg.GetWebSites().CreateOrUpdateAsync(WaitUntil.Completed, functionAppName, data);
        return op.Value;
    }

    private static async Task<ContainerAppResource> EnsureMinimalContainerApp(
        ResourceGroupResource rg,
        string name,
        string location,
        string runtime,
        StorageProvisioningResult storage,
        bool useManagedIdentity,
        string? containerAppsEnvironmentName = null)
    {
        var envs = rg.GetContainerAppManagedEnvironments();
        var envName = containerAppsEnvironmentName ?? $"{name}-env";

        ContainerAppManagedEnvironmentResource env;
        if (await envs.ExistsAsync(envName))
        {
            env = await envs.GetAsync(envName);
        }
        else
        {
            var envData = new ContainerAppManagedEnvironmentData(location);
            env = (await envs.CreateOrUpdateAsync(WaitUntil.Completed, envName, envData)).Value;
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

        return (await apps.CreateOrUpdateAsync(WaitUntil.Completed, name, data)).Value;
    }

    internal static FunctionAppInfo ConvertToFunctionAppModel(WebSiteResource siteResource)
    {
        var data = siteResource.Data;
        var os = data.Kind?.Contains("linux", StringComparison.OrdinalIgnoreCase) == true ? "linux" : "windows";
        return new FunctionAppInfo(
            data.Name,
            siteResource.Id.ResourceGroupName,
            data.Location.ToString(),
            data.AppServicePlanId.Name,
            data.State,
            data.DefaultHostName,
            os,
            data.Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
    }

    internal static class AppServiceStrategy
    {
        public static async Task<FunctionAppInfo> CreateFunctionAppAsync(
            SubscriptionResource subscription,
            ResourceGroupResource rg,
            string functionAppName,
            string location,
            string? planName,
            CreateOptions options,
            string? storageAccountName = null)
        {
            var plan = await FunctionAppPlanProvisioner.EnsureAppServicePlan(rg, planName, functionAppName, location, options);
            var storage = await FunctionAppStorageProvisioner.EnsureStorageForFunctionApp(subscription, rg, functionAppName, location, storageAccountName);
            var site = await CreateAppServiceSiteAsync(rg, functionAppName, location, plan, options, storage);
            await ApplyAppSettings(site, options, storage);
            return ConvertToFunctionAppModel(site);
        }
    }

    internal static class ContainerAppsStrategy
    {
        public static async Task<FunctionAppInfo> CreateFunctionAppAsync(
            SubscriptionResource subscription,
            ResourceGroupResource rg,
            string functionAppName,
            string location,
            CreateOptions options,
            string? storageAccountName = null,
            string? containerAppsEnvironmentName = null)
        {
            var storage = await FunctionAppStorageProvisioner.EnsureStorageForFunctionApp(subscription, rg, functionAppName, location, storageAccountName);
            var containerApp = await EnsureMinimalContainerApp(rg, functionAppName, location, options.Runtime, storage, options.UseManagedIdentityStorage, containerAppsEnvironmentName);
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
    }
}
