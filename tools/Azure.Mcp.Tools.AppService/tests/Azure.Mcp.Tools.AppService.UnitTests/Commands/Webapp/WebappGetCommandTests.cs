// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AppService.Commands;
using Azure.Mcp.Tools.AppService.Commands.Webapp;
using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppService.UnitTests.Commands.Webapp;

[Trait("Command", "WebappGet")]
public class WebappGetCommandTests : CommandUnitTestsBase<WebappGetCommand, IAppServiceService>
{
    [Theory]
    [InlineData("sub123", null, null)]
    [InlineData("sub123", "rg1", null)]
    [InlineData("sub123", "rg1", "test-app")]
    public async Task ExecuteAsync_WithValidParameters_CallsServiceWithCorrectArguments(string subscription, string? resourceGroup, string? appName)
    {
        // Arrange
        List<WebappDetails> expectedWebappDetails = [
            new("name", "type", "location", "kind", true, "state", "rg", ["hostname"], DateTimeOffset.UtcNow, "sku")
        ];

        // Set up the mock to return success for any arguments
        Service.GetWebAppsAsync(subscription, resourceGroup, appName, Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedWebappDetails);

        List<string> unparsedArgs = ["--subscription", subscription];
        if (!string.IsNullOrEmpty(resourceGroup))
        {
            unparsedArgs.AddRange(["--resource-group", resourceGroup]);
        }
        if (!string.IsNullOrEmpty(appName))
        {
            unparsedArgs.AddRange(["--app", appName]);
        }

        // Act
        var response = await ExecuteCommandAsync(unparsedArgs.ToArray());

        // Assert
        // Verify that the mock was called with the expected parameters
        await Service.Received(1).GetWebAppsAsync(subscription, resourceGroup, appName, Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, AppServiceJsonContext.Default.WebappGetResult);

        Assert.NotNull(result);
        Assert.Equal(JsonSerializer.Serialize(expectedWebappDetails), JsonSerializer.Serialize(result.Webapps));
    }

    [Theory]
    [InlineData("--resource-group", "rg1")] // Missing subscription
    [InlineData("--subscription", "sub123", "--app", "test-app")] // Missing resource group
    public async Task ExecuteAsync_MissingRequiredParameter_ReturnsErrorResponse(params string[] commandArgs)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(commandArgs);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);

        await Service.DidNotReceive().GetWebAppsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        Service.GetWebAppsAsync("sub123", "rg1", "test-app", Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);

        await Service.Received(1).GetWebAppsAsync("sub123", "rg1", "test-app",
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }
}
