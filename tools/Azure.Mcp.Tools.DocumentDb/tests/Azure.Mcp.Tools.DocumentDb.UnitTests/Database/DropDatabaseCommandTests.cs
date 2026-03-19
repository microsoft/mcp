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

public class DropDatabaseCommandTests
{
    private const string ConnectionString = "mongodb://localhost:27017";

    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<DropDatabaseCommand> _logger;
    private readonly DropDatabaseCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public DropDatabaseCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<DropDatabaseCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_DropsDatabase_WhenDatabaseExists()
    {
        // Arrange
        var dbName = "testdb";

        _documentDbService.DropDatabaseAsync(
            Arg.Is(ConnectionString),
            Arg.Is(dbName),
            Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = $"Database '{dbName}' dropped successfully",
                Data = new Dictionary<string, object?>
                {
                    ["name"] = dbName,
                    ["deleted"] = true
                }
            });

        var args = _commandDefinition.Parse(["--connection-string", ConnectionString, "--db-name", dbName]);

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

        _documentDbService.DropDatabaseAsync(
            Arg.Is(ConnectionString),
            Arg.Is(dbName),
            Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = $"Database '{dbName}' was not found."
            });

        var args = _commandDefinition.Parse(["--connection-string", ConnectionString, "--db-name", dbName]);

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

        _documentDbService.DropDatabaseAsync(
            Arg.Is(ConnectionString),
            Arg.Is(dbName),
            Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.InternalServerError,
                Message = $"Failed to drop database: {expectedError}"
            });

        var args = _commandDefinition.Parse(["--connection-string", ConnectionString, "--db-name", dbName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Failed to drop database", response.Message);
    }
}