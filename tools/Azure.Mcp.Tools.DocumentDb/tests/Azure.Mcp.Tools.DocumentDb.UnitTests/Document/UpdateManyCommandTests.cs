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

public class UpdateManyCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<UpdateManyCommand> _logger;
    private readonly UpdateManyCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public UpdateManyCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<UpdateManyCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesMultipleDocuments_WhenMatchesFound()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var filter = "{\"status\": \"inactive\"}";
        var update = "{\"$set\": {\"status\": \"active\"}}";
        var expectedResult = new Dictionary<string, object?>
        {
            ["success"] = true,
            ["statusCode"] = HttpStatusCode.OK,
            ["message"] = "Documents updated successfully",
            ["data"] = new Dictionary<string, object?>
            {
                ["matched_count"] = 10L,
                ["modified_count"] = 10L,
                ["acknowledged"] = true
            }
        };

        _documentDbService.UpdateManyAsync(
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
    public async Task ExecuteAsync_Returns400_WhenCollectionNotFound()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var filter = "{\"status\": \"old\"}";
        var update = "{\"$set\": {\"status\": \"new\"}}";

        _documentDbService.UpdateManyAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<BsonDocument>(),
            Arg.Any<BsonDocument>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, object?>
            {
                ["success"] = false,
                ["statusCode"] = HttpStatusCode.BadRequest,
                ["message"] = "Failed to update documents: Collection not found",
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
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.StartsWith("Failed to update documents: Collection not found", response.Message);
    }
}
