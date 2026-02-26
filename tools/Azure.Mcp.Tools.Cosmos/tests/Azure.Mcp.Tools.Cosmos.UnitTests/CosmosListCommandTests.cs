// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Cosmos.Commands;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Cosmos.UnitTests;

public class CosmosListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICosmosService _cosmosService;
    private readonly ILogger<CosmosListCommand> _logger;

    public CosmosListCommandTests()
    {
        _cosmosService = Substitute.For<ICosmosService>();
        _logger = Substitute.For<ILogger<CosmosListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_cosmosService);
        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public void Name_IsCorrect()
    {
        var command = new CosmosListCommand(_logger);
        Assert.Equal("list", command.Name);
    }

    [Fact]
    public void Description_IsCorrect()
    {
        var command = new CosmosListCommand(_logger);
        Assert.Contains("accounts", command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("databases", command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("containers", command.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        var command = new CosmosListCommand(_logger);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.ReadOnly);
    }

    [Fact]
    public async Task ExecuteAsync_ListsAccounts_WhenNoAccountOrDatabaseProvided()
    {
        // Arrange
        var expectedAccounts = new List<string> { "account1", "account2" };
        _cosmosService.GetCosmosAccounts(
            Arg.Is("sub123"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedAccounts);

        var command = new CosmosListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123"]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, CosmosJsonContext.Default.CosmosListCommandResult);
        Assert.NotNull(result);
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
        _cosmosService.ListDatabases(
            Arg.Is("account123"),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedDatabases);

        var command = new CosmosListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--account", "account123"
        ]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, CosmosJsonContext.Default.CosmosListCommandResult);
        Assert.NotNull(result);
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
        _cosmosService.ListContainers(
            Arg.Is("account123"),
            Arg.Is("database123"),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedContainers);

        var command = new CosmosListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--account", "account123",
            "--database", "database123"
        ]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, CosmosJsonContext.Default.CosmosListCommandResult);
        Assert.NotNull(result);
        Assert.Null(result.Accounts);
        Assert.Null(result.Databases);
        Assert.NotNull(result.Containers);
        Assert.Equal(expectedContainers, result.Containers);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoAccountsExist()
    {
        // Arrange
        _cosmosService.GetCosmosAccounts(
            Arg.Is("sub123"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var command = new CosmosListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123"]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, CosmosJsonContext.Default.CosmosListCommandResult);
        Assert.NotNull(result);
        Assert.NotNull(result.Accounts);
        Assert.Empty(result.Accounts);
        Assert.Null(result.Databases);
        Assert.Null(result.Containers);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoDatabasesExist()
    {
        // Arrange
        _cosmosService.ListDatabases(
            Arg.Is("account123"),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var command = new CosmosListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--account", "account123"
        ]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, CosmosJsonContext.Default.CosmosListCommandResult);
        Assert.NotNull(result);
        Assert.Null(result.Accounts);
        Assert.NotNull(result.Databases);
        Assert.Empty(result.Databases);
        Assert.Null(result.Containers);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoContainersExist()
    {
        // Arrange
        _cosmosService.ListContainers(
            Arg.Is("account123"),
            Arg.Is("database123"),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var command = new CosmosListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--account", "account123",
            "--database", "database123"
        ]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, CosmosJsonContext.Default.CosmosListCommandResult);
        Assert.NotNull(result);
        Assert.Null(result.Accounts);
        Assert.Null(result.Databases);
        Assert.NotNull(result.Containers);
        Assert.Empty(result.Containers);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenDatabaseSpecifiedWithoutAccount()
    {
        // Arrange
        var command = new CosmosListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--database", "database123"
        ]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--account", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenSubscriptionIsMissing()
    {
        // Arrange
        var command = new CosmosListCommand(_logger);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, command.GetCommand().Parse([]), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenListAccountsThrows()
    {
        // Arrange
        _cosmosService.GetCosmosAccounts(
            Arg.Is("sub123"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var command = new CosmosListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123"]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenListDatabasesThrows()
    {
        // Arrange
        _cosmosService.ListDatabases(
            Arg.Is("account123"),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var command = new CosmosListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--account", "account123"
        ]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Access denied", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenListContainersThrows()
    {
        // Arrange
        _cosmosService.ListContainers(
            Arg.Is("account123"),
            Arg.Is("database123"),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var command = new CosmosListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--account", "account123",
            "--database", "database123"
        ]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Access denied", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns503_WhenServiceIsUnavailable()
    {
        // Arrange
        _cosmosService.GetCosmosAccounts(
            Arg.Is("sub123"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Service Unavailable", null, HttpStatusCode.ServiceUnavailable));

        var command = new CosmosListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123"]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.Status);
        Assert.Contains("Service Unavailable", response.Message);
    }
}
