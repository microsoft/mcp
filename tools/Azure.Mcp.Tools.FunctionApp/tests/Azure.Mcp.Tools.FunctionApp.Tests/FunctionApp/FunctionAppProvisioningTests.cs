// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Tools.FunctionApp.Services;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Storage.Models;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FunctionApp.Tests.FunctionApp;

public sealed class FunctionAppProvisioningTests
{
    private static readonly ResourceIdentifier s_planId =
        new("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Web/serverfarms/myapp-plan");

    private static readonly ResourceIdentifier s_environmentId =
        new("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.App/managedEnvironments/myapp-env");

    private static readonly StorageProvisioningResult s_managedIdentityStorage = new("mystorage123", string.Empty);

    private static readonly StorageProvisioningResult s_connectionStringStorage =
        new("mystorage123", FunctionAppStorageProvisioner.BuildConnectionString("mystorage123", "key123"));

    private static CreateOptions Options(string runtime, string? version = null, HostingKind hostingKind = HostingKind.Consumption, bool requiresLinux = false, bool useManagedIdentity = true) =>
        new(runtime, version, hostingKind, requiresLinux, null, null, useManagedIdentity);

    #region App Service strategy

    [Theory]
    [InlineData("python", null, "Python|3.12")]
    [InlineData("python", "3.11", "Python|3.11")]
    [InlineData("node", null, "Node|22")]
    [InlineData("node", "20", "Node|20")]
    [InlineData("dotnet", null, "DOTNET|8.0")]
    [InlineData("dotnet", "7.0", "DOTNET|7.0")]
    [InlineData("dotnet-isolated", null, "DOTNET-ISOLATED|8.0")]
    [InlineData("java", "21.0", "Java|21")]
    [InlineData("java", null, "Java|17")]
    [InlineData("powershell", null, "PowerShell|7.4")]
    [InlineData("powershell", "7.3", "PowerShell|7.3")]
    public void CreateLinuxSiteConfig_ComposesLinuxFxVersion(string runtime, string? version, string expected)
    {
        var config = FunctionAppAppServiceStrategy.CreateLinuxSiteConfig(runtime, version);

        Assert.Equal(expected, config.LinuxFxVersion);
    }

    [Theory]
    [InlineData(null, "7.4")]
    [InlineData("7.3", "7.3")]
    public void CreateWindowsPowerShellSiteConfig_SetsVersion(string? input, string expected)
    {
        var config = FunctionAppAppServiceStrategy.CreateWindowsPowerShellSiteConfig(input);

        Assert.NotNull(config);
        Assert.Equal(expected, config.PowerShellVersion);
    }

    [Theory]
    [InlineData(true, "dotnet", "8.0", "DOTNET|8.0", null)]
    [InlineData(true, "node", "20", "Node|20", null)]
    [InlineData(true, "python", null, "Python|3.12", null)]
    [InlineData(false, "powershell", "7.4", null, "7.4")]
    public void BuildSiteConfig_ConfiguresRuntime(bool isLinux, string runtime, string? runtimeVersion, string? expectedLinuxFxVersion, string? expectedPowerShellVersion)
    {
        var config = FunctionAppAppServiceStrategy.BuildSiteConfig(isLinux, Options(runtime, runtimeVersion, requiresLinux: isLinux));

        Assert.NotNull(config);
        Assert.Equal(expectedLinuxFxVersion, config.LinuxFxVersion);
        Assert.Equal(expectedPowerShellVersion, config.PowerShellVersion);
    }

    [Theory]
    [InlineData("dotnet")]
    [InlineData("node")]
    public void BuildSiteConfig_ReturnsNullForWindowsNonPowerShellRuntimes(string runtime)
    {
        Assert.Null(FunctionAppAppServiceStrategy.BuildSiteConfig(isLinux: false, Options(runtime)));
    }

    [Theory]
    [InlineData(true, "functionapp,linux")]
    [InlineData(false, "functionapp")]
    public void BuildKind_ReflectsOperatingSystem(bool isLinux, string expected)
    {
        Assert.Equal(expected, FunctionAppAppServiceStrategy.BuildKind(isLinux));
    }

