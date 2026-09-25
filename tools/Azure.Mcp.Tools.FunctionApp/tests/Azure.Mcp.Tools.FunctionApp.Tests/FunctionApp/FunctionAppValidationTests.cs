// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FunctionApp.Services;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Xunit;

namespace Azure.Mcp.Tools.FunctionApp.Tests.FunctionApp;

public sealed class FunctionAppValidationTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("ab", null)]
    [InlineData("my-function-app", null)]
    [InlineData("a", "between 2 and 43 characters")]
    [InlineData("this-function-app-name-is-way-too-long-for-azure-to-accept", "between 2 and 43 characters")]
    public void ValidateFunctionAppNameLength_ReturnsErrorOnlyForInvalidLengths(string? name, string? expectedFragment)
    {
        var error = FunctionAppValidation.ValidateFunctionAppNameLength(name);

        if (expectedFragment is null)
        {
            Assert.Null(error);
        }
        else
        {
            Assert.NotNull(error);
            Assert.Contains(expectedFragment, error);
        }
    }

    [Theory]
    [InlineData("containerapp", true)]
    [InlineData("containerapps", true)]
    [InlineData(" ContainerApp ", true)]
    [InlineData("consumption", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsContainerAppPlanType_DetectsAliases(string? planType, bool expected)
    {
        Assert.Equal(expected, FunctionAppValidation.IsContainerAppPlanType(planType));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseStorageAuthMode_UnspecifiedReturnsNull(string? mode)
    {
        Assert.Null(FunctionAppValidation.ParseStorageAuthMode(mode));
    }

    [Theory]
    [InlineData("managed-identity")]
    [InlineData("managedidentity")]
    [InlineData("mi")]
    [InlineData("MANAGED-IDENTITY")]
    [InlineData("  Managed-Identity  ")]
    public void ParseStorageAuthMode_ManagedIdentityAliasesReturnTrue(string mode)
    {
        Assert.True(FunctionAppValidation.ParseStorageAuthMode(mode));
    }

    [Theory]
    [InlineData("connection-string")]
    [InlineData("connectionstring")]
    [InlineData("key")]
    [InlineData("Connection-String")]
    public void ParseStorageAuthMode_ConnectionStringAliasesReturnFalse(string mode)
    {
        Assert.False(FunctionAppValidation.ParseStorageAuthMode(mode));
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("oauth")]
    [InlineData("none")]
    public void ParseStorageAuthMode_UnknownValuesThrow(string mode)
    {
        var ex = Assert.Throws<ArgumentException>(() => FunctionAppValidation.ParseStorageAuthMode(mode));
        Assert.Contains("--storage-auth-mode", ex.Message);
    }

    [Theory]
    [InlineData(null, HostingKind.Consumption)]
    [InlineData("", HostingKind.Consumption)]
    [InlineData("consumption", HostingKind.Consumption)]
    [InlineData("Consumption", HostingKind.Consumption)]
    [InlineData("flex", HostingKind.FlexConsumption)]
    [InlineData("flexconsumption", HostingKind.FlexConsumption)]
    [InlineData("premium", HostingKind.Premium)]
    [InlineData("functionspremium", HostingKind.Premium)]
    [InlineData("appservice", HostingKind.AppService)]
    [InlineData("containerapp", HostingKind.ContainerApp)]
    [InlineData("containerapps", HostingKind.ContainerApp)]
    public void ParseHostingKind_MapsCorrectly(string? planType, HostingKind expected)
    {
        Assert.Equal(expected, FunctionAppValidation.ParseHostingKind(planType));
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("basic")]
    public void ParseHostingKind_ThrowsForUnsupportedValue(string planType)
    {
        var ex = Assert.Throws<ArgumentException>(() => FunctionAppValidation.ParseHostingKind(planType));
        Assert.Contains(planType, ex.Message);
    }

    [Theory]
    [InlineData("FC1", "FlexConsumption")]
    [InlineData("fc2", "FlexConsumption")]
    [InlineData("EP1", "ElasticPremium")]
    [InlineData("ep2", "ElasticPremium")]
    [InlineData("P1V3", "PremiumV3")]
    [InlineData("p0v3", "PremiumV3")]
    [InlineData("P1V2", "PremiumV2")]
    [InlineData("P1", "Premium")]
    [InlineData("I1V2", "IsolatedV2")]
    [InlineData("B1", "Basic")]
    [InlineData("S1", "Standard")]
    [InlineData("Y1", "Dynamic")]
    [InlineData("unknown", "Standard")]
    public void InferTier_CorrectlyInfersTierFromSku(string sku, string expectedTier)
    {
        Assert.Equal(expectedTier, FunctionAppValidation.InferTier(sku));
    }

    [Theory]
    [InlineData("", "", "", "")]
    [InlineData("sub", "", "", "")]
    [InlineData("sub", "rg", "", "")]
    [InlineData("sub", "rg", "app", "")]
    public void ValidateAndNormalizeInputs_ThrowsForMissingRequiredParameters(string subscription, string resourceGroup, string functionAppName, string location)
    {
        Assert.Throws<ArgumentException>(() =>
            FunctionAppValidation.ValidateAndNormalizeInputs(subscription, resourceGroup, functionAppName, location, null, null, null, null, null, null, null));
    }

    [Fact]
    public void ValidateAndNormalizeInputs_NormalizesInputsCorrectly()
    {
        var result = FunctionAppValidation.ValidateAndNormalizeInputs(
            "sub", "rg", "app", "eastus",
            "  NODE  ", "  20  ", "  Flex  ", "  EP1  ", "  LINUX  ", "  storage123abc  ", null);

        Assert.Equal("node", result.Runtime);
        Assert.Equal("20", result.RuntimeVersion);
        Assert.Equal("flex", result.PlanType);
        Assert.Equal("EP1", result.PlanSku);
        Assert.Equal("linux", result.OperatingSystem);
        Assert.Equal("storage123abc", result.StorageAccountName);
        Assert.Null(result.ContainerAppsEnvironmentName);
    }

    [Fact]
    public void ValidateAndNormalizeInputs_DefaultsRuntimeToDotnet()
    {
        var result = FunctionAppValidation.ValidateAndNormalizeInputs("sub", "rg", "app", "eastus", null, null, null, null, null, null, null);

        Assert.Equal(FunctionAppValidation.DefaultRuntime, result.Runtime);
    }

    [Theory]
    [InlineData("invalidRuntime")]
    [InlineData("javascript")]
    [InlineData("csharp")]
    public void ValidateParameterCombinations_ThrowsForUnsupportedRuntime(string runtime)
    {
        var inputs = new NormalizedInputs(runtime, null, null, null, null, null, null);

        var ex = Assert.Throws<ArgumentException>(() => FunctionAppValidation.ValidateParameterCombinations(inputs));
        Assert.Contains(runtime, ex.Message);
    }

    [Fact]
    public void ValidateParameterCombinations_ThrowsForPythonWithWindows()
    {
        var inputs = new NormalizedInputs("python", null, null, null, "windows", null, null);

        Assert.Throws<InvalidOperationException>(() => FunctionAppValidation.ValidateParameterCombinations(inputs));
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("tooLongStorageAccountName123")]
    [InlineData("Storage123")]
    [InlineData("storage-123")]
    public void ValidateParameterCombinations_ThrowsForInvalidStorageAccountName(string storageAccountName)
    {
        var inputs = new NormalizedInputs("dotnet", null, null, null, null, storageAccountName, null);

        Assert.Throws<ArgumentException>(() => FunctionAppValidation.ValidateParameterCombinations(inputs));
    }

    [Fact]
    public void ValidateParameterCombinations_ThrowsForContainerAppsEnvironmentWithoutContainerApps()
    {
        var inputs = new NormalizedInputs("dotnet", null, "consumption", null, null, null, "env123");

        Assert.Throws<InvalidOperationException>(() => FunctionAppValidation.ValidateParameterCombinations(inputs));
    }

    [Fact]
    public void ValidateParameterCombinations_ThrowsForContainerAppsWithSku()
    {
        var inputs = new NormalizedInputs("dotnet", null, "containerapp", "B1", null, null, null);

        Assert.Throws<InvalidOperationException>(() => FunctionAppValidation.ValidateParameterCombinations(inputs));
    }

    [Fact]
    public void ValidateParameterCombinations_ThrowsForFlexConsumptionDotnetWithVersion()
    {
        var inputs = new NormalizedInputs("dotnet", "8.0", "flex", null, null, null, null);

        Assert.Throws<InvalidOperationException>(() => FunctionAppValidation.ValidateParameterCombinations(inputs));
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("mac")]
    public void ValidateParameterCombinations_ThrowsForInvalidOperatingSystem(string os)
    {
        var inputs = new NormalizedInputs("dotnet", null, null, null, os, null, null);

        Assert.Throws<ArgumentException>(() => FunctionAppValidation.ValidateParameterCombinations(inputs));
    }

    [Theory]
    [InlineData("dotnet", null, "flex", "dotnet-isolated", "8.0")]
    [InlineData("dotnet", "8.0", "consumption", "dotnet", "8.0")]
    [InlineData("node", "20", "premium", "node", "20")]
    [InlineData("python", null, "appservice", "python", "3.12")]
    public void BuildCreateOptions_SelectsRuntimeAndVersion(string runtime, string? runtimeVersion, string? planType, string expectedRuntime, string expectedRuntimeVersion)
    {
        var inputs = new NormalizedInputs(runtime, runtimeVersion, planType, null, null, null, null);

        var result = FunctionAppValidation.BuildCreateOptions(inputs);

        Assert.Equal(expectedRuntime, result.Runtime);
        Assert.Equal(expectedRuntimeVersion, result.RuntimeVersion);
    }

    [Theory]
    [InlineData(null, HostingKind.Consumption)]
    [InlineData("flex", HostingKind.FlexConsumption)]
    [InlineData("premium", HostingKind.Premium)]
    [InlineData("appservice", HostingKind.AppService)]
    [InlineData("containerapp", HostingKind.ContainerApp)]
    public void BuildCreateOptions_PropagatesStorageAuthModeAcrossHostingKinds(string? planType, HostingKind expected)
    {
        var inputs = new NormalizedInputs("dotnet", null, planType, null, null, null, null);

        var managedIdentity = FunctionAppValidation.BuildCreateOptions(inputs, useManagedIdentityStorage: true);
        var connectionString = FunctionAppValidation.BuildCreateOptions(inputs, useManagedIdentityStorage: false);

        Assert.Equal(expected, managedIdentity.HostingKind);
        Assert.True(managedIdentity.UseManagedIdentityStorage);
        Assert.False(connectionString.UseManagedIdentityStorage);
    }

    [Fact]
    public void BuildCreateOptions_DefaultsToManagedIdentity()
    {
        var options = FunctionAppValidation.BuildCreateOptions(new NormalizedInputs("dotnet", null, null, null, null, null, null));

        Assert.True(options.UseManagedIdentityStorage);
    }

    [Theory]
    [InlineData("node", HostingKind.FlexConsumption, null, true, null)]
    [InlineData("dotnet", HostingKind.FlexConsumption, null, true, null)]
    [InlineData("dotnet", HostingKind.Consumption, null, false, null)]
    [InlineData("java", HostingKind.Consumption, null, false, null)]
    [InlineData("python", HostingKind.AppService, null, true, null)]
    [InlineData("node", HostingKind.Consumption, "linux", true, "linux")]
    [InlineData("node", HostingKind.ContainerApp, null, true, null)]
    [InlineData("dotnet", HostingKind.Consumption, "windows", false, "windows")]
    public void ResolveOs_CorrectlyEvaluates(string runtime, HostingKind hostingKind, string? operatingSystem, bool expectedRequiresLinux, string? expectedNormalizedOs)
    {
        var (requiresLinux, normalizedOs) = FunctionAppValidation.ResolveOs(runtime, hostingKind, operatingSystem);

        Assert.Equal(expectedRequiresLinux, requiresLinux);
        Assert.Equal(expectedNormalizedOs, normalizedOs);
    }

    [Theory]
    [InlineData("python", HostingKind.Consumption)]
    [InlineData("dotnet", HostingKind.FlexConsumption)]
    [InlineData("dotnet", HostingKind.ContainerApp)]
    public void ResolveOs_ThrowsWhenWindowsRequestedForLinuxOnlyHosting(string runtime, HostingKind hostingKind)
    {
        Assert.Throws<InvalidOperationException>(() => FunctionAppValidation.ResolveOs(runtime, hostingKind, "windows"));
    }

    [Theory]
    [InlineData("22", "22")]
    [InlineData("22.3.1", "22")]
    [InlineData("22 LTS", "22")]
    [InlineData("abc", null)]
    [InlineData(null, null)]
    [InlineData("", null)]
    public void ExtractMajorVersion_ExtractsCorrectly(string? version, string? expected)
    {
        Assert.Equal(expected, FunctionAppValidation.ExtractMajorVersion(version));
    }

    [Theory]
    [InlineData("functionapp", true)]
    [InlineData("functionapp,linux", true)]
    [InlineData("functionapp,linux,container,azurecontainerapps", true)]
    [InlineData("app", false)]
    [InlineData(null, false)]
    public void IsFunctionApp_DetectsCorrectly(string? kind, bool expected)
    {
        var siteData = new WebSiteData("eastus") { Kind = kind };

        Assert.Equal(expected, FunctionAppValidation.IsFunctionApp(siteData));
    }

    [Theory]
    [InlineData("FlexConsumption", true)]
    [InlineData("flexconsumption", true)]
    [InlineData("Dynamic", false)]
    [InlineData("ElasticPremium", false)]
    [InlineData(null, false)]
    public void IsFlexConsumption_DetectsCorrectly(string? tier, bool expected)
    {
        var planData = new AppServicePlanData("eastus")
        {
            Sku = tier is null ? null : new AppServiceSkuDescription { Tier = tier }
        };

        Assert.Equal(expected, FunctionAppValidation.IsFlexConsumption(planData));
    }

    [Theory]
    [InlineData("functionapp", "windows")]
    [InlineData("functionapp,linux", "linux")]
    [InlineData("functionapp,linux,container,azurecontainerapps", "linux")]
    [InlineData(null, "windows")]
    public void GetOperatingSystem_DerivesFromKind(string? kind, string expected)
    {
        Assert.Equal(expected, FunctionAppValidation.GetOperatingSystem(kind));
    }
}
