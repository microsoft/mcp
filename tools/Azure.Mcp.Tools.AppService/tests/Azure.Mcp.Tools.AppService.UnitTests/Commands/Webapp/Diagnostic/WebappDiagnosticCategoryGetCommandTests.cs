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

[Trait("Command", "WebappDiagnosticCategoryGet")]
public class WebappDiagnositcCategoryGetCommandTests
{
    private readonly IAppServiceService _appServiceService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WebappDiagnosticCategoryGetCommand> _logger;
    private readonly WebappDiagnosticCategoryGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public WebappDiagnositcCategoryGetCommandTests()
    {
        _appServiceService = Substitute.For<IAppServiceService>();
        _logger = Substitute.For<ILogger<WebappDiagnosticCategoryGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_appServiceService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("category1")]
    public async Task ExecuteAsync_WithValidParameters_CallsServiceWithCorrectArguments(string? diagnosticCategory)
    {
        // Arrange
        List<WebappDiagnosticCategoryDetails> expectedDiagnosticCategories = [new("name", "type", "kind", "description")];

        // Set up the mock to return success for any arguments
        _appServiceService.GetWebAppDiagnosticCategoriesAsync("sub123", "rg1", "test-app", diagnosticCategory,
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedDiagnosticCategories);

        List<string> unparsedArgs = ["--subscription", "sub123", "--resource-group", "rg1", "--app", "test-app"];
        if (!string.IsNullOrEmpty(diagnosticCategory))
        {
            unparsedArgs.AddRange(["--diagnostic-category", diagnosticCategory]);
        }

        var args = _commandDefinition.Parse(unparsedArgs);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        // Verify that the mock was called with the expected parameters
        await _appServiceService.Received(1).GetWebAppDiagnosticCategoriesAsync("sub123", "rg1", "test-app",
            diagnosticCategory, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AppServiceJsonContext.Default.WebappDiagnosticCategoryGetResult);

        Assert.NotNull(result);
        Assert.Equal(JsonSerializer.Serialize(expectedDiagnosticCategories), JsonSerializer.Serialize(result.WebappDiagnosticCategories));
    }

    [Theory]
    [InlineData("")] // Missing subscription, resource group, and app
    [InlineData("--subscription", "sub123")] // Missing resource group and app
    [InlineData("--subscription", "sub123", "--resource-group", "rg1")] // Missing app
    [InlineData("--resource-group", "rg1")] // Missing subscription and app
    [InlineData("--app", "test-app")] // Missing subscription and resource group
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

        await _appServiceService.DidNotReceive().GetWebAppDiagnosticCategoriesAsync(
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
        _appServiceService.GetWebAppDiagnosticCategoriesAsync(
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
            "--app", "test-app"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);

        await _appServiceService.Received(1).GetWebAppDiagnosticCategoriesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
