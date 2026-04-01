// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AppService.Commands;
using Azure.Mcp.Tools.AppService.Commands.Webapp.Deployment;
using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppService.UnitTests.Commands.Webapp.Deployment;

[Trait("Command", "DeploymentGet")]
public class DeploymentGetCommandTests
{
    private readonly IAppServiceService _appServiceService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeploymentGetCommand> _logger;
    private readonly DeploymentGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public DeploymentGetCommandTests()
    {
        _appServiceService = Substitute.For<IAppServiceService>();
        _logger = Substitute.For<ILogger<DeploymentGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_appServiceService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

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
        _appServiceService.GetDeploymentsAsync("sub123", "rg1", "test-app", deploymentId, Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedDeployments);

        List<string> unparsedArgs = ["--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app"];
        if (!string.IsNullOrEmpty(deploymentId))
        {
            unparsedArgs.AddRange(["--deployment-id", deploymentId]);
        }

        var args = _commandDefinition.Parse(unparsedArgs);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        // Verify that the mock was called with the expected parameters
        await _appServiceService.Received(1).GetDeploymentsAsync("sub123", "rg1", "test-app", deploymentId,
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AppServiceJsonContext.Default.DeploymentGetResult);

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
        // Arrange
        var args = _commandDefinition.Parse(commandArgs);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);

        await _appServiceService.DidNotReceive().GetDeploymentsAsync(
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
        _appServiceService.GetDeploymentsAsync("sub123", "rg1", "test-app", deploymentId, Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        List<string> unparsedArgs = ["--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app"];
        if (!string.IsNullOrEmpty(deploymentId))
        {
            unparsedArgs.AddRange(["--deployment-id", deploymentId]);
        }

        var args = _commandDefinition.Parse(unparsedArgs);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);

        await _appServiceService.Received(1).GetDeploymentsAsync("sub123", "rg1", "test-app", deploymentId,
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }
}
