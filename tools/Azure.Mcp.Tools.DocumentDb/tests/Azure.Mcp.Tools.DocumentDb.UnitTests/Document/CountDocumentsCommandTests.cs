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

public class CountDocumentsCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<CountDocumentsCommand> _logger;
    private readonly CountDocumentsCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public CountDocumentsCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<CountDocumentsCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_CountsDocuments_WhenQueryIsProvided()
    {
        var connectionString = "mongodb://localhost";
        var dbName = "testdb";
        var collectionName = "testcollection";
        var query = "{\"status\": \"active\"}";
        var expectedResult = new DocumentDbResponse
        {
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Documents counted successfully",
            Data = new Dictionary<string, object?>
            {
                ["count"] = 42L,
                ["query"] = "{\"status\":\"active\"}"
            }
        };

        _documentDbService.CountDocumentsAsync(
            Arg.Is(connectionString),
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--connection-string", connectionString,
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
    public async Task ExecuteAsync_CountsAllDocuments_WhenNoQueryProvided()
    {
        var connectionString = "mongodb://localhost";
        var dbName = "testdb";
        var collectionName = "testcollection";
        var expectedResult = new DocumentDbResponse
        {
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Documents counted successfully",
            Data = new Dictionary<string, object?>
            {
                ["count"] = 100L,
                ["query"] = "{}"
            }
        };

        _documentDbService.CountDocumentsAsync(
            Arg.Is(connectionString),
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Is<BsonDocument?>(x => x == null),
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
    public async Task ExecuteAsync_Returns400_WhenCollectionNotFound()
    {
        var connectionString = "mongodb://localhost";
        var dbName = "testdb";
        var collectionName = "nonexistent";

        _documentDbService.CountDocumentsAsync(
            Arg.Is(connectionString),
            Arg.Is(dbName),
            Arg.Is(collectionName),
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

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }
}