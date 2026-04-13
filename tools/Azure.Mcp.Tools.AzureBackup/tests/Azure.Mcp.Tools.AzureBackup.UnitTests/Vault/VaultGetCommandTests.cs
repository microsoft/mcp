// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Vault;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.Vault;

public class VaultGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAzureBackupService _backupService;
    private readonly ILogger<VaultGetCommand> _logger;
    private readonly VaultGetCommand _command;
    private readonly CommandContext _context;
    private readonly System.CommandLine.Command _commandDefinition;

    public VaultGetCommandTests()
    {
        _backupService = Substitute.For<IAzureBackupService>();
        _logger = Substitute.For<ILogger<VaultGetCommand>>();

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
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ListsVaults_WhenNoVaultSpecified()
    {
        // Arrange
        var subscription = "sub123";
        var expectedVaults = new List<BackupVaultInfo>
        {
            new("id1", "vault1", "rsv", "eastus", "rg1", "Succeeded", "Standard", "GeoRedundant", null),
            new("id2", "vault2", "dpp", "westus", "rg2", "Succeeded", "Standard", "LocallyRedundant", null)
        };

        _backupService.ListVaultsAsync(
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVaults));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.VaultGetCommandResult);

        Assert.NotNull(result);
        Assert.Equal(2, result.Vaults.Count);
        Assert.Equal("vault1", result.Vaults[0].Name);
        Assert.Equal("vault2", result.Vaults[1].Name);
    }

    [Fact]
    public async Task ExecuteAsync_GetsSingleVault_WhenVaultSpecified()
    {
        // Arrange
        var subscription = "sub123";
        var vaultName = "myVault";
        var resourceGroup = "myRg";
        var expectedVault = new BackupVaultInfo("id1", vaultName, "rsv", "eastus", resourceGroup, "Succeeded", "Standard", "GeoRedundant", null);

        _backupService.GetVaultAsync(
            Arg.Is(vaultName),
            Arg.Is(resourceGroup),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVault));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--vault", vaultName, "--resource-group", resourceGroup]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.VaultGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Vaults);
        Assert.Equal(vaultName, result.Vaults[0].Name);
        Assert.Equal("eastus", result.Vaults[0].Location);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoVaultsExist()
    {
        // Arrange
        var subscription = "sub123";

        _backupService.ListVaultsAsync(
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<BackupVaultInfo>()));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.VaultGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Vaults);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var subscription = "sub123";

        _backupService.ListVaultsAsync(
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        var subscription = "sub123";
        var vaultName = "nonexistent";
        var resourceGroup = "myRg";

        _backupService.GetVaultAsync(
            Arg.Is(vaultName),
            Arg.Is(resourceGroup),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Vault not found"));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--vault", vaultName, "--resource-group", resourceGroup]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("RSV-type")]
    [InlineData("backup")]
    [InlineData("recovery")]
    public async Task ExecuteAsync_RejectsInvalidVaultType(string vaultType)
    {
        var args = _commandDefinition.Parse(["--subscription", "sub123", "--vault-type", vaultType]);
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
        _backupService.ListVaultsAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<BackupVaultInfo>()));

        var args = _commandDefinition.Parse(["--subscription", "sub123", "--vault-type", vaultType]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --vault myVault --resource-group myRg", true)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _backupService.ListVaultsAsync(
                Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new List<BackupVaultInfo>()));

            _backupService.GetVaultAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new BackupVaultInfo("id1", "myVault", "rsv", "eastus", "myRg", "Succeeded", "Standard", "GeoRedundant", null)));
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        }
    }
}
