// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Security;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.Security;

public class SecurityConfigureEncryptionCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAzureBackupService _backupService;
    private readonly ILogger<SecurityConfigureEncryptionCommand> _logger;
    private readonly SecurityConfigureEncryptionCommand _command;
    private readonly CommandContext _context;
    private readonly System.CommandLine.Command _commandDefinition;

    private const string TestKeyVaultUri = "https://kv-security-prod.vault.azure.net/";
    private const string TestKeyName = "backup-cmk";
    private const string TestKeyVersion = "abc123";
    private const string TestUserAssignedIdentityId = "/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg-identity/providers/Microsoft.ManagedIdentity/userAssignedIdentities/test-identity";

    public SecurityConfigureEncryptionCommandTests()
    {
        _backupService = Substitute.For<IAzureBackupService>();
        _logger = Substitute.For<ILogger<SecurityConfigureEncryptionCommand>>();

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
        Assert.Equal("configure-encryption", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_SystemAssigned_ConfiguresEncryption()
    {
        // Arrange
        var expected = new OperationResult("Succeeded", null, "CMK configured");

        _backupService.ConfigureEncryptionAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Is(TestKeyVaultUri), Arg.Is(TestKeyName), Arg.Is("SystemAssigned"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri, "--key-name", TestKeyName, "--identity-type", "SystemAssigned"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.SecurityConfigureEncryptionCommandResult);

        Assert.NotNull(result);
        Assert.Equal("Succeeded", result.Result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_UserAssigned_ConfiguresEncryption()
    {
        // Arrange
        var expected = new OperationResult("Succeeded", null, "CMK configured with UA identity");

        _backupService.ConfigureEncryptionAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Is(TestKeyVaultUri), Arg.Is(TestKeyName), Arg.Is("UserAssigned"),
            Arg.Any<string?>(), Arg.Is(TestUserAssignedIdentityId), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri, "--key-name", TestKeyName, "--identity-type", "UserAssigned",
            "--user-assigned-identity-id", TestUserAssignedIdentityId]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_WithKeyVersion_ConfiguresEncryption()
    {
        // Arrange
        var expected = new OperationResult("Succeeded", null, "CMK configured with version");

        _backupService.ConfigureEncryptionAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"),
            Arg.Is(TestKeyVaultUri), Arg.Is(TestKeyName), Arg.Is("SystemAssigned"),
            Arg.Is(TestKeyVersion), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri, "--key-name", TestKeyName, "--identity-type", "SystemAssigned",
            "--key-version", TestKeyVersion]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        _backupService.ConfigureEncryptionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri, "--key-name", TestKeyName, "--identity-type", "SystemAssigned"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFoundError()
    {
        // Arrange
        _backupService.ConfigureEncryptionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri, "--key-name", TestKeyName, "--identity-type", "SystemAssigned"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbiddenError()
    {
        // Arrange
        _backupService.ConfigureEncryptionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri, "--key-name", TestKeyName, "--identity-type", "SystemAssigned"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesBadRequestError()
    {
        // Arrange
        _backupService.ConfigureEncryptionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(400, "Bad request"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri, "--key-name", TestKeyName, "--identity-type", "SystemAssigned"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

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
        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri, "--key-name", TestKeyName, "--identity-type", identityType]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsUserAssigned_WithoutIdentityId()
    {
        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri, "--key-name", TestKeyName, "--identity-type", "UserAssigned"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Theory]
    [InlineData("rsv")]
    [InlineData("dpp")]
    [InlineData("RSV")]
    [InlineData("DPP")]
    public async Task ExecuteAsync_AcceptsValidVaultType(string vaultType)
    {
        _backupService.ConfigureEncryptionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(vaultType),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new OperationResult("Succeeded", null, null)));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri, "--key-name", TestKeyName, "--identity-type", "SystemAssigned",
            "--vault-type", vaultType]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("RSV-type")]
    public async Task ExecuteAsync_RejectsInvalidVaultType(string vaultType)
    {
        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri, "--key-name", TestKeyName, "--identity-type", "SystemAssigned",
            "--vault-type", vaultType]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--vault-type must be", response.Message);
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

        _backupService.ConfigureEncryptionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri, "--key-name", TestKeyName, "--identity-type", "SystemAssigned"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureBackupJsonContext.Default.SecurityConfigureEncryptionCommandResult);

        Assert.NotNull(result);
        Assert.Equal("Succeeded", result.Result.Status);
        Assert.Equal("CMK configured successfully", result.Result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_CallsCorrectServiceMethod()
    {
        // Arrange
        _backupService.ConfigureEncryptionAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new OperationResult("Succeeded", null, null)));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--vault", "v", "--resource-group", "rg",
            "--key-vault-uri", TestKeyVaultUri, "--key-name", TestKeyName, "--identity-type", "SystemAssigned"]);

        // Act
        await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        await _backupService.Received(1).ConfigureEncryptionAsync(
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
            _backupService.ConfigureEncryptionAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new OperationResult("Succeeded", null, null)));
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
}
