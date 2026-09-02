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
using Azure.Mcp.Tests.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using Xunit.Sdk;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.Account;

public class AccountCreateCommandTests : SubscriptionCommandUnitTestsBase<AccountCreateCommand, INetAppFilesService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--account myanfaccount --resource-group myrg --location eastus --subscription sub123", true)]
    [InlineData("--resource-group myrg --location eastus --subscription sub123", false)] // Missing account
    [InlineData("--account myanfaccount --location eastus --subscription sub123", false)] // Missing resource-group
    [InlineData("--account myanfaccount --resource-group myrg --subscription sub123", false)] // Missing location
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedAccount = new NetAppAccountCreateResult(
                Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount",
                Name: "myanfaccount",
                Type: "Microsoft.NetApp/netAppAccounts",
                Location: "eastus",
                ResourceGroup: "myrg",
                ProvisioningState: "Succeeded");

            Service.CreateAccount(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
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
    public async Task ExecuteAsync_CreatesAccount_Successfully()
    {
        // Arrange
        var account = "myanfaccount";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";

        var expectedAccount = new NetAppAccountCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}",
            Name: account,
            Type: "Microsoft.NetApp/netAppAccounts",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded");

        Service.CreateAccount(
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

        // Act
        var response = await ExecuteCommandAsync([
            "--account", account, "--resource-group", resourceGroup,
            "--location", location, "--subscription", subscription
        ]);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.AccountCreateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Account);
        Assert.Equal(account, result.Account.Name);
        Assert.Equal(location, result.Account.Location);
        Assert.Equal(resourceGroup, result.Account.ResourceGroup);
        Assert.Equal("Succeeded", result.Account.ProvisioningState);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        Service.CreateAccount(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
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
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--resource-group", "myrg",
            "--location", "eastus", "--subscription", "sub123"
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
        Service.CreateAccount(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
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
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Conflict, "Account already exists"));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--resource-group", "myrg",
            "--location", "eastus", "--subscription", "sub123"
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
        Service.CreateAccount(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
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
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Resource group not found"));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--resource-group", "nonexistentrg",
            "--location", "eastus", "--subscription", "sub123"
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
        Service.CreateAccount(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
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
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--resource-group", "myrg",
            "--location", "eastus", "--subscription", "sub123"
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
        Service.CreateAccount(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
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
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<NetAppAccountCreateResult>(new Exception("Test error")));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--resource-group", "myrg",
            "--location", "eastus", "--subscription", "sub123"
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
        var expectedAccount = new NetAppAccountCreateResult(
            Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount",
            Name: "myanfaccount",
            Type: "Microsoft.NetApp/netAppAccounts",
            Location: "westus2",
            ResourceGroup: "myrg",
            ProvisioningState: "Succeeded");

        Service.CreateAccount(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
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
            Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedAccount));

        // Act
        var response = await ExecuteCommandAsync([
            "--account", "myanfaccount", "--resource-group", "myrg",
            "--location", "westus2", "--subscription", "sub123"
        ]);

        // Assert
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.AccountCreateCommandResult);

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
        var account = "myanfaccount";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";

        var expectedAccount = new NetAppAccountCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}",
            Name: account,
            Type: "Microsoft.NetApp/netAppAccounts",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded");

        Service.CreateAccount(
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

        // Act
        var response = await ExecuteCommandAsync([
            "--account", account, "--resource-group", resourceGroup,
            "--location", location, "--subscription", subscription
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateAccount(
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
    public async Task ExecuteAsync_CallsServiceWithCreateOptionalParameters()
    {
        // Arrange
        var account = "myanfaccount";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";
        var tagsJson = "{\"env\":\"prod\"}";
        var userAssignedIdentitiesJson = "{\"/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/u1\":{}}";
        var activeDirectoriesJson = "[{\"dns\":\"10.0.0.4\"}]";

        var expectedAccount = new NetAppAccountCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}",
            Name: account,
            Type: "Microsoft.NetApp/netAppAccounts",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded");

        Service.CreateAccount(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
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

        // Act
        var response = await ExecuteCommandAsync([
            "--account", account,
            "--resource-group", resourceGroup,
            "--location", location,
            "--subscription", subscription,
            "--tags", tagsJson,
            "--key-name", "cmkKey",
            "--key-source", "Microsoft.KeyVault",
            "--key-vault-resource-id", "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.KeyVault/vaults/kv1",
            "--key-vault-uri", "https://kv1.vault.azure.net/",
            "--federated-client-id", "fed-client-id",
            "--user-assigned-identity", "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/u1",
            "--identity-type", "UserAssigned",
            "--user-assigned-identities", userAssignedIdentitiesJson,
            "--active-directories", activeDirectoriesJson,
            "--nfs-v4-id-domain", "contoso.local"
        ]);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateAccount(
            account,
            resourceGroup,
            location,
            subscription,
            Arg.Is<Dictionary<string, string>?>(d => d != null && d.GetValueOrDefault("env") == "prod"),
            "cmkKey",
            "Microsoft.KeyVault",
            "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.KeyVault/vaults/kv1",
            "https://kv1.vault.azure.net/",
            "fed-client-id",
            "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/u1",
            "UserAssigned",
            Arg.Is<JsonElement?>(value => value.HasValue && value.Value.ValueKind == JsonValueKind.Object),
            Arg.Is<JsonElement?>(value => value.HasValue && value.Value.ValueKind == JsonValueKind.Array),
            "contoso.local",
            null,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }
}
