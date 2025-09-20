// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Deploy.Commands.App;
using Azure.Mcp.Tools.Deploy.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Deploy.UnitTests.Commands.App;


public class LogsGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LogsGetCommand> _logger;
    private readonly IDeployService _deployService;
    private readonly Command _commandDefinition;
    private readonly CommandContext _context;
    private readonly LogsGetCommand _command;

    public LogsGetCommandTests()
    {
        _logger = Substitute.For<ILogger<LogsGetCommand>>();
        _deployService = Substitute.For<IDeployService>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_deployService);
        _serviceProvider = collection.BuildServiceProvider();
        _context = new(_serviceProvider);
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task Should_get_azd_app_logs()
    {
        // arrange
        var expectedLogs = "App logs retrieved:\n[2024-01-01 10:00:00] Application started\n[2024-01-01 10:01:00] Processing request";
        _deployService.GetAzdResourceLogsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>())
            .Returns(expectedLogs);

        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription-id",
            "--workspace-folder", "C:/Users/",
            "--azd-env-name", "dotnet-demo",
            "--limit", "10"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.StartsWith("App logs retrieved:", result.Message);
        Assert.Contains("Application started", result.Message);

    }

    [Fact]
    public async Task Should_get_azd_app_logs_with_default_limit()
    {
        // arrange
        var expectedLogs = "App logs retrieved:\nSample log entry";
        _deployService.GetAzdResourceLogsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>())
            .Returns(expectedLogs);

        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription-id",
            "--workspace-folder", "C:/project",
            "--azd-env-name", "my-env"
            // No limit specified - should use default
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.StartsWith("App logs retrieved:", result.Message);
    }

    [Fact]
    public async Task Should_handle_no_logs_found()
    {
        // arrange
        _deployService.GetAzdResourceLogsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>())
            .Returns("No logs found.");

        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription-id",
            "--workspace-folder", "C:/empty-project",
            "--azd-env-name", "empty-env",
            "--limit", "50"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.Equal("No logs found.", result.Message);
    }

    [Fact]
    public async Task Should_handle_error_during_log_retrieval()
    {
        // arrange
        var errorMessage = "Error during retrieval of app logs of azd project:\nNo resource group with tag {\"azd-env-name\": test-env} found.";
        _deployService.GetAzdResourceLogsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>())
            .Returns(errorMessage);

        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription-id",
            "--workspace-folder", "C:/invalid-project",
            "--azd-env-name", "test-env"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.StartsWith("Error during retrieval of app logs", result.Message);
        Assert.Contains("test-env", result.Message);
    }

    [Fact]
    public async Task Should_handle_service_exception()
    {
        // arrange
        _deployService.GetAzdResourceLogsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>())
            .ThrowsAsync(new InvalidOperationException("Failed to connect to Azure"));

        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription-id",
            "--workspace-folder", "C:/project",
            "--azd-env-name", "test-env"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

        // assert
        Assert.NotNull(result);
        Assert.NotEqual(HttpStatusCode.OK, result.Status); // Should be an error status
        Assert.NotNull(result.Message);
        Assert.Contains("Failed to connect to Azure", result.Message);
    }

    [Fact]
    public async Task Should_validate_required_parameters()
    {
        // arrange - missing required workspace-folder parameter
        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription-id",
            "--azd-env-name", "test-env"
            // Missing workspace-folder
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

        // assert
        Assert.NotNull(result);
        Assert.NotEqual(HttpStatusCode.OK, result.Status); // Should fail validation
    }


}