    [Theory]
    [InlineData("dotnet", "dotnet-isolated")]
    [InlineData("dotnet-isolated", "dotnet-isolated")]
    [InlineData("node", "node")]
    [InlineData("java", "java")]
    [InlineData("python", "python")]
    [InlineData("powershell", "powershell")]
    [InlineData("custom", "custom")]
    [InlineData("unknown", "custom")]
    public void MapToFunctionAppRuntimeName_MapsCorrectly(string runtime, string expectedRuntimeName)
    {
        var name = FunctionAppAppServiceStrategy.MapToFunctionAppRuntimeName(runtime);

        Assert.Equal(new FunctionAppRuntimeName(expectedRuntimeName), name);
    }

    [Theory]
    [InlineData("dotnet", "8.0", "8.0")]
    [InlineData("java", "21.0", "21")]
    [InlineData("java", "17", "17")]
    [InlineData("node", null, "22")]
    [InlineData("python", "", "3.12")]
    [InlineData("custom", null, "")]
    public void NormalizeRuntimeVersionForConfig_NormalizesCorrectly(string runtime, string? runtimeVersion, string expected)
    {
        Assert.Equal(expected, FunctionAppAppServiceStrategy.NormalizeRuntimeVersionForConfig(runtime, runtimeVersion));
    }

    [Theory]
    [InlineData("22", "~22")]
    [InlineData("22.3.1", "~22")]
    [InlineData("22 LTS", "~22")]
    [InlineData(null, "~22")]
    public void BuildAppSettings_SetsWebsiteNodeDefaultVersionForWindowsNode(string? runtimeVersion, string expected)
    {
        var settings = FunctionAppAppServiceStrategy.BuildAppSettings(Options("node", runtimeVersion), s_managedIdentityStorage, isLinux: false);

        Assert.Equal(expected, settings.Properties["WEBSITE_NODE_DEFAULT_VERSION"]);
        Assert.Equal("node", settings.Properties["FUNCTIONS_WORKER_RUNTIME"]);
        Assert.Equal("~4", settings.Properties["FUNCTIONS_EXTENSION_VERSION"]);
    }

    [Theory]
    [InlineData("node", true)]
    [InlineData("python", true)]
    [InlineData("dotnet", false)]
    public void BuildAppSettings_OmitsWebsiteNodeDefaultVersionWhenNotApplicable(string runtime, bool isLinux)
    {
        var settings = FunctionAppAppServiceStrategy.BuildAppSettings(Options(runtime, requiresLinux: isLinux), s_managedIdentityStorage, isLinux);

        Assert.False(settings.Properties.ContainsKey("WEBSITE_NODE_DEFAULT_VERSION"));
        Assert.Equal(runtime, settings.Properties["FUNCTIONS_WORKER_RUNTIME"]);
    }

    [Fact]
    public void BuildAppSettings_FlexConsumptionOmitsWorkerRuntimeSettings()
    {
        var settings = FunctionAppAppServiceStrategy.BuildAppSettings(Options("node", hostingKind: HostingKind.FlexConsumption, requiresLinux: true), s_managedIdentityStorage, isLinux: true);

        Assert.False(settings.Properties.ContainsKey("FUNCTIONS_WORKER_RUNTIME"));
        Assert.False(settings.Properties.ContainsKey("WEBSITE_NODE_DEFAULT_VERSION"));
        Assert.Equal("~4", settings.Properties["FUNCTIONS_EXTENSION_VERSION"]);
    }

    [Fact]
    public void BuildAppSettings_ManagedIdentityUsesAccountNameAndCredential()
    {
        var settings = FunctionAppAppServiceStrategy.BuildAppSettings(Options("dotnet"), s_managedIdentityStorage, isLinux: false);

        Assert.Equal("mystorage123", settings.Properties["AzureWebJobsStorage__accountName"]);
        Assert.Equal("managedidentity", settings.Properties["AzureWebJobsStorage__credential"]);
        Assert.False(settings.Properties.ContainsKey("AzureWebJobsStorage"));
    }

    [Fact]
    public void BuildAppSettings_ConnectionStringUsesAzureWebJobsStorage()
    {
        var settings = FunctionAppAppServiceStrategy.BuildAppSettings(Options("dotnet", useManagedIdentity: false), s_connectionStringStorage, isLinux: false);

        Assert.Equal(s_connectionStringStorage.ConnectionString, settings.Properties["AzureWebJobsStorage"]);
        Assert.False(settings.Properties.ContainsKey("AzureWebJobsStorage__accountName"));
    }

