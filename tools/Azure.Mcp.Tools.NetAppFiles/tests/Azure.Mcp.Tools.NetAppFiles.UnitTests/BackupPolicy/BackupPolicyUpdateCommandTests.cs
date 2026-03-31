// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
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

public class BackupPolicyUpdateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INetAppFilesService _netAppFilesService;
    private readonly ILogger<BackupPolicyUpdateCommand> _logger;
    private readonly BackupPolicyUpdateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public BackupPolicyUpdateCommandTests()
    {
        _netAppFilesService = Substitute.For<INetAppFilesService>();
        _logger = Substitute.For<ILogger<BackupPolicyUpdateCommand>>();

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
    [InlineData("--account myanfaccount --backupPolicy mypolicy --resource-group myrg --location eastus --subscription sub123", true)]
    [InlineData("--backupPolicy mypolicy --resource-group myrg --location eastus --subscription sub123", false)] // Missing account
    [InlineData("--account myanfaccount --resource-group myrg --location eastus --subscription sub123", false)] // Missing backupPolicy
    [InlineData("--account myanfaccount --backupPolicy mypolicy --location eastus --subscription sub123", false)] // Missing resource-group
    [InlineData("--account myanfaccount --backupPolicy mypolicy --resource-group myrg --subscription sub123", false)] // Missing location
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedPolicy = new BackupPolicyCreateResult(
                Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/backupPolicies/mypolicy",
                Name: "myanfaccount/mypolicy",
                Type: "Microsoft.NetApp/netAppAccounts/backupPolicies",
                Location: "eastus",
                ResourceGroup: "myrg",
                ProvisioningState: "Succeeded",
                DailyBackupsToKeep: null,
                WeeklyBackupsToKeep: null,
                MonthlyBackupsToKeep: null,
                Enabled: true);

            _netAppFilesService.UpdateBackupPolicy(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int?>(),
                Arg.Any<int?>(),
                Arg.Any<int?>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedPolicy);
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
    public async Task ExecuteAsync_UpdatesBackupPolicy_Successfully()
    {
        // Arrange
        var account = "myanfaccount";
        var backupPolicy = "mypolicy";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";

        var expectedPolicy = new BackupPolicyCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/backupPolicies/{backupPolicy}",
            Name: $"{account}/{backupPolicy}",
            Type: "Microsoft.NetApp/netAppAccounts/backupPolicies",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            DailyBackupsToKeep: 5,
            WeeklyBackupsToKeep: 2,
            MonthlyBackupsToKeep: 1,
            Enabled: true);

        _netAppFilesService.UpdateBackupPolicy(
            Arg.Is(account), Arg.Is(backupPolicy), Arg.Is(resourceGroup), Arg.Is(location), Arg.Is(subscription),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPolicy));

        var args = _commandDefinition.Parse([
            "--account", account, "--backupPolicy", backupPolicy,
            "--resource-group", resourceGroup, "--location", location,
            "--subscription", subscription, "--dailyBackupsToKeep", "5",
            "--weeklyBackupsToKeep", "2", "--monthlyBackupsToKeep", "1"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.BackupPolicyUpdateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.BackupPolicy);
        Assert.Equal($"{account}/{backupPolicy}", result.BackupPolicy.Name);
        Assert.Equal(location, result.BackupPolicy.Location);
        Assert.Equal(resourceGroup, result.BackupPolicy.ResourceGroup);
        Assert.Equal("Succeeded", result.BackupPolicy.ProvisioningState);
        Assert.Equal(5, result.BackupPolicy.DailyBackupsToKeep);
        Assert.Equal(2, result.BackupPolicy.WeeklyBackupsToKeep);
        Assert.Equal(1, result.BackupPolicy.MonthlyBackupsToKeep);
        Assert.True(result.BackupPolicy.Enabled);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesBackupPolicy_WithoutOptionalParameters()
    {
        // Arrange
        var account = "myanfaccount";
        var backupPolicy = "mypolicy";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";

        var expectedPolicy = new BackupPolicyCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/backupPolicies/{backupPolicy}",
            Name: $"{account}/{backupPolicy}",
            Type: "Microsoft.NetApp/netAppAccounts/backupPolicies",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            DailyBackupsToKeep: null,
            WeeklyBackupsToKeep: null,
            MonthlyBackupsToKeep: null,
            Enabled: true);

        _netAppFilesService.UpdateBackupPolicy(
            Arg.Is(account), Arg.Is(backupPolicy), Arg.Is(resourceGroup), Arg.Is(location), Arg.Is(subscription),
            null, null, null,
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPolicy));

        var args = _commandDefinition.Parse([
            "--account", account, "--backupPolicy", backupPolicy,
            "--resource-group", resourceGroup, "--location", location,
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

        _netAppFilesService.UpdateBackupPolicy(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--backupPolicy", "mypolicy",
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
        _netAppFilesService.UpdateBackupPolicy(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Conflict, "Backup policy already exists"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--backupPolicy", "mypolicy",
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
        _netAppFilesService.UpdateBackupPolicy(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Backup policy not found"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--backupPolicy", "mypolicy",
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
        _netAppFilesService.UpdateBackupPolicy(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--backupPolicy", "mypolicy",
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
        _netAppFilesService.UpdateBackupPolicy(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<BackupPolicyCreateResult>(new Exception("Test error")));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--backupPolicy", "mypolicy",
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
        var expectedPolicy = new BackupPolicyCreateResult(
            Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/backupPolicies/mypolicy",
            Name: "myanfaccount/mypolicy",
            Type: "Microsoft.NetApp/netAppAccounts/backupPolicies",
            Location: "westus2",
            ResourceGroup: "myrg",
            ProvisioningState: "Succeeded",
            DailyBackupsToKeep: 5,
            WeeklyBackupsToKeep: 2,
            MonthlyBackupsToKeep: 1,
            Enabled: true);

        _netAppFilesService.UpdateBackupPolicy(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPolicy));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--backupPolicy", "mypolicy",
            "--resource-group", "myrg", "--location", "westus2",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.BackupPolicyUpdateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.BackupPolicy);
        Assert.Equal("myanfaccount/mypolicy", result.BackupPolicy.Name);
        Assert.Equal("westus2", result.BackupPolicy.Location);
        Assert.Equal("myrg", result.BackupPolicy.ResourceGroup);
        Assert.Equal("Succeeded", result.BackupPolicy.ProvisioningState);
        Assert.Equal("Microsoft.NetApp/netAppAccounts/backupPolicies", result.BackupPolicy.Type);
        Assert.Equal(5, result.BackupPolicy.DailyBackupsToKeep);
        Assert.Equal(2, result.BackupPolicy.WeeklyBackupsToKeep);
        Assert.Equal(1, result.BackupPolicy.MonthlyBackupsToKeep);
        Assert.True(result.BackupPolicy.Enabled);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var account = "myanfaccount";
        var backupPolicy = "mypolicy";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";

        var expectedPolicy = new BackupPolicyCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/backupPolicies/{backupPolicy}",
            Name: $"{account}/{backupPolicy}",
            Type: "Microsoft.NetApp/netAppAccounts/backupPolicies",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            DailyBackupsToKeep: 5,
            WeeklyBackupsToKeep: 2,
            MonthlyBackupsToKeep: 1,
            Enabled: true);

        _netAppFilesService.UpdateBackupPolicy(
            account, backupPolicy, resourceGroup, location, subscription,
            5, 2, 1,
            null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedPolicy);

        var args = _commandDefinition.Parse([
            "--account", account, "--backupPolicy", backupPolicy,
            "--resource-group", resourceGroup, "--location", location,
            "--subscription", subscription, "--dailyBackupsToKeep", "5",
            "--weeklyBackupsToKeep", "2", "--monthlyBackupsToKeep", "1"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _netAppFilesService.Received(1).UpdateBackupPolicy(
            account, backupPolicy, resourceGroup, location, subscription,
            5, 2, 1,
            null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }
}
