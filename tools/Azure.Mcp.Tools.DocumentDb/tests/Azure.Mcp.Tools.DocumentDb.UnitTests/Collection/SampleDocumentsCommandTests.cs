// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.DocumentDb.Commands.Collection;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using MongoDB.Bson;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.DocumentDb.UnitTests.Collection;

public class SampleDocumentsCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<SampleDocumentsCommand> _logger;
    private readonly SampleDocumentsCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public SampleDocumentsCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<SampleDocumentsCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSampleDocuments_WhenDocumentsExist()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var sampleSize = 5;
        var expectedDocuments = new List<BsonDocument>
        {
            new() { { "_id", ObjectId.GenerateNewId() }, { "name", "doc1" } },
            new() { { "_id", ObjectId.GenerateNewId() }, { "name", "doc2" } }
        };

        _documentDbService.SampleDocumentsAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Is(sampleSize),
            Arg.Any<CancellationToken>())
            .Returns(expectedDocuments);

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--sample-size", sampleSize.ToString()
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyList_WhenNoDocumentsExist()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "emptycollection";

        _documentDbService.SampleDocumentsAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

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
    public async Task ExecuteAsync_UsesDefaultSampleSize_WhenNotProvided()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";
        var defaultSampleSize = 10;

        _documentDbService.SampleDocumentsAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Is(defaultSampleSize),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _documentDbService.Received(1).SampleDocumentsAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Is(defaultSampleSize),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenCollectionNotFound()
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "nonexistent";
        var expectedError = "Collection not found";

        _documentDbService.SampleDocumentsAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<int>(),
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

        _documentDbService.SampleDocumentsAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Any<int>(),
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

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task ExecuteAsync_HandlesVariousSampleSizes(int sampleSize)
    {
        // Arrange
        var dbName = "testdb";
        var collectionName = "testcollection";

        _documentDbService.SampleDocumentsAsync(
            Arg.Is(dbName),
            Arg.Is(collectionName),
            Arg.Is(sampleSize),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var args = _commandDefinition.Parse([
            "--db-name", dbName,
            "--collection-name", collectionName,
            "--sample-size", sampleSize.ToString()
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }
}
