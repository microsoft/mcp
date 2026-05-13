// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.ProtectedItem;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.ProtectedItem;

public class ProtectedItemProtectCommandTests : CommandUnitTestsBase<ProtectedItemProtectCommand, IAzureBackupService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("protect", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ProtectsItem_Successfully()
    {
        // Arrange
        Service.ProtectItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("/subscriptions/.../vm1"), Arg.Is("DefaultPolicy"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new ProtectResult("Succeeded", "vm1-backup", "job123", "Protection enabled"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", "/subscriptions/.../vm1",
            "--policy", "DefaultPolicy");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ProtectedItemProtectCommandResult);

        Assert.Equal("Succeeded", result.Result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        Service.ProtectItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("/subscriptions/.../vm1"), Arg.Is("DefaultPolicy"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", "/subscriptions/.../vm1",
            "--policy", "DefaultPolicy");

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
            Service.ProtectItemAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("ds1"), Arg.Is("pol1"),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(new ProtectResult("Succeeded", "item1", "job1", null));
        }

        // Act
        var response = await ExecuteCommandAsync(args);

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
        Service.ProtectItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("/subscriptions/.../vm1"), Arg.Is("DefaultPolicy"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", "/subscriptions/.../vm1",
            "--policy", "DefaultPolicy");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
        Assert.Contains("Backup Contributor", response.Message);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange & Act
        var options = CommandDefinition.Options;

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
        // Arrange  -  DPP protection is not a job; result should expose ProtectionStatus
        // (read back from the backup instance) and leave JobId null.
        Service.ProtectItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("/subscriptions/.../disks/d1"), Arg.Is("policy-disk"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new ProtectResult(
                Status: "Succeeded",
                ProtectedItemName: "rg-mydisk-abcd1234",
                JobId: null,
                Message: "Protection configured for backup instance 'rg-mydisk-abcd1234' (status: ProtectionConfigured).",
                ProtectionStatus: "ProtectionConfigured",
                ErrorMessage: null));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", "/subscriptions/.../disks/d1",
            "--policy", "policy-disk");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ProtectedItemProtectCommandResult);
        Assert.Equal("Succeeded", result.Result.Status);
        Assert.Null(result.Result.JobId);
        Assert.Equal("ProtectionConfigured", result.Result.ProtectionStatus);
    }

    [Fact]
    public async Task ExecuteAsync_DppResult_SurfacesFailureWithErrorMessage()
    {
        // Arrange  -  when DPP backend rejects (e.g. VaultMSIUnauthorized) the result must
        // carry Status="Failed" + ErrorMessage rather than a misleading "Accepted".
        Service.ProtectItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("/subscriptions/.../sa1"), Arg.Is("policy-blob"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new ProtectResult(
                Status: "Failed",
                ProtectedItemName: "rg-blob-xyz",
                JobId: null,
                Message: "Protection failed for backup instance 'rg-blob-xyz': VaultMSIUnauthorized",
                ProtectionStatus: null,
                ErrorMessage: "VaultMSIUnauthorized: Vault MSI is not authorized."));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", "/subscriptions/.../sa1",
            "--policy", "policy-blob");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ProtectedItemProtectCommandResult);
        Assert.Equal("Failed", result.Result.Status);
        Assert.Null(result.Result.JobId);
        Assert.Contains("VaultMSIUnauthorized", result.Result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_RsvResult_SurfacesTerminalJobStatus()
    {
        // Arrange  -  RSV protection should report the polled ConfigureBackup job's
        // terminal status (Completed, CompletedWithWarnings, Failed) along with the job id.
        Service.ProtectItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("/subscriptions/.../vms/myvm"), Arg.Is("policy-vm"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new ProtectResult(
                Status: "Completed",
                ProtectedItemName: "vm;iaasvmcontainerv2;rg;myvm",
                JobId: "11111111-1111-1111-1111-111111111111",
                Message: "VM protection completed. Use 'azurebackup protecteditem get' to verify."));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", "/subscriptions/.../vms/myvm",
            "--policy", "policy-vm");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ProtectedItemProtectCommandResult);
        Assert.Equal("Completed", result.Result.Status);
        Assert.Equal("11111111-1111-1111-1111-111111111111", result.Result.JobId);
    }

    [Fact]
    public async Task ExecuteAsync_RsvResult_SurfacesFailedJobWithErrorMessage()
    {
        // Arrange  -  when ConfigureBackup ends in Failed, MCP must surface Status=Failed
        // and ErrorMessage from the job rather than the previous "Accepted".
        Service.ProtectItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("/subscriptions/.../sa/fileServices/default/shares/share"), Arg.Is("policy-afs"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new ProtectResult(
                Status: "Failed",
                ProtectedItemName: "afsfileshare;sa;share",
                JobId: "22222222-2222-2222-2222-222222222222",
                Message: "File share protection failed: Item not found. See 'azurebackup job get --job 22222222-...' for details.",
                ProtectionStatus: null,
                ErrorMessage: "Item not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", "/subscriptions/.../sa/fileServices/default/shares/share",
            "--policy", "policy-afs");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ProtectedItemProtectCommandResult);
        Assert.Equal("Failed", result.Result.Status);
        Assert.Equal("Item not found", result.Result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_RsvResult_SurfacesInProgressWhenPollingBudgetExpires()
    {
        // Arrange  -  long-running ConfigureBackup must not cause the tool to fail; it
        // should return InProgress with the job id so the caller can keep monitoring.
        Service.ProtectItemAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("/subscriptions/.../vms/slowvm"), Arg.Is("policy-vm"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new ProtectResult(
                Status: "InProgress",
                ProtectedItemName: "vm;iaasvmcontainerv2;rg;slowvm",
                JobId: "33333333-3333-3333-3333-333333333333",
                Message: "VM protection is still running after the polling budget elapsed. Use 'azurebackup job get --job 33333333-...' to continue monitoring."));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--datasource-id", "/subscriptions/.../vms/slowvm",
            "--policy", "policy-vm");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.ProtectedItemProtectCommandResult);
        Assert.Equal("InProgress", result.Result.Status);
        Assert.Equal("33333333-3333-3333-3333-333333333333", result.Result.JobId);
    }
}
