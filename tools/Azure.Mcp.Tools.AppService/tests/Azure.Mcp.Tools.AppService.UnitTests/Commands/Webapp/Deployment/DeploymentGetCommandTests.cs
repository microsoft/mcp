// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AppService.Commands;
using Azure.Mcp.Tools.AppService.Commands.Webapp.Deployment;
using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppService.UnitTests.Commands.Webapp.Deployment;

[Trait("Command", "DeploymentGet")]
public class DeploymentGetCommandTests : CommandUnitTestsBase<DeploymentGetCommand, IAppServiceService>
{
    [Theory]
    [InlineData(null)]
    [InlineData("deployment123")]
    public async Task ExecuteAsync_WithValidParameters_CallsServiceWithCorrectArguments(string? deploymentId)
    {
        // Arrange
        List<DeploymentDetails> expectedDeployments = [
            new("name", "type", "kind", true, 0, "author", "deployer", DateTimeOffset.UtcNow, null)
        ];

        // Set up the mock to return success for any arguments
        Service.GetDeploymentsAsync("sub123", "rg1", "test-app", deploymentId, Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedDeployments);

        List<string> unparsedArgs = ["--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app"];
        if (!string.IsNullOrEmpty(deploymentId))
        {
            unparsedArgs.AddRange(["--deployment-id", deploymentId]);
        }

        // Act
        var response = await ExecuteCommandAsync(unparsedArgs.ToArray());

        // Assert
        // Verify that the mock was called with the expected parameters
        await Service.Received(1).GetDeploymentsAsync("sub123", "rg1", "test-app", deploymentId,
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, AppServiceJsonContext.Default.DeploymentGetResult);

        Assert.NotNull(result);
        Assert.Equal(JsonSerializer.Serialize(expectedDeployments), JsonSerializer.Serialize(result.Deployments));
    }

    [Theory]
    [InlineData("--resource-group", "rg1")] // Missing subscription and app name
    [InlineData("--subscription", "sub123")] // Missing resource group and app name
    [InlineData("--app", "test-app")] // Missing subscription and resource group
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

        await Service.DidNotReceive().GetDeploymentsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("deployment123")]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorResponse(string? deploymentId)
    {
        // Arrange
        Service.GetDeploymentsAsync("sub123", "rg1", "test-app", deploymentId, Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        List<string> unparsedArgs = ["--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app"];
        if (!string.IsNullOrEmpty(deploymentId))
        {
            unparsedArgs.AddRange(["--deployment-id", deploymentId]);
        }

        // Act
        var response = await ExecuteCommandAsync(unparsedArgs.ToArray());

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);

        await Service.Received(1).GetDeploymentsAsync("sub123", "rg1", "test-app", deploymentId,
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }
}
