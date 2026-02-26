// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.AppService.Commands;
using Azure.Mcp.Tools.AppService.Commands.Webapp;
using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppService.UnitTests.Commands.Webapp;

[Trait("Command", "WebappGet")]
public class WebappGetCommandTests
{
    private readonly IAppServiceService _appServiceService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WebappGetCommand> _logger;
    private readonly WebappGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public WebappGetCommandTests()
    {
        _appServiceService = Substitute.For<IAppServiceService>();
        _logger = Substitute.For<ILogger<WebappGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_appServiceService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

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
        _appServiceService.GetWebAppsAsync(subscription, resourceGroup, appName, Arg.Any<string?>(),
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

        var args = _commandDefinition.Parse(unparsedArgs);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        // Verify that the mock was called with the expected parameters
        await _appServiceService.Received(1).GetWebAppsAsync(subscription, resourceGroup, appName, Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AppServiceJsonContext.Default.WebappGetResult);

        Assert.NotNull(result);
        Assert.Equal(JsonSerializer.Serialize(expectedWebappDetails), JsonSerializer.Serialize(result.Webapps));
    }

    [Theory]
    [InlineData("--resource-group", "rg1")] // Missing subscription
    [InlineData("--subscription", "sub123", "--app", "test-app")] // Missing resource group
    public async Task ExecuteAsync_MissingRequiredParameter_ReturnsErrorResponse(params string[] commandArgs)
    {
        // Arrange
        var args = _commandDefinition.Parse(commandArgs);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);

        await _appServiceService.DidNotReceive().GetWebAppsAsync(
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
        var subscription = "sub123";
        var resourceGroup = "rg1";
        var appName = "test-app";

        _appServiceService.GetWebAppsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        var args = _commandDefinition.Parse([
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--app", appName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
    }
}
