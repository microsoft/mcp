// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// cspell:ignore mydisk myvm slowvm afsfileshare iaasvmcontainerv azurebackup protecteditem

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.ProtectedItem;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.ProtectedItem;

public class ProtectedItemProtectCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAzureBackupService _backupService;
    private readonly ILogger<ProtectedItemProtectCommand> _logger;
    private readonly ProtectedItemProtectCommand _command;
    private readonly CommandContext _context;
    private readonly System.CommandLine.Command _commandDefinition;

    public ProtectedItemProtectCommandTests()
    {
        _backupService = Substitute.For<IAzureBackupService>();
        _logger = Substitute.For<ILogger<ProtectedItemProtectCommand>>();

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
        Assert.Equal("protect", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ProtectsItem_Successfully()
    {
        // Arrange
        var expected = new ProtectResult("Succeeded", "vm1-backup", "job123", "Protection enabled");

        _backupService.ProtectItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("/subscriptions/.../vm1"), Arg.Is("DefaultPolicy"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--datasource-id", "/subscriptions/.../vm1", "--policy", "DefaultPolicy"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.ProtectedItemProtectCommandResult);

        Assert.NotNull(result);
        Assert.Equal("Succeeded", result.Result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        _backupService.ProtectItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("/subscriptions/.../vm1"), Arg.Is("DefaultPolicy"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--datasource-id", "/subscriptions/.../vm1", "--policy", "DefaultPolicy"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg --datasource-id ds1 --policy pol1", true)]
    [InlineData("--subscription sub --vault v --resource-group rg", false)] // Missing datasource-id and policy
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _backupService.ProtectItemAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("ds1"), Arg.Is("pol1"),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new ProtectResult("Succeeded", "item1", "job1", null)));
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
    public async Task ExecuteAsync_HandlesForbiddenError()
    {
        // Arrange
        _backupService.ProtectItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("/subscriptions/.../vm1"), Arg.Is("DefaultPolicy"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--datasource-id", "/subscriptions/.../vm1", "--policy", "DefaultPolicy"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
        Assert.Contains("Backup Contributor", response.Message);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange & Act
        var command = _command.GetCommand();
        var options = command.Options;

        // Assert
        Assert.Contains(options, o => o.Name == "--subscription");
        Assert.Contains(options, o => o.Name == "--resource-group");
        Assert.Contains(options, o => o.Name == "--vault");
        Assert.Contains(options, o => o.Name == "--vault-type");
        Assert.Contains(options, o => o.Name == "--datasource-id");
        Assert.Contains(options, o => o.Name == "--policy");
        Assert.Contains(options, o => o.Name == "--container");
        Assert.Contains(options, o => o.Name == "--datasource-type");
    }

    [Fact]
    public async Task ExecuteAsync_DppResult_SurfacesProtectionStatusAndOmitsJobId()
    {
        // Arrange — DPP protection is not a job; result should expose ProtectionStatus
        // (read back from the backup instance) and leave JobId null.
        var expected = new ProtectResult(
            Status: "Succeeded",
            ProtectedItemName: "rg-mydisk-abcd1234",
            JobId: null,
            Message: "Protection configured for backup instance 'rg-mydisk-abcd1234' (status: ProtectionConfigured).",
            ProtectionStatus: "ProtectionConfigured",
            ErrorMessage: null);

        _backupService.ProtectItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("/subscriptions/.../disks/d1"), Arg.Is("policy-disk"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--datasource-id", "/subscriptions/.../disks/d1", "--policy", "policy-disk"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.ProtectedItemProtectCommandResult);
        Assert.NotNull(result);
        Assert.Equal("Succeeded", result.Result.Status);
        Assert.Null(result.Result.JobId);
        Assert.Equal("ProtectionConfigured", result.Result.ProtectionStatus);
    }

    [Fact]
    public async Task ExecuteAsync_DppResult_SurfacesFailureWithErrorMessage()
    {
        // Arrange — when DPP backend rejects (e.g. VaultMSIUnauthorized) the result must
        // carry Status="Failed" + ErrorMessage rather than a misleading "Accepted".
        var expected = new ProtectResult(
            Status: "Failed",
            ProtectedItemName: "rg-blob-xyz",
            JobId: null,
            Message: "Protection failed for backup instance 'rg-blob-xyz': VaultMSIUnauthorized",
            ProtectionStatus: null,
            ErrorMessage: "VaultMSIUnauthorized: Vault MSI is not authorized.");

        _backupService.ProtectItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("/subscriptions/.../sa1"), Arg.Is("policy-blob"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--datasource-id", "/subscriptions/.../sa1", "--policy", "policy-blob"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.ProtectedItemProtectCommandResult);
        Assert.NotNull(result);
        Assert.Equal("Failed", result.Result.Status);
        Assert.Null(result.Result.JobId);
        Assert.Contains("VaultMSIUnauthorized", result.Result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_RsvResult_SurfacesTerminalJobStatus()
    {
        // Arrange — RSV protection should report the polled ConfigureBackup job's
        // terminal status (Completed, CompletedWithWarnings, Failed) along with the job id.
        var expected = new ProtectResult(
            Status: "Completed",
            ProtectedItemName: "vm;iaasvmcontainerv2;rg;myvm",
            JobId: "11111111-1111-1111-1111-111111111111",
            Message: "VM protection completed. Use 'azurebackup protecteditem get' to verify.");

        _backupService.ProtectItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("/subscriptions/.../vms/myvm"), Arg.Is("policy-vm"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--datasource-id", "/subscriptions/.../vms/myvm", "--policy", "policy-vm"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.ProtectedItemProtectCommandResult);
        Assert.NotNull(result);
        Assert.Equal("Completed", result.Result.Status);
        Assert.Equal("11111111-1111-1111-1111-111111111111", result.Result.JobId);
    }

    [Fact]
    public async Task ExecuteAsync_RsvResult_SurfacesFailedJobWithErrorMessage()
    {
        // Arrange — when ConfigureBackup ends in Failed, MCP must surface Status=Failed
        // and ErrorMessage from the job rather than the previous "Accepted".
        var expected = new ProtectResult(
            Status: "Failed",
            ProtectedItemName: "afsfileshare;sa;share",
            JobId: "22222222-2222-2222-2222-222222222222",
            Message: "File share protection failed: Item not found. See 'azurebackup job get --job 22222222-...' for details.",
            ProtectionStatus: null,
            ErrorMessage: "Item not found");

        _backupService.ProtectItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("/subscriptions/.../sa/fileServices/default/shares/share"), Arg.Is("policy-afs"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--datasource-id", "/subscriptions/.../sa/fileServices/default/shares/share", "--policy", "policy-afs"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.ProtectedItemProtectCommandResult);
        Assert.NotNull(result);
        Assert.Equal("Failed", result.Result.Status);
        Assert.Equal("Item not found", result.Result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_RsvResult_SurfacesInProgressWhenPollingBudgetExpires()
    {
        // Arrange — long-running ConfigureBackup must not cause the tool to fail; it
        // should return InProgress with the job id so the caller can keep monitoring.
        var expected = new ProtectResult(
            Status: "InProgress",
            ProtectedItemName: "vm;iaasvmcontainerv2;rg;slowvm",
            JobId: "33333333-3333-3333-3333-333333333333",
            Message: "VM protection is still running after the polling budget elapsed. Use 'azurebackup job get --job 33333333-...' to continue monitoring.");

        _backupService.ProtectItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("/subscriptions/.../vms/slowvm"), Arg.Is("policy-vm"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--datasource-id", "/subscriptions/.../vms/slowvm", "--policy", "policy-vm"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.ProtectedItemProtectCommandResult);
        Assert.NotNull(result);
        Assert.Equal("InProgress", result.Result.Status);
        Assert.Equal("33333333-3333-3333-3333-333333333333", result.Result.JobId);
    }
}
