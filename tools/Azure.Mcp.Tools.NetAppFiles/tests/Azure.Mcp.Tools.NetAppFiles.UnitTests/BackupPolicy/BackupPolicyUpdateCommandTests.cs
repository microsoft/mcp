// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Microsoft.Mcp.Core.Options;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.BackupPolicy;
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

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.BackupPolicy;

public class BackupPolicyUpdateCommandTests : SubscriptionCommandUnitTestsBase<BackupPolicyUpdateCommand, INetAppFilesService>
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
    [InlineData("--account myanfaccount --backup-policy mypolicy --resource-group myrg --location eastus --subscription sub123", true)]
    [InlineData("--account myanfaccount --backup-policy mypolicy --resource-group myrg --location eastus --subscription sub123 --enabled false", true)]
    [InlineData("--backup-policy mypolicy --resource-group myrg --location eastus --subscription sub123", false)] // Missing account
    [InlineData("--account myanfaccount --resource-group myrg --location eastus --subscription sub123", false)] // Missing backupPolicy
    [InlineData("--account myanfaccount --backup-policy mypolicy --location eastus --subscription sub123", false)] // Missing resource-group
    [InlineData("--account myanfaccount --backup-policy mypolicy --resource-group myrg --subscription sub123", false)] // Missing location
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

            Service.UpdateBackupPolicy(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int?>(),
                Arg.Any<int?>(),
                Arg.Any<int?>(),
                Arg.Any<bool?>(),
                Arg.Any<Dictionary<string, string>?>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedPolicy);
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

        Service.UpdateBackupPolicy(
            Arg.Is(account), Arg.Is(backupPolicy), Arg.Is(resourceGroup), Arg.Is(location), Arg.Is(subscription),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<bool?>(), Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPolicy));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", account, "--backup-policy", backupPolicy,
            "--resource-group", resourceGroup, "--location", location,
            "--subscription", subscription, "--daily-backups-to-keep", "5",
            "--weekly-backups-to-keep", "2", "--monthly-backups-to-keep", "1"
        ]);

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

        Service.UpdateBackupPolicy(
            Arg.Is(account), Arg.Is(backupPolicy), Arg.Is(resourceGroup), Arg.Is(location), Arg.Is(subscription),
            null, null, null,
            null, null,
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPolicy));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", account, "--backup-policy", backupPolicy,
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

        Service.UpdateBackupPolicy(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<bool?>(), Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-policy", "mypolicy",
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
        Service.UpdateBackupPolicy(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<bool?>(), Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Conflict, "Backup policy already exists"));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-policy", "mypolicy",
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
        Service.UpdateBackupPolicy(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<bool?>(), Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Backup policy not found"));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-policy", "mypolicy",
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
        Service.UpdateBackupPolicy(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<bool?>(), Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-policy", "mypolicy",
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
        Service.UpdateBackupPolicy(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<bool?>(), Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<BackupPolicyCreateResult>(new Exception("Test error")));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-policy", "mypolicy",
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

        Service.UpdateBackupPolicy(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<bool?>(), Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPolicy));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--backup-policy", "mypolicy",
            "--resource-group", "myrg", "--location", "westus2",
            "--subscription", "sub123"
        ]);

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

        Service.UpdateBackupPolicy(
            account, backupPolicy, resourceGroup, location, subscription,
            5, 2, 1,
            null, null,
            null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedPolicy);

        // Act
        var response = await ExecuteCommandAsync([
            "--account", account, "--backup-policy", backupPolicy,
            "--resource-group", resourceGroup, "--location", location,
            "--subscription", subscription, "--daily-backups-to-keep", "5",
            "--weekly-backups-to-keep", "2", "--monthly-backups-to-keep", "1"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateBackupPolicy(
            account, backupPolicy, resourceGroup, location, subscription,
            5, 2, 1,
            null, null,
            null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithEnabledAndTags()
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
            Enabled: false);

        Service.UpdateBackupPolicy(
            account, backupPolicy, resourceGroup, location, subscription,
            5, 2, 1,
            false, Arg.Any<Dictionary<string, string>?>(),
            null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedPolicy);

        // Act
        var response = await ExecuteCommandAsync([
            "--account", account, "--backup-policy", backupPolicy,
            "--resource-group", resourceGroup, "--location", location,
            "--subscription", subscription, "--daily-backups-to-keep", "5",
            "--weekly-backups-to-keep", "2", "--monthly-backups-to-keep", "1",
            "--enabled", "false", "--tags", "{\"env\":\"test\",\"owner\":\"anf\"}"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateBackupPolicy(
            account, backupPolicy, resourceGroup, location, subscription,
            5, 2, 1,
            false,
            Arg.Is<Dictionary<string, string>?>(tags =>
                tags != null &&
                tags.Count == 2 &&
                tags["env"] == "test" &&
                tags["owner"] == "anf"),
            null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--ids /subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/backupPolicies/mypolicy")]
    [InlineData("--no-wait true")]
    [InlineData("--add properties.enabled=true")]
    [InlineData("--set properties.enabled=false")]
    [InlineData("--remove properties.weeklyBackupsToKeep")]
    [InlineData("--force-string true")]
    public async Task ExecuteAsync_UnsupportedArguments_ReturnBadRequest(string unsupportedArg)
    {
        // Arrange
        var baseArgs = new List<string>
        {
            "--account", "myanfaccount",
            "--backup-policy", "mypolicy",
            "--resource-group", "myrg",
            "--location", "eastus",
            "--subscription", "sub123"
        };
        baseArgs.AddRange(unsupportedArg.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        var response = await ExecuteCommandAsync(baseArgs.ToArray());

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("not supported", response.Message);
        await Service.DidNotReceive().UpdateBackupPolicy(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<int?>(),
            Arg.Any<bool?>(), Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }
}
