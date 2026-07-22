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

public class SecurityConfigureEncryptionCommandTests : SubscriptionCommandUnitTestsBase<SecurityConfigureEncryptionCommand, IAzureBackupService>
{
    private const string TestKeyVaultUri = "https://kv-security-prod.vault.azure.net/";
    private const string TestKeyName = "backup-cmk";
    private const string TestKeyVersion = "abc123";
    private const string TestUserAssignedIdentityId = "/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg-identity/providers/Microsoft.ManagedIdentity/userAssignedIdentities/test-identity";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("configure-encryption", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_SystemAssigned_ConfiguresEncryption()
    {
        // Arrange
        var expected = new OperationResult("Succeeded", null, "CMK configured");

        Service.ConfigureEncryptionAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Is(TestKeyVaultUri), Arg.Is(TestKeyName), Arg.Is("SystemAssigned"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri,
            "--key-name", TestKeyName,
            "--identity-type", "SystemAssigned");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.SecurityConfigureEncryptionCommandResult);

        Assert.Equal("Succeeded", result.Result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_UserAssigned_ConfiguresEncryption()
    {
        // Arrange
        var expected = new OperationResult("Succeeded", null, "CMK configured with UA identity");

        Service.ConfigureEncryptionAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Is(TestKeyVaultUri), Arg.Is(TestKeyName), Arg.Is("UserAssigned"),
            Arg.Any<string?>(), Arg.Is(TestUserAssignedIdentityId), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri,
            "--key-name", TestKeyName,
            "--identity-type", "UserAssigned",
            "--user-assigned-identity-id", TestUserAssignedIdentityId);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_WithKeyVersion_ConfiguresEncryption()
    {
        // Arrange
        var expected = new OperationResult("Succeeded", null, "CMK configured with version");

        Service.ConfigureEncryptionAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Is(TestKeyVaultUri), Arg.Is(TestKeyName), Arg.Is("SystemAssigned"),
            Arg.Is(TestKeyVersion), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri,
            "--key-name", TestKeyName,
            "--identity-type", "SystemAssigned",
            "--key-version", TestKeyVersion);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        Service.ConfigureEncryptionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri,
            "--key-name", TestKeyName,
            "--identity-type", "SystemAssigned");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFoundError()
    {
        // Arrange
        Service.ConfigureEncryptionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri,
            "--key-name", TestKeyName,
            "--identity-type", "SystemAssigned");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbiddenError()
    {
        // Arrange
        Service.ConfigureEncryptionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri,
            "--key-name", TestKeyName,
            "--identity-type", "SystemAssigned");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesBadRequestError()
    {
        // Arrange
        Service.ConfigureEncryptionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(400, "Bad request"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri,
            "--key-name", TestKeyName,
            "--identity-type", "SystemAssigned");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("managed identity", response.Message);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("None")]
    [InlineData("SystemAssigned,UserAssigned")]
    public async Task ExecuteAsync_RejectsInvalidIdentityType(string identityType)
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri,
            "--key-name", TestKeyName,
            "--identity-type", identityType);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsUserAssigned_WithoutIdentityId()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri,
            "--key-name", TestKeyName,
            "--identity-type", "UserAssigned");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Theory]
    [InlineData("rsv")]
    [InlineData("dpp")]
    [InlineData("RSV")]
    [InlineData("DPP")]
    public async Task ExecuteAsync_AcceptsValidVaultType(string vaultType)
    {
        Service.ConfigureEncryptionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(vaultType),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult("Succeeded", null, null));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri,
            "--key-name", TestKeyName,
            "--identity-type", "SystemAssigned",
            "--vault-type", vaultType);

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("RSV-type")]
    public async Task ExecuteAsync_RejectsInvalidVaultType(string vaultType)
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri,
            "--key-name", TestKeyName,
            "--identity-type", "SystemAssigned",
            "--vault-type", vaultType);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--vault-type must be", response.Message);
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
        Assert.Contains(options, o => o.Name == "--key-vault-uri");
        Assert.Contains(options, o => o.Name == "--key-name");
        Assert.Contains(options, o => o.Name == "--key-version");
        Assert.Contains(options, o => o.Name == "--identity-type");
        Assert.Contains(options, o => o.Name == "--user-assigned-identity-id");
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expected = new OperationResult("Succeeded", null, "CMK configured successfully");

        Service.ConfigureEncryptionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri,
            "--key-name", TestKeyName,
            "--identity-type", "SystemAssigned");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.SecurityConfigureEncryptionCommandResult);

        Assert.Equal("Succeeded", result.Result.Status);
        Assert.Equal("CMK configured successfully", result.Result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_CallsCorrectServiceMethod()
    {
        // Arrange
        Service.ConfigureEncryptionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult("Succeeded", null, null));

        // Act
        await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri,
            "--key-name", TestKeyName,
            "--identity-type", "SystemAssigned");

        // Assert
        await Service.Received(1).ConfigureEncryptionAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Is(TestKeyVaultUri), Arg.Is(TestKeyName), Arg.Is("SystemAssigned"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg --key-vault-uri https://kv.vault.azure.net/ --key-name mykey --identity-type SystemAssigned", true)]
    [InlineData("--subscription sub --vault v --resource-group rg", false)] // Missing required options
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ConfigureEncryptionAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
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
}
