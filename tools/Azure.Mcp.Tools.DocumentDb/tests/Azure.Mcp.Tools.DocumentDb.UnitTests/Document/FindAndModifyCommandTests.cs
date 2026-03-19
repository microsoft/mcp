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

public class FindAndModifyCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<FindAndModifyCommand> _logger;
    private readonly FindAndModifyCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public FindAndModifyCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<FindAndModifyCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_FindsAndModifiesDocument_WhenMatchFound()
    {
        var connectionString = "mongodb://localhost";
        var dbName = "testdb";
        var collectionName = "testcollection";
        var filter = "{\"status\": \"pending\"}";
        var update = "{\"$set\": {\"status\": \"processing\"}}";
        var expectedResult = new DocumentDbResponse
        {
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Find and modify completed successfully",
            Data = new Dictionary<string, object?>
            {
                ["original_document"] = "{\"_id\":\"123\",\"status\":\"processing\"}",
                ["matched"] = true
            }
        };

        _documentDbService.FindAndModifyAsync(
            Arg.Is(connectionString),
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument>(),
            Arg.Any<BsonDocument>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--connection-string", connectionString,
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--filter", filter,
            "--update", update
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenCollectionNotFound()
    {
        var connectionString = "mongodb://localhost";
        var dbName = "testdb";
        var collectionName = "testcollection";
        var filter = "{\"_id\": \"123\"}";
        var update = "{\"$set\": {\"name\": \"updated\"}}";

        _documentDbService.FindAndModifyAsync(
            Arg.Is(connectionString),
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument>(),
            Arg.Any<BsonDocument>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Failed to find and modify document: Collection not found"
            });

        var args = _commandDefinition.Parse([
            "--connection-string", connectionString,
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--filter", filter,
            "--update", update
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.StartsWith("Failed to find and modify document: Collection not found", response.Message);
    }
}