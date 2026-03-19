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

public class RenameCollectionCommandTests
{
    private const string ConnectionString = "mongodb://localhost:27017";

    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<RenameCollectionCommand> _logger;
    private readonly RenameCollectionCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public RenameCollectionCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<RenameCollectionCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_RenamesCollection_WhenCollectionExists()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "oldname";
        var newCollectionName = "newname";

        _documentDbService.RenameCollectionAsync(
            Arg.Is(ConnectionString),
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Is(newCollectionName),
            Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = $"Collection '{collectionName}' was renamed to '{newCollectionName}' successfully.",
                Data = new Dictionary<string, object?>
                {
                    ["databaseName"] = dbName,
                    ["oldName"] = collectionName,
                    ["newName"] = newCollectionName,
                    ["renamed"] = true
                }
            });

        var args = _commandDefinition.Parse([
            "--connection-string", ConnectionString,
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--new-collection-name", newCollectionName
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
        var newCollectionName = "newname";

        _documentDbService.RenameCollectionAsync(
            Arg.Is(ConnectionString),
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Is(newCollectionName),
            Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = $"Collection '{collectionName}' was not found in database '{dbName}'."
            });

        var args = _commandDefinition.Parse([
            "--connection-string", ConnectionString,
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--new-collection-name", newCollectionName
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
        var newCollectionName = "newname";

        _documentDbService.RenameCollectionAsync(
            Arg.Is(ConnectionString),
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Is(newCollectionName),
            Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = $"Database '{dbName}' was not found."
            });

        var args = _commandDefinition.Parse([
            "--connection-string", ConnectionString,
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--new-collection-name", newCollectionName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_Returns409_WhenNewNameAlreadyExists()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "oldname";
        var newCollectionName = "existingname";
        var expectedError = $"Collection '{newCollectionName}' already exists in database '{dbName}'.";

        _documentDbService.RenameCollectionAsync(
            Arg.Is(ConnectionString),
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Is(newCollectionName),
            Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.Conflict,
                Message = $"Collection '{newCollectionName}' already exists in database '{dbName}'."
            });

        var args = _commandDefinition.Parse([
            "--connection-string", ConnectionString,
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--new-collection-name", newCollectionName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--connection-string", ConnectionString, "--db-name", "testdb", "--collection-name", "oldname")]
    [InlineData("--connection-string", ConnectionString, "--db-name", "testdb", "--new-collection-name", "newname")]
    [InlineData("--connection-string", ConnectionString, "--collection-name", "oldname", "--new-collection-name", "newname")]
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        // Arrange & Act
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }
}