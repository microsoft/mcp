// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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

public sealed class FunctionAppListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFunctionAppService _functionAppService;
    private readonly ILogger<FunctionAppListCommand> _logger;
    private readonly FunctionAppListCommand _command;

    public FunctionAppListCommandTests()
    {
        _functionAppService = Substitute.For<IFunctionAppService>();
        _logger = Substitute.For<ILogger<FunctionAppListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_functionAppService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --tenant tenant123", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var testFunctionApps = new List<FunctionAppInfo>
            {
                new("functionApp1", null, "eastus", "plan1", "Running", "functionapp1.azurewebsites.net", null),
                new("functionApp2", null, "westus", "plan2", "Stopped", "functionapp2.azurewebsites.net", null)
            };
            _functionAppService.ListFunctionApps(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
                .Returns(testFunctionApps);
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
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
        _functionAppService.ListFunctionApps(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedFunctionApps);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        // Verify the mock was called
        await _functionAppService.Received(1).ListFunctionApps(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>());

        var json = JsonSerializer.Serialize(response.Results);

        var result = JsonSerializer.Deserialize(json, FunctionAppJsonContext.Default.FunctionAppListCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedFunctionApps.Count, result.Results.Count);
        Assert.Equal(expectedFunctionApps[0].Name, result.Results[0].Name);
        Assert.Equal(expectedFunctionApps[0].ResourceGroupName, result.Results[0].ResourceGroupName);
        Assert.Equal(expectedFunctionApps[0].AppServicePlanName, result.Results[0].AppServicePlanName);
        Assert.Equal(expectedFunctionApps[0].Location, result.Results[0].Location);
        Assert.Equal(expectedFunctionApps[0].Status, result.Results[0].Status);
        Assert.Equal(expectedFunctionApps[0].DefaultHostName, result.Results[0].DefaultHostName);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNullWhenNoFunctionApp()
    {
        // Arrange
        _functionAppService.ListFunctionApps(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .Returns(new List<FunctionAppInfo>());

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _functionAppService.ListFunctionApps(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromException<List<FunctionAppInfo>?>(new Exception("Test error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
