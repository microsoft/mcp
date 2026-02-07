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

public class InsertManyCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<InsertManyCommand> _logger;
    private readonly InsertManyCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public InsertManyCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<InsertManyCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_InsertsMultipleDocuments_WhenValidDocumentsProvided()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var documents = "[{\"name\": \"test1\"}, {\"name\": \"test2\"}]";
        var expectedResult = new Dictionary<string, object?>
        {
            ["success"] = true,
            ["statusCode"] = HttpStatusCode.OK,
            ["message"] = "Documents inserted successfully",
            ["data"] = new Dictionary<string, object?>
            {
                ["inserted_count"] = 2,
                ["inserted_ids"] = new List<string> { "id1", "id2" },
                ["acknowledged"] = true
            }
        };

        _documentDbService.InsertManyAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<List<BsonDocument>>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--documents", documents
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenInsertFails()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var documents = "[{\"name\": \"test\"}]";

        _documentDbService.InsertManyAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<List<BsonDocument>>(),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, object?>
            {
                ["success"] = false,
                ["statusCode"] = HttpStatusCode.InternalServerError,
                ["message"] = "Failed to insert documents: Duplicate key error",
                ["data"] = null
            });

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--documents", documents
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith("Failed to insert documents: Duplicate key error", response.Message);
    }

    [Theory]
    [InlineData("--db-name", "testdb", "--collection-name", "coll")]
    [InlineData("--db-name", "testdb", "--documents", "[{\"a\":1}]")]
    [InlineData("--collection-name", "coll", "--documents", "[{\"a\":1}]")]
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        // Arrange & Act
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }
}
