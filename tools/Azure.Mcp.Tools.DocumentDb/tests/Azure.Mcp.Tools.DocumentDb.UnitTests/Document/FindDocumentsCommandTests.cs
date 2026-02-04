// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.DocumentDb.Commands.Document;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using MongoDB.Bson;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.DocumentDb.UnitTests.Document;

public class FindDocumentsCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<FindDocumentsCommand> _logger;
    private readonly FindDocumentsCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public FindDocumentsCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<FindDocumentsCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_FindsDocuments_WhenQueryIsProvided()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var query = "{\"status\": \"active\"}";
        var expectedResult = new Dictionary<string, object?>
        {
            ["documents"] = new List<string> { "{\"_id\":\"1\",\"status\":\"active\"}" },
            ["total_count"] = 1L,
            ["returned_count"] = 1,
            ["has_more"] = false,
            ["query"] = "{\"status\":\"active\"}",
            ["applied_options"] = new Dictionary<string, object?>
            {
                ["limit"] = 100,
                ["skip"] = 0,
                ["sort"] = null,
                ["projection"] = null
            }
        };

        _documentDbService.FindDocumentsAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument>(),
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--query", query
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_FindsAllDocuments_WhenNoQueryProvided()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var expectedResult = new Dictionary<string, object?>
        {
            ["documents"] = new List<string> { "{\"_id\":\"1\"}", "{\"_id\":\"2\"}" },
            ["total_count"] = 2L,
            ["returned_count"] = 2,
            ["has_more"] = false,
            ["query"] = "{}",
            ["applied_options"] = new Dictionary<string, object?>
            {
                ["limit"] = 100,
                ["skip"] = 0,
                ["sort"] = null,
                ["projection"] = null
            }
        };

        _documentDbService.FindDocumentsAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Is<BsonDocument?>(x => x == null),
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

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
    public async Task ExecuteAsync_FindsDocuments_WhenOptionsProvided()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var query = "{\"status\": \"active\"}";
        var options = "{\"limit\": 10, \"skip\": 5}";
        var expectedResult = new Dictionary<string, object?>
        {
            ["documents"] = new List<string> { "{\"_id\":\"1\"}" },
            ["total_count"] = 15L,
            ["returned_count"] = 1,
            ["has_more"] = true,
            ["query"] = "{\"status\":\"active\"}",
            ["applied_options"] = new Dictionary<string, object?>
            {
                ["limit"] = 10,
                ["skip"] = 5,
                ["sort"] = null,
                ["projection"] = null
            }
        };

        _documentDbService.FindDocumentsAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument>(),
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--query", query,
            "--options", options
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenInvalidJsonInQuery()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var invalidQuery = "{invalid json}";
        var expectedError = "Invalid JSON in query";

        _documentDbService.FindDocumentsAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument?>(),
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--query", invalidQuery
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenCollectionNotFound()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "nonexistent";
        var expectedError = "Collection not found";

        _documentDbService.FindDocumentsAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument?>(),
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyResult_WhenNoDocumentsMatch()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var query = "{\"status\": \"nonexistent\"}";
        var expectedResult = new Dictionary<string, object?>
        {
            ["documents"] = new List<string>(),
            ["total_count"] = 0L,
            ["returned_count"] = 0,
            ["has_more"] = false,
            ["query"] = "{\"status\":\"nonexistent\"}",
            ["applied_options"] = new Dictionary<string, object?>
            {
                ["limit"] = 100,
                ["skip"] = 0,
                ["sort"] = null,
                ["projection"] = null
            }
        };

        _documentDbService.FindDocumentsAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument>(),
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--query", query
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Theory]
    [InlineData("--db-name", "testdb")]
    [InlineData("--collection-name", "testcollection")]
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        // Arrange & Act
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Theory]
    [InlineData("{\"name\": \"test\"}")]
    [InlineData("{\"age\": {\"$gt\": 18}}")]
    [InlineData("{\"$and\": [{\"status\": \"active\"}, {\"verified\": true}]}")]
    public async Task ExecuteAsync_HandlesVariousQueryFormats(string query)
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var expectedResult = new Dictionary<string, object?>
        {
            ["documents"] = new List<string>(),
            ["total_count"] = 0L,
            ["returned_count"] = 0,
            ["has_more"] = false,
            ["query"] = query,
            ["applied_options"] = new Dictionary<string, object?>
            {
                ["limit"] = 100,
                ["skip"] = 0,
                ["sort"] = null,
                ["projection"] = null
            }
        };

        _documentDbService.FindDocumentsAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument?>(),
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--query", query
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }
}
