// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Backup;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.Backup;

public class BackupStatusCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAzureBackupService _backupService;
    private readonly ILogger<BackupStatusCommand> _logger;
    private readonly BackupStatusCommand _command;
    private readonly CommandContext _context;
    private readonly System.CommandLine.Command _commandDefinition;

    public BackupStatusCommandTests()
    {
        _backupService = Substitute.For<IAzureBackupService>();
        _logger = Substitute.For<ILogger<BackupStatusCommand>>();

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
        Assert.Equal("status", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsProtectedStatus_WhenResourceIsProtected()
    {
        // Arrange
        var expected = new BackupStatusResult(
            "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1",
            "Protected",
            "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.RecoveryServices/vaults/vault1",
            "DefaultPolicy",
            null,
            null,
            null);

        _backupService.GetBackupStatusAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var args = _commandDefinition.Parse(["--subscription", "sub1", "--datasource-id", "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1", "--location", "eastus"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.BackupStatusCommandResult);

        Assert.NotNull(result);
        Assert.Equal("Protected", result.Status.ProtectionStatus);
        Assert.NotNull(result.Status.VaultId);
        Assert.Equal("DefaultPolicy", result.Status.PolicyName);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNotProtectedStatus_WhenResourceIsNotProtected()
    {
        // Arrange
        var expected = new BackupStatusResult(
            "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm2",
            "NotProtected",
            null,
            null,
            null,
            null,
            null);

        _backupService.GetBackupStatusAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var args = _commandDefinition.Parse(["--subscription", "sub1", "--datasource-id", "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm2", "--location", "eastus"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.BackupStatusCommandResult);

        Assert.NotNull(result);
        Assert.Equal("NotProtected", result.Status.ProtectionStatus);
        Assert.Null(result.Status.VaultId);
        Assert.Null(result.Status.PolicyName);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesGenericException()
    {
        // Arrange
        _backupService.GetBackupStatusAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var args = _commandDefinition.Parse(["--subscription", "sub1", "--datasource-id", "ds1", "--location", "eastus"]);

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
        _backupService.GetBackupStatusAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));

        var args = _commandDefinition.Parse(["--subscription", "sub1", "--datasource-id", "ds1", "--location", "eastus"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Resource not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbiddenError()
    {
        // Arrange
        _backupService.GetBackupStatusAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));

        var args = _commandDefinition.Parse(["--subscription", "sub1", "--datasource-id", "ds1", "--location", "eastus"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Theory]
    [InlineData("--subscription sub1 --datasource-id ds1 --location eastus", true)]
    [InlineData("--subscription sub1", false)] // Missing datasource-id and location
    [InlineData("--subscription sub1 --datasource-id ds1", false)] // Missing location
    [InlineData("--subscription sub1 --location eastus", false)] // Missing datasource-id
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _backupService.GetBackupStatusAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new BackupStatusResult("ds1", "Protected", "v1", "pol", null, null, null)));
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
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expected = new BackupStatusResult(
            "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1",
            "Protected",
            "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.RecoveryServices/vaults/vault1",
            "DefaultPolicy",
            DateTimeOffset.UtcNow.AddHours(-1),
            "Completed",
            "Healthy");

        _backupService.GetBackupStatusAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var args = _commandDefinition.Parse(["--subscription", "sub1", "--datasource-id", "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1", "--location", "eastus"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.BackupStatusCommandResult);

        Assert.NotNull(result);
        Assert.Equal("Protected", result.Status.ProtectionStatus);
        Assert.Equal("DefaultPolicy", result.Status.PolicyName);
        Assert.Equal("Completed", result.Status.LastBackupStatus);
        Assert.Equal("Healthy", result.Status.HealthStatus);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var args = _commandDefinition.Parse(["--subscription", "sub1", "--datasource-id", "/subscriptions/test/vm1", "--location", "westus2"]);

        // Act - use reflection or test via ExecuteAsync
        // The binding is tested implicitly through ExecuteAsync tests above
        // Verify the command has the right options registered
        var command = _command.GetCommand();
        var options = command.Options;

        Assert.Contains(options, o => o.Name == "--datasource-id");
        Assert.Contains(options, o => o.Name == "--location");
        Assert.Contains(options, o => o.Name == "--subscription");
    }

    [Fact]
    public async Task ExecuteAsync_PassesCorrectParametersToService()
    {
        // Arrange
        var expectedDatasourceId = "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1";
        var expectedSubscription = "sub1";
        var expectedLocation = "eastus";

        _backupService.GetBackupStatusAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new BackupStatusResult(expectedDatasourceId, "Protected", null, null, null, null, null)));

        var args = _commandDefinition.Parse(["--subscription", expectedSubscription, "--datasource-id", expectedDatasourceId, "--location", expectedLocation]);

        // Act
        await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        await _backupService.Received(1).GetBackupStatusAsync(
            expectedDatasourceId,
            expectedSubscription,
            expectedLocation,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
