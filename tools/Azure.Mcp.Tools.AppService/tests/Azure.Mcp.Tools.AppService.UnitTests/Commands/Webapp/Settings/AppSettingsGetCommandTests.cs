// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.AppService.Commands;
using Azure.Mcp.Tools.AppService.Commands.Webapp.Settings;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppService.UnitTests.Commands.Webapp.Settings;

[Trait("Command", "AppSettingsGet")]
public class AppSettingsGetCommandTests
{
    private readonly IAppServiceService _appServiceService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AppSettingsGetCommand> _logger;
    private readonly AppSettingsGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public AppSettingsGetCommandTests()
    {
        _appServiceService = Substitute.For<IAppServiceService>();
        _logger = Substitute.For<ILogger<AppSettingsGetCommand>>();

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
        IDictionary<string, string> expectedSettings = new Dictionary<string, string>()
        {
            {"Setting1", "Value1"},
            {"Setting2", "Value2"}
        };

        // Set up the mock to return success for any arguments
        _appServiceService.GetAppSettingsAsync("sub123", "rg1", "test-app", Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedSettings);

        var args = _commandDefinition.Parse(["--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        // Verify that the mock was called with the expected parameters
        await _appServiceService.Received(1).GetAppSettingsAsync("sub123", "rg1", "test-app", Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AppServiceJsonContext.Default.AppSettingsGetResult);

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
        // Arrange
        var args = _commandDefinition.Parse(commandArgs);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);

        await _appServiceService.DidNotReceive().GetAppSettingsAsync(
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
        _appServiceService.GetAppSettingsAsync("sub123", "rg1", "test-app", Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        var args = _commandDefinition.Parse(["--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);

        await _appServiceService.Received(1).GetAppSettingsAsync("sub123", "rg1", "test-app", Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }
}
