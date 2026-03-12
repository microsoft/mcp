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
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.DocumentDb.UnitTests.Database;

public class ListDatabasesCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<ListDatabasesCommand> _logger;
    private readonly ListDatabasesCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ListDatabasesCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<ListDatabasesCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDatabases_WhenDatabasesExist()
    {
        // Arrange
        var expectedDatabases = new List<Dictionary<string, object?>>
        {
            new()
            {
                ["name"] = "database1"
            },
            new()
            {
                ["name"] = "database2"
            }
        };

        _documentDbService.GetDatabasesAsync(null, Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Databases retrieved successfully.",
                Data = expectedDatabases
            });

        var args = _commandDefinition.Parse([]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _documentDbService.Received(1).GetDatabasesAsync(null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSingleDatabase_WhenDbNameIsProvided()
    {
        // Arrange
        const string dbName = "database1";

        _documentDbService.GetDatabasesAsync(dbName, Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = $"Database '{dbName}' retrieved successfully.",
                Data = new List<Dictionary<string, object?>>
                {
                    new()
                    {
                        ["name"] = dbName,
                        ["collectionCount"] = 1,
                        ["collections"] = new List<Dictionary<string, object?>>
                        {
                            new() { ["name"] = "items", ["documentCount"] = 42L }
                        }
                    }
                }
            });

        var args = _commandDefinition.Parse(["--db-name", dbName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _documentDbService.Received(1).GetDatabasesAsync(dbName, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Returns404_WhenDatabaseIsMissing()
    {
        // Arrange
        const string dbName = "missingdb";

        _documentDbService.GetDatabasesAsync(dbName, Arg.Any<CancellationToken>())
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
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}