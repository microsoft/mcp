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

public class DeleteDocumentsCommandTests
{
    private readonly IDocumentDbService _documentDbService = Substitute.For<IDocumentDbService>();
    private readonly DeleteDocumentsCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public DeleteDocumentsCommandTests()
    {
        _command = new DeleteDocumentsCommand(Substitute.For<ILogger<DeleteDocumentsCommand>>());
        _commandDefinition = _command.GetCommand();
        _context = new CommandContext(new ServiceCollection().AddSingleton(_documentDbService).BuildServiceProvider());
    }

    [Fact]
    public async Task ExecuteAsync_DeletesSingleDocument_ByDefault()
    {
        _documentDbService.DeleteDocumentAsync(Arg.Is("mongodb://localhost"), Arg.Is("testdb"), Arg.Is("testcollection"), Arg.Any<BsonDocument>(), Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse { Success = true, StatusCode = HttpStatusCode.OK, Data = new Dictionary<string, object?> { ["deleted_count"] = 1L } });

        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(["--connection-string", "mongodb://localhost", "--db-name", "testdb", "--collection-name", "testcollection", "--filter", "{\"_id\":\"1\"}"]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _documentDbService.Received(1).DeleteDocumentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<BsonDocument>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DeletesManyDocuments_WhenModeMany()
    {
        _documentDbService.DeleteManyAsync(Arg.Is("mongodb://localhost"), Arg.Is("testdb"), Arg.Is("testcollection"), Arg.Any<BsonDocument>(), Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse { Success = true, StatusCode = HttpStatusCode.OK, Data = new Dictionary<string, object?> { ["deleted_count"] = 3L } });

        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(["--connection-string", "mongodb://localhost", "--db-name", "testdb", "--collection-name", "testcollection", "--filter", "{\"status\":\"inactive\"}", "--mode", "many"]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _documentDbService.Received(1).DeleteManyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<BsonDocument>(), Arg.Any<CancellationToken>());
    }
}