    [Fact]
    public void BuildSiteData_ConsumptionPlanSetsKindPlanAndIdentity()
    {
        var data = FunctionAppAppServiceStrategy.BuildSiteData("eastus", s_planId, isLinux: false, Options("dotnet"), s_managedIdentityStorage, FunctionAppStorageProvisioner.DefaultStorageEndpointSuffix);

        Assert.Equal("functionapp", data.Kind);
        Assert.Equal(s_planId, data.AppServicePlanId);
        Assert.Null(data.FunctionAppConfig);
        Assert.NotNull(data.Identity);
        Assert.Equal(Azure.ResourceManager.Models.ManagedServiceIdentityType.SystemAssigned, data.Identity.ManagedServiceIdentityType);
    }

    [Fact]
    public void BuildSiteData_ConnectionStringDoesNotAssignIdentity()
    {
        var data = FunctionAppAppServiceStrategy.BuildSiteData("eastus", s_planId, isLinux: false, Options("dotnet", useManagedIdentity: false), s_connectionStringStorage, FunctionAppStorageProvisioner.DefaultStorageEndpointSuffix);

        Assert.Null(data.Identity);
    }

    [Fact]
    public void BuildSiteData_FlexConsumptionUsesFunctionAppConfig()
    {
        var options = Options("dotnet-isolated", "8.0", HostingKind.FlexConsumption, requiresLinux: true);

        var data = FunctionAppAppServiceStrategy.BuildSiteData("eastus", s_planId, isLinux: true, options, s_managedIdentityStorage, FunctionAppStorageProvisioner.DefaultStorageEndpointSuffix);

        Assert.Equal("functionapp,linux", data.Kind);
        Assert.Null(data.SiteConfig?.LinuxFxVersion);
        Assert.NotNull(data.FunctionAppConfig);
        Assert.Equal(FunctionAppRuntimeName.DotnetIsolated, data.FunctionAppConfig.Runtime.Name);
        Assert.Equal("8.0", data.FunctionAppConfig.Runtime.Version);
        Assert.Equal(2048, data.FunctionAppConfig.ScaleAndConcurrency.InstanceMemoryMB);
        Assert.Equal(100, data.FunctionAppConfig.ScaleAndConcurrency.MaximumInstanceCount);
        Assert.NotNull(data.FunctionAppConfig.DeploymentStorage);
        Assert.Equal(FunctionAppStorageAccountAuthenticationType.SystemAssignedIdentity, data.FunctionAppConfig.DeploymentStorage.Authentication.AuthenticationType);
    }

    [Theory]
    [InlineData("B1", "B1", "Basic")]
    [InlineData("P1v3", "P1v3", "PremiumV3")]
    [InlineData(null, "Y1", "Dynamic")]
    public void ResolveSku_PrefersExplicitSku(string? explicitSku, string expectedName, string expectedTier)
    {
        var options = new CreateOptions("dotnet", null, HostingKind.Consumption, false, explicitSku, null);

        var sku = FunctionAppPlanProvisioner.ResolveSku(options);

        Assert.Equal(expectedName, sku.Name);
        Assert.Equal(expectedTier, sku.Tier);
    }

    [Theory]
    [InlineData(HostingKind.Consumption, "Y1", "Dynamic")]
    [InlineData(HostingKind.FlexConsumption, "FC1", "FlexConsumption")]
    [InlineData(HostingKind.Premium, "EP1", "ElasticPremium")]
    [InlineData(HostingKind.AppService, "B1", "Basic")]
    public void ResolveSku_UsesDefaultForHostingKind(HostingKind hostingKind, string expectedName, string expectedTier)
    {
        var sku = FunctionAppPlanProvisioner.ResolveSku(Options("dotnet", hostingKind: hostingKind));

        Assert.Equal(expectedName, sku.Name);
        Assert.Equal(expectedTier, sku.Tier);
    }

    [Fact]
    public void ValidateExistingPlan_ThrowsForWindowsPlanWithLinuxRuntime()
    {
        var plan = CreatePlan(new AppServicePlanData("eastus") { IsReserved = false });

        Assert.Throws<InvalidOperationException>(() =>
            FunctionAppPlanProvisioner.ValidateExistingPlan(plan, "test-plan", Options("python", "3.12", requiresLinux: true)));
    }

    [Fact]
    public void ValidateExistingPlan_ThrowsForNonFlexPlanWithFlexHosting()
    {
        var plan = CreatePlan(new AppServicePlanData("eastus") { Sku = new AppServiceSkuDescription { Tier = "Dynamic" }, IsReserved = true });

        Assert.Throws<InvalidOperationException>(() =>
            FunctionAppPlanProvisioner.ValidateExistingPlan(plan, "test-plan", Options("dotnet", "8.0", HostingKind.FlexConsumption, requiresLinux: true)));
    }

