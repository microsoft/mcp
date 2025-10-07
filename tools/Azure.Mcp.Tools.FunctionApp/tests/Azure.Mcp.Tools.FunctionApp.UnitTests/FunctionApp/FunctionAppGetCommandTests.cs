// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.FunctionApp.Commands;
using Azure.Mcp.Tools.FunctionApp.Commands.FunctionApp;
using Azure.Mcp.Tools.FunctionApp.Models;
using Azure.Mcp.Tools.FunctionApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FunctionApp.UnitTests.FunctionApp;

public sealed class FunctionAppGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFunctionAppService _service;
    private readonly ILogger<FunctionAppGetCommand> _logger;
    private readonly FunctionAppGetCommand _command;

    public FunctionAppGetCommandTests()
    {
        _service = Substitute.For<IFunctionAppService>();
        _logger = Substitute.For<ILogger<FunctionAppGetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
    }

    [Theory]
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --tenant tenant123", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_Listing_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var testFunctionApps = new List<FunctionAppInfo>
            {
                new("functionApp1", null, "eastus", "plan1", "Running", "functionapp1.azurewebsites.net", null),
                new("functionApp2", null, "westus", "plan2", "Stopped", "functionapp2.azurewebsites.net", null)
            };
            _service.GetFunctionApp(
                Arg.Any<string>(),
                Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
                Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>())
                .Returns(testFunctionApps);
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFunctionAppList()
    {
        // Arrange
        var expectedFunctionApps = new List<FunctionAppInfo>
        {
            new("functionApp1", "rg1", "eastus", "plan1", "Running", "functionapp1.azurewebsites.net", null),
            new("functionApp2", "rg2", "westus", "plan2", "Stopped", "functionapp2.azurewebsites.net", null)
        };
        _service.GetFunctionApp(
            Arg.Any<string>(),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedFunctionApps);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        // Verify the mock was called
        await _service.Received(1).GetFunctionApp(
            Arg.Any<string>(),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>());

        var json = JsonSerializer.Serialize(response.Results);

        var result = JsonSerializer.Deserialize(json, FunctionAppJsonContext.Default.FunctionAppGetCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedFunctionApps.Count, result.FunctionApps.Count);
        Assert.Equal(expectedFunctionApps[0].Name, result.FunctionApps[0].Name);
        Assert.Equal(expectedFunctionApps[0].ResourceGroupName, result.FunctionApps[0].ResourceGroupName);
        Assert.Equal(expectedFunctionApps[0].AppServicePlanName, result.FunctionApps[0].AppServicePlanName);
        Assert.Equal(expectedFunctionApps[0].Location, result.FunctionApps[0].Location);
        Assert.Equal(expectedFunctionApps[0].Status, result.FunctionApps[0].Status);
        Assert.Equal(expectedFunctionApps[0].DefaultHostName, result.FunctionApps[0].DefaultHostName);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyWhenNoFunctionApp()
    {
        // Arrange
        _service.GetFunctionApp(
            Arg.Any<string>(),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns([]);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, FunctionAppJsonContext.Default.FunctionAppGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.FunctionApps);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _service.GetFunctionApp(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromException<List<FunctionAppInfo>?>(new Exception("Test error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --function-app app1", true)]
    [InlineData("--subscription sub123 --function-app app1", false)]
    [InlineData("--resource-group rg1 --function-app app1", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _service.GetFunctionApp(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
                .Returns([new FunctionAppInfo("app1", "rg1", "eastus", "plan1", "Running", "app1.azurewebsites.net", null)]);
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        var response = await _command.ExecuteAsync(context, parseResult);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFunctionApp()
    {
        var expected = new FunctionAppInfo("app1", "rg1", "eastus", "plan1", "Running", "app1.azurewebsites.net", null);
        _service.GetFunctionApp(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .Returns([expected]);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123 --resource-group rg1 --function-app app1");

        var response = await _command.ExecuteAsync(context, parseResult);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, FunctionAppJsonContext.Default.FunctionAppGetCommandResult);
        Assert.NotNull(result);
        Assert.Equal(expected.Name, result.FunctionApps[0].Name);
        Assert.Equal(expected.ResourceGroupName, result.FunctionApps[0].ResourceGroupName);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyWhenNotFound()
    {
        _service.GetFunctionApp(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .Returns((List<FunctionAppInfo>?)null);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123 --resource-group rg1 --function-app app1");

        var response = await _command.ExecuteAsync(context, parseResult);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, FunctionAppJsonContext.Default.FunctionAppGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.FunctionApps);
    }
}
