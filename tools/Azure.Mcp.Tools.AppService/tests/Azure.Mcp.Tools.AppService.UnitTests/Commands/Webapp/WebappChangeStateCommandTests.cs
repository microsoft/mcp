// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AppService.Commands;
using Azure.Mcp.Tools.AppService.Commands.Webapp;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppService.UnitTests.Commands.Webapp;

[Trait("Command", "WebappChangeState")]
public class WebappChangeStateCommandTests : CommandUnitTestsBase<WebappChangeStateCommand, IAppServiceService>
{
    [Theory]
    [InlineData("start", false, false)]
    [InlineData("stop", false, false)]
    [InlineData("restart", false, false)]
    [InlineData("restart", true, true)]
    public async Task ExecuteAsync_WithValidParameters_CallsServiceWithCorrectArguments(string stateChange, bool softRestart, bool waitForCompletion)
    {
        // Arrange
        var expected = $"Web app state change '{stateChange}' initiated successfully.";

        // Set up the mock to return success for any arguments
        Service.ChangeWebAppStateAsync("sub123", "rg1", "test-app", stateChange, softRestart,
            waitForCompletion, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        List<string> unparsedArgs = ["--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app", "--state-change", stateChange];
        if (softRestart)
        {
            unparsedArgs.Add("--soft-restart");
        }
        if (waitForCompletion)
        {
            unparsedArgs.Add("--wait-for-completion");
        }

        // Act
        var response = await ExecuteCommandAsync(unparsedArgs.ToArray());

        // Assert
        // Verify that the mock was called with the expected parameters
        await Service.Received(1).ChangeWebAppStateAsync("sub123", "rg1", "test-app", stateChange,
            softRestart, waitForCompletion, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, AppServiceJsonContext.Default.WebappChangeStateResult);
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
        // Arrange & Act
        var response = await ExecuteCommandAsync(commandArgs);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);

        await Service.DidNotReceive().ChangeWebAppStateAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_InvalidStateChange_ReturnsErrorResponse()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--app", "test-app",
            "--state-change", "invalid-state");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);

        await Service.DidNotReceive().ChangeWebAppStateAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("start", false, false)]
    [InlineData("stop", false, false)]
    [InlineData("restart", false, false)]
    [InlineData("restart", true, true)]

    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorResponse(string stateChange, bool softRestart, bool waitForCompletion)
    {
        // Arrange
        Service.ChangeWebAppStateAsync("sub123", "rg1", "test-app", stateChange, softRestart,
            waitForCompletion, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        List<string> unparsedArgs = ["--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app", "--state-change", stateChange];
        if (softRestart)
        {
            unparsedArgs.Add("--soft-restart");
        }
        if (waitForCompletion)
        {
            unparsedArgs.Add("--wait-for-completion");
        }

        // Act
        var response = await ExecuteCommandAsync(unparsedArgs.ToArray());

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);

        await Service.Received(1).ChangeWebAppStateAsync("sub123", "rg1", "test-app", stateChange,
            softRestart, waitForCompletion, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
