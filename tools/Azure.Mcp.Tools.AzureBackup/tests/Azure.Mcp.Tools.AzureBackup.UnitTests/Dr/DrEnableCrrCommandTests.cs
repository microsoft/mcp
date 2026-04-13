// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Dr;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.Dr;

public class DrEnableCrrCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAzureBackupService _backupService;
    private readonly ILogger<DrEnableCrrCommand> _logger;
    private readonly DrEnableCrrCommand _command;
    private readonly CommandContext _context;
    private readonly System.CommandLine.Command _commandDefinition;

    public DrEnableCrrCommandTests()
    {
        _backupService = Substitute.For<IAzureBackupService>();
        _logger = Substitute.For<ILogger<DrEnableCrrCommand>>();

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
        Assert.Equal("enablecrr", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_EnablesCrr_Successfully()
    {
        // Arrange
        var expected = new OperationResult("Succeeded", null, "Cross-region restore enabled");

        _backupService.ConfigureCrossRegionRestoreAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.DrEnableCrrCommandResult);

        Assert.NotNull(result);
        Assert.Equal("Succeeded", result.Result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        _backupService.ConfigureCrossRegionRestoreAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFoundError()
    {
        // Arrange
        _backupService.ConfigureCrossRegionRestoreAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Vault not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesBadRequestForNonGrsVault()
    {
        // Arrange
        _backupService.ConfigureCrossRegionRestoreAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(400, "CRR not supported"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Bad request enabling Cross-Region Restore", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflictWhenAlreadyEnabled()
    {
        // Arrange
        _backupService.ConfigureCrossRegionRestoreAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(409, "Already enabled"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("already enabled", response.Message);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("RSV-type")]
    [InlineData("backup")]
    [InlineData("recovery")]
    public async Task ExecuteAsync_RejectsInvalidVaultType(string vaultType)
    {
        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg", "--vault-type", vaultType]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--vault-type must be", response.Message);
    }

    [Theory]
    [InlineData("rsv")]
    [InlineData("dpp")]
    [InlineData("RSV")]
    [InlineData("DPP")]
    public async Task ExecuteAsync_AcceptsValidVaultType(string vaultType)
    {
        _backupService.ConfigureCrossRegionRestoreAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new OperationResult("Succeeded", null, null)));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg", "--vault-type", vaultType]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg", true)]
    [InlineData("--subscription sub", false)] // Missing vault and resource-group
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _backupService.ConfigureCrossRegionRestoreAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
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
}
