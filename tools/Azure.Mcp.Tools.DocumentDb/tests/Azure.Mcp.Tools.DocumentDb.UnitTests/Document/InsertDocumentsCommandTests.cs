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

public class InsertDocumentsCommandTests
{
    private readonly IDocumentDbService _documentDbService = Substitute.For<IDocumentDbService>();
    private readonly InsertDocumentsCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public InsertDocumentsCommandTests()
    {
        _command = new InsertDocumentsCommand(Substitute.For<ILogger<InsertDocumentsCommand>>());
        _commandDefinition = _command.GetCommand();
        _context = new CommandContext(new ServiceCollection().AddSingleton(_documentDbService).BuildServiceProvider());
    }

    [Fact]
    public async Task ExecuteAsync_InsertsSingleDocument_WhenPayloadIsObject()
    {
        _documentDbService.InsertDocumentAsync(Arg.Is("mongodb://localhost"), Arg.Is("testdb"), Arg.Is("testcollection"), Arg.Any<BsonDocument>(), Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse { Success = true, StatusCode = HttpStatusCode.OK, Data = new Dictionary<string, object?> { ["inserted_id"] = "1" } });

        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(["--connection-string", "mongodb://localhost", "--db-name", "testdb", "--collection-name", "testcollection", "--documents", "{\"name\":\"test\"}"]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _documentDbService.Received(1).InsertDocumentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<BsonDocument>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_InsertsManyDocuments_WhenPayloadIsArray()
    {
        _documentDbService.InsertManyAsync(Arg.Is("mongodb://localhost"), Arg.Is("testdb"), Arg.Is("testcollection"), Arg.Any<List<BsonDocument>>(), Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse { Success = true, StatusCode = HttpStatusCode.OK, Data = new Dictionary<string, object?> { ["inserted_count"] = 2 } });

        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(["--connection-string", "mongodb://localhost", "--db-name", "testdb", "--collection-name", "testcollection", "--documents", "[{\"name\":\"a\"},{\"name\":\"b\"}]"]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _documentDbService.Received(1).InsertManyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<List<BsonDocument>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenModeSingleButPayloadIsArray()
    {
        var response = await _command.ExecuteAsync(
            _context,
            _commandDefinition.Parse([
                "--connection-string", "mongodb://localhost",
                "--db-name", "testdb",
                "--collection-name", "testcollection",
                "--documents", "[{\"name\":\"a\"},{\"name\":\"b\"}]",
                "--mode", "single"
            ]),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("JSON array payloads require --mode many", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenModeManyButPayloadIsObject()
    {
        var response = await _command.ExecuteAsync(
            _context,
            _commandDefinition.Parse([
                "--connection-string", "mongodb://localhost",
                "--db-name", "testdb",
                "--collection-name", "testcollection",
                "--documents", "{\"name\":\"test\"}",
                "--mode", "many"
            ]),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--mode many requires a JSON array payload.", response.Message);
    }
}