// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.FunctionApp.Commands;
using Azure.Mcp.Tools.FunctionApp.Commands.FunctionApp;
using Azure.Mcp.Tools.FunctionApp.Models;
using Azure.Mcp.Tools.FunctionApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FunctionApp.UnitTests.FunctionApp;

public sealed class FunctionAppCreateContainerAppCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFunctionAppService _service;
    private readonly ILogger<FunctionAppCreateContainerAppCommand> _logger;
    private readonly FunctionAppCreateContainerAppCommand _command;

    public FunctionAppCreateContainerAppCommandTests()
    {
        _service = Substitute.For<IFunctionAppService>();
        _logger = Substitute.For<ILogger<FunctionAppCreateContainerAppCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger, _service);
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
    [InlineData("--subscription sub --resource-group rg --location eastus", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _service.CreateContainerAppFunctionApp(
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
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new FunctionAppInfo(
                    "myapp",
                    "rg",
                    "eastus",
                    "containerapp",
                    "Running",
                    "myapp.azurecontainerapps.io",
                    "linux",
                    null));
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCreatedFunctionApp()
    {
        var expected = new FunctionAppInfo("myapp", "rg", "eastus", "containerapp", "Running", "myapp.azurecontainerapps.io", "linux", null);
        _service.CreateContainerAppFunctionApp(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>()).Returns(expected);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub --resource-group rg --function-app myapp --location eastus");

        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<FunctionAppCreateContainerAppCommand.FunctionAppCreateContainerAppCommandResult>(json, FunctionAppJsonContext.Default.FunctionAppCreateContainerAppCommandResult);
        Assert.NotNull(result);
        Assert.Equal("myapp", result.FunctionApp.Name);
        Assert.Equal("linux", result.FunctionApp.OperatingSystem);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        _service.CreateContainerAppFunctionApp(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FunctionAppInfo>(new Exception("Container App create error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub --resource-group rg --function-app myapp --location eastus");

        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Container App create error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DefaultsRuntimeToDotnet_WhenNotProvided()
    {
        var expected = new FunctionAppInfo("myapp", "rg", "eastus", "containerapp", "Running", "myapp.azurecontainerapps.io", "linux", null);
        _service.CreateContainerAppFunctionApp(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>()).Returns(expected);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub --resource-group rg --function-app myapp --location eastus");

        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _service.Received(1).CreateContainerAppFunctionApp(
            "sub", "rg", "myapp", "eastus",
            Arg.Is<string?>(r => r == "dotnet"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("dotnet-isolated", "8.0")]
    [InlineData("node", "22")]
    [InlineData("python", "3.12")]
    [InlineData("java", "17")]
    [InlineData("powershell", "7.4")]
    public async Task ExecuteAsync_PassesRuntimeAndVersion(string runtime, string runtimeVersion)
    {
        var expected = new FunctionAppInfo("myapp", "rg", "eastus", "containerapp", "Running", "myapp.azurecontainerapps.io", "linux", null);
        _service.CreateContainerAppFunctionApp(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>()).Returns(expected);

        var context = new CommandContext(_serviceProvider);
        var args = $"--subscription sub --resource-group rg --function-app myapp --location eastus --runtime {runtime} --runtime-version {runtimeVersion}";
        var parseResult = _command.GetCommand().Parse(args);

        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _service.Received(1).CreateContainerAppFunctionApp(
            "sub", "rg", "myapp", "eastus",
            Arg.Is<string?>(r => r == runtime),
            Arg.Is<string?>(v => v == runtimeVersion),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesStorageAccount()
    {
        var expected = new FunctionAppInfo("myapp", "rg", "eastus", "containerapp", "Running", "myapp.azurecontainerapps.io", "linux", null);
        _service.CreateContainerAppFunctionApp(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>()).Returns(expected);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub --resource-group rg --function-app myapp --location eastus --storage-account existingstorage");

        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _service.Received(1).CreateContainerAppFunctionApp(
            "sub", "rg", "myapp", "eastus",
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is<string?>(s => s == "existingstorage"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesContainerAppsEnvironment()
    {
        var expected = new FunctionAppInfo("myapp", "rg", "eastus", "containerapp", "Running", "myapp.azurecontainerapps.io", "linux", null);
        _service.CreateContainerAppFunctionApp(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>()).Returns(expected);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub --resource-group rg --function-app myapp --location eastus --container-apps-environment shared-env");

        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _service.Received(1).CreateContainerAppFunctionApp(
            "sub", "rg", "myapp", "eastus",
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is<string?>(e => e == "shared-env"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("a")]
    [InlineData("this-function-app-name-is-way-too-long-for-azure-to-accept")]
    public async Task ExecuteAsync_RejectsInvalidFunctionAppNameLength(string functionAppName)
    {
        var context = new CommandContext(_serviceProvider);
        var args = $"--subscription sub --resource-group rg --function-app {functionAppName} --location eastus";
        var parseResult = _command.GetCommand().Parse(args);

        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("function-app name must be between 2 and 43 characters", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MapsConflictToFriendlyMessage()
    {
        _service.CreateContainerAppFunctionApp(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FunctionAppInfo>(new RequestFailedException(status: (int)HttpStatusCode.Conflict, message: "name taken")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub --resource-group rg --function-app myapp --location eastus");

        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("Container App name already exists", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MapsForbiddenToFriendlyMessage()
    {
        _service.CreateContainerAppFunctionApp(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<FunctionAppInfo>(new RequestFailedException(status: (int)HttpStatusCode.Forbidden, message: "no permission")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub --resource-group rg --function-app myapp --location eastus");

        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }
}
