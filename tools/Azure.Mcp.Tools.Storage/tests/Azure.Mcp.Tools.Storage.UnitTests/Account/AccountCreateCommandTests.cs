// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Commands.Account;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.UnitTests.Account;

public class AccountCreateCommandTests : CommandUnitTestsBase<AccountCreateCommand, IStorageService>
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
    [InlineData("--account testaccount --resource-group testrg --location eastus --subscription sub123", true)]
    [InlineData("--account testaccount --resource-group testrg --location eastus --sku Standard_GRS --subscription sub123", true)]
    [InlineData("--account testaccount --resource-group testrg --location eastus --access-tier Cool --subscription sub123", true)]
    [InlineData("--resource-group testrg --location eastus --subscription sub123", false)] // Missing account name
    [InlineData("--account testaccount --location eastus --subscription sub123", false)] // Missing resource group
    [InlineData("--account testaccount --resource-group testrg --subscription sub123", false)] // Missing location
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var properties = new Dictionary<string, object>
            {
                { "hnsEnabled", false },
                { "provisioningState", "Succeeded" },
                { "creationTime", DateTimeOffset.UtcNow.ToString("o") },
                { "allowBlobPublicAccess", false },
                { "enableHttpsTrafficOnly", true }
            };
            var expectedAccount = new StorageAccountResult(
                HasData: true,
                Id: "/subscriptions/sub123/resourceGroups/testrg/providers/Microsoft.Storage/storageAccounts/testaccount",
                Name: "testaccount",
                Type: "Microsoft.Storage/storageAccounts",
                Location: "eastus",
                SkuName: "Standard_LRS",
                SkuTier: "Standard",
                Kind: "StorageV2",
                Properties: properties);

            Service.CreateStorageAccount(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<bool?>(),
                Arg.Any<string>(),
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
            var result = ValidateAndDeserializeResponse(response, StorageJsonContext.Default.AccountCreateCommandResult);
            Assert.NotNull(result.Account);
            Assert.Equal("testaccount", result.Account.Name);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesStorageAccountNameAlreadyExists()
    {
        // Arrange
        var conflictException = new RequestFailedException((int)HttpStatusCode.Conflict, "Storage account name already exists");

        Service.CreateStorageAccount(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(conflictException);

        // Act
        var response = await ExecuteCommandAsync(
            "--account", "existingaccount",
            "--resource-group", "testrg",
            "--location", "eastus",
            "--subscription", "sub123");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("already exists", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesResourceGroupNotFound()
    {
        // Arrange
        var notFoundException = new RequestFailedException((int)HttpStatusCode.NotFound, "Resource group not found");

        Service.CreateStorageAccount(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(notFoundException);

        // Act
        var response = await ExecuteCommandAsync(
            "--account", "testaccount",
            "--resource-group", "nonexistentrg",
            "--location", "eastus",
            "--subscription", "sub123");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        var authException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed");

        Service.CreateStorageAccount(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(authException);

        // Act
        var response = await ExecuteCommandAsync(
            "--account", "testaccount",
            "--resource-group", "testrg",
            "--location", "eastus",
            "--subscription", "sub123");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.CreateStorageAccount(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<StorageAccountResult>(new Exception("Test error")));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", "testaccount",
            "--resource-group", "testrg",
            "--location", "eastus",
            "--subscription", "sub123");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var properties = new Dictionary<string, object>
            {
                { "hnsEnabled", true },
                { "provisioningState", "Succeeded" },
                { "creationTime", DateTimeOffset.UtcNow.ToString("o") },
                { "allowBlobPublicAccess", false },
                { "enableHttpsTrafficOnly", true }
            };
        var expectedAccount = new StorageAccountResult(
            HasData: true,
            Id: "/subscriptions/sub123/resourceGroups/testrg/providers/Microsoft.Storage/storageAccounts/testaccount",
            Name: "testaccount",
            Type: "Microsoft.Storage/storageAccounts",
            Location: "eastus",
            SkuName: "Standard_GRS",
            SkuTier: "Standard",
            Kind: "StorageV2",
            Properties: properties);

        Service.CreateStorageAccount(
            "testaccount",
            "testrg",
            "eastus",
            "sub123",
            "Standard_GRS",
            "Cool",
            true,
            null,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedAccount);

        // Act
        var response = await ExecuteCommandAsync(
            "--account", "testaccount",
            "--resource-group", "testrg",
            "--location", "eastus",
            "--subscription", "sub123",
            "--sku", "Standard_GRS",
            "--access-tier", "Cool",
            "--enable-hierarchical-namespace", "true");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateStorageAccount(
            "testaccount",
            "testrg",
            "eastus",
            "sub123",
            "Standard_GRS",
            "Cool",
            true,
            null,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }
}
