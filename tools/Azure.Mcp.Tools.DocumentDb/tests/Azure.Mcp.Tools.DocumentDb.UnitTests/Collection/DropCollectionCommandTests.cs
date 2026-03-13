// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Tools.DocumentDb.Commands.Collection;
using Azure.Mcp.Tools.DocumentDb.Models;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.DocumentDb.UnitTests.Collection;

public class DropCollectionCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<DropCollectionCommand> _logger;
    private readonly DropCollectionCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public DropCollectionCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<DropCollectionCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_DropsCollection_WhenCollectionExists()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";

        _documentDbService.DropCollectionAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = $"Collection '{collectionName}' dropped successfully.",
                Data = new Dictionary<string, object?>
                {
                    ["databaseName"] = dbName,
                    ["collectionName"] = collectionName,
                    ["deleted"] = true
                }
            });

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns404_WhenCollectionNotFound()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "nonexistent";

        _documentDbService.DropCollectionAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = $"Collection '{collectionName}' was not found in database '{dbName}'."
            });

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_Returns404_WhenDatabaseNotFound()
    {
        // Arrange
        var dbName = "nonexistentdb";
        var collectionName = "testcollection";

        _documentDbService.DropCollectionAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = $"Database '{dbName}' was not found."
            });

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
    }

    [Theory]
    [InlineData("--db-name", "testdb")]
    [InlineData("--collection-name", "testcollection")]
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        // Arrange & Act
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args), TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_OnUnexpectedException()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var expectedError = "Unexpected error occurred";

        _documentDbService.DropCollectionAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.InternalServerError,
                Message = $"Failed to drop collection: {expectedError}"
            });

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains(expectedError, response.Message);
    }
}