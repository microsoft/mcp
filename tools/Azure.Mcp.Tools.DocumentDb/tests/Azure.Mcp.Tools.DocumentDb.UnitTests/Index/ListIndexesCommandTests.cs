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

public class ListIndexesCommandTests
{
    private readonly IDocumentDbService _documentDbService;
    private readonly ListIndexesCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ListIndexesCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _command = new(Substitute.For<ILogger<ListIndexesCommand>>());
        _commandDefinition = _command.GetCommand();
        _context = new(new ServiceCollection().AddSingleton(_documentDbService).BuildServiceProvider());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsIndexes_WhenIndexesExist()
    {
        const string connectionString = "mongodb://localhost:27017";

        _documentDbService.ListIndexesAsync(connectionString, "testdb", "testcollection", Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Indexes retrieved successfully",
                Data = new Dictionary<string, object?>
                {
                    ["indexes"] = new List<string> { "{\"name\":\"_id_\"}" },
                    ["count"] = 1
                }
            });

        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse([
            "--connection-string", connectionString,
            "--db-name", "testdb",
            "--collection-name", "testcollection"]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenCollectionNotFound()
    {
        const string connectionString = "mongodb://localhost:27017";

        _documentDbService.ListIndexesAsync(connectionString, "testdb", "nonexistent", Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Collection 'nonexistent' not found"
            });

        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse([
            "--connection-string", connectionString,
            "--db-name", "testdb",
            "--collection-name", "nonexistent"]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns401_WhenUnauthorized()
    {
        const string connectionString = "mongodb://localhost:27017";

        _documentDbService.ListIndexesAsync(connectionString, "testdb", "testcollection", Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized access"
            });

        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse([
            "--connection-string", connectionString,
            "--db-name", "testdb",
            "--collection-name", "testcollection"]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.Status);
        Assert.Contains("Unauthorized access", response.Message);
    }

    [Theory]
    [InlineData("--connection-string", "mongodb://localhost:27017", "--db-name", "testdb")]
    [InlineData("--connection-string", "mongodb://localhost:27017", "--collection-name", "testcollection")]
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLowerInvariant());
    }
}