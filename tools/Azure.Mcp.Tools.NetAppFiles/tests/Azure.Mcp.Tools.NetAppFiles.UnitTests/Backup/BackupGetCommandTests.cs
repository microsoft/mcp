// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.Backup;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.Backup;

public class BackupGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INetAppFilesService _netAppFilesService;
    private readonly ILogger<BackupGetCommand> _logger;
    private readonly BackupGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public BackupGetCommandTests()
    {
        _netAppFilesService = Substitute.For<INetAppFilesService>();
        _logger = Substitute.For<ILogger<BackupGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_netAppFilesService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_NoBackupParameter_ReturnsAllBackups()
    {
        // Arrange
        var subscription = "sub123";
        var expectedBackups = new ResourceQueryResults<BackupInfo>(
        [
            new("account1/vault1/backup1", "eastus", "rg1", "Succeeded", "Manual", 1024, "label1", "2025-01-01T00:00:00Z"),
            new("account1/vault1/backup2", "westus", "rg2", "Succeeded", "Scheduled", 2048, "label2", "2025-01-02T00:00:00Z")
        ], false);

        _netAppFilesService.GetBackupDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedBackups));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.BackupGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Backups);
        Assert.Equal(expectedBackups.Results.Count, result.Backups.Count);
        Assert.Equal(expectedBackups.Results.Select(b => b.Name), result.Backups.Select(b => b.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoBackups()
    {
        // Arrange
        var subscription = "sub123";

        _netAppFilesService.GetBackupDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<BackupInfo>([], false));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.BackupGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Backups);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscription = "sub123";

        _netAppFilesService.GetBackupDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(subscription),
            null,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --account myanfaccount", true)]
    [InlineData("--subscription sub123 --account myanfaccount --backupVault myvault", true)]
    [InlineData("--subscription sub123 --account myanfaccount --backupVault myvault --backup mybackup", true)]
    [InlineData("--account myanfaccount", false)] // Missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedBackups = new ResourceQueryResults<BackupInfo>(
                [new("account1/vault1/backup1", "eastus", "rg1", "Succeeded", "Manual", 1024, "label1", "2025-01-01T00:00:00Z")],
                false);

            _netAppFilesService.GetBackupDetails(
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(expectedBackups));
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBackupDetails_WhenBackupExists()
    {
        // Arrange
        var account = "myanfaccount";
        var backupVault = "myvault";
        var backup = "mybackup";
        var subscription = "sub123";
        var expectedBackups = new ResourceQueryResults<BackupInfo>(
            [new($"{account}/{backupVault}/{backup}", "eastus", "rg1", "Succeeded", "Manual", 1024, "testlabel", "2025-01-01T00:00:00Z")],
            false);

        _netAppFilesService.GetBackupDetails(
            Arg.Is(account), Arg.Is(backupVault), Arg.Is(backup), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedBackups));

        var args = _commandDefinition.Parse(["--account", account, "--backupVault", backupVault, "--backup", backup, "--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.BackupGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Backups);
        Assert.Equal($"{account}/{backupVault}/{backup}", result.Backups[0].Name);
        Assert.Equal("eastus", result.Backups[0].Location);
        Assert.Equal("rg1", result.Backups[0].ResourceGroup);
        Assert.Equal("Succeeded", result.Backups[0].ProvisioningState);
        Assert.Equal("Manual", result.Backups[0].BackupType);
        Assert.Equal(1024, result.Backups[0].Size);
        Assert.Equal("testlabel", result.Backups[0].Label);
        Assert.Equal("2025-01-01T00:00:00Z", result.Backups[0].CreationDate);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var subscription = "sub123";

        _netAppFilesService.GetBackupDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var parseResult = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        var subscription = "sub123";
        var backup = "nonexistentbackup";

        _netAppFilesService.GetBackupDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(backup), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Backup not found"));

        var parseResult = _commandDefinition.Parse(["--backup", backup, "--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Backup not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        var subscription = "sub123";

        _netAppFilesService.GetBackupDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var parseResult = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var subscription = "sub123";
        var expectedBackups = new ResourceQueryResults<BackupInfo>(
            [new("account1/vault1/backup1", "eastus", "rg1", "Succeeded", "Manual", 1024, "label1", "2025-01-01T00:00:00Z")],
            false);

        _netAppFilesService.GetBackupDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedBackups));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.BackupGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Backups);
        var backupInfo = result.Backups[0];
        Assert.Equal("account1/vault1/backup1", backupInfo.Name);
        Assert.Equal("eastus", backupInfo.Location);
        Assert.Equal("rg1", backupInfo.ResourceGroup);
        Assert.Equal("Succeeded", backupInfo.ProvisioningState);
        Assert.Equal("Manual", backupInfo.BackupType);
        Assert.Equal(1024, backupInfo.Size);
        Assert.Equal("label1", backupInfo.Label);
        Assert.Equal("2025-01-01T00:00:00Z", backupInfo.CreationDate);
    }
}
