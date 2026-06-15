// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Security;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Security;

public class SecurityConfigureMuaCommandTests : SubscriptionCommandUnitTestsBase<SecurityConfigureMuaCommand, IAzureBackupService>
{
    private const string TestResourceGuardId = "/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg-security/providers/Microsoft.DataProtection/resourceGuards/test-guard";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("configure-mua", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_EnablesMua_WithResourceGuardId()
    {
        // Arrange
        var expected = new OperationResult("Succeeded", null, "MUA enabled");

        Service.ConfigureMultiUserAuthorizationAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is(TestResourceGuardId),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--resource-guard-id", TestResourceGuardId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.SecurityConfigureMuaCommandResult);

        Assert.Equal("Succeeded", result.Result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_DisablesMua_WithoutResourceGuardId()
    {
        // Arrange
        var expected = new OperationResult("Succeeded", null, "MUA disabled");

        Service.DisableMultiUserAuthorizationAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.SecurityConfigureMuaCommandResult);

        Assert.Equal("Succeeded", result.Result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        Service.DisableMultiUserAuthorizationAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFoundError()
    {
        // Arrange
        Service.ConfigureMultiUserAuthorizationAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is(TestResourceGuardId),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--resource-guard-id", TestResourceGuardId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbiddenError()
    {
        // Arrange
        Service.ConfigureMultiUserAuthorizationAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is(TestResourceGuardId),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--resource-guard-id", TestResourceGuardId);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesBadRequestError()
    {
        // Arrange
        Service.ConfigureMultiUserAuthorizationAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is(TestResourceGuardId),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(400, "Region mismatch"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--resource-guard-id", TestResourceGuardId);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("same region", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflictError()
    {
        // Arrange
        Service.ConfigureMultiUserAuthorizationAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is(TestResourceGuardId),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(409, "Already configured"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--resource-guard-id", TestResourceGuardId);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("conflict", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("RSV-type")]
    [InlineData("backup")]
    [InlineData("recovery")]
    public async Task ExecuteAsync_RejectsInvalidVaultType(string vaultType)
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--vault-type", vaultType);

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
        Service.DisableMultiUserAuthorizationAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is(vaultType),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new OperationResult("Succeeded", null, null)));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--vault-type", vaultType);

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg", true)]
    [InlineData("--subscription sub", false)] // Missing vault and resource-group
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.DisableMultiUserAuthorizationAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(new OperationResult("Succeeded", null, null));
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
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange & Act
        var command = Command.GetCommand();
        var options = command.Options;

        // Assert
        Assert.Contains(options, o => o.Name == "--subscription");
        Assert.Contains(options, o => o.Name == "--resource-group");
        Assert.Contains(options, o => o.Name == "--vault");
        Assert.Contains(options, o => o.Name == "--vault-type");
        Assert.Contains(options, o => o.Name == "--resource-guard-id");
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expected = new OperationResult("Succeeded", null, "MUA enabled with guard");

        Service.ConfigureMultiUserAuthorizationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--resource-guard-id", TestResourceGuardId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.SecurityConfigureMuaCommandResult);

        Assert.Equal("Succeeded", result.Result.Status);
        Assert.Equal("MUA enabled with guard", result.Result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_EnableMua_CallsCorrectServiceMethod()
    {
        // Arrange
        Service.ConfigureMultiUserAuthorizationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult("Succeeded", null, null));

        // Act
        await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--resource-guard-id", TestResourceGuardId);

        // Assert - Enable was called, not Disable
        await Service.Received(1).ConfigureMultiUserAuthorizationAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is(TestResourceGuardId),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());

        await Service.DidNotReceive().DisableMultiUserAuthorizationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DisableMua_CallsCorrectServiceMethod()
    {
        // Arrange
        Service.DisableMultiUserAuthorizationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult("Succeeded", null, null));

        // Act
        await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg");

        // Assert - Disable was called, not Enable
        await Service.Received(1).DisableMultiUserAuthorizationAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());

        await Service.DidNotReceive().ConfigureMultiUserAuthorizationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }
}
