// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.AppService.Commands;
using Azure.Mcp.Tools.AppService.Commands.Webapp;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppService.UnitTests.Commands.Webapp;

[Trait("Command", "WebappChangeState")]
public class WebappChangeStateCommandTests
{
    private readonly IAppServiceService _appServiceService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WebappChangeStateCommand> _logger;
    private readonly WebappChangeStateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public WebappChangeStateCommandTests()
    {
        _appServiceService = Substitute.For<IAppServiceService>();
        _logger = Substitute.For<ILogger<WebappChangeStateCommand>>();

        var collection = new ServiceCollection().AddSingleton(_appServiceService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Theory]
    [InlineData("start", null, null)]
    [InlineData("stop", null, null)]
    [InlineData("restart", null, null)]
    [InlineData("restart", true, true)]
    public async Task ExecuteAsync_WithValidParameters_CallsServiceWithCorrectArguments(string stateChange, bool? softRestart, bool? waitForCompletion)
    {
        // Arrange
        var expected = $"Web app state change '{stateChange}' initiated successfully.";

        // Set up the mock to return success for any arguments
        _appServiceService.ChangeWebAppStateAsync("sub123", "rg1", "test-app", stateChange, softRestart,
            waitForCompletion, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        List<string> unparsedArgs = ["--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app", "--state-change", stateChange];
        if (softRestart.HasValue)
        {
            unparsedArgs.Add("--soft-restart");
        }
        if (waitForCompletion.HasValue)
        {
            unparsedArgs.Add("--wait-for-completion");
        }

        var args = _commandDefinition.Parse(unparsedArgs);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        // Verify that the mock was called with the expected parameters
        await _appServiceService.Received(1).ChangeWebAppStateAsync("sub123", "rg1", "test-app", stateChange,
            softRestart, waitForCompletion, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AppServiceJsonContext.Default.WebappChangeStateResult);

        Assert.NotNull(result);
        Assert.Equal(expected, result.StateChangeStatus);
    }

    [Theory]
    [InlineData("--resource-group", "rg1")] // Missing subscription, app, and state change
    [InlineData("--subscription", "sub123")] // Missing resource group, app, and state change
    [InlineData("--app", "test-app")] // Missing subscription, resource group, and state change
    [InlineData("--state-change", "start")] // Missing subscription, resource group, and app
    [InlineData("--subscription", "sub123", "--state-change", "start")] // Missing resource group and app
    [InlineData("--subscription", "sub123", "--app", "test-app")] // Missing resource group and state change
    [InlineData("--subscription", "sub123", "--resource-group", "rg1")] // Missing app and state change
    [InlineData("--resource-group", "rg1", "--app", "test-app")] // Missing subscription and state change
    [InlineData("--resource-group", "rg1", "--state-change", "start")] // Missing subscription and app
    [InlineData("--resource-group", "rg1", "subscription", "sub123")] // Missing app and state change
    [InlineData("--app", "test-app", "--resource-group", "rg1")] // Missing subscription and state change
    [InlineData("--app", "test-app", "--state-change", "start")] // Missing subscription and resource group
    [InlineData("--state-change", "start", "--app", "test-app")] // Missing subscription and resource group
    [InlineData("--state-change", "start", "--resource-group", "rg1")] // Missing subscription and app
    [InlineData("--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app")] // Missing state change
    [InlineData("--subscription", "sub123", "--resource-group", "rg1", "--state-change", "start")] // Missing app
    [InlineData("--subscription", "sub123", "--app", "test-app", "--state-change", "start")] // Missing resource group
    [InlineData("--resource-group", "rg1", "--app", "test-app", "--state-change", "start")] // Missing subscription
    public async Task ExecuteAsync_MissingRequiredParameter_ReturnsErrorResponse(params string[] commandArgs)
    {
        // Arrange
        var args = _commandDefinition.Parse(commandArgs);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);

        await _appServiceService.DidNotReceive().ChangeWebAppStateAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_InvalidStateChange_ReturnsErrorResponse()
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--app", "test-app",
            "--state-change", "invalid-state"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);

        await _appServiceService.DidNotReceive().ChangeWebAppStateAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("start", null, null)]
    [InlineData("stop", null, null)]
    [InlineData("restart", null, null)]
    [InlineData("restart", true, true)]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorResponse(string stateChange, bool? softRestart, bool? waitForCompletion)
    {
        // Arrange

        _appServiceService.ChangeWebAppStateAsync("sub123", "rg1", "test-app", stateChange, softRestart,
            waitForCompletion, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        List<string> unparsedArgs = ["--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app", "--state-change", stateChange];
        if (softRestart.HasValue)
        {
            unparsedArgs.Add("--soft-restart");
        }
        if (waitForCompletion.HasValue)
        {
            unparsedArgs.Add("--wait-for-completion");
        }

        var args = _commandDefinition.Parse(unparsedArgs);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);

        await _appServiceService.Received(1).ChangeWebAppStateAsync("sub123", "rg1", "test-app", stateChange,
            softRestart, waitForCompletion, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
