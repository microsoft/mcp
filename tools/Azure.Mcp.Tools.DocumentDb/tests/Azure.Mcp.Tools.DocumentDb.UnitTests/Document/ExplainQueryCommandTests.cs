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

public class ExplainQueryCommandTests
{
    private readonly IDocumentDbService _documentDbService = Substitute.For<IDocumentDbService>();
    private readonly ExplainQueryCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ExplainQueryCommandTests()
    {
        _command = new ExplainQueryCommand(Substitute.For<ILogger<ExplainQueryCommand>>());
        _commandDefinition = _command.GetCommand();
        _context = new CommandContext(new ServiceCollection().AddSingleton(_documentDbService).BuildServiceProvider());
    }

    [Fact]
    public async Task ExecuteAsync_ExplainsFind_WhenOperationFind()
    {
        _documentDbService.ExplainFindQueryAsync(Arg.Is("mongodb://localhost"), Arg.Is("testdb"), Arg.Is("testcollection"), Arg.Any<BsonDocument?>(), Arg.Any<BsonDocument?>(), Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse { Success = true, StatusCode = HttpStatusCode.OK, Data = new Dictionary<string, object?> { ["explain"] = "{}" } });

        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(["--connection-string", "mongodb://localhost", "--db-name", "testdb", "--collection-name", "testcollection", "--operation", "find", "--query-body", "{\"filter\":{\"status\":\"active\"},\"options\":{\"limit\":1}}"]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _documentDbService.Received(1).ExplainFindQueryAsync(
            "mongodb://localhost",
            "testdb",
            "testcollection",
            Arg.Is<BsonDocument?>(doc => doc != null && doc["status"] == "active"),
            Arg.Is<BsonDocument?>(doc => doc != null && doc["limit"] == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ExplainsCount_WhenOperationCount()
    {
        _documentDbService.ExplainCountQueryAsync(Arg.Is("mongodb://localhost"), Arg.Is("testdb"), Arg.Is("testcollection"), Arg.Any<BsonDocument?>(), Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse { Success = true, StatusCode = HttpStatusCode.OK, Data = new Dictionary<string, object?> { ["explain"] = "{}" } });

        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(["--connection-string", "mongodb://localhost", "--db-name", "testdb", "--collection-name", "testcollection", "--operation", "count", "--query-body", "{\"filter\":{\"status\":\"active\"}}"]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _documentDbService.Received(1).ExplainCountQueryAsync(
            "mongodb://localhost",
            "testdb",
            "testcollection",
            Arg.Is<BsonDocument?>(doc => doc != null && doc["status"] == "active"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ExplainsAggregate_WhenOperationAggregate()
    {
        _documentDbService.ExplainAggregateQueryAsync(Arg.Is("mongodb://localhost"), Arg.Is("testdb"), Arg.Is("testcollection"), Arg.Any<List<BsonDocument>>(), Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse { Success = true, StatusCode = HttpStatusCode.OK, Data = new Dictionary<string, object?> { ["explain"] = "{}" } });

        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(["--connection-string", "mongodb://localhost", "--db-name", "testdb", "--collection-name", "testcollection", "--operation", "aggregate", "--query-body", "{\"pipeline\":[{\"$match\":{\"status\":\"active\"}}]}" ]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _documentDbService.Received(1).ExplainAggregateQueryAsync(
            "mongodb://localhost",
            "testdb",
            "testcollection",
            Arg.Is<List<BsonDocument>>(pipeline => pipeline.Count == 1 && pipeline[0]["$match"].AsBsonDocument["status"] == "active"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenAggregateQueryBodyMissingPipeline()
    {
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(["--connection-string", "mongodb://localhost", "--db-name", "testdb", "--collection-name", "testcollection", "--operation", "aggregate", "--query-body", "{\"filter\":{\"status\":\"active\"}}"]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }
}