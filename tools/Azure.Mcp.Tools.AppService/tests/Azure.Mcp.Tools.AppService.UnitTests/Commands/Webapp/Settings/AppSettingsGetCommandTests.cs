// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AppService.Commands;
using Azure.Mcp.Tools.AppService.Commands.Webapp.Settings;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppService.UnitTests.Commands.Webapp.Settings;

[Trait("Command", "AppSettingsGet")]
public class AppSettingsGetCommandTests : CommandUnitTestsBase<AppSettingsGetCommand, IAppServiceService>
{
    [Fact]
    public async Task ExecuteAsync_WithValidParameters_CallsServiceWithCorrectArguments()
    {
        // Arrange
        IDictionary<string, string> expectedSettings = new Dictionary<string, string>()
        {
            {"Setting1", "Value1"},
            {"Setting2", "Value2"}
        };

        // Set up the mock to return success for any arguments
        Service.GetAppSettingsAsync("sub123", "rg1", "test-app", Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedSettings);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app");

        // Assert
        // Verify that the mock was called with the expected parameters
        await Service.Received(1).GetAppSettingsAsync("sub123", "rg1", "test-app", Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, AppServiceJsonContext.Default.AppSettingsGetResult);

        Assert.NotNull(result);
        Assert.Equal(JsonSerializer.Serialize(expectedSettings), JsonSerializer.Serialize(result.AppSettings));
    }

    [Theory]
    [InlineData()] // Missing all parameters
    [InlineData("--subscription", "sub123")] // Missing resource group and app name
    [InlineData("--resource-group", "rg1")] // Missing subscription and app name
    [InlineData("--app", "app")] // Missing subscription and resource group
    [InlineData("--subscription", "sub123", "--resource-group", "rg1")] // Missing app name
    [InlineData("--subscription", "sub123", "--app", "test-app")] // Missing resource group
    [InlineData("--resource-group", "rg1", "--app", "test-app")] // Missing subscription
    public async Task ExecuteAsync_MissingRequiredParameter_ReturnsErrorResponse(params string[] commandArgs)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(commandArgs);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);

        await Service.DidNotReceive().GetAppSettingsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        Service.GetAppSettingsAsync("sub123", "rg1", "test-app", Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);

        await Service.Received(1).GetAppSettingsAsync("sub123", "rg1", "test-app", Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }
}
