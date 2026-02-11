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

public class ExplainFindQueryCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<ExplainFindQueryCommand> _logger;
    private readonly ExplainFindQueryCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ExplainFindQueryCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<ExplainFindQueryCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_ExplainsQuery_WhenValidQueryProvided()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var query = "{\"status\": \"active\"}";
        var expectedResult = new Dictionary<string, object?>
        {
            ["success"] = true,
            ["statusCode"] = HttpStatusCode.OK,
            ["message"] = "Query plan retrieved successfully",
            ["data"] = new Dictionary<string, object?>
            {
                ["queryPlanner"] = new Dictionary<string, object?>
                {
                    ["winningPlan"] = new Dictionary<string, object?> { ["stage"] = "COLLSCAN" }
                },
                ["executionStats"] = new Dictionary<string, object?>
                {
                    ["nReturned"] = 10,
                    ["executionTimeMillis"] = 5
                }
            }
        };

        _documentDbService.ExplainFindQueryAsync(
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
    public async Task ExecuteAsync_Returns400_WhenCollectionNotFound()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "nonexistent";
        var query = "{\"_id\": \"123\"}";

        _documentDbService.ExplainFindQueryAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument?>(),
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, object?>
            {
                ["success"] = false,
                ["statusCode"] = HttpStatusCode.BadRequest,
                ["message"] = "Failed to explain find query: Collection not found",
                ["data"] = null
            });

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--query", query
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.StartsWith("Failed to explain find query: Collection not found", response.Message);
    }
}
