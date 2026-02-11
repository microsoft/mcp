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

public class AggregateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<AggregateCommand> _logger;
    private readonly AggregateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public AggregateCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<AggregateCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_ExecutesAggregation_WhenValidPipelineProvided()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var pipeline = "[{\"$match\": {\"status\": \"active\"}}, {\"$group\": {\"_id\": \"$category\", \"count\": {\"$sum\": 1}}}]";
        var expectedResult = new Dictionary<string, object?>
        {
            ["success"] = true,
            ["statusCode"] = HttpStatusCode.OK,
            ["message"] = "Aggregation executed successfully",
            ["data"] = new Dictionary<string, object?>
            {
                ["results"] = new List<string>
                {
                    "{\"_id\":\"electronics\",\"count\":25}",
                    "{\"_id\":\"books\",\"count\":15}"
                },
                ["result_count"] = 2
            }
        };

        _documentDbService.AggregateAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<List<BsonDocument>>(),
            Arg.Any<bool>(),
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
    public async Task ExecuteAsync_ExecutesWithAllowDiskUse_WhenFlagSet()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var pipeline = "[{\"$sort\": {\"created_at\": -1}}]";
        var expectedResult = new Dictionary<string, object?>
        {
            ["success"] = true,
            ["statusCode"] = HttpStatusCode.OK,
            ["message"] = "Aggregation executed successfully",
            ["data"] = new Dictionary<string, object?>
            {
                ["results"] = new List<string>(),
                ["result_count"] = 0
            }
        };

        _documentDbService.AggregateAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<List<BsonDocument>>(),
            Arg.Is(true),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--pipeline", pipeline,
            "--allow-disk-use", "true"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenAggregationFails()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var pipeline = "[{\"$invalidStage\": {}}]";

        _documentDbService.AggregateAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<List<BsonDocument>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, object?>
            {
                ["success"] = false,
                ["statusCode"] = HttpStatusCode.InternalServerError,
                ["message"] = "Failed to execute aggregation: Invalid pipeline stage",
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
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith("Failed to execute aggregation: Invalid pipeline stage", response.Message);
    }
}
