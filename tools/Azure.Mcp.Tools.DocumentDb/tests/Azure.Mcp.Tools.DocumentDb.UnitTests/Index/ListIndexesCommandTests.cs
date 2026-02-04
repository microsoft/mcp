// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.DocumentDb.Commands.Index;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.DocumentDb.UnitTests.Index;

public class ListIndexesCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<ListIndexesCommand> _logger;
    private readonly ListIndexesCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ListIndexesCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<ListIndexesCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsIndexes_WhenIndexesExist()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var expectedResult = new Dictionary<string, object?>
        {
            ["indexes"] = new List<string>
            {
                "{\"name\":\"_id_\",\"key\":{\"_id\":1}}",
                "{\"name\":\"status_1\",\"key\":{\"status\":1}}"
            },
            ["count"] = 2
        };

        _documentDbService.ListIndexesAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

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
    public async Task ExecuteAsync_ReturnsDefaultIndex_WhenNoCustomIndexesExist()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var expectedResult = new Dictionary<string, object?>
        {
            ["indexes"] = new List<string>
            {
                "{\"name\":\"_id_\",\"key\":{\"_id\":1}}"
            },
            ["count"] = 1
        };

        _documentDbService.ListIndexesAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

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
    public async Task ExecuteAsync_Returns500_WhenCollectionNotFound()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "nonexistent";
        var expectedError = "Collection not found";

        _documentDbService.ListIndexesAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenDatabaseNotFound()
    {
        // Arrange
        var dbName = "nonexistentdb";
        var collectionName = "testcollection";
        var expectedError = "Database not found";

        _documentDbService.ListIndexesAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--db-name", "testdb")]
    [InlineData("--collection-name", "testcollection")]
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        // Arrange & Act
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCompoundIndexes_WhenTheyExist()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var expectedResult = new Dictionary<string, object?>
        {
            ["indexes"] = new List<string>
            {
                "{\"name\":\"_id_\",\"key\":{\"_id\":1}}",
                "{\"name\":\"name_1_age_1\",\"key\":{\"name\":1,\"age\":1}}"
            },
            ["count"] = 2
        };

        _documentDbService.ListIndexesAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

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
    public async Task ExecuteAsync_ReturnsTextIndexes_WhenTheyExist()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var expectedResult = new Dictionary<string, object?>
        {
            ["indexes"] = new List<string>
            {
                "{\"name\":\"_id_\",\"key\":{\"_id\":1}}",
                "{\"name\":\"description_text\",\"key\":{\"description\":\"text\"}}"
            },
            ["count"] = 2
        };

        _documentDbService.ListIndexesAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

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
    public async Task ExecuteAsync_Returns500_WhenServiceThrowsUnauthorizedException()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var expectedError = "Unauthorized access";

        _documentDbService.ListIndexesAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new UnauthorizedAccessException(expectedError));

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
