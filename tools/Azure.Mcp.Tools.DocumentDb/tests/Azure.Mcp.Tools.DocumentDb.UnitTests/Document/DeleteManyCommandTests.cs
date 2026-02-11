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

public class DeleteManyCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<DeleteManyCommand> _logger;
    private readonly DeleteManyCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public DeleteManyCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<DeleteManyCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_DeletesMultipleDocuments_WhenMatchesFound()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var filter = "{\"status\": \"inactive\"}";
        var expectedResult = new Dictionary<string, object?>
        {
            ["success"] = true,
            ["statusCode"] = HttpStatusCode.OK,
            ["message"] = "Documents deleted successfully",
            ["data"] = new Dictionary<string, object?>
            {
                ["deleted_count"] = 15L,
                ["acknowledged"] = true
            }
        };

        _documentDbService.DeleteManyAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
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
    public async Task ExecuteAsync_Returns400_WhenCollectionNotFound()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var filter = "{\"status\": \"old\"}";

        _documentDbService.DeleteManyAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument>(),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, object?>
            {
                ["success"] = false,
                ["statusCode"] = HttpStatusCode.BadRequest,
                ["message"] = "Failed to delete documents: Collection not found",
                ["data"] = null
            });

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--filter", filter
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.StartsWith("Failed to delete documents: Collection not found", response.Message);
    }
}
