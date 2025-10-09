// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Tables.Commands;
using Azure.Mcp.Tools.Tables.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Tables.UnitTests;

public class TablesListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITablesService _tablesService;
    private readonly ILogger<TablesListCommand> _logger;
    private readonly TablesListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownStorageAccount = "storage123";
    private readonly string _knownCosmosDbAccount = "cosmosdb123";
    private readonly string _knownSubscription = "sub123";

    public TablesListCommandTests()
    {
        _tablesService = Substitute.For<ITablesService>();
        _logger = Substitute.For<ILogger<TablesListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_tablesService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsStorageTables()
    {
        // Arrange
        var expectedTables = new List<string> { "table1", "table2" };

        _tablesService.ListTables(
            Arg.Is(_knownStorageAccount),
            false,
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedTables);

        var args = _commandDefinition.Parse([
            "--storage-account", _knownStorageAccount,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, TablesJsonContext.Default.TablesListCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedTables, result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCosmosDbTables()
    {
        // Arrange
        var expectedTables = new List<string> { "table1", "table2" };

        _tablesService.ListTables(
            Arg.Is(_knownCosmosDbAccount),
            true,
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedTables);

        var args = _commandDefinition.Parse([
            "--cosmosdb-account", _knownCosmosDbAccount,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, TablesJsonContext.Default.TablesListCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedTables, result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoStorageTables()
    {
        // Arrange
        _tablesService.ListTables(
            Arg.Is(_knownStorageAccount),
            false,
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var args = _commandDefinition.Parse([
            "--storage-account", _knownStorageAccount,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, TablesJsonContext.Default.TablesListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoCosmosDbTables()
    {
        // Arrange
        _tablesService.ListTables(
            Arg.Is(_knownCosmosDbAccount),
            true,
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var args = _commandDefinition.Parse([
            "--cosmosdb-account", _knownCosmosDbAccount,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, TablesJsonContext.Default.TablesListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Tables);
    }

    [Theory]
    [InlineData("--storage-account mystorageaccount")] // Missing subscription
    [InlineData("--cosmosdb-account mycosmosdbaccount")] // Missing subscription
    [InlineData("")] // No arguments
    public async Task ExecuteAsync_ValidatesMissingSubscriptionCorrectly(string args)
    {
        // Arrange
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("--subscription sub123")] // Missing Storage and Cosmos DB account
    [InlineData("--storage-account mystorageaccount --cosmosdb-account mycosmosdbaccount --subscription sub123")] // Both Storage and Cosmos DB account
    [InlineData("")] // No arguments
    public async Task ExecuteAsync_ValidatesIncorrectAccountInputCorrectly(string args)
    {
        // Arrange
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Contains("one of", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesStorageException()
    {
        // Arrange
        var expectedError = "Test error";

        _tablesService.ListTables(
            Arg.Is(_knownStorageAccount),
            false,
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _command.GetCommand().Parse([
            "--storage-account", _knownStorageAccount,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesCosmosDbException()
    {
        // Arrange
        var expectedError = "Test error";

        _tablesService.ListTables(
            Arg.Is(_knownCosmosDbAccount),
            true,
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _command.GetCommand().Parse([
            "--cosmosdb-account", _knownCosmosDbAccount,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
