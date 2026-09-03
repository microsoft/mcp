// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Microsoft.Mcp.Core.Options;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.BackupVault;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tests.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.BackupVault;

public class BackupVaultUpdateCommandTests : SubscriptionCommandUnitTestsBase<BackupVaultUpdateCommand, INetAppFilesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("update", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--account myanfaccount --backup-vault myvault --resource-group myrg --location eastus --subscription sub123", true)]
    [InlineData("--backup-vault myvault --resource-group myrg --location eastus --subscription sub123", false)] // Missing account
    [InlineData("--account myanfaccount --resource-group myrg --location eastus --subscription sub123", false)] // Missing backupVault
    [InlineData("--account myanfaccount --backup-vault myvault --location eastus --subscription sub123", false)] // Missing resource-group
    [InlineData("--account myanfaccount --backup-vault myvault --resource-group myrg --subscription sub123", false)] // Missing location
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

            Service.UpdateBackupVault(
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

        // Act
        var response = await ExecuteCommandAsync(args);

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

        Service.UpdateBackupVault(
            Arg.Is(account), Arg.Is(backupVault), Arg.Is(resourceGroup), Arg.Is(location), Arg.Is(subscription),
            Arg.Any<Dictionary<string, string>>(), Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVault));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", account, "--backup-vault", backupVault,
            "--resource-group", resourceGroup, "--location", location,
            "--subscription", subscription
        ]);

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

        Service.UpdateBackupVault(
            Arg.Is(account), Arg.Is(backupVault), Arg.Is(resourceGroup), Arg.Is(location), Arg.Is(subscription),
            Arg.Is<Dictionary<string, string>>(d => d.ContainsKey("env") && d["env"] == "prod"),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVault));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", account, "--backup-vault", backupVault,
            "--resource-group", resourceGroup, "--location", location,
            "--subscription", subscription, "--tags", tagsJson
        ]);

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

        Service.UpdateBackupVault(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-vault", "myvault",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

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
        Service.UpdateBackupVault(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Conflict, "Backup vault already exists"));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-vault", "myvault",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("already exists", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        Service.UpdateBackupVault(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Backup vault not found"));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-vault", "myvault",
            "--resource-group", "nonexistentrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        Service.UpdateBackupVault(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-vault", "myvault",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.UpdateBackupVault(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<BackupVaultCreateResult>(new Exception("Test error")));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-vault", "myvault",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123"
        ]);

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

        Service.UpdateBackupVault(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Dictionary<string, string>>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVault));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-vault", "myvault",
            "--resource-group", "myrg", "--location", "westus2",
            "--subscription", "sub123"
        ]);

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

        Service.UpdateBackupVault(
            account, backupVault, resourceGroup, location, subscription,
            null, null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedVault);

        // Act
        var response = await ExecuteCommandAsync([
            "--account", account, "--backup-vault", backupVault,
            "--resource-group", resourceGroup, "--location", location,
            "--subscription", subscription
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateBackupVault(
            account, backupVault, resourceGroup, location, subscription,
            null, null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesInvalidTagsJson()
    {
        // Arrange
        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-vault", "myvault",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123", "--tags", "invalid-json"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid tags JSON format", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsNoWaitArgument()
    {
        // Arrange
        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-vault", "myvault",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123", "--no-wait"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--no-wait", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsAcquirePolicyTokenArgument()
    {
        // Arrange
        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-vault", "myvault",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123", "--acquirePolicyToken"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--acquirePolicyToken", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsChangeReferenceArgument()
    {
        // Arrange
        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-vault", "myvault",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123", "--changeReference", "chg-123"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--changeReference", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsIdsArgument()
    {
        // Arrange
        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-vault", "myvault",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123",
            "--ids", "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/backupVaults/myvault"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--ids", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsAddArgument()
    {
        // Arrange
        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-vault", "myvault",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123", "--add", "properties.foo=bar"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--add", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsSetArgument()
    {
        // Arrange
        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-vault", "myvault",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123", "--set", "properties.foo=bar"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--set", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsRemoveArgument()
    {
        // Arrange
        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-vault", "myvault",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123", "--remove", "properties.foo"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--remove", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsForceStringArgument()
    {
        // Arrange
        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-vault", "myvault",
            "--resource-group", "myrg", "--location", "eastus",
            "--subscription", "sub123", "--force-string"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--force-string", response.Message);
    }
}
