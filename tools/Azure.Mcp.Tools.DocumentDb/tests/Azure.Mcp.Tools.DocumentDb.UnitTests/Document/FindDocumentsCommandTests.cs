// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Tools.DocumentDb.Commands.Document;
using Azure.Mcp.Tools.DocumentDb.Models;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using MongoDB.Bson;
using NSubstitute;
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
    public async Task ExecuteAsync_FindsDocuments_WhenFilterIsProvided()
    {
        var connectionString = "mongodb://localhost";
        var dbName = "testdb";
        var collectionName = "testcollection";
        var filter = "{\"status\": \"active\"}";
        var expectedResult = new DocumentDbResponse
        {
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Documents retrieved successfully",
            Data = new Dictionary<string, object?>
            {
                ["documents"] = new List<string> { "{\"_id\":\"1\",\"status\":\"active\"}" },
                ["total_count"] = 1L,
                ["returned_count"] = 1,
                ["has_more"] = false,
                ["filter"] = "{\"status\":\"active\"}",
                ["applied_options"] = new Dictionary<string, object?>
                {
                    ["limit"] = 100,
                    ["skip"] = 0,
                    ["sort"] = null,
                    ["projection"] = null
                }
            }
        };

        _documentDbService.FindDocumentsAsync(
            Arg.Is(connectionString),
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument>(),
            Arg.Any<BsonDocument?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--connection-string", connectionString,
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--filter", filter
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_FindsAllDocuments_WhenNoFilterProvided()
    {
        var connectionString = "mongodb://localhost";
        var dbName = "testdb";
        var collectionName = "testcollection";
        var expectedResult = new DocumentDbResponse
        {
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Documents retrieved successfully",
            Data = new Dictionary<string, object?>
            {
                ["documents"] = new List<string> { "{\"_id\":\"1\"}", "{\"_id\":\"2\"}" },
                ["total_count"] = 2L,
                ["returned_count"] = 2,
                ["has_more"] = false,
                ["filter"] = "{}",
                ["applied_options"] = new Dictionary<string, object?>
                {
                    ["limit"] = 100,
                    ["skip"] = 0,
                    ["sort"] = null,
                    ["projection"] = null
                }
            }
        };

        _documentDbService.FindDocumentsAsync(
            Arg.Is(connectionString),
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Is<BsonDocument?>(x => x == null),
            Arg.Any<BsonDocument?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--connection-string", connectionString,
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
        var connectionString = "mongodb://localhost";
        var dbName = "testdb";
        var collectionName = "testcollection";
        var filter = "{\"status\": \"active\"}";
        var options = "{\"limit\": 10, \"skip\": 5}";
        var expectedResult = new DocumentDbResponse
        {
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Documents retrieved successfully",
            Data = new Dictionary<string, object?>
            {
                ["documents"] = new List<string> { "{\"_id\":\"1\"}" },
                ["total_count"] = 15L,
                ["returned_count"] = 1,
                ["has_more"] = true,
                ["filter"] = "{\"status\":\"active\"}",
                ["applied_options"] = new Dictionary<string, object?>
                {
                    ["limit"] = 10,
                    ["skip"] = 5,
                    ["sort"] = null,
                    ["projection"] = null
                }
            }
        };

        _documentDbService.FindDocumentsAsync(
            Arg.Is(connectionString),
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument>(),
            Arg.Any<BsonDocument?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--connection-string", connectionString,
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--filter", filter,
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
    public async Task ExecuteAsync_Returns400_WhenInvalidJsonInFilter()
    {
        var connectionString = "mongodb://localhost";
        var dbName = "testdb";
        var collectionName = "testcollection";
        var invalidFilter = "{invalid json}";

        var args = _commandDefinition.Parse([
            "--connection-string", connectionString,
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--filter", invalidFilter
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("not valid bson/json", response.Message.ToLowerInvariant());
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenCollectionNotFound()
    {
        var connectionString = "mongodb://localhost";
        var dbName = "testdb";
        var collectionName = "nonexistent";

        _documentDbService.FindDocumentsAsync(
            Arg.Is(connectionString),
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument?>(),
            Arg.Any<BsonDocument?>(),
            Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse { Success = false, StatusCode = HttpStatusCode.BadRequest, Message = "Collection 'nonexistent' not found" });

        var args = _commandDefinition.Parse([
            "--connection-string", connectionString,
            "--db-name", dbName,
            "--collection-name", collectionName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Theory]
    [InlineData("--connection-string", "mongodb://localhost", "--db-name", "testdb")]
    [InlineData("--connection-string", "mongodb://localhost", "--collection-name", "testcollection")]
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }
}