    [Fact]
    public void ValidateExistingPlan_ThrowsForNonPremiumPlanWithPremiumHosting()
    {
        var plan = CreatePlan(new AppServicePlanData("eastus") { Sku = new AppServiceSkuDescription { Tier = "Dynamic" }, IsReserved = true });

        Assert.Throws<InvalidOperationException>(() =>
            FunctionAppPlanProvisioner.ValidateExistingPlan(plan, "test-plan", Options("dotnet", "8.0", HostingKind.Premium, requiresLinux: true)));
    }

    [Fact]
    public void ValidateExistingPlan_AcceptsMatchingPlan()
    {
        var plan = CreatePlan(new AppServicePlanData("eastus") { Sku = new AppServiceSkuDescription { Tier = "ElasticPremium" }, IsReserved = true });

        FunctionAppPlanProvisioner.ValidateExistingPlan(plan, "test-plan", Options("node", "22", HostingKind.Premium, requiresLinux: true));
    }

    private static AppServicePlanResource CreatePlan(AppServicePlanData data)
    {
        var plan = Substitute.For<AppServicePlanResource>();
        plan.Data.Returns(data);
        return plan;
    }

    #endregion

    #region Storage provisioner

    [Fact]
    public void CreateStorageAccountOptions_UsesSecureDefaults()
    {
        var content = FunctionAppStorageProvisioner.CreateStorageAccountOptions("eastus");

        Assert.Equal(StorageSkuName.StandardLrs, content.Sku.Name);
        Assert.Equal(StorageKind.StorageV2, content.Kind);
        Assert.Equal(StorageAccountAccessTier.Hot, content.AccessTier);
        Assert.True(content.EnableHttpsTrafficOnly);
        Assert.False(content.AllowBlobPublicAccess);
        Assert.False(content.IsHnsEnabled);
    }

    [Theory]
    [InlineData("my-app", "myapp")]
    [InlineData("My-App-123", "myapp123")]
    [InlineData("@#$%", "func")]
    public void CreateStorageAccountName_GeneratesValidName(string functionAppName, string expectedPrefix)
    {
        var name = FunctionAppStorageProvisioner.CreateStorageAccountName(functionAppName);

        Assert.InRange(name.Length, 3, 24);
        Assert.StartsWith(expectedPrefix, name);
        Assert.All(name, c => Assert.True(char.IsAsciiDigit(c) || char.IsAsciiLetterLower(c)));
    }

    [Fact]
    public void CreateStorageAccountName_TruncatesLongNamesToStorageLimit()
    {
        const string functionAppName = "very-long-function-app-name-that-exceeds-the-limit";
        var expectedPrefix = new string(functionAppName.Where(char.IsAsciiLetterOrDigit).ToArray())[..18];

        var name = FunctionAppStorageProvisioner.CreateStorageAccountName(functionAppName);

        Assert.Equal(24, name.Length);
        Assert.StartsWith(expectedPrefix, name);
    }

    [Fact]
    public void BuildConnectionString_FormatsCorrectly()
    {
        var connectionString = FunctionAppStorageProvisioner.BuildConnectionString("storageaccount", "key123");

        Assert.Equal("DefaultEndpointsProtocol=https;AccountName=storageaccount;AccountKey=key123;EndpointSuffix=core.windows.net", connectionString);
    }

    [Fact]
    public void BuildConnectionString_UsesEndpointSuffix()
    {
        var connectionString = FunctionAppStorageProvisioner.BuildConnectionString("storageaccount", "key123", "core.usgovcloudapi.net");

        Assert.EndsWith("EndpointSuffix=core.usgovcloudapi.net", connectionString);
    }

    [Fact]
    public void BuildDeploymentStorage_ManagedIdentityEmitsSystemAssignedAuth()
    {
        var storage = FunctionAppStorageProvisioner.BuildDeploymentStorage("mystorage123", useManagedIdentity: true);

        Assert.NotNull(storage);
        Assert.Equal(FunctionAppStorageType.BlobContainer, storage.StorageType);
        Assert.Equal(new Uri("https://mystorage123.blob.core.windows.net/azure-webjobs-hosts"), storage.Value);
        Assert.Equal(FunctionAppStorageAccountAuthenticationType.SystemAssignedIdentity, storage.Authentication.AuthenticationType);
        Assert.Null(storage.Authentication.StorageAccountConnectionStringName);
    }

