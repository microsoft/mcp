// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.FunctionApp.Commands;
using Azure.Mcp.Tools.FunctionApp.Commands.FunctionApp;
using Azure.Mcp.Tools.FunctionApp.Models;
using Azure.Mcp.Tools.FunctionApp.Services;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Storage.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AzureMcp.FunctionApp.UnitTests.FunctionApp;

public sealed class FunctionAppCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFunctionAppService _service;
    private readonly ILogger<FunctionAppCreateCommand> _logger;
    private readonly FunctionAppCreateCommand _command;

    public FunctionAppCreateCommandTests()
    {
        _service = Substitute.For<IFunctionAppService>();
        _logger = Substitute.For<ILogger<FunctionAppCreateCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub --resource-group rg --function-app myapp --location eastus", true)]
    [InlineData("--subscription sub --resource-group rg --function-app myapp", false)]
    [InlineData("--subscription sub --location eastus --function-app myapp", false)]
    [InlineData("--resource-group rg --location eastus --function-app myapp", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _service.CreateFunctionApp(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>())
                .Returns(new FunctionAppInfo("myapp", "rg", "eastus", "plan", "Running", "myapp.azurewebsites.net", null, null));
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        var response = await _command.ExecuteAsync(context, parseResult);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCreatedFunctionApp()
    {
        var expected = new FunctionAppInfo("myapp", "rg", "eastus", "plan", "Running", "myapp.azurewebsites.net", null, null);
        _service.CreateFunctionApp(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>())
            .Returns(expected);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub --resource-group rg --function-app myapp --location eastus");

        var response = await _command.ExecuteAsync(context, parseResult);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<FunctionAppCreateCommand.FunctionAppCreateCommandResult>(json, FunctionAppJsonContext.Default.FunctionAppCreateCommandResult);
        Assert.NotNull(result);
        Assert.Equal("myapp", result.FunctionApp.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        _service.CreateFunctionApp(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>())
            .Returns(Task.FromException<FunctionAppInfo>(new Exception("Create error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub --resource-group rg --function-app myapp --location eastus");

        var response = await _command.ExecuteAsync(context, parseResult);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Create error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_PassesPlanTypeAndRuntimeVersionToService()
    {
        var expected = new FunctionAppInfo("myapp", "rg", "eastus", "plan", "Running", "myapp.azurewebsites.net", null, null);
        _service.CreateFunctionApp(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>())
            .Returns(expected);

        var context = new CommandContext(_serviceProvider);
        var args = "--subscription sub --resource-group rg --function-app myapp --location eastus --plan-type flex --runtime node --runtime-version 22";
        var parseResult = _command.GetCommand().Parse(args);

        var response = await _command.ExecuteAsync(context, parseResult);

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_DefaultsRuntimeToDotnet_WhenNotProvided()
    {
        var expected = new FunctionAppInfo("myapp", "rg", "eastus", "plan", "Running", "myapp.azurewebsites.net", null, null);
        _service.CreateFunctionApp(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>())
            .Returns(expected);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub --resource-group rg --function-app myapp --location eastus");

        var response = await _command.ExecuteAsync(context, parseResult);

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("", null, null)]
    [InlineData("--plan-type consumption", "consumption", null)]
    [InlineData("--plan-type flex", "flex", null)]
    [InlineData("--plan-type premium", "premium", null)]
    [InlineData("--plan-type appservice", "appservice", null)]
    [InlineData("--plan-sku S1", null, "S1")]
    [InlineData("--plan-type premium --plan-sku EP2", "premium", "EP2")]
    [InlineData("--plan-type containerapp", "containerapp", null)]
    public async Task ExecuteAsync_PlanSelection_Matrix(string argsSuffix, string? expectedPlanType, string? expectedPlanSku)
    {
        var expected = new FunctionAppInfo("myapp", "rg", "eastus", expectedPlanType ?? "plan", "Running", "myapp.azurewebsites.net", null, null);
        _service.CreateFunctionApp(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>())
            .Returns(expected);

        var baseArgs = "--subscription sub --resource-group rg --function-app myapp --location eastus";
        var fullArgs = string.IsNullOrWhiteSpace(argsSuffix) ? baseArgs : $"{baseArgs} {argsSuffix}";
        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(fullArgs);
        var response = await _command.ExecuteAsync(context, parseResult);

        Assert.Equal(HttpStatusCode.OK, response.Status);

        await _service.Received(1).CreateFunctionApp(
            "sub",
            "rg",
            "myapp",
            "eastus",
            Arg.Any<string?>(),
            Arg.Is<string?>(pt => pt == expectedPlanType),
            Arg.Is<string?>(ps => ps == expectedPlanSku),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>());
    }


    [Theory]
    [InlineData("python", null, "Python|3.12")]
    [InlineData("python", "3.11", "Python|3.11")]
    [InlineData("node", null, "Node|22")]
    [InlineData("node", "20", "Node|20")]
    [InlineData("dotnet", null, "DOTNET|8.0")]
    [InlineData("dotnet", "7.0", "DOTNET|7.0")]
    [InlineData("java", "21.0", "Java|21")]
    [InlineData("java", null, "Java|17")]
    [InlineData("powershell", null, "PowerShell|7.4")]
    [InlineData("powershell", "7.3", "PowerShell|7.3")]
    public void CreateLinuxSiteConfig_ComposesLinuxFxVersion(string runtime, string? version, string expected)
    {
        SiteConfigProperties? cfg = FunctionAppService.CreateLinuxSiteConfig(runtime, version);
        Assert.NotNull(cfg);
        Assert.Equal(expected, cfg!.LinuxFxVersion);
    }

    [Theory]
    [InlineData("node", "22", false, "~22")]
    [InlineData("node", "22.3.1", false, "~22")]
    [InlineData("node", "22 LTS", false, "~22")]
    [InlineData("node", null, false, "~22")]
    [InlineData("node", "20", true, null)]
    [InlineData("python", "3.12", false, null)]
    public void BuildAppSettings_ComposesWebsiteNodeDefaultVersion_WhenApplicable(string runtime, string? runtimeVersion, bool requiresLinux, string? expected)
    {
        var dict = FunctionAppService.BuildAppSettings(runtime, runtimeVersion, requiresLinux, "UseDevelopmentStorage=true");

        dict.Properties.TryGetValue("WEBSITE_NODE_DEFAULT_VERSION", out var actualObj);
        var actual = actualObj as string;

        Assert.Equal(expected, actual);
        Assert.Equal(runtime, dict.Properties["FUNCTIONS_WORKER_RUNTIME"]);
        Assert.Equal("~4", dict.Properties["FUNCTIONS_EXTENSION_VERSION"]);
        Assert.Equal("UseDevelopmentStorage=true", dict.Properties["AzureWebJobsStorage"]);
    }

    [Fact]
    public async Task ExecuteAsync_ExistingPlan_NoPlanTypeOrSku()
    {
        var expected = new FunctionAppInfo("myapp", "rg", "eastus", "existingPlan", "Running", "myapp.azurewebsites.net", null, null);
        _service.CreateFunctionApp(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>())
            .Returns(expected);
        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub --resource-group rg --function-app myapp --location eastus --app-service-plan existingPlan");
        var response = await _command.ExecuteAsync(context, parseResult);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _service.Received(1).CreateFunctionApp(
            "sub", "rg", "myapp", "eastus",
            Arg.Is<string?>(p => p == "existingPlan"),
            Arg.Is<string?>(pt => pt == null),
            Arg.Is<string?>(ps => ps == null),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_SkuPrecedence_OverridesPlanType()
    {
        var expected = new FunctionAppInfo("myapp", "rg", "eastus", "plan", "Running", "myapp.azurewebsites.net", null, null);
        _service.CreateFunctionApp(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>())
            .Returns(expected);
        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub --resource-group rg --function-app myapp --location eastus --plan-type flex --plan-sku B1");
        var response = await _command.ExecuteAsync(context, parseResult);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _service.Received(1).CreateFunctionApp(
            "sub", "rg", "myapp", "eastus",
            Arg.Any<string?>(),
            Arg.Is<string?>(pt => pt == "flex"),
            Arg.Is<string?>(ps => ps == "B1"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_DotnetIsolated_RuntimeAccepted()
    {
        var expected = new FunctionAppInfo("myapp", "rg", "eastus", "plan", "Running", "myapp.azurewebsites.net", null, null);
        _service.CreateFunctionApp(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>())
            .Returns(expected);
        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub --resource-group rg --function-app myapp --location eastus --runtime dotnet-isolated");
        var response = await _command.ExecuteAsync(context, parseResult);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _service.Received(1).CreateFunctionApp(
            "sub", "rg", "myapp", "eastus",
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is<string?>(r => r == "dotnet-isolated"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesOperatingSystemOption()
    {
        var expected = new FunctionAppInfo("myapp", "rg", "eastus", "plan", "Running", "myapp.azurewebsites.net", null, null);
        _service.CreateFunctionApp(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is<string?>(os => os == "linux"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>())
            .Returns(expected);
        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub --resource-group rg --function-app myapp --location eastus --os linux");
        var response = await _command.ExecuteAsync(context, parseResult);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _service.Received(1).CreateFunctionApp(
            "sub", "rg", "myapp", "eastus",
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is<string?>(os => os == "linux"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions?>());
    }

    [Fact]
    public void BuildAppSettings_NodeOnLinux_DoesNotSetWebsiteNodeDefaultVersion()
    {
        var dict = FunctionAppService.BuildAppSettings("node", "22", true, "UseDevelopmentStorage=true");
        Assert.False(dict.Properties.ContainsKey("WEBSITE_NODE_DEFAULT_VERSION"));
    }

    [Fact]
    public void BuildAppSettings_FlexConsumption_OmitsFunctionsWorkerRuntime()
    {
        var dict = FunctionAppService.BuildAppSettings("dotnet", null, false, "UseDevelopmentStorage=true", includeWorkerRuntime: false);
        Assert.False(dict.Properties.ContainsKey("FUNCTIONS_WORKER_RUNTIME"));
        Assert.Equal("~4", dict.Properties["FUNCTIONS_EXTENSION_VERSION"]);
        Assert.Equal("UseDevelopmentStorage=true", dict.Properties["AzureWebJobsStorage"]);
    }

    [Fact]
    public void CreateStorageAccountOptions_Defaults()
    {
        var opts = FunctionAppService.CreateStorageAccountOptions("eastus");
        Assert.Equal(StorageSkuName.StandardLrs, opts.Sku.Name);
        Assert.Equal(StorageKind.StorageV2, opts.Kind);
        Assert.Equal(StorageAccountAccessTier.Hot, opts.AccessTier);
        Assert.True(opts.EnableHttpsTrafficOnly);
        Assert.False(opts.AllowBlobPublicAccess);
        Assert.False(opts.IsHnsEnabled);
    }

    [Theory]
    [InlineData("dotnet", "mcr.microsoft.com/azure-functions/dotnet:4")]
    [InlineData("dotnet-isolated", "mcr.microsoft.com/azure-functions/dotnet-isolated:4")]
    [InlineData("node", "mcr.microsoft.com/azure-functions/node:4")]
    [InlineData("python", "mcr.microsoft.com/azure-functions/python:4")]
    [InlineData("java", "mcr.microsoft.com/azure-functions/java:4")]
    [InlineData("powershell", "mcr.microsoft.com/azure-functions/powershell:4")]
    [InlineData("unknownRuntime", "mcr.microsoft.com/azure-functions/dotnet-isolated:4")]
    public void GetContainerImage_MapsRuntimes(string runtime, string expectedImage)
    {
        var image = FunctionAppService.GetContainerImage(runtime);
        Assert.Equal(expectedImage, image);
    }

    [Theory]
    [InlineData("node", "flex", null, true, null)]
    [InlineData("dotnet", "flex", null, true, null)]
    [InlineData("dotnet", null, null, false, null)]
    [InlineData("java", null, null, false, null)]
    [InlineData("python", "appservice", null, true, null)]
    [InlineData("node", null, "linux", true, "linux")]
    [InlineData("node", "containerapp", null, true, null)]
    [InlineData("dotnet", null, "windows", false, "windows")]
    public void ResolveOs_CorrectlyEvaluates(string runtime, string? planType, string? operatingSystem, bool expectedRequiresLinux, string? expectedNormalizedOs)
    {
        var hostingKind = FunctionAppService.ParseHostingKind(planType);
        var (actualRequiresLinux, actualNormalizedOs) = FunctionAppService.ResolveOs(runtime, hostingKind, operatingSystem);
        Assert.Equal(expectedRequiresLinux, actualRequiresLinux);
        Assert.Equal(expectedNormalizedOs, actualNormalizedOs);
    }

    [Fact]
    public void ResolveOs_ThrowsWhenPythonWithWindows()
    {
        var hostingKind = FunctionAppService.ParseHostingKind(null);
        Assert.Throws<InvalidOperationException>(() =>
            FunctionAppService.ResolveOs("python", hostingKind, "windows"));
    }

    [Theory]
    [InlineData(null, "7.4")]
    [InlineData("7.3", "7.3")]
    public void CreateWindowsPowerShellSiteConfig_SetsVersion(string? input, string expected)
    {
        var cfg = FunctionAppService.CreateWindowsPowerShellSiteConfig(input);
        Assert.NotNull(cfg);
        Assert.Equal(expected, cfg!.PowerShellVersion);
    }

    [Theory]
    [InlineData("DefaultEndpointsProtocol=https;AccountName=account1name123457891011;AccountKey=key;EndpointSuffix=core.windows.net", "account1name123457891011")]
    [InlineData("AccountKey=key;EndpointSuffix=core.windows.net;AccountName=abc123", "abc123")]
    [InlineData("AccountKey=key;EndpointSuffix=core.windows.net", null)]
    [InlineData(null, null)]
    public void ExtractStorageAccountName_ParsesName(string? connection, string? expected)
    {
        var actual = FunctionAppService.ExtractStorageAccountName(connection);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("DefaultEndpointsProtocol=https;AccountName=account-name1;AccountKey=key;EndpointSuffix=core.windows.net", true)]
    [InlineData("AccountKey=key;EndpointSuffix=core.windows.net", false)]
    [InlineData(null, false)]
    public void BuildDeploymentStorage_CreatesStorageObjectWhenAccountPresent(string? connection, bool expectCreated)
    {
        var storage = FunctionAppService.BuildDeploymentStorage(connection ?? string.Empty);
        if (expectCreated)
        {
            Assert.NotNull(storage);
            Assert.Equal(FunctionAppStorageType.BlobContainer, storage!.StorageType);
            Assert.NotNull(storage.Authentication);
            Assert.EndsWith(".blob.core.windows.net/azure-webjobs-hosts", storage.Value.AbsoluteUri);
        }
        else
        {
            Assert.Null(storage);
        }
    }

    [Theory]
    [InlineData(null, "consumption")]
    [InlineData("", "consumption")]
    [InlineData("flex", "flexconsumption")]
    [InlineData("flexconsumption", "flexconsumption")]
    [InlineData("premium", "premium")]
    [InlineData("functionspremium", "premium")]
    [InlineData("appservice", "appservice")]
    [InlineData("containerapp", "containerapp")]
    [InlineData("containerapps", "containerapp")]
    [InlineData("unknown", "consumption")]
    public void ParseHostingKind_MapsCorrectly(string? planType, string expected)
    {
        var result = FunctionAppService.ParseHostingKind(planType);
        var expectedEnum = expected switch
        {
            "flexconsumption" => FunctionAppService.HostingKind.FlexConsumption,
            "premium" => FunctionAppService.HostingKind.Premium,
            "appservice" => FunctionAppService.HostingKind.AppService,
            "containerapp" => FunctionAppService.HostingKind.ContainerApp,
            _ => FunctionAppService.HostingKind.Consumption
        };
        Assert.Equal(expectedEnum, result);
    }

    [Theory]
    [InlineData("FC1", "FlexConsumption")]
    [InlineData("fc2", "FlexConsumption")]
    [InlineData("EP1", "ElasticPremium")]
    [InlineData("ep2", "ElasticPremium")]
    [InlineData("P1V3", "PremiumV3")]
    [InlineData("P2V3", "PremiumV3")]
    [InlineData("P3V3", "PremiumV3")]
    [InlineData("P1", "Premium")]
    [InlineData("P2", "Premium")]
    [InlineData("B1", "Basic")]
    [InlineData("B2", "Basic")]
    [InlineData("S1", "Standard")]
    [InlineData("S2", "Standard")]
    [InlineData("Y1", "Dynamic")]
    [InlineData("unknown", "Standard")]
    public void InferTier_CorrectlyInfersTierFromSku(string sku, string expectedTier)
    {
        var result = FunctionAppService.InferTier(sku);
        Assert.Equal(expectedTier, result);
    }


    [Theory]
    [InlineData("", "", "", "")]
    [InlineData("sub", "", "", "")]
    [InlineData("sub", "rg", "", "")]
    [InlineData("sub", "rg", "app", "")]
    public void ValidateAndNormalizeInputs_ThrowsForMissingRequiredParameters(
        string subscription, string resourceGroup, string functionAppName, string location)
    {
        Assert.Throws<ArgumentException>(() =>
            FunctionAppService.ValidateAndNormalizeInputs(
                subscription, resourceGroup, functionAppName, location,
                null, null, null, null, null, null, null));
    }

    [Fact]
    public void ValidateAndNormalizeInputs_NormalizesInputsCorrectly()
    {
        var result = FunctionAppService.ValidateAndNormalizeInputs(
            "sub",
            "rg",
            "app",
            "eastus",
            "  NODE  ",
            "  20  ",
            "  flex  ",
            "  EP1  ",
            "  LINUX  ",
            "  storage123abc  ",
            null);

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
        var result = FunctionAppService.ValidateAndNormalizeInputs(
            "sub", "rg", "app", "eastus", null, null, null, null, null, null, null);

        Assert.Equal("dotnet", result.Runtime);
    }

    [Theory]
    [InlineData("invalidRuntime")]
    [InlineData("javascript")]
    [InlineData("csharp")]
    public void ValidateParameterCombinations_ThrowsForUnsupportedRuntime(string runtime)
    {
        var inputs = new FunctionAppService.NormalizedInputs(runtime, null, null, null, null, null, null);
        Assert.Throws<ArgumentException>(() =>
            FunctionAppService.ValidateParameterCombinations(inputs));
    }

    [Fact]
    public void ValidateParameterCombinations_ThrowsForPythonWithWindows()
    {
        var inputs = new FunctionAppService.NormalizedInputs("python", null, null, null, "windows", null, null);
        Assert.Throws<InvalidOperationException>(() =>
            FunctionAppService.ValidateParameterCombinations(inputs));
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("tooLongStorageAccountName123")]
    [InlineData("Storage123")]
    [InlineData("storage-123")]
    public void ValidateParameterCombinations_ThrowsForInvalidStorageAccountName(string storageAccountName)
    {
        var inputs = new FunctionAppService.NormalizedInputs("dotnet", null, null, null, null, storageAccountName, null);
        Assert.Throws<ArgumentException>(() =>
            FunctionAppService.ValidateParameterCombinations(inputs));
    }

    [Fact]
    public void ValidateParameterCombinations_ThrowsForContainerAppsEnvironmentWithoutContainerApps()
    {
        var inputs = new FunctionAppService.NormalizedInputs("dotnet", null, "consumption", null, null, null, "env123");
        Assert.Throws<InvalidOperationException>(() =>
            FunctionAppService.ValidateParameterCombinations(inputs));
    }

    [Fact]
    public void ValidateParameterCombinations_ThrowsForContainerAppsWithSku()
    {
        var inputs = new FunctionAppService.NormalizedInputs("dotnet", null, "containerapp", "B1", null, null, null);
        Assert.Throws<InvalidOperationException>(() =>
            FunctionAppService.ValidateParameterCombinations(inputs));
    }

    [Fact]
    public void ValidateParameterCombinations_ThrowsForFlexConsumptionDotnetWithVersion()
    {
        var inputs = new FunctionAppService.NormalizedInputs("dotnet", "8.0", "flex", null, null, null, null);
        Assert.Throws<InvalidOperationException>(() =>
            FunctionAppService.ValidateParameterCombinations(inputs));
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("mac")]
    [InlineData("WINDOWS")]
    public void ValidateParameterCombinations_ThrowsForInvalidOperatingSystem(string os)
    {
        var inputs = new FunctionAppService.NormalizedInputs("dotnet", null, null, null, os, null, null);
        Assert.Throws<ArgumentException>(() =>
            FunctionAppService.ValidateParameterCombinations(inputs));
    }

    [Theory]
    [InlineData("dotnet", null, "flex", null, "dotnet-isolated", "8.0")]
    [InlineData("dotnet", "8.0", "consumption", null, "dotnet", "8.0")]
    [InlineData("node", "20", "premium", null, "node", "20")]
    [InlineData("python", null, "appservice", null, "python", "3.12")]
    public void BuildCreateOptions_ConfiguresCorrectly(string runtime, string? runtimeVersion, string? planType, string? os, string expectedRuntime, string expectedRuntimeVersion)
    {
        var inputs = new FunctionAppService.NormalizedInputs(runtime, runtimeVersion, planType, null, os, null, null);
        var result = FunctionAppService.BuildCreateOptions(inputs);

        Assert.Equal(expectedRuntime, result.Runtime);
        Assert.Equal(expectedRuntimeVersion, result.RuntimeVersion);
    }

    [Theory]
    [InlineData(true, "dotnet", "8.0", "DOTNET|8.0")]
    [InlineData(true, "node", "20", "Node|20")]
    [InlineData(true, "python", null, "Python|3.12")]
    [InlineData(false, "dotnet", "8.0", null)]
    [InlineData(false, "powershell", "7.4", "7.4")]
    [InlineData(false, "node", "20", null)]
    public void BuildSiteConfig_ConfiguresCorrectly(bool isLinux, string runtime, string? runtimeVersion, string? expectedValue)
    {
        var options = new FunctionAppService.CreateOptions(runtime, runtimeVersion, FunctionAppService.HostingKind.Consumption, isLinux, null, null);
        var result = FunctionAppService.BuildSiteConfig(isLinux, options);

        if (expectedValue == null)
        {
            if (isLinux || runtime != "powershell")
                Assert.Null(result);
        }
        else if (isLinux)
        {
            Assert.NotNull(result);
            Assert.Equal(expectedValue, result!.LinuxFxVersion);
        }
        else
        {
            Assert.NotNull(result);
            Assert.Equal(expectedValue, result!.PowerShellVersion);
        }
    }

    [Theory]
    [InlineData(true, "functionapp,linux")]
    [InlineData(false, "functionapp")]
    public void BuildKind_ConfiguresCorrectly(bool isLinux, string expected)
    {
        var result = FunctionAppService.BuildKind(isLinux);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ValidateExistingPlan_ThrowsForWindowsPlanWithLinuxRuntime()
    {
        var planData = new AppServicePlanData("eastus") { IsReserved = false };
        var plan = Substitute.For<AppServicePlanResource>();
        plan.Data.Returns(planData);
        var options = new FunctionAppService.CreateOptions("python", "3.12", FunctionAppService.HostingKind.Consumption, true, null, null);

        Assert.Throws<InvalidOperationException>(() =>
            FunctionAppService.ValidateExistingPlan(plan, "test-plan", options));
    }

    [Fact]
    public void ValidateExistingPlan_ThrowsForNonFlexPlanWithFlexHosting()
    {
        var sku = new AppServiceSkuDescription { Tier = "Dynamic" };
        var planData = new AppServicePlanData("eastus") { Sku = sku, IsReserved = true };
        var plan = Substitute.For<AppServicePlanResource>();
        plan.Data.Returns(planData);
        var options = new FunctionAppService.CreateOptions("dotnet", "8.0", FunctionAppService.HostingKind.FlexConsumption, true, null, null);

        Assert.Throws<InvalidOperationException>(() =>
            FunctionAppService.ValidateExistingPlan(plan, "test-plan", options));
    }

    [Fact]
    public void ValidateExistingPlan_ThrowsForNonPremiumPlanWithPremiumHosting()
    {
        var sku = new AppServiceSkuDescription { Tier = "Dynamic" };
        var planData = new AppServicePlanData("eastus") { Sku = sku, IsReserved = true };
        var plan = Substitute.For<AppServicePlanResource>();
        plan.Data.Returns(planData);
        var options = new FunctionAppService.CreateOptions("dotnet", "8.0", FunctionAppService.HostingKind.Premium, true, null, null);

        Assert.Throws<InvalidOperationException>(() =>
            FunctionAppService.ValidateExistingPlan(plan, "test-plan", options));
    }

    [Theory]
    [InlineData("functionapp", true)]
    [InlineData("functionapp,linux", true)]
    [InlineData("app", false)]
    [InlineData("web", false)]
    [InlineData(null, false)]
    public void IsFunctionApp_DetectsCorrectly(string? kind, bool expected)
    {
        var siteData = new WebSiteData("eastus") { Kind = kind };
        var result = FunctionAppService.IsFunctionApp(siteData);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("my-app", "myapp")]
    [InlineData("My-App-123", "myapp123")]
    [InlineData("@#$%", "func")]
    [InlineData("very-long-function-app-name", "verylongfunctionap")]
    public void CreateStorageAccountName_GeneratesValidName(string functionAppName, string expectedPrefix)
    {
        var result = FunctionAppService.CreateStorageAccountName(functionAppName);

        Assert.True(result.Length >= 9 && result.Length <= 24);
        Assert.StartsWith(expectedPrefix, result);
        Assert.True(result.All(c => char.IsLetterOrDigit(c) && (char.IsDigit(c) || char.IsLower(c))));
    }

    [Fact]
    public void BuildConnectionString_FormatsCorrectly()
    {
        var result = FunctionAppService.BuildConnectionString("storageaccount", "key123");
        var expected = "DefaultEndpointsProtocol=https;AccountName=storageaccount;AccountKey=key123;EndpointSuffix=core.windows.net";
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("FlexConsumption", true)]
    [InlineData("flexconsumption", true)]
    [InlineData("Dynamic", false)]
    [InlineData("ElasticPremium", false)]
    [InlineData(null, false)]
    public void IsFlexConsumption_DetectsCorrectly(string? tier, bool expected)
    {
        var sku = tier != null ? new AppServiceSkuDescription { Tier = tier } : null;
        var planData = new AppServicePlanData("eastus") { Sku = sku };
        var result = FunctionAppService.IsFlexConsumption(planData);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void MapToFunctionAppRuntimeName_MapsCorrectly()
    {
        Assert.Equal(FunctionAppRuntimeName.DotnetIsolated, FunctionAppService.MapToFunctionAppRuntimeName("dotnet"));
        Assert.Equal(FunctionAppRuntimeName.DotnetIsolated, FunctionAppService.MapToFunctionAppRuntimeName("dotnet-isolated"));
        Assert.Equal(FunctionAppRuntimeName.Node, FunctionAppService.MapToFunctionAppRuntimeName("node"));
        Assert.Equal(FunctionAppRuntimeName.Java, FunctionAppService.MapToFunctionAppRuntimeName("java"));
        Assert.Equal(FunctionAppRuntimeName.Python, FunctionAppService.MapToFunctionAppRuntimeName("python"));
        Assert.Equal(FunctionAppRuntimeName.Powershell, FunctionAppService.MapToFunctionAppRuntimeName("powershell"));
        Assert.Equal(FunctionAppRuntimeName.Custom, FunctionAppService.MapToFunctionAppRuntimeName("custom"));
        Assert.Equal(FunctionAppRuntimeName.Custom, FunctionAppService.MapToFunctionAppRuntimeName("unknown"));
    }

    [Theory]
    [InlineData("dotnet", "8.0", "8.0")]
    [InlineData("java", "21.0", "21")]
    [InlineData("java", "17", "17")]
    [InlineData("node", null, "22")]
    [InlineData("python", "", "3.12")]
    public void NormalizeRuntimeVersionForConfig_NormalizesCorrectly(string runtime, string? runtimeVersion, string expected)
    {
        var result = FunctionAppService.NormalizeRuntimeVersionForConfig(runtime, runtimeVersion);
        Assert.Equal(expected, result);
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
        var result = FunctionAppService.ExtractMajorVersion(version);
        Assert.Equal(expected, result);
    }
}
