// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Governance;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
namespace Azure.Mcp.Tools.AzureBackup.UnitTests.Governance;

public class GovernanceSoftDeleteCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAzureBackupService _backupService;
    private readonly ILogger<GovernanceSoftDeleteCommand> _logger;
    private readonly GovernanceSoftDeleteCommand _command;
    private readonly CommandContext _context;
    private readonly System.CommandLine.Command _commandDefinition;

    public GovernanceSoftDeleteCommandTests()
    {
        _backupService = Substitute.For<IAzureBackupService>();
        _logger = Substitute.For<ILogger<GovernanceSoftDeleteCommand>>();

        var collection = new ServiceCollection().AddSingleton(_backupService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger, _backupService);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("soft-delete", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ConfiguresSoftDelete_Successfully()
    {
        // Arrange
        var expected = new OperationResult("Succeeded", null, "Soft delete configured");

        _backupService.ConfigureSoftDeleteAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--soft-delete", "AlwaysOn"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.GovernanceSoftDeleteCommandResult);

        Assert.NotNull(result);
        Assert.Equal("Succeeded", result.Result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        _backupService.ConfigureSoftDeleteAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--soft-delete", "On"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg --soft-delete On", true)]
    [InlineData("--subscription sub --vault v --resource-group rg", false)] // soft-delete is required
    [InlineData("--subscription sub", false)] // Missing vault and resource-group
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _backupService.ConfigureSoftDeleteAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new OperationResult("Succeeded", null, null)));
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFoundError()
    {
        // Arrange
        _backupService.ConfigureSoftDeleteAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--soft-delete", "On"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflictError()
    {
        // Arrange
        _backupService.ConfigureSoftDeleteAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(409, "Cannot change"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--soft-delete", "On"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("Cannot change", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbiddenError()
    {
        // Arrange
        _backupService.ConfigureSoftDeleteAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--soft-delete", "On"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Forbidden", response.Message);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("Enable")]
    [InlineData("Disable")]
    [InlineData("always")]
    public async Task ExecuteAsync_RejectsInvalidSoftDeleteState(string softDeleteState)
    {
        // Arrange
        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--soft-delete", softDeleteState]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--soft-delete must be", response.Message);
    }

    [Theory]
    [InlineData("0")]   // below 14
    [InlineData("13")]  // below 14
    [InlineData("181")] // above 180
    [InlineData("abc")] // non-numeric
    public async Task ExecuteAsync_RejectsInvalidRetentionDays(string retentionDays)
    {
        // Arrange
        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--soft-delete", "On", "--soft-delete-retention-days", retentionDays]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--soft-delete-retention-days", response.Message);
    }

    [Theory]
    [InlineData("14")]
    [InlineData("90")]
    [InlineData("180")]
    public async Task ExecuteAsync_AcceptsValidRetentionDays(string retentionDays)
    {
        // Arrange
        _backupService.ConfigureSoftDeleteAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new OperationResult("Succeeded", null, null)));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--soft-delete", "On", "--soft-delete-retention-days", retentionDays]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expected = new OperationResult("Succeeded", null, "Soft delete set to 'On'");

        _backupService.ConfigureSoftDeleteAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--soft-delete", "On", "--soft-delete-retention-days", "30"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.GovernanceSoftDeleteCommandResult);
        Assert.NotNull(result);
        Assert.Equal("Succeeded", result.Result.Status);
    }
}
