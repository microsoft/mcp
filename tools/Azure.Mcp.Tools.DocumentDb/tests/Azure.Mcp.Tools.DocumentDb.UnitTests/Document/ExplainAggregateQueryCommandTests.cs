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

public class ExplainAggregateQueryCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<ExplainAggregateQueryCommand> _logger;
    private readonly ExplainAggregateQueryCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ExplainAggregateQueryCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<ExplainAggregateQueryCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_ExplainsAggregationPipeline_WhenValidPipelineProvided()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var pipeline = "[{\"$match\": {\"status\": \"active\"}}, {\"$group\": {\"_id\": \"$category\", \"total\": {\"$sum\": 1}}}]";
        var expectedResult = new Dictionary<string, object?>
        {
            ["success"] = true,
            ["statusCode"] = HttpStatusCode.OK,
            ["message"] = "Aggregation query plan retrieved successfully",
            ["data"] = new Dictionary<string, object?>
            {
                ["stages"] = new List<Dictionary<string, object?>>
                {
                    new() { ["$cursor"] = new Dictionary<string, object?> { ["queryPlanner"] = new Dictionary<string, object?> { ["stage"] = "COLLSCAN" } } },
                    new() { ["$group"] = new Dictionary<string, object?> { ["_id"] = "$category" } }
                },
                ["executionStats"] = new Dictionary<string, object?>
                {
                    ["nReturned"] = 5,
                    ["executionTimeMillis"] = 10
                }
            }
        };

        _documentDbService.ExplainAggregateQueryAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<List<BsonDocument>>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--pipeline", pipeline
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenCollectionNotFound()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "nonexistent";
        var pipeline = "[{\"$match\": {\"_id\": \"123\"}}]";

        _documentDbService.ExplainAggregateQueryAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<List<BsonDocument>>(),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, object?>
            {
                ["success"] = false,
                ["statusCode"] = HttpStatusCode.BadRequest,
                ["message"] = "Failed to explain aggregate query: Collection not found",
                ["data"] = null
            });

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--pipeline", pipeline
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.StartsWith("Failed to explain aggregate query: Collection not found", response.Message);
    }
}
