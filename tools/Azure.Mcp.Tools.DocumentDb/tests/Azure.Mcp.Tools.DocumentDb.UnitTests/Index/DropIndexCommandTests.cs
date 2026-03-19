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
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.DocumentDb.UnitTests.Index;

public class DropIndexCommandTests
{
    private readonly IDocumentDbService _documentDbService;
    private readonly DropIndexCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public DropIndexCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _command = new(Substitute.For<ILogger<DropIndexCommand>>());
        _commandDefinition = _command.GetCommand();
        _context = new(new ServiceCollection().AddSingleton(_documentDbService).BuildServiceProvider());
    }

    [Fact]
    public async Task ExecuteAsync_DropsIndex_WhenIndexExists()
    {
        const string connectionString = "mongodb://localhost:27017";

        _documentDbService.DropIndexAsync(connectionString, "testdb", "testcollection", "status_1", Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Index dropped successfully",
                Data = new Dictionary<string, object?> { ["index_name"] = "status_1" }
            });

        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse([
            "--connection-string", connectionString,
            "--db-name", "testdb",
            "--collection-name", "testcollection",
            "--index-name", "status_1"]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenCollectionNotFound()
    {
        const string connectionString = "mongodb://localhost:27017";

        _documentDbService.DropIndexAsync(connectionString, "testdb", "nonexistent", "status_1", Arg.Any<CancellationToken>())
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
            "--index-name", "status_1"]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenIndexDoesNotExist()
    {
        const string connectionString = "mongodb://localhost:27017";

        _documentDbService.DropIndexAsync(connectionString, "testdb", "testcollection", "nonexistent_index", Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Index 'nonexistent_index' not found"
            });

        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse([
            "--connection-string", connectionString,
            "--db-name", "testdb",
            "--collection-name", "testcollection",
            "--index-name", "nonexistent_index"]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Theory]
    [InlineData("--connection-string", "mongodb://localhost:27017", "--db-name", "testdb", "--collection-name", "coll")]
    [InlineData("--connection-string", "mongodb://localhost:27017", "--db-name", "testdb", "--index-name", "idx")]
    [InlineData("--connection-string", "mongodb://localhost:27017", "--collection-name", "coll", "--index-name", "idx")]
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLowerInvariant());
    }
}