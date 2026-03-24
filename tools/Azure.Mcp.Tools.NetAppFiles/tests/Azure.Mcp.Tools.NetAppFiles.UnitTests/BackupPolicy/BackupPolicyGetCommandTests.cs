// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.BackupPolicy;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.BackupPolicy;

public class BackupPolicyGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INetAppFilesService _netAppFilesService;
    private readonly ILogger<BackupPolicyGetCommand> _logger;
    private readonly BackupPolicyGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public BackupPolicyGetCommandTests()
    {
        _netAppFilesService = Substitute.For<INetAppFilesService>();
        _logger = Substitute.For<ILogger<BackupPolicyGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_netAppFilesService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_NoBackupPolicyParameter_ReturnsAllBackupPolicies()
    {
        // Arrange
        var subscription = "sub123";
        var expectedPolicies = new ResourceQueryResults<BackupPolicyInfo>(
        [
            new("account1/policy1", "eastus", "rg1", "Succeeded", 5, 4, 12, 3, true),
            new("account1/policy2", "westus", "rg2", "Succeeded", 7, 4, 6, 1, false)
        ], false);

        _netAppFilesService.GetBackupPolicyDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPolicies));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.BackupPolicyGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.BackupPolicies);
        Assert.Equal(expectedPolicies.Results.Count, result.BackupPolicies.Count);
        Assert.Equal(expectedPolicies.Results.Select(p => p.Name), result.BackupPolicies.Select(p => p.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoBackupPolicies()
    {
        // Arrange
        var subscription = "sub123";

        _netAppFilesService.GetBackupPolicyDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<BackupPolicyInfo>([], false));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.BackupPolicyGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.BackupPolicies);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscription = "sub123";

        _netAppFilesService.GetBackupPolicyDetails(
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
    [InlineData("--subscription sub123 --account myanfaccount --backupPolicy mypolicy", true)]
    [InlineData("--account myanfaccount", false)] // Missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedPolicies = new ResourceQueryResults<BackupPolicyInfo>(
                [new("account1/policy1", "eastus", "rg1", "Succeeded", 5, 4, 12, 3, true)],
                false);

            _netAppFilesService.GetBackupPolicyDetails(
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(expectedPolicies));
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
    public async Task ExecuteAsync_ReturnsBackupPolicyDetails_WhenPolicyExists()
    {
        // Arrange
        var account = "myanfaccount";
        var backupPolicy = "mypolicy";
        var subscription = "sub123";
        var expectedPolicies = new ResourceQueryResults<BackupPolicyInfo>(
            [new($"{account}/{backupPolicy}", "eastus", "rg1", "Succeeded", 5, 4, 12, 3, true)],
            false);

        _netAppFilesService.GetBackupPolicyDetails(
            Arg.Is(account), Arg.Is(backupPolicy), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPolicies));

        var args = _commandDefinition.Parse(["--account", account, "--backupPolicy", backupPolicy, "--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.BackupPolicyGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.BackupPolicies);
        Assert.Equal($"{account}/{backupPolicy}", result.BackupPolicies[0].Name);
        Assert.Equal("eastus", result.BackupPolicies[0].Location);
        Assert.Equal("rg1", result.BackupPolicies[0].ResourceGroup);
        Assert.True(result.BackupPolicies[0].Enabled);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var subscription = "sub123";

        _netAppFilesService.GetBackupPolicyDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
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
        var backupPolicy = "nonexistentpolicy";

        _netAppFilesService.GetBackupPolicyDetails(
            Arg.Any<string?>(), Arg.Is(backupPolicy), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Backup policy not found"));

        var parseResult = _commandDefinition.Parse(["--backupPolicy", backupPolicy, "--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Backup policy not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        var subscription = "sub123";

        _netAppFilesService.GetBackupPolicyDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
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
        var expectedPolicies = new ResourceQueryResults<BackupPolicyInfo>(
            [new("account1/policy1", "eastus", "rg1", "Succeeded", 5, 4, 12, 3, true)],
            false);

        _netAppFilesService.GetBackupPolicyDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPolicies));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.BackupPolicyGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.BackupPolicies);
        var policyInfo = result.BackupPolicies[0];
        Assert.Equal("account1/policy1", policyInfo.Name);
        Assert.Equal("eastus", policyInfo.Location);
        Assert.Equal("rg1", policyInfo.ResourceGroup);
        Assert.Equal("Succeeded", policyInfo.ProvisioningState);
        Assert.Equal(5, policyInfo.DailyBackupsToKeep);
        Assert.Equal(4, policyInfo.WeeklyBackupsToKeep);
        Assert.Equal(12, policyInfo.MonthlyBackupsToKeep);
        Assert.Equal(3, policyInfo.VolumeBackupsCount);
        Assert.True(policyInfo.Enabled);
    }
}