    [Fact]
    public void BuildDeploymentStorage_ConnectionStringEmitsConnectionStringAuth()
    {
        var storage = FunctionAppStorageProvisioner.BuildDeploymentStorage("mystorage123", useManagedIdentity: false, "core.chinacloudapi.cn");

        Assert.NotNull(storage);
        Assert.Equal(new Uri("https://mystorage123.blob.core.chinacloudapi.cn/azure-webjobs-hosts"), storage.Value);
        Assert.Equal(FunctionAppStorageAccountAuthenticationType.StorageAccountConnectionString, storage.Authentication.AuthenticationType);
        Assert.Equal("AzureWebJobsStorage", storage.Authentication.StorageAccountConnectionStringName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BuildDeploymentStorage_EmptyAccountReturnsNull(string? accountName)
    {
        Assert.Null(FunctionAppStorageProvisioner.BuildDeploymentStorage(accountName, useManagedIdentity: true));
        Assert.Null(FunctionAppStorageProvisioner.BuildDeploymentStorage(accountName, useManagedIdentity: false));
    }

    #endregion

    #region Container Apps strategy

    [Theory]
    [InlineData("dotnet", null, "mcr.microsoft.com/azure-functions/dotnet:4-dotnet8.0")]
    [InlineData("dotnet-isolated", null, "mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0")]
    [InlineData("node", null, "mcr.microsoft.com/azure-functions/node:4-node22")]
    [InlineData("node", "22.0", "mcr.microsoft.com/azure-functions/node:4-node22")]
    [InlineData("python", "3.12", "mcr.microsoft.com/azure-functions/python:4-python3.12")]
    [InlineData("java", "17.0", "mcr.microsoft.com/azure-functions/java:4-java17")]
    [InlineData("java", "21", "mcr.microsoft.com/azure-functions/java:4-java21")]
    [InlineData("powershell", null, "mcr.microsoft.com/azure-functions/powershell:4-powershell7.4")]
    [InlineData("custom", null, "mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated")]
    public void GetContainerImage_MapsRuntimesWithVersion(string runtime, string? runtimeVersion, string expectedImage)
    {
        Assert.Equal(expectedImage, FunctionAppContainerAppStrategy.GetContainerImage(runtime, runtimeVersion));
    }

    [Fact]
    public void ContainerAppBuildSiteData_ManagedIdentityConfiguresImageSettingsAndIdentity()
    {
        var data = FunctionAppContainerAppStrategy.BuildSiteData("eastus", s_environmentId, Options("node", "22", HostingKind.ContainerApp, requiresLinux: true), s_managedIdentityStorage);

        Assert.Equal("functionapp,linux,container,azurecontainerapps", data.Kind);
        Assert.Equal(s_environmentId.ToString(), data.ManagedEnvironmentId);
        Assert.NotNull(data.SiteConfig);
        Assert.Equal("DOCKER|mcr.microsoft.com/azure-functions/node:4-node22", data.SiteConfig.LinuxFxVersion);
        Assert.NotNull(data.Identity);
        Assert.Equal(Azure.ResourceManager.Models.ManagedServiceIdentityType.SystemAssigned, data.Identity.ManagedServiceIdentityType);

        var settings = data.SiteConfig.AppSettings.ToDictionary(s => s.Name, s => s.Value);
        Assert.Equal("~4", settings["FUNCTIONS_EXTENSION_VERSION"]);
        Assert.Equal("node", settings["FUNCTIONS_WORKER_RUNTIME"]);
        Assert.Equal("mystorage123", settings["AzureWebJobsStorage__accountName"]);
        Assert.Equal("managedidentity", settings["AzureWebJobsStorage__credential"]);
        Assert.False(settings.ContainsKey("AzureWebJobsStorage"));
    }

    [Fact]
    public void ContainerAppBuildSiteData_ConnectionStringUsesAzureWebJobsStorage()
    {
        var data = FunctionAppContainerAppStrategy.BuildSiteData("eastus", s_environmentId, Options("python", null, HostingKind.ContainerApp, requiresLinux: true, useManagedIdentity: false), s_connectionStringStorage);

        Assert.Null(data.Identity);
        Assert.NotNull(data.SiteConfig);

        var settings = data.SiteConfig.AppSettings.ToDictionary(s => s.Name, s => s.Value);
        Assert.Equal(s_connectionStringStorage.ConnectionString, settings["AzureWebJobsStorage"]);
        Assert.False(settings.ContainsKey("AzureWebJobsStorage__accountName"));
    }

    #endregion
}
