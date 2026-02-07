// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
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

public class UpdateDocumentCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<UpdateDocumentCommand> _logger;
    private readonly UpdateDocumentCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public UpdateDocumentCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<UpdateDocumentCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesDocument_WhenMatchFound()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var filter = "{\"_id\": \"123\"}";
        var update = "{\"$set\": {\"name\": \"updated\"}}";
        var expectedResult = new Dictionary<string, object?>
        {
            ["success"] = true,
            ["statusCode"] = HttpStatusCode.OK,
            ["message"] = "Document updated successfully",
            ["data"] = new Dictionary<string, object?>
            {
                ["matched_count"] = 1L,
                ["modified_count"] = 1L,
                ["acknowledged"] = true
            }
        };

        _documentDbService.UpdateDocumentAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument>(),
            Arg.Any<BsonDocument>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--filter", filter,
            "--update", update
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesWithUpsert_WhenUpsertFlagSet()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var filter = "{\"_id\": \"new123\"}";
        var update = "{\"$set\": {\"name\": \"new\"}}";
        var expectedResult = new Dictionary<string, object?>
        {
            ["success"] = true,
            ["statusCode"] = HttpStatusCode.OK,
            ["message"] = "Document upserted successfully",
            ["data"] = new Dictionary<string, object?>
            {
                ["matched_count"] = 0L,
                ["modified_count"] = 0L,
                ["upserted_id"] = "new123",
                ["acknowledged"] = true
            }
        };

        _documentDbService.UpdateDocumentAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument>(),
            Arg.Any<BsonDocument>(),
            Arg.Is(true),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--filter", filter,
            "--update", update,
            "--upsert", "true"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenUpdateFails()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var filter = "{\"_id\": \"123\"}";
        var update = "{\"$set\": {\"name\": \"updated\"}}";

        _documentDbService.UpdateDocumentAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument>(),
            Arg.Any<BsonDocument>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, object?>
            {
                ["success"] = false,
                ["statusCode"] = HttpStatusCode.InternalServerError,
                ["message"] = "Failed to update document: Invalid update operation",
                ["data"] = null
            });

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--filter", filter,
            "--update", update
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith("Failed to update document: Invalid update operation", response.Message);
    }
}
