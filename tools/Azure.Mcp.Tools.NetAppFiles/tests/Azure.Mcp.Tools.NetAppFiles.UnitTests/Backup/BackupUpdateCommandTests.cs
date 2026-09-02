// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Microsoft.Mcp.Core.Options;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.Backup;
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

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.Backup;

public class BackupUpdateCommandTests : SubscriptionCommandUnitTestsBase<BackupUpdateCommand, INetAppFilesService>
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
    [InlineData("--account myanfaccount --backup-vault myvault --backup mybackup --resource-group myrg --location eastus --subscription sub123", true)]
    [InlineData("--backup-vault myvault --backup mybackup --resource-group myrg --location eastus --subscription sub123", false)] // Missing account
    [InlineData("--account myanfaccount --backup mybackup --resource-group myrg --location eastus --subscription sub123", false)] // Missing backupVault
    [InlineData("--account myanfaccount --backup-vault myvault --resource-group myrg --location eastus --subscription sub123", false)] // Missing backup
    [InlineData("--account myanfaccount --backup-vault myvault --backup mybackup --location eastus --subscription sub123", false)] // Missing resource-group
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedBackup = new BackupCreateResult(
                Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/backupVaults/myvault/backups/mybackup",
                Name: "myanfaccount/myvault/mybackup",
                Type: "Microsoft.NetApp/netAppAccounts/backupVaults/backups",
                Location: "eastus",
                ResourceGroup: "myrg",
                ProvisioningState: "Succeeded",
                VolumeResourceId: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvolume",
                Label: null,
                BackupType: "Manual",
                Size: 0);

            Service.UpdateBackup(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedBackup);
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
    public async Task ExecuteAsync_UpdatesBackup_Successfully()
    {
        // Arrange
        var account = "myanfaccount";
        var backupVault = "myvault";
        var backup = "mybackup";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";
        var label = "updated-label";

        var expectedBackup = new BackupCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/backupVaults/{backupVault}/backups/{backup}",
            Name: $"{account}/{backupVault}/{backup}",
            Type: "Microsoft.NetApp/netAppAccounts/backupVaults/backups",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            VolumeResourceId: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvolume",
            Label: label,
            BackupType: "Manual",
            Size: 107374182400);

        Service.UpdateBackup(
            Arg.Is(account), Arg.Is(backupVault), Arg.Is(backup), Arg.Is(resourceGroup), Arg.Is(location), Arg.Is(subscription),
            Arg.Is(label),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedBackup));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", account, "--backup-vault", backupVault,
            "--backup", backup,
            "--resource-group", resourceGroup, "--location", location,
            "--subscription", subscription, "--label", label
        ]);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.BackupUpdateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Backup);
        Assert.Equal($"{account}/{backupVault}/{backup}", result.Backup.Name);
        Assert.Equal(location, result.Backup.Location);
        Assert.Equal(resourceGroup, result.Backup.ResourceGroup);
        Assert.Equal("Succeeded", result.Backup.ProvisioningState);
        Assert.Equal(label, result.Backup.Label);
        Assert.Equal("Manual", result.Backup.BackupType);
        Assert.Equal(107374182400, result.Backup.Size);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesBackup_WithoutOptionalParameters()
    {
        // Arrange
        var account = "myanfaccount";
        var backupVault = "myvault";
        var backup = "mybackup";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";

        var expectedBackup = new BackupCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/backupVaults/{backupVault}/backups/{backup}",
            Name: $"{account}/{backupVault}/{backup}",
            Type: "Microsoft.NetApp/netAppAccounts/backupVaults/backups",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            VolumeResourceId: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvolume",
            Label: null,
            BackupType: "Manual",
            Size: 0);

        Service.UpdateBackup(
            Arg.Is(account), Arg.Is(backupVault), Arg.Is(backup), Arg.Is(resourceGroup), Arg.Is(location), Arg.Is(subscription),
            null,
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedBackup));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", account, "--backup-vault", backupVault,
            "--backup", backup,
            "--resource-group", resourceGroup, "--location", location,
            "--subscription", subscription
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

        Service.UpdateBackup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-vault", "myvault",
            "--backup", "mybackup",
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
        Service.UpdateBackup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Conflict, "Backup already exists"));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-vault", "myvault",
            "--backup", "mybackup",
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
        Service.UpdateBackup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Backup not found"));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-vault", "myvault",
            "--backup", "mybackup",
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
        Service.UpdateBackup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-vault", "myvault",
            "--backup", "mybackup",
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
        Service.UpdateBackup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<BackupCreateResult>(new Exception("Test error")));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-vault", "myvault",
            "--backup", "mybackup",
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
        var volumeResourceId = "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvolume";

        var expectedBackup = new BackupCreateResult(
            Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/backupVaults/myvault/backups/mybackup",
            Name: "myanfaccount/myvault/mybackup",
            Type: "Microsoft.NetApp/netAppAccounts/backupVaults/backups",
            Location: "westus2",
            ResourceGroup: "myrg",
            ProvisioningState: "Succeeded",
            VolumeResourceId: volumeResourceId,
            Label: "test-label",
            BackupType: "Manual",
            Size: 107374182400);

        Service.UpdateBackup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedBackup));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-vault", "myvault",
            "--backup", "mybackup",
            "--resource-group", "myrg", "--location", "westus2",
            "--subscription", "sub123"
        ]);

        // Assert
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.BackupUpdateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Backup);
        Assert.Equal("myanfaccount/myvault/mybackup", result.Backup.Name);
        Assert.Equal("westus2", result.Backup.Location);
        Assert.Equal("myrg", result.Backup.ResourceGroup);
        Assert.Equal("Succeeded", result.Backup.ProvisioningState);
        Assert.Equal("Microsoft.NetApp/netAppAccounts/backupVaults/backups", result.Backup.Type);
        Assert.Equal(volumeResourceId, result.Backup.VolumeResourceId);
        Assert.Equal("test-label", result.Backup.Label);
        Assert.Equal("Manual", result.Backup.BackupType);
        Assert.Equal(107374182400, result.Backup.Size);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var account = "myanfaccount";
        var backupVault = "myvault";
        var backup = "mybackup";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";
        var label = "my-label";

        var expectedBackup = new BackupCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/backupVaults/{backupVault}/backups/{backup}",
            Name: $"{account}/{backupVault}/{backup}",
            Type: "Microsoft.NetApp/netAppAccounts/backupVaults/backups",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            VolumeResourceId: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvolume",
            Label: label,
            BackupType: "Manual",
            Size: 0);

        Service.UpdateBackup(
            account, backupVault, backup, resourceGroup, location, subscription,
            label,
            null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedBackup);

        // Act
        var response = await ExecuteCommandAsync([
            "--account", account, "--backup-vault", backupVault,
            "--backup", backup,
            "--resource-group", resourceGroup, "--location", location,
            "--subscription", subscription, "--label", label
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateBackup(
            account, backupVault, backup, resourceGroup, location, subscription,
            label,
            null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }
}
