// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Tools.DocumentDb.Commands.Database;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.DocumentDb.UnitTests.Database;

public class GetDbInfoCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<GetDbInfoCommand> _logger;
    private readonly GetDbInfoCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public GetDbInfoCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<GetDbInfoCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDbInfo_WhenDatabaseExists()
    {
        // Arrange
        var dbName = "testdb";
        var collections = new List<Dictionary<string, object?>>
        {
            new() { ["name"] = "collection1", ["count"] = 100 },
            new() { ["name"] = "collection2", ["count"] = 50 }
        };

        _documentDbService.GetDatabaseInfoAsync(
            Arg.Is(dbName),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, object?>
            {
                ["success"] = true,
                ["statusCode"] = HttpStatusCode.OK,
                ["message"] = "Database information retrieved successfully",
                ["data"] = new Dictionary<string, object?>
                {
                    ["database_name"] = dbName,
                    ["collections"] = collections
                }
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
    public async Task ExecuteAsync_Returns400_WhenDatabaseNotFound()
    {
        // Arrange
        var dbName = "nonexistentdb";

        _documentDbService.GetDatabaseInfoAsync(
            Arg.Is(dbName),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, object?>
            {
                ["success"] = false,
                ["statusCode"] = HttpStatusCode.BadRequest,
                ["message"] = $"Database '{dbName}' not found",
                ["data"] = null
            });

        var args = _commandDefinition.Parse(["--db-name", dbName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
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

        _documentDbService.GetDatabaseInfoAsync(
            Arg.Is(dbName),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, object?>
            {
                ["success"] = false,
                ["statusCode"] = HttpStatusCode.InternalServerError,
                ["message"] = $"Failed to get database info: {expectedError}",
                ["data"] = null
            });

        var args = _commandDefinition.Parse(["--db-name", dbName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Failed to get database info", response.Message);
    }
}
