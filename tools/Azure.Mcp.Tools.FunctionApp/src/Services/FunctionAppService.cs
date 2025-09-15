// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tools.FunctionApp.Models;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;
using Azure.ResourceManager.AppContainers;
using Azure.ResourceManager.AppContainers.Models;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.FunctionApp.Services;

public sealed class FunctionAppService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ICacheService cacheService) : BaseAzureService(tenantService), IFunctionAppService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));

    private const string CacheGroup = "functionapp";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    private static readonly HashSet<string> SupportedRuntimes = new()
    {
        "dotnet", "dotnet-isolated", "node", "python", "java", "powershell", "custom"
    };

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

    private static string? GetDefaultRuntimeVersion(string runtime) => runtime switch
    {
        "python" => "3.12",
        "node" => "22",
        "dotnet" => "8.0",
        "dotnet-isolated" => "8.0",
        "java" => "17",
        "powershell" => "7.4",
        _ => null
    };

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

    private static AppServiceSkuDescription GetDefaultSku(HostingKind hostingKind) => hostingKind switch
    {
        HostingKind.FlexConsumption => new AppServiceSkuDescription { Name = "FC1", Tier = "FlexConsumption" },
        HostingKind.Premium => new AppServiceSkuDescription { Name = "EP1", Tier = "ElasticPremium" },
        HostingKind.Consumption => new AppServiceSkuDescription { Name = "Y1", Tier = "Dynamic" },
        HostingKind.AppService => new AppServiceSkuDescription { Name = "B1", Tier = "Basic" },
        _ => new AppServiceSkuDescription { Name = "Y1", Tier = "Dynamic" }
    };

    internal readonly record struct NormalizedInputs(
        string Runtime,
        string? RuntimeVersion,
        string? PlanType,
        string? PlanSku,
        string? OperatingSystem,
        string? StorageAccountName,
        string? ContainerAppsEnvironmentName);

    internal static NormalizedInputs ValidateAndNormalizeInputs(
        string subscription,
        string resourceGroup,
        string functionAppName,
        string location,
        string? runtime,
        string? runtimeVersion,
        string? hostingKind,
        string? sku,
        string? os,
        string? storageAccountName,
        string? containerAppsEnvironmentName)
    {
        ValidateRequiredParameters(subscription, resourceGroup, functionAppName, location);

        var inputs = new NormalizedInputs(
            string.IsNullOrWhiteSpace(runtime) ? "dotnet" : runtime.Trim().ToLowerInvariant(),
            string.IsNullOrWhiteSpace(runtimeVersion) ? null : runtimeVersion.Trim(),
            string.IsNullOrWhiteSpace(hostingKind) ? null : hostingKind.Trim().ToLowerInvariant(),
            string.IsNullOrWhiteSpace(sku) ? null : sku.Trim(),
            string.IsNullOrWhiteSpace(os) ? null : os.Trim().ToLowerInvariant(),
            string.IsNullOrWhiteSpace(storageAccountName) ? null : storageAccountName.Trim(),
            string.IsNullOrWhiteSpace(containerAppsEnvironmentName) ? null : containerAppsEnvironmentName.Trim());

        ValidateParameterCombinations(inputs);
        return inputs;
    }

    public async Task<List<FunctionAppInfo>?> GetFunctionApp(
        string subscription,
        string? functionAppName,
        string? resourceGroup,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);
        var cacheKey = string.IsNullOrEmpty(tenant) ? subscription : $"{subscription}_{tenant}";
        var cachedResults = await _cacheService.GetAsync<List<FunctionAppInfo>>(CacheGroup, cacheKey, CacheDuration);
        if (cachedResults != null)
        {
            return cachedResults;
        }

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var functionApps = new List<FunctionAppInfo>();

        await foreach (var site in subscriptionResource.GetWebSitesAsync())
        {
            if (site?.Data is { } d && IsFunctionApp(d))
                functionApps.Add(ConvertToFunctionAppModel(site));
        }
        await _cacheService.SetAsync(CacheGroup, cacheKey, functionApps, CacheDuration);

        return functionApps;
    }

    private static async Task RetrieveAndAddFunctionApp(AsyncPageable<WebSiteResource> sites, List<FunctionAppInfo> functionApps)
    {
        ValidateRequiredParameters(subscription, resourceGroup, functionAppName);
        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var rg = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy)
            ?? throw new InvalidOperationException($"Resource group '{resourceGroup}' not found in subscription '{subscription}'.");

        var sites = rg.GetWebSites();
        if (!await sites.ExistsAsync(functionAppName))
            return null;

        var site = await sites.GetAsync(functionAppName);
        if (!IsFunctionApp(site.Value.Data))
            return null;

        return ConvertToFunctionAppModel(site.Value);
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
        string? containerAppsEnvironmentName = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        var inputs = ValidateAndNormalizeInputs(
            subscription, resourceGroup, functionAppName, location,
            runtime, runtimeVersion, hostingKind, sku, os,
            storageAccountName, containerAppsEnvironmentName);
        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var rg = await _resourceGroupService.CreateOrUpdateResourceGroup(subscription, resourceGroup, location, tenant, retryPolicy);
        var options = BuildCreateOptions(inputs);

        return options.HostingKind == HostingKind.ContainerApp
            ? await ContainerAppsStrategy.CreateFunctionAppAsync(subscriptionResource, rg, functionAppName, location, options, inputs.StorageAccountName, inputs.ContainerAppsEnvironmentName)
            : await AppServiceStrategy.CreateFunctionAppAsync(subscriptionResource, rg, functionAppName, location, planName, options, inputs.StorageAccountName);
    }

    internal static void ValidateParameterCombinations(NormalizedInputs inputs)
    {
        var hostingKind = ParseHostingKind(inputs.PlanType);

        if (inputs.OperatingSystem is not null && inputs.OperatingSystem != "windows" && inputs.OperatingSystem != "linux")
            throw new ArgumentException("Operating system must be either 'windows' or 'linux'.");

        if (inputs.StorageAccountName is not null)
        {
            if (inputs.StorageAccountName.Length < 3 || inputs.StorageAccountName.Length > 24)
                throw new ArgumentException("Storage account name must be between 3 and 24 characters long.");
            if (!inputs.StorageAccountName.All(c => char.IsLetterOrDigit(c) && (char.IsDigit(c) || char.IsLower(c))))
                throw new ArgumentException("Storage account name must contain only lowercase letters and numbers.");
        }

        if (inputs.ContainerAppsEnvironmentName is not null && hostingKind != HostingKind.ContainerApp)
            throw new InvalidOperationException("Container Apps environment name can only be specified when using Container Apps hosting.");

        if (hostingKind == HostingKind.ContainerApp && inputs.PlanSku is not null)
            throw new InvalidOperationException("Plan SKU cannot be specified for Container Apps hosting.");

        if (!SupportedRuntimes.Contains(inputs.Runtime))
            throw new ArgumentException($"Runtime '{inputs.Runtime}' is not supported. Supported runtimes: {string.Join(", ", SupportedRuntimes)}.");

        if (inputs.Runtime == "python" && inputs.OperatingSystem == "windows")
            throw new InvalidOperationException("Python runtime requires Linux operating system.");

        if (hostingKind == HostingKind.FlexConsumption && inputs.Runtime == "dotnet" && inputs.RuntimeVersion is not null)
        {
            throw new InvalidOperationException("Flex Consumption with .NET runtime automatically uses dotnet-isolated. Specify runtime as 'dotnet-isolated' instead.");
        }
    }

    internal static CreateOptions BuildCreateOptions(NormalizedInputs inputs)
    {
        var hostingKind = ParseHostingKind(inputs.PlanType);
        var selectedRuntime = hostingKind == HostingKind.FlexConsumption && inputs.Runtime == "dotnet"
            ? "dotnet-isolated"
            : inputs.Runtime;
        var selectedRuntimeVersion = inputs.RuntimeVersion ?? GetDefaultRuntimeVersion(selectedRuntime);
        var (requiresLinux, normalizedOs) = ResolveOs(selectedRuntime, hostingKind, inputs.OperatingSystem);
        return new CreateOptions(selectedRuntime, selectedRuntimeVersion, hostingKind, requiresLinux, inputs.PlanSku, normalizedOs);
    }

    internal static HostingKind ParseHostingKind(string? planType)
    {
        return planType switch
        {
            "flex" or "flexconsumption" => HostingKind.FlexConsumption,
            "premium" or "functionspremium" => HostingKind.Premium,
            "appservice" => HostingKind.AppService,
            "containerapp" or "containerapps" => HostingKind.ContainerApp,
            _ => HostingKind.Consumption
        };
    }

    internal static async Task<AppServicePlanResource> CreatePlan(ResourceGroupResource rg, string planName, string location, CreateOptions options)
    {
        var sku = !string.IsNullOrWhiteSpace(options.ExplicitSku)
            ? new AppServiceSkuDescription { Name = options.ExplicitSku!.Trim(), Tier = InferTier(options.ExplicitSku!) }
            : GetDefaultSku(options.HostingKind);

        var data = new AppServicePlanData(location) { Sku = sku, IsReserved = options.RequiresLinux };
        var op = await rg.GetAppServicePlans().CreateOrUpdateAsync(WaitUntil.Completed, planName, data);
        return op.Value;
    }

    internal static void ValidateExistingPlan(AppServicePlanResource plan, string planName, CreateOptions options)
    {
        if (options.RequiresLinux && plan.Data.IsReserved != true)
            throw new InvalidOperationException($"App Service plan '{planName}' must be Linux for runtime '{options.Runtime}'.");

        if (options.HostingKind == HostingKind.FlexConsumption && !IsFlexConsumption(plan.Data))
            throw new InvalidOperationException($"App Service plan '{planName}' is not a Flex Consumption plan.");
        if (options.HostingKind == HostingKind.Premium && !string.Equals(plan.Data.Sku?.Tier, "ElasticPremium", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"App Service plan '{planName}' is not an Elastic Premium plan.");
    }

    internal static SiteConfigProperties? BuildSiteConfig(bool isLinux, CreateOptions options)
    {
        if (isLinux)
            return CreateLinuxSiteConfig(options.Runtime, options.RuntimeVersion);
        return options.Runtime == "powershell" ? CreateWindowsPowerShellSiteConfig(options.RuntimeVersion) : null;
    }

    internal static string BuildKind(bool isLinux) => isLinux ? "functionapp,linux" : "functionapp";

    internal static async Task<AppServicePlanResource> EnsureAppServicePlan(ResourceGroupResource rg, string? planName, string functionAppName, string location, CreateOptions options)
    {
        var effectivePlanName = planName ?? $"{functionAppName}-plan";
        var plans = rg.GetAppServicePlans();

        if (await plans.ExistsAsync(effectivePlanName))
        {
            var existing = await plans.GetAsync(effectivePlanName);
            ValidateExistingPlan(existing, effectivePlanName, options);
            return existing;
        }

        return await CreatePlan(rg, effectivePlanName, location, options);
    }

    internal static async Task<WebSiteResource> CreateAppServiceSiteAsync(ResourceGroupResource rg, string functionAppName, string location, AppServicePlanResource plan, CreateOptions options, string storageConnection)
    {
        var isLinux = plan.Data.IsReserved == true;
        var data = new WebSiteData(location)
        {
            Kind = BuildKind(isLinux),
            AppServicePlanId = plan.Id,
            SiteConfig = BuildSiteConfig(isLinux, options)
        };
        if (options.HostingKind == HostingKind.FlexConsumption)
        {
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
                DeploymentStorage = BuildDeploymentStorage(storageConnection),
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


    internal static async Task ApplyAppSettings(WebSiteResource site, CreateOptions options, string storageConnectionString)
    {
        var appSettings = BuildAppSettings(options.Runtime, options.RuntimeVersion, options.RequiresLinux, storageConnectionString, includeWorkerRuntime: options.HostingKind != HostingKind.FlexConsumption);
        if (options.HostingKind == HostingKind.FlexConsumption)
            SanitizeAppSettingsForFlexConsumption(appSettings);
        await site.UpdateApplicationSettingsAsync(appSettings);
    }

    internal static bool IsFunctionApp(WebSiteData siteData)
    {
        return siteData.Kind?.Contains("functionapp", StringComparison.OrdinalIgnoreCase) == true;
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
            data.Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        );
    }

    internal static string CreateStorageAccountName(string functionAppName)
    {
        var baseName = new string(functionAppName.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
        if (string.IsNullOrEmpty(baseName))
        {
            baseName = "func";
        }
        var trimmed = baseName.Length > 18 ? baseName.Substring(0, 18) : baseName;
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 6);
        return $"{trimmed}{suffix}";
    }

    internal static string BuildConnectionString(string accountName, string key)
    {
        return $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={key};EndpointSuffix=core.windows.net";
    }

    internal static async Task<string> EnsureStorageForFunctionApp(SubscriptionResource subscription, ResourceGroupResource rg, string functionAppName, string location, string? storageAccountName = null)
    {
        var accountName = storageAccountName ?? CreateStorageAccountName(functionAppName);
        var storageAccounts = rg.GetStorageAccounts();

        StorageAccountResource storage;
        if (await storageAccounts.ExistsAsync(accountName))
        {
            storage = await storageAccounts.GetAsync(accountName);
        }
        else
        {
            var createOptions = CreateStorageAccountOptions(location);
            var op = await storageAccounts.CreateOrUpdateAsync(Azure.WaitUntil.Completed, accountName, createOptions);
            storage = op.Value;
        }

        var keys = new List<StorageAccountKey>();
        await foreach (var key in storage.GetKeysAsync())
        {
            keys.Add(key);
        }
        var primary = keys.FirstOrDefault() ?? throw new InvalidOperationException($"No keys found for storage account '{accountName}'");
        return BuildConnectionString(accountName, primary.Value);
    }

    internal static StorageAccountCreateOrUpdateContent CreateStorageAccountOptions(string location)
    {
        return new StorageAccountCreateOrUpdateContent(
            new StorageSku(StorageSkuName.StandardLrs),
            StorageKind.StorageV2,
            location)
        {
            AccessTier = StorageAccountAccessTier.Hot,
            EnableHttpsTrafficOnly = true,
            AllowBlobPublicAccess = false,
            IsHnsEnabled = false
        };
    }

    internal static SiteConfigProperties? CreateLinuxSiteConfig(string runtime, string? runtimeVersion)
    {
        var config = new SiteConfigProperties();
        var version = string.IsNullOrWhiteSpace(runtimeVersion) ? GetDefaultRuntimeVersion(runtime) : runtimeVersion;
        if (runtime == "java" && version != null && version.EndsWith(".0", StringComparison.Ordinal))
            version = version.Substring(0, version.Length - 2);
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
        var version = string.IsNullOrWhiteSpace(runtimeVersion) ? GetDefaultRuntimeVersion(runtime) : runtimeVersion;
        if (string.IsNullOrWhiteSpace(version))
            return string.Empty;
        if (runtime == "java" && version.EndsWith(".0", StringComparison.Ordinal))
            version = version[..^2];
        return version;
    }

    internal static AppServiceConfigurationDictionary BuildAppSettings(string runtime, string? runtimeVersion, bool requiresLinux, string storageConnectionString, bool includeWorkerRuntime = true)
    {
        var settings = new AppServiceConfigurationDictionary
        {
            Properties =
            {
                ["AzureWebJobsStorage"] = storageConnectionString,
                ["FUNCTIONS_EXTENSION_VERSION"] = "~4"
            }
        };
        if (includeWorkerRuntime)
        {
            settings.Properties["FUNCTIONS_WORKER_RUNTIME"] = runtime;
        }
        var effectiveVersion = string.IsNullOrWhiteSpace(runtimeVersion) ? GetDefaultRuntimeVersion(runtime) : runtimeVersion;
        if (!requiresLinux && runtime == "node" && !string.IsNullOrWhiteSpace(effectiveVersion))
        {
            var major = ExtractMajorVersion(effectiveVersion!);
            if (!string.IsNullOrEmpty(major))
                settings.Properties["WEBSITE_NODE_DEFAULT_VERSION"] = $"~{major}";
        }
        return settings;
    }

    internal static string? ExtractMajorVersion(string? version) => string.IsNullOrWhiteSpace(version)
        ? null
        : new string(version.Trim().TakeWhile(char.IsDigit).ToArray()) switch { "" => null, var v => v };


    internal static string InferTier(string skuName)
    {
        var upper = skuName.Trim().ToUpperInvariant();

        if (upper.StartsWith("FC"))
            return "FlexConsumption";
        if (upper.StartsWith("EP"))
            return "ElasticPremium";
        if (upper.StartsWith("P") && (upper.Contains("V3") || upper.StartsWith("P1V3") || upper.StartsWith("P2V3") || upper.StartsWith("P3V3")))
            return "PremiumV3";
        if (upper.StartsWith("P"))
            return "Premium";
        if (upper.StartsWith("B"))
            return "Basic";
        if (upper.StartsWith("S"))
            return "Standard";
        if (upper.StartsWith("Y"))
            return "Dynamic";

        return "Standard";
    }

    internal static void SanitizeAppSettingsForFlexConsumption(AppServiceConfigurationDictionary settings)
    {
        if (settings?.Properties is null)
            return;
        settings.Properties.Remove("WEBSITE_NODE_DEFAULT_VERSION");
        settings.Properties.Remove("FUNCTIONS_WORKER_RUNTIME");
    }

    internal static async Task<ContainerAppResource> EnsureMinimalContainerApp(ResourceGroupResource rg, string name, string location, string runtime, string storageConnectionString, string? containerAppsEnvironmentName = null)
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
        var data = new ContainerAppData(location)
        {
            ManagedEnvironmentId = env.Id,
            Configuration = new ContainerAppConfiguration()
            {
                Ingress = new ContainerAppIngressConfiguration()
                {
                    External = ContainerAppsDefaults.ExternalIngress,
                    TargetPort = ContainerAppsDefaults.IngressPort
                }
            },
            Template = new ContainerAppTemplate()
            {
                Containers =
                {
                    new ContainerAppContainer()
                    {
                        Name = name,
                        Image = image,
                        Resources = new AppContainerResources()
                        {
                            Cpu = ContainerAppsDefaults.CpuCores,
                            Memory = ContainerAppsDefaults.Memory
                        },
                        Env =
                        {
                            new ContainerAppEnvironmentVariable()
                            {
                                Name = "FUNCTIONS_WORKER_RUNTIME",
                                Value = runtime
                            },
                            new ContainerAppEnvironmentVariable()
                            {
                                Name = "FUNCTIONS_EXTENSION_VERSION",
                                Value = "~4"
                            },
                            new ContainerAppEnvironmentVariable()
                            {
                                Name = "AzureWebJobsStorage",
                                Value = storageConnectionString
                            }
                        }
                    }
                }
            }
        };

        var app = (await apps.CreateOrUpdateAsync(WaitUntil.Completed, name, data)).Value;
        return app;
    }

    internal static SiteConfigProperties? CreateWindowsPowerShellSiteConfig(string? runtimeVersion)
    {
        var version = string.IsNullOrWhiteSpace(runtimeVersion) ? GetDefaultRuntimeVersion("powershell") : runtimeVersion;
        if (string.IsNullOrWhiteSpace(version))
            return null;
        return new SiteConfigProperties { PowerShellVersion = version };
    }

    internal static FunctionAppStorage? BuildDeploymentStorage(string storageConnectionString)
    {
        if (string.IsNullOrWhiteSpace(storageConnectionString))
            return null;
        var accountName = ExtractStorageAccountName(storageConnectionString);
        if (string.IsNullOrWhiteSpace(accountName))
            return null;
        return new FunctionAppStorage
        {
            StorageType = FunctionAppStorageType.BlobContainer,
            Value = new Uri($"https://{accountName}.blob.core.windows.net/azure-webjobs-hosts"),
            Authentication = new FunctionAppStorageAuthentication
            {
                AuthenticationType = FunctionAppStorageAccountAuthenticationType.StorageAccountConnectionString,
                StorageAccountConnectionStringName = "AzureWebJobsStorage"
            }
        };
    }

    internal static string? ExtractStorageAccountName(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return null;
        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var p in parts)
        {
            if (p.StartsWith("AccountName=", StringComparison.OrdinalIgnoreCase))
                return p[12..];
        }
        return null;
    }


    internal static (bool RequiresLinux, string? NormalizedOs) ResolveOs(string runtime, HostingKind hostingKind, string? operatingSystem)
    {
        var forcedLinux = runtime == "python" || hostingKind == HostingKind.FlexConsumption || hostingKind == HostingKind.ContainerApp;
        bool requiresLinux = forcedLinux;

        if (string.IsNullOrEmpty(operatingSystem))
        {
            return (requiresLinux, null);
        }

        if (forcedLinux && operatingSystem == "windows")
            throw new InvalidOperationException("Selected runtime/plan requires Linux operating system.");

        if (!forcedLinux)
        {
            requiresLinux = operatingSystem == "linux";
        }

        return (requiresLinux, operatingSystem);
    }

    internal static bool IsFlexConsumption(AppServicePlanData plan)
    {
        return string.Equals(plan.Sku?.Tier, "FlexConsumption", StringComparison.OrdinalIgnoreCase);
    }

    internal enum HostingKind
    {
        Consumption,
        FlexConsumption,
        Premium,
        AppService,
        ContainerApp
    }

    internal readonly record struct CreateOptions(
        string Runtime,
        string? RuntimeVersion,
        HostingKind HostingKind,
        bool RequiresLinux,
        string? ExplicitSku,
        string? ExplicitOs);

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
            var plan = await EnsureAppServicePlan(rg, planName, functionAppName, location, options);
            var storageConnection = await EnsureStorageForFunctionApp(subscription, rg, functionAppName, location, storageAccountName);
            var site = await CreateAppServiceSiteAsync(
                rg,
                functionAppName,
                location,
                plan,
                options,
                storageConnection);
            await ApplyAppSettings(site, options, storageConnection);
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
            var storage = await EnsureStorageForFunctionApp(subscription, rg, functionAppName, location, storageAccountName);
            var containerApp = await EnsureMinimalContainerApp(rg, functionAppName, location, options.Runtime, storage, containerAppsEnvironmentName);
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
