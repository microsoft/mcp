// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Tools.DocumentDb.Commands.Collection;
using Azure.Mcp.Tools.DocumentDb.Models;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using MongoDB.Bson;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.DocumentDb.UnitTests.Collection;

public class CollectionStatsCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<CollectionStatsCommand> _logger;
    private readonly CollectionStatsCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public CollectionStatsCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<CollectionStatsCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCollectionStats_WhenCollectionExists()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var expectedStats = new BsonDocument
        {
            { "ns", $"{dbName}.{collectionName}" },
            { "count", 100 },
            { "size", 10240 },
            { "storageSize", 20480 }
        };

        _documentDbService.GetCollectionStatsAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = $"Collection statistics for '{collectionName}' retrieved successfully.",
                Data = expectedStats
            });

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns404_WhenCollectionNotFound()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "nonexistent";

        _documentDbService.GetCollectionStatsAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = $"Collection '{collectionName}' was not found in database '{dbName}'."
            });

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_Returns404_WhenDatabaseNotFound()
    {
        // Arrange
        var dbName = "nonexistentdb";
        var collectionName = "testcollection";

        _documentDbService.GetCollectionStatsAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = $"Database '{dbName}' was not found."
            });

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
    }

    [Theory]
    [InlineData("--db-name", "testdb")]
    [InlineData("--collection-name", "testcollection")]
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        // Arrange & Act
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args), TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyStats_WhenCollectionIsEmpty()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "emptycollection";
        var emptyStats = new BsonDocument
        {
            { "ns", $"{dbName}.{collectionName}" },
            { "count", 0 },
            { "size", 0 }
        };

        _documentDbService.GetCollectionStatsAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = $"Collection statistics for '{collectionName}' retrieved successfully.",
                Data = emptyStats
            });

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }
}