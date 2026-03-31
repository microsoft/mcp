// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
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

public class BackupCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INetAppFilesService _netAppFilesService;
    private readonly ILogger<BackupCreateCommand> _logger;
    private readonly BackupCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public BackupCreateCommandTests()
    {
        _netAppFilesService = Substitute.For<INetAppFilesService>();
        _logger = Substitute.For<ILogger<BackupCreateCommand>>();

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
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--account myanfaccount --backupVault myvault --backup mybackup --resource-group myrg --location eastus --volumeResourceId /subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvolume --subscription sub123", true)]
    [InlineData("--backupVault myvault --backup mybackup --resource-group myrg --location eastus --volumeResourceId /subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvolume --subscription sub123", false)] // Missing account
    [InlineData("--account myanfaccount --backup mybackup --resource-group myrg --location eastus --volumeResourceId /subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvolume --subscription sub123", false)] // Missing backupVault
    [InlineData("--account myanfaccount --backupVault myvault --resource-group myrg --location eastus --volumeResourceId /subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvolume --subscription sub123", false)] // Missing backup
    [InlineData("--account myanfaccount --backupVault myvault --backup mybackup --location eastus --volumeResourceId /subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvolume --subscription sub123", false)] // Missing resource-group
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

            _netAppFilesService.CreateBackup(
                Arg.Any<string>(),
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
    public async Task ExecuteAsync_CreatesBackup_Successfully()
    {
        // Arrange
        var account = "myanfaccount";
        var backupVault = "myvault";
        var backup = "mybackup";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";
        var volumeResourceId = "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvolume";
        var label = "daily-backup";

        var expectedBackup = new BackupCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/backupVaults/{backupVault}/backups/{backup}",
            Name: $"{account}/{backupVault}/{backup}",
            Type: "Microsoft.NetApp/netAppAccounts/backupVaults/backups",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            VolumeResourceId: volumeResourceId,
            Label: label,
            BackupType: "Manual",
            Size: 107374182400);

        _netAppFilesService.CreateBackup(
            Arg.Is(account), Arg.Is(backupVault), Arg.Is(backup), Arg.Is(resourceGroup), Arg.Is(location), Arg.Is(volumeResourceId), Arg.Is(subscription),
            Arg.Is(label),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedBackup));

        var args = _commandDefinition.Parse([
            "--account", account, "--backupVault", backupVault,
            "--backup", backup,
            "--resource-group", resourceGroup, "--location", location,
            "--volumeResourceId", volumeResourceId,
            "--subscription", subscription, "--label", label
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.BackupCreateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Backup);
        Assert.Equal($"{account}/{backupVault}/{backup}", result.Backup.Name);
        Assert.Equal(location, result.Backup.Location);
        Assert.Equal(resourceGroup, result.Backup.ResourceGroup);
        Assert.Equal("Succeeded", result.Backup.ProvisioningState);
        Assert.Equal(volumeResourceId, result.Backup.VolumeResourceId);
        Assert.Equal(label, result.Backup.Label);
        Assert.Equal("Manual", result.Backup.BackupType);
        Assert.Equal(107374182400, result.Backup.Size);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesBackup_WithoutOptionalParameters()
    {
        // Arrange
        var account = "myanfaccount";
        var backupVault = "myvault";
        var backup = "mybackup";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";
        var volumeResourceId = "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvolume";

        var expectedBackup = new BackupCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/backupVaults/{backupVault}/backups/{backup}",
            Name: $"{account}/{backupVault}/{backup}",
            Type: "Microsoft.NetApp/netAppAccounts/backupVaults/backups",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            VolumeResourceId: volumeResourceId,
            Label: null,
            BackupType: "Manual",
            Size: 0);

        _netAppFilesService.CreateBackup(
            Arg.Is(account), Arg.Is(backupVault), Arg.Is(backup), Arg.Is(resourceGroup), Arg.Is(location), Arg.Is(volumeResourceId), Arg.Is(subscription),
            null,
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedBackup));

        var args = _commandDefinition.Parse([
            "--account", account, "--backupVault", backupVault,
            "--backup", backup,
            "--resource-group", resourceGroup, "--location", location,
            "--volumeResourceId", volumeResourceId,
            "--subscription", subscription
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

        _netAppFilesService.CreateBackup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--backupVault", "myvault",
            "--backup", "mybackup",
            "--resource-group", "myrg", "--location", "eastus",
            "--volumeResourceId", "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvolume",
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
        _netAppFilesService.CreateBackup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Conflict, "Backup already exists"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--backupVault", "myvault",
            "--backup", "mybackup",
            "--resource-group", "myrg", "--location", "eastus",
            "--volumeResourceId", "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvolume",
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
        _netAppFilesService.CreateBackup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Account not found"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--backupVault", "myvault",
            "--backup", "mybackup",
            "--resource-group", "nonexistentrg", "--location", "eastus",
            "--volumeResourceId", "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvolume",
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
        _netAppFilesService.CreateBackup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--backupVault", "myvault",
            "--backup", "mybackup",
            "--resource-group", "myrg", "--location", "eastus",
            "--volumeResourceId", "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvolume",
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
        _netAppFilesService.CreateBackup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<BackupCreateResult>(new Exception("Test error")));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--backupVault", "myvault",
            "--backup", "mybackup",
            "--resource-group", "myrg", "--location", "eastus",
            "--volumeResourceId", "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvolume",
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

        _netAppFilesService.CreateBackup(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedBackup));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--backupVault", "myvault",
            "--backup", "mybackup",
            "--resource-group", "myrg", "--location", "westus2",
            "--volumeResourceId", volumeResourceId,
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.BackupCreateCommandResult);

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
        var volumeResourceId = "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool/volumes/myvolume";
        var label = "my-label";

        var expectedBackup = new BackupCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/backupVaults/{backupVault}/backups/{backup}",
            Name: $"{account}/{backupVault}/{backup}",
            Type: "Microsoft.NetApp/netAppAccounts/backupVaults/backups",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            VolumeResourceId: volumeResourceId,
            Label: label,
            BackupType: "Manual",
            Size: 0);

        _netAppFilesService.CreateBackup(
            account, backupVault, backup, resourceGroup, location, volumeResourceId, subscription,
            label,
            null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedBackup);

        var args = _commandDefinition.Parse([
            "--account", account, "--backupVault", backupVault,
            "--backup", backup,
            "--resource-group", resourceGroup, "--location", location,
            "--volumeResourceId", volumeResourceId,
            "--subscription", subscription, "--label", label
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _netAppFilesService.Received(1).CreateBackup(
            account, backupVault, backup, resourceGroup, location, volumeResourceId, subscription,
            label,
            null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }
}
