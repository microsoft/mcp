// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.AppService.Commands;
using Azure.Mcp.Tools.AppService.Commands.Webapp.Diagnostic;
using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppService.UnitTests.Commands.Webapp.Diagnostic;

[Trait("Command", "WebappDetectorList")]
public class WebappDetectorListCommandTests
{
    private readonly IAppServiceService _appServiceService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WebappDetectorListCommand> _logger;
    private readonly WebappDetectorListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public WebappDetectorListCommandTests()
    {
        _appServiceService = Substitute.For<IAppServiceService>();
        _logger = Substitute.For<ILogger<WebappDetectorListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_appServiceService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_CallsServiceWithCorrectArguments()
    {
        // Arrange
        List<WebappDetectorDetails> expectedDetectors = [new("name", "type", "kind", "description", "category")];

        // Set up the mock to return success for any arguments
        _appServiceService.ListWebAppDetectorsAsync("sub123", "rg1", "test-app", Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedDetectors);

        var args = _commandDefinition.Parse(["--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        // Verify that the mock was called with the expected parameters
        await _appServiceService.Received(1).ListWebAppDetectorsAsync("sub123", "rg1", "test-app", Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AppServiceJsonContext.Default.WebappDetectorListResult);

        Assert.NotNull(result);
        Assert.Equal(JsonSerializer.Serialize(expectedDetectors), JsonSerializer.Serialize(result.WebappDetectors));
    }

    [Theory]
    [InlineData("")] // Missing subscription, resource group, and app
    [InlineData("--subscription", "sub123")] // Missing resource group and app
    [InlineData("--resource-group", "rg1")] // Missing subscription and app
    [InlineData("--app", "test-app")] // Missing subscription and resource group
    [InlineData("--subscription", "sub123", "--resource-group", "rg1")] // Missing app
    [InlineData("--subscription", "sub123", "--app", "test-app")] // Missing resource group
    [InlineData("--resource-group", "rg1", "--app", "test-app")] // Missing subscription
    [InlineData("--resource-group", "rg1", "--diagnostic-category", "category1")] // Missing subscription and app
    public async Task ExecuteAsync_MissingRequiredParameter_ReturnsErrorResponse(params string[] commandArgs)
    {
        // Arrange
        var args = _commandDefinition.Parse(commandArgs);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);

        await _appServiceService.DidNotReceive().ListWebAppDetectorsAsync(
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
        _appServiceService.ListWebAppDetectorsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        var args = _commandDefinition.Parse(["--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);

        await _appServiceService.Received(1).ListWebAppDetectorsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
