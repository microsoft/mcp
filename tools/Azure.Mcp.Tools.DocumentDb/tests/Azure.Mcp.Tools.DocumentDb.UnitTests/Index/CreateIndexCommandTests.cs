// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Tools.DocumentDb.Commands.Index;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using MongoDB.Bson;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.DocumentDb.UnitTests.Index;

public class CreateIndexCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<CreateIndexCommand> _logger;
    private readonly CreateIndexCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public CreateIndexCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<CreateIndexCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesIndex_WhenValidKeysProvided()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var keys = "{\"status\": 1}";
        var expectedResult = new Dictionary<string, object?>
        {
            ["success"] = true,
            ["statusCode"] = HttpStatusCode.OK,
            ["message"] = "Index created successfully",
            ["data"] = new Dictionary<string, object?>
            {
                ["index_name"] = "status_1",
                ["acknowledged"] = true
            }
        };

        _documentDbService.CreateIndexAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument>(),
            Arg.Any<BsonDocument>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--keys", keys
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesIndexWithOptions_WhenOptionsProvided()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var keys = "{\"email\": 1}";
        var options = "{\"unique\": true}";
        var expectedResult = new Dictionary<string, object?>
        {
            ["success"] = true,
            ["statusCode"] = HttpStatusCode.OK,
            ["message"] = "Index created successfully",
            ["data"] = new Dictionary<string, object?>
            {
                ["index_name"] = "email_1",
                ["acknowledged"] = true,
                ["unique"] = true
            }
        };

        _documentDbService.CreateIndexAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument>(),
            Arg.Any<BsonDocument>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--keys", keys,
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
    public async Task ExecuteAsync_Returns400_WhenCollectionNotFound()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "nonexistent";
        var keys = "{\"status\": 1}";

        _documentDbService.CreateIndexAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument>(),
            Arg.Any<BsonDocument>(),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, object?>
            {
                ["success"] = false,
                ["statusCode"] = HttpStatusCode.BadRequest,
                ["message"] = "Collection 'nonexistent' not found",
                ["data"] = null
            });

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--keys", keys
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Theory]
    [InlineData("--db-name", "testdb", "--collection-name", "coll")]
    [InlineData("--db-name", "testdb", "--keys", "{\"a\":1}")]
    [InlineData("--collection-name", "coll", "--keys", "{\"a\":1}")]
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        // Arrange & Act
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }
}
