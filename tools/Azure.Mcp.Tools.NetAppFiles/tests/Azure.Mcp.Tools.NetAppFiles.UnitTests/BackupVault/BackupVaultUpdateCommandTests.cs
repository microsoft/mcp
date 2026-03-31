// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.BackupVault;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.BackupVault;

public class BackupVaultUpdateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INetAppFilesService _netAppFilesService;
    private readonly ILogger<BackupVaultUpdateCommand> _logger;
    private readonly BackupVaultUpdateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public BackupVaultUpdateCommandTests()
    {
        _netAppFilesService = Substitute.For<INetAppFilesService>();
        _logger = Substitute.For<ILogger<BackupVaultUpdateCommand>>();

        var collection = new ServiceCollection().AddSingleton(_netAppFilesService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("update", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--account myanfaccount --backupVault myvault --resource-group myrg --location eastus --subscription sub123", true)]
    [InlineData("--backupVault myvault --resource-group myrg --location eastus --subscription sub123", false)] // Missing account
    [InlineData("--account myanfaccount --resource-group myrg --location eastus --subscription sub123", false)] // Missing backupVault
    [InlineData("--account myanfaccount --backupVault myvault --location eastus --subscription sub123", false)] // Missing resource-group
    [InlineData("--account myanfaccount --backupVault myvault --resource-group myrg --subscription sub123", false)] // Missing location
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedVault = new BackupVaultCreateResult(
                Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/backupVaults/myvault",
                Name: "myanfaccount/myvault",
                Type: "Microsoft.NetApp/netAppAccounts/backupVaults",
                Location: "eastus",
                ResourceGroup: "myrg",
                ProvisioningState: "Succeeded");

            _netAppFilesService.UpdateBackupVault(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Dictionary<string, string>>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedVault);
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
    public async Task ExecuteAsync_UpdatesBackupVault_Successfully()
    {
        // Arrange
        var account = "myanfaccount";
        var backupVault = "myvault";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";

        var expectedVault = new BackupVaultCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/backupVaults/{backupVault}",
            Name: $"{account}/{backupVault}",
            Type: "Microsoft.NetApp/netAppAccounts/backupVaults",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded");

        _netAppFilesService.UpdateBackupVault(
            Arg.Is(account), Arg.Is(backupVault), Arg.Is(resourceGroup), Arg.Is(location), Arg.Is(subscription),
            Arg.Any<Dictionary<string, string>>(), Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVault));

        var args = _commandDefinition.Parse([
            "--account", account, "--backupVault", backupVault,
            "--resource-group", resourceGroup, "--location", location,
            "--subscription", subscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.BackupVaultUpdateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.BackupVault);
        Assert.Equal($"{account}/{backupVault}", result.BackupVault.Name);
        Assert.Equal(location, result.BackupVault.Location);
        Assert.Equal(resourceGroup, result.BackupVault.ResourceGroup);
        Assert.Equal("Succeeded", result.BackupVault.ProvisioningState);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesBackupVaultWithTags_Successfully()
    {
        // Arrange
        var account = "myanfaccount";
        var backupVault = "myvault";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";
        var tagsJson = "{\"env\":\"prod\",\"team\":\"storage\"}";

        var expectedVault = new BackupVaultCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/backupVaults/{backupVault}",
            Name: $"{account}/{backupVault}",
            Type: "Microsoft.NetApp/netAppAccounts/backupVaults",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded");

        _netAppFilesService.UpdateBackupVault(
            Arg.Is(account), Arg.Is(backupVault), Arg.Is(resourceGroup), Arg.Is(location), Arg.Is(subscription),
            Arg.Is<Dictionary<string, string>>(d => d.ContainsKey("env") && d["env"] == "prod"),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVault));

        var args = _commandDefinition.Parse([
            "--account", account, "--backupVault", backupVault,
            "--resource-group", resourceGroup, "--location", location,
            "--subscription", subscription, "--tags", tagsJson
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        _netAppFilesService.UpdateBackupVault(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--backupVault", "myvault",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains(expectedError, response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflict()
    {
        // Arrange
        _netAppFilesService.UpdateBackupVault(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Conflict, "Backup vault already exists"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--backupVault", "myvault",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("already exists", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        _netAppFilesService.UpdateBackupVault(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Backup vault not found"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--backupVault", "myvault",
            "--resource-group", "nonexistentrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        _netAppFilesService.UpdateBackupVault(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--backupVault", "myvault",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _netAppFilesService.UpdateBackupVault(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<BackupVaultCreateResult>(new Exception("Test error")));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--backupVault", "myvault",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expectedVault = new BackupVaultCreateResult(
            Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/backupVaults/myvault",
            Name: "myanfaccount/myvault",
            Type: "Microsoft.NetApp/netAppAccounts/backupVaults",
            Location: "westus2",
            ResourceGroup: "myrg",
            ProvisioningState: "Succeeded");

        _netAppFilesService.UpdateBackupVault(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVault));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--backupVault", "myvault",
            "--resource-group", "myrg", "--location", "westus2",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.BackupVaultUpdateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.BackupVault);
        Assert.Equal("myanfaccount/myvault", result.BackupVault.Name);
        Assert.Equal("westus2", result.BackupVault.Location);
        Assert.Equal("myrg", result.BackupVault.ResourceGroup);
        Assert.Equal("Succeeded", result.BackupVault.ProvisioningState);
        Assert.Equal("Microsoft.NetApp/netAppAccounts/backupVaults", result.BackupVault.Type);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var account = "myanfaccount";
        var backupVault = "myvault";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";

        var expectedVault = new BackupVaultCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/backupVaults/{backupVault}",
            Name: $"{account}/{backupVault}",
            Type: "Microsoft.NetApp/netAppAccounts/backupVaults",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded");

        _netAppFilesService.UpdateBackupVault(
            account, backupVault, resourceGroup, location, subscription,
            null, null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedVault);

        var args = _commandDefinition.Parse([
            "--account", account, "--backupVault", backupVault,
            "--resource-group", resourceGroup, "--location", location,
            "--subscription", subscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _netAppFilesService.Received(1).UpdateBackupVault(
            account, backupVault, resourceGroup, location, subscription,
            null, null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesInvalidTagsJson()
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--backupVault", "myvault",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123", "--tags", "invalid-json"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid tags JSON format", response.Message);
    }
}
