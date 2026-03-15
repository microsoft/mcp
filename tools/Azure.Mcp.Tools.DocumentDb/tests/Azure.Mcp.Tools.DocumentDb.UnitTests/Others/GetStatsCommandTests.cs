// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Tools.DocumentDb.Commands.Others;
using Azure.Mcp.Tools.DocumentDb.Models;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using MongoDB.Bson;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.DocumentDb.UnitTests.Others;

public class GetStatsCommandTests
{
    private const string ConnectionString = "mongodb://localhost:27017";

    private readonly IDocumentDbService _documentDbService;
    private readonly GetStatsCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public GetStatsCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _command = new(Substitute.For<ILogger<GetStatsCommand>>());
        _commandDefinition = _command.GetCommand();
        _context = new(new ServiceCollection().AddSingleton(_documentDbService).BuildServiceProvider());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDatabaseStats_WhenResourceTypeIsDatabase()
    {
        _documentDbService.GetDatabaseStatsAsync(ConnectionString, "testdb", Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Database statistics retrieved successfully",
                Data = new BsonDocument { { "db", "testdb" }, { "collections", 5 } }
            });

        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse([
            "--connection-string", ConnectionString,
            "--resource-type", "database",
            "--db-name", "testdb"]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCollectionStats_WhenResourceTypeIsCollection()
    {
        _documentDbService.GetCollectionStatsAsync(ConnectionString, "testdb", "testcollection", Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Collection statistics retrieved successfully",
                Data = new BsonDocument { { "ns", "testdb.testcollection" }, { "count", 10 } }
            });

        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse([
            "--connection-string", ConnectionString,
            "--resource-type", "collection",
            "--db-name", "testdb",
            "--collection-name", "testcollection"]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsIndexStats_WhenResourceTypeIsIndex()
    {
        _documentDbService.GetIndexStatsAsync(ConnectionString, "testdb", "testcollection", Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Index statistics retrieved successfully",
                Data = new Dictionary<string, object?>
                {
                    ["stats"] = new List<string> { "{\"name\":\"_id_\"}" },
                    ["count"] = 1
                }
            });

        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse([
            "--connection-string", ConnectionString,
            "--resource-type", "index",
            "--db-name", "testdb",
            "--collection-name", "testcollection"]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenCollectionNameMissingForCollectionStats()
    {
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse([
            "--connection-string", ConnectionString,
            "--resource-type", "collection",
            "--db-name", "testdb"]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--collection-name is required", response.Message);
    }

    [Theory]
    [InlineData("--connection-string", ConnectionString, "--resource-type", "database")]
    [InlineData("--connection-string", ConnectionString, "--db-name", "testdb")]
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLowerInvariant());
    }
}