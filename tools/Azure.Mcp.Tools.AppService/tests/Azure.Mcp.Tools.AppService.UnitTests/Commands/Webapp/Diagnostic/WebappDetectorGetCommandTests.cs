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

[Trait("Command", "WebappDetectorGet")]
public class WebappDetectorGetCommandTests
{
    private readonly IAppServiceService _appServiceService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WebappDetectorGetCommand> _logger;
    private readonly WebappDetectorGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public WebappDetectorGetCommandTests()
    {
        _appServiceService = Substitute.For<IAppServiceService>();
        _logger = Substitute.For<ILogger<WebappDetectorGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_appServiceService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("detector1")]
    public async Task ExecuteAsync_WithValidParameters_CallsServiceWithCorrectArguments(string? detectorName)
    {
        // Arrange
        List<WebappDetectorDetails> expectedDetectors = [new("name", "type", "kind", "description", true, 1.0)];

        // Set up the mock to return success for any arguments
        _appServiceService.GetWebAppDetectorsAsync("sub123", "rg1", "test-app", "category1", detectorName,
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedDetectors);

        List<string> unparsedArgs = [
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--app", "test-app",
            "--diagnostic-category", "category1"
        ];
        if (!string.IsNullOrEmpty(detectorName))
        {
            unparsedArgs.AddRange(["--detector-name", detectorName]);
        }

        var args = _commandDefinition.Parse(unparsedArgs);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        // Verify that the mock was called with the expected parameters
        await _appServiceService.Received(1).GetWebAppDetectorsAsync("sub123", "rg1", "test-app", "category1",
            detectorName, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AppServiceJsonContext.Default.WebappDetectorGetResult);

        Assert.NotNull(result);
        Assert.Equal(JsonSerializer.Serialize(expectedDetectors), JsonSerializer.Serialize(result.WebappDetectors));
    }

    [Theory]
    [InlineData("")] // Missing subscription, resource group, app, and diagnostic category
    [InlineData("--subscription", "sub123")] // Missing resource group, app, and diagnostic category
    [InlineData("--resource-group", "rg1")] // Missing subscription, app, and diagnostic category
    [InlineData("--app", "test-app")] // Missing subscription, resource group, and diagnostic category
    [InlineData("--subscription", "sub123", "--resource-group", "rg1")] // Missing app and diagnostic category
    [InlineData("--subscription", "sub123", "--app", "test-app")] // Missing resource group and diagnostic category
    [InlineData("--subscription", "sub123", "--diagnostic-category", "category1")] // Missing resource group and app
    [InlineData("--resource-group", "rg1", "--app", "test-app")] // Missing subscription and diagnostic category
    [InlineData("--resource-group", "rg1", "--diagnostic-category", "category1")] // Missing subscription and app
    [InlineData("--app", "test-app", "--diagnostic-category", "category1")] // Missing subscription and resource group
    [InlineData("--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app")] // Missing diagnostic category
    [InlineData("--subscription", "sub123", "--resource-group", "rg1", "--diagnostic-category", "category1")] // Missing app
    [InlineData("--subscription", "sub123", "--app", "test-app", "--diagnostic-category", "category1")] // Missing resource group
    public async Task ExecuteAsync_MissingRequiredParameter_ReturnsErrorResponse(params string[] commandArgs)
    {
        // Arrange
        var args = _commandDefinition.Parse(commandArgs);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);

        await _appServiceService.DidNotReceive().GetWebAppDetectorsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        _appServiceService.GetWebAppDetectorsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--app", "test-app",
            "--diagnostic-category", "category1"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);

        await _appServiceService.Received(1).GetWebAppDetectorsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
