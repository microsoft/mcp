// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Microsoft.Mcp.Core.Options;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.Account;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using Xunit.v3;
using Microsoft.Testing.Platform.TestHost;
using System.Diagnostics;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.Account;

public class AccountUpdateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INetAppFilesService _netAppFilesService;
    private readonly ILogger<AccountUpdateCommand> _logger;
    private readonly AccountUpdateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public AccountUpdateCommandTests()
    {
        _netAppFilesService = Substitute.For<INetAppFilesService>();
        _logger = Substitute.For<ILogger<AccountUpdateCommand>>();

        var collection = new ServiceCollection().AddSingleton(_netAppFilesService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger, _netAppFilesService);
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
    [InlineData("--account myanfaccount --resource-group myrg --location eastus --subscription 00000000-0000-0000-0000-000000000000", true)]
    [InlineData("--account myanfaccount --resource-group myrg --subscription 00000000-0000-0000-0000-000000000000", true)] // Location optional
    [InlineData("--resource-group myrg --location eastus --subscription 00000000-0000-0000-0000-000000000000", false)] // Missing account
    [InlineData("--account myanfaccount --location eastus --subscription 00000000-0000-0000-0000-000000000000", false)] // Missing resource-group
    [InlineData($"--ids /subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount --subscription 00000000-0000-0000-0000-000000000000", true)]
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        TestEnvironment.ClearAzureSubscriptionId();
        if (shouldSucceed)
        {
            var expectedAccount = new NetAppAccountCreateResult(
                Id: "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount",
                Name: "myanfaccount",
                Type: "Microsoft.NetApp/netAppAccounts",
                Location: "eastus",
                ResourceGroup: "myrg",
                ProvisioningState: "Succeeded");

            _netAppFilesService.UpdateAccount(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<Dictionary<string, string>?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<JsonElement?>(),
                Arg.Any<JsonElement?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedAccount);
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
            Assert.Contains("--ids or both --account and --resource-group", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesAccount_Successfully()
    {
        // Arrange
        TestEnvironment.ClearAzureSubscriptionId();
        var account = "myanfaccount";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "00000000-0000-0000-0000-000000000000";

        var expectedAccount = new NetAppAccountCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}",
            Name: account,
            Type: "Microsoft.NetApp/netAppAccounts",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded");

        _netAppFilesService.UpdateAccount(
            Arg.Is(account), Arg.Is(resourceGroup), Arg.Is(location), Arg.Is(subscription),
            Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<JsonElement?>(),
            Arg.Any<JsonElement?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedAccount));

        var args = _commandDefinition.Parse([
            "--account", account, "--resource-group", resourceGroup,
            "--location", location, "--subscription", subscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.AccountUpdateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Account);
        Assert.Equal(account, result.Account.Name);
        Assert.Equal(location, result.Account.Location);
        Assert.Equal(resourceGroup, result.Account.ResourceGroup);
        Assert.Equal("Succeeded", result.Account.ProvisioningState);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesAccountWithTags_Successfully()
    {
        // Arrange
        TestEnvironment.ClearAzureSubscriptionId();
        var account = "myanfaccount";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "00000000-0000-0000-0000-000000000000";
        var tagsJson = "{\"env\":\"prod\",\"team\":\"storage\"}";

        var expectedAccount = new NetAppAccountCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}",
            Name: account,
            Type: "Microsoft.NetApp/netAppAccounts",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded");

        _netAppFilesService.UpdateAccount(
            Arg.Is(account), Arg.Is(resourceGroup), Arg.Is(location), Arg.Is(subscription),
            Arg.Is<Dictionary<string, string>>(d => d.ContainsKey("env") && d["env"] == "prod"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<JsonElement?>(),
            Arg.Any<JsonElement?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedAccount));

        var args = _commandDefinition.Parse([
            "--account", account, "--resource-group", resourceGroup,
            "--location", location, "--subscription", subscription,
            "--tags", tagsJson
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
        TestEnvironment.ClearAzureSubscriptionId();
        var expectedError = "Test error";

        _netAppFilesService.UpdateAccount(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string>(), Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<JsonElement?>(), Arg.Any<JsonElement?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--resource-group", "myrg",
            "--location", "eastus", "--subscription", "00000000-0000-0000-0000-000000000000"
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
        TestEnvironment.ClearAzureSubscriptionId();
        _netAppFilesService.UpdateAccount(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string>(), Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<JsonElement?>(), Arg.Any<JsonElement?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Conflict, "Account already exists"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--resource-group", "myrg",
            "--location", "eastus", "--subscription", "00000000-0000-0000-0000-000000000000"
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
        TestEnvironment.ClearAzureSubscriptionId();
        _netAppFilesService.UpdateAccount(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string>(), Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<JsonElement?>(), Arg.Any<JsonElement?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Resource not found"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--resource-group", "nonexistentrg",
            "--location", "eastus", "--subscription", "00000000-0000-0000-0000-000000000000"
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
        TestEnvironment.ClearAzureSubscriptionId();
        _netAppFilesService.UpdateAccount(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string>(), Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<JsonElement?>(), Arg.Any<JsonElement?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--resource-group", "myrg",
            "--location", "eastus", "--subscription", "00000000-0000-0000-0000-000000000000"
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
        TestEnvironment.ClearAzureSubscriptionId();
        _netAppFilesService.UpdateAccount(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string>(), Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<JsonElement?>(), Arg.Any<JsonElement?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<NetAppAccountCreateResult>(new Exception("Test error")));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--resource-group", "myrg",
            "--location", "eastus", "--subscription", "00000000-0000-0000-0000-000000000000"
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
        TestEnvironment.ClearAzureSubscriptionId();
        var expectedAccount = new NetAppAccountCreateResult(
            Id: "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount",
            Name: "myanfaccount",
            Type: "Microsoft.NetApp/netAppAccounts",
            Location: "westus2",
            ResourceGroup: "myrg",
            ProvisioningState: "Succeeded");

        _netAppFilesService.UpdateAccount(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string>(), Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<JsonElement?>(), Arg.Any<JsonElement?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedAccount));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--resource-group", "myrg",
            "--location", "westus2", "--subscription", "00000000-0000-0000-0000-000000000000"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.AccountUpdateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Account);
        Assert.Equal("myanfaccount", result.Account.Name);
        Assert.Equal("westus2", result.Account.Location);
        Assert.Equal("myrg", result.Account.ResourceGroup);
        Assert.Equal("Succeeded", result.Account.ProvisioningState);
        Assert.Equal("Microsoft.NetApp/netAppAccounts", result.Account.Type);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        TestEnvironment.ClearAzureSubscriptionId();
        var account = "myanfaccount";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "00000000-0000-0000-0000-000000000000";

        var expectedAccount = new NetAppAccountCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}",
            Name: account,
            Type: "Microsoft.NetApp/netAppAccounts",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded");

        _netAppFilesService.UpdateAccount(
            account, resourceGroup, location, subscription,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedAccount);

        var args = _commandDefinition.Parse([
            "--account", account, "--resource-group", resourceGroup,
            "--location", location, "--subscription", subscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _netAppFilesService.Received(1).UpdateAccount(
            account, resourceGroup, location, subscription,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithIdsAndExtendedParameters()
    {
        // Arrange
        TestEnvironment.ClearAzureSubscriptionId();
        var id = "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount";
        var expectedAccount = new NetAppAccountCreateResult(
            Id: id,
            Name: "myanfaccount",
            Type: "Microsoft.NetApp/netAppAccounts",
            Location: "eastus",
            ResourceGroup: "myrg",
            ProvisioningState: "Succeeded");

        _netAppFilesService.UpdateAccount(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<JsonElement?>(), Arg.Any<JsonElement?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedAccount);

        var args = _commandDefinition.Parse([
            "--ids", id,
            "--subscription", "00000000-0000-0000-0000-000000000000",
            "--tags", "{\"env\":\"prod\"}",
            "--keySource", "Microsoft.KeyVault",
            "--keyName", "cmkKey",
            "--keyVaultResourceId", "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/myrg/providers/Microsoft.KeyVault/vaults/kv1",
            "--keyVaultUri", "https://kv1.vault.azure.net/",
            "--federatedClientId", "fed-id",
            "--userAssignedIdentity", "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/myrg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/u1",
            "--identityType", "UserAssigned",
            "--userAssignedIdentities", "{\"/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/myrg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/u1\":{}}",
            "--activeDirectories", "[{\"dns\":\"10.0.0.4\"}]",
            "--nfsV4IdDomain", "contoso.local"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _netAppFilesService.Received(1).UpdateAccount(
            "myanfaccount",
            "myrg",
            null,
            "00000000-0000-0000-0000-000000000000",
            Arg.Is<Dictionary<string, string>?>(d => d != null && d["env"] == "prod"),
            "cmkKey",
            "Microsoft.KeyVault",
            "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/myrg/providers/Microsoft.KeyVault/vaults/kv1",
            "https://kv1.vault.azure.net/",
            "fed-id",
            "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/myrg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/u1",
            "UserAssigned",
            Arg.Is<JsonElement?>(j => j.HasValue && j.Value.ValueKind == JsonValueKind.Object),
            Arg.Is<JsonElement?>(j => j.HasValue && j.Value.ValueKind == JsonValueKind.Array),
            "contoso.local",
            null,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesInvalidTagsJson()
    {
        // Arrange
        TestEnvironment.ClearAzureSubscriptionId();
        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--resource-group", "myrg",
            "--location", "eastus", "--subscription", "00000000-0000-0000-0000-000000000000",
            "--tags", "invalid-json"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid tags JSON format", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsNoWait()
    {
        // Arrange
        TestEnvironment.ClearAzureSubscriptionId();
        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--resource-group", "myrg",
            "--subscription", "00000000-0000-0000-0000-000000000000", "--no-wait"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--no-wait", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsGenericUpdateArguments()
    {
        // Arrange
        TestEnvironment.ClearAzureSubscriptionId();
        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--resource-group", "myrg",
            "--subscription", "00000000-0000-0000-0000-000000000000", "--set", "properties.foo=bar"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--set", response.Message);
    }
}
