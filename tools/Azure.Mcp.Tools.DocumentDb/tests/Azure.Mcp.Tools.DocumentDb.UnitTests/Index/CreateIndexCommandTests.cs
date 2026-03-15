// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Tools.DocumentDb.Commands.Index;
using Azure.Mcp.Tools.DocumentDb.Models;
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
    private readonly IDocumentDbService _documentDbService;
    private readonly CreateIndexCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public CreateIndexCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _command = new(Substitute.For<ILogger<CreateIndexCommand>>());
        _commandDefinition = _command.GetCommand();
        _context = new(new ServiceCollection().AddSingleton(_documentDbService).BuildServiceProvider());
    }

    [Fact]
    public async Task ExecuteAsync_CreatesIndex_WhenValidKeysProvided()
    {
        const string connectionString = "mongodb://localhost:27017";
        const string dbName = "testdb";
        const string collectionName = "testcollection";
        const string keys = "{\"status\": 1}";

        _documentDbService.CreateIndexAsync(connectionString, dbName, collectionName, Arg.Any<BsonDocument>(), Arg.Any<BsonDocument?>(), Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Index created successfully",
                Data = new Dictionary<string, object?> { ["index_name"] = "status_1" }
            });

        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse([
            "--connection-string", connectionString,
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--keys", keys]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesIndexWithOptions_WhenOptionsProvided()
    {
        const string connectionString = "mongodb://localhost:27017";
        const string dbName = "testdb";
        const string collectionName = "testcollection";
        const string keys = "{\"email\": 1}";
        const string options = "{\"unique\": true}";

        _documentDbService.CreateIndexAsync(connectionString, dbName, collectionName, Arg.Any<BsonDocument>(), Arg.Any<BsonDocument?>(), Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Index created successfully",
                Data = new Dictionary<string, object?> { ["index_name"] = "email_1" }
            });

        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse([
            "--connection-string", connectionString,
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--keys", keys,
            "--options", options]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenCollectionNotFound()
    {
        const string connectionString = "mongodb://localhost:27017";

        _documentDbService.CreateIndexAsync(connectionString, "testdb", "nonexistent", Arg.Any<BsonDocument>(), Arg.Any<BsonDocument?>(), Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Collection 'nonexistent' not found"
            });

        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse([
            "--connection-string", connectionString,
            "--db-name", "testdb",
            "--collection-name", "nonexistent",
            "--keys", "{\"status\": 1}"]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Theory]
    [InlineData("--connection-string", "mongodb://localhost:27017", "--db-name", "testdb", "--collection-name", "coll")]
    [InlineData("--connection-string", "mongodb://localhost:27017", "--db-name", "testdb", "--keys", "{\"a\":1}")]
    [InlineData("--connection-string", "mongodb://localhost:27017", "--collection-name", "coll", "--keys", "{\"a\":1}")]
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLowerInvariant());
    }
}