// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Cosmos.Commands;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Cosmos.Tests;

public class CosmosListCommandTests : SubscriptionCommandUnitTestsBase<CosmosListCommand, ICosmosService>
{
    [Fact]
    public void Name_IsCorrect()
    {
        Assert.Equal("list", Command.Name);
    }

    [Fact]
    public void Description_IsCorrect()
    {
        Assert.NotNull(Command.Description);
        Assert.NotEmpty(Command.Description);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
    }

    [Fact]
    public async Task ExecuteAsync_ListsAccounts_WhenNoAccountOrDatabaseProvided()
    {
        // Arrange
        var expectedAccounts = new List<string> { "account1", "account2" };
        Service.GetCosmosAccounts(
            Arg.Is("sub123"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedAccounts);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, CosmosJsonContext.Default.CosmosListCommandResult);
        Assert.NotNull(result.Accounts);
        Assert.Equal(expectedAccounts, result.Accounts);
        Assert.Null(result.Databases);
        Assert.Null(result.Containers);
    }

    [Fact]
    public async Task ExecuteAsync_ListsDatabases_WhenAccountProvided()
    {
        // Arrange
        var expectedDatabases = new List<string> { "database1", "database2" };
        Service.ListDatabases(
            Arg.Is("account123"),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedDatabases);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--account", "account123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, CosmosJsonContext.Default.CosmosListCommandResult);
        Assert.Null(result.Accounts);
        Assert.NotNull(result.Databases);
        Assert.Equal(expectedDatabases, result.Databases);
        Assert.Null(result.Containers);
    }

    [Fact]
    public async Task ExecuteAsync_ListsContainers_WhenAccountAndDatabaseProvided()
    {
        // Arrange
        var expectedContainers = new List<string> { "container1", "container2" };
        Service.ListContainers(
            Arg.Is("account123"),
            Arg.Is("database123"),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedContainers);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account123",
            "--database", "database123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, CosmosJsonContext.Default.CosmosListCommandResult);
        Assert.Null(result.Accounts);
        Assert.Null(result.Databases);
        Assert.NotNull(result.Containers);
        Assert.Equal(expectedContainers, result.Containers);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoAccountsExist()
    {
        // Arrange
        Service.GetCosmosAccounts(
            Arg.Is("sub123"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, CosmosJsonContext.Default.CosmosListCommandResult);
        Assert.NotNull(result.Accounts);
        Assert.Empty(result.Accounts);
        Assert.Null(result.Databases);
        Assert.Null(result.Containers);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoDatabasesExist()
    {
        // Arrange
        Service.ListDatabases(
            Arg.Is("account123"),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123", "--account", "account123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, CosmosJsonContext.Default.CosmosListCommandResult);
        Assert.Null(result.Accounts);
        Assert.NotNull(result.Databases);
        Assert.Empty(result.Databases);
        Assert.Null(result.Containers);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoContainersExist()
    {
        // Arrange
        Service.ListContainers(
            Arg.Is("account123"),
            Arg.Is("database123"),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account123",
            "--database", "database123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, CosmosJsonContext.Default.CosmosListCommandResult);
        Assert.Null(result.Accounts);
        Assert.Null(result.Databases);
        Assert.NotNull(result.Containers);
        Assert.Empty(result.Containers);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenDatabaseSpecifiedWithoutAccount()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--database", "database123");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--account", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenSubscriptionIsMissing()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenListAccountsThrows()
    {
        // Arrange
        Service.GetCosmosAccounts(
            Arg.Is("sub123"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenListDatabasesThrows()
    {
        // Arrange
        Service.ListDatabases(
            Arg.Is("account123"),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account123");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Access denied", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenListContainersThrows()
    {
        // Arrange
        Service.ListContainers(
            Arg.Is("account123"),
            Arg.Is("database123"),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account123",
            "--database", "database123");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Access denied", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns404_WhenAccountNotFound()
    {
        // Arrange: the service throws KeyNotFoundException when the account does not exist
        Service.ListDatabases(
            Arg.Is("missingaccount"),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new KeyNotFoundException("Cosmos DB account 'missingaccount' not found in subscription 'sub123'."));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "missingaccount");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsResourceGroup_WhenListingDatabases()
    {
        // Arrange
        var expectedDatabases = new List<string> { "database1" };
        Service.ListDatabases(
            Arg.Is("account123"),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            Arg.Is("rg1"),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedDatabases);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--account", "account123",
            "--resource-group", "rg1");

        // Assert
        var result = ValidateAndDeserializeResponse(response, CosmosJsonContext.Default.CosmosListCommandResult);
        Assert.NotNull(result.Databases);
        Assert.Equal(expectedDatabases, result.Databases);
        await Service.Received(1).ListDatabases(
            "account123",
            "sub123",
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            "rg1",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsResourceGroup_WhenListingAccounts()
    {
        // Arrange
        var expectedAccounts = new List<string> { "account1" };
        Service.GetCosmosAccounts(
            Arg.Is("sub123"),
            Arg.Is("rg1"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedAccounts);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1");

        // Assert
        var result = ValidateAndDeserializeResponse(response, CosmosJsonContext.Default.CosmosListCommandResult);
        Assert.NotNull(result.Accounts);
        Assert.Equal(expectedAccounts, result.Accounts);
        await Service.Received(1).GetCosmosAccounts(
            "sub123",
            "rg1",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Returns503_WhenServiceIsUnavailable()
    {
        // Arrange
        Service.GetCosmosAccounts(
            Arg.Is("sub123"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Service Unavailable", null, HttpStatusCode.ServiceUnavailable));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.Status);
        Assert.Contains("Service Unavailable", response.Message);
    }
}
