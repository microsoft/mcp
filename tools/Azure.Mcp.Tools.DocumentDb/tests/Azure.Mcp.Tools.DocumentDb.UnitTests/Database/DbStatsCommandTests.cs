// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Tools.DocumentDb.Commands.Database;
using Azure.Mcp.Tools.DocumentDb.Models;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using MongoDB.Bson;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.DocumentDb.UnitTests.Database;

public class DbStatsCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<DbStatsCommand> _logger;
    private readonly DbStatsCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public DbStatsCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<DbStatsCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDbStats_WhenDatabaseExists()
    {
        // Arrange
        var dbName = "testdb";
        var stats = new BsonDocument
        {
            { "db", dbName },
            { "collections", 5 },
            { "views", 0 },
            { "objects", 1000 },
            { "avgObjSize", 512.5 },
            { "dataSize", 512500 },
            { "storageSize", 1048576 },
            { "indexes", 10 },
            { "indexSize", 204800 }
        };

        _documentDbService.GetDatabaseStatsAsync(
            Arg.Is(dbName),
            Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Database statistics retrieved successfully",
                Data = stats
            });

        var args = _commandDefinition.Parse(["--db-name", dbName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns404_WhenDatabaseNotFound()
    {
        // Arrange
        var dbName = "nonexistentdb";

        _documentDbService.GetDatabaseStatsAsync(
            Arg.Is(dbName),
            Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = $"Database '{dbName}' was not found."
            });

        var args = _commandDefinition.Parse(["--db-name", dbName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenDbNameIsMissing()
    {
        // Arrange & Act
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse([]), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_OnUnexpectedError()
    {
        // Arrange
        var dbName = "testdb";
        var expectedError = "Unexpected error occurred";

        _documentDbService.GetDatabaseStatsAsync(
            Arg.Is(dbName),
            Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.InternalServerError,
                Message = $"Failed to get database stats: {expectedError}"
            });

        var args = _commandDefinition.Parse(["--db-name", dbName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Failed to get database stats", response.Message);
    }
}