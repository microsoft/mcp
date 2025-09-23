// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Search.Commands;
using Azure.Mcp.Tools.Search.Commands.Index;
using Azure.Mcp.Tools.Search.Models;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using static Azure.Mcp.Tools.Search.Commands.Index.IndexGetCommand;

namespace Azure.Mcp.Tools.Search.UnitTests.Index;

public class IndexGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISearchService _searchService = Substitute.For<ISearchService>();
    private readonly ILogger<IndexGetCommand> _logger = Substitute.For<ILogger<IndexGetCommand>>();

    public IndexGetCommandTests()
    {
        var collection = new ServiceCollection();
        collection.AddSingleton(_searchService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsIndexes_WhenIndexesExist()
    {
        var expectedIndexes = new List<IndexInfo> {
            new("index1", null, null),
            new("index2", "This is the second index", null)
        };

        _searchService.GetIndexDetails(
            Arg.Is("service123"),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedIndexes);

        var command = new IndexGetCommand(_logger);

        var args = command.GetCommand().Parse("--service service123");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, SearchJsonContext.Default.IndexGetCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedIndexes, result.Indexes);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoIndexes()
    {
        _searchService.GetIndexDetails(
            Arg.Any<string>(),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var command = new IndexGetCommand(_logger);

        var args = command.GetCommand().Parse("--service service123");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, SearchJsonContext.Default.IndexGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Indexes);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        var serviceName = "service123";

        _searchService.GetIndexDetails(
            Arg.Is(serviceName),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var command = new IndexGetCommand(_logger);

        var args = command.GetCommand().Parse($"--service {serviceName}");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsIndexDefinition_WhenIndexExists()
    {
        // Arrange
        var serviceName = "service123";
        var indexName = "index1";
        var expectedDefinition = CreateMockIndexDefinition();

        // When using ThrowsAsync or Returns with NSubstitute, we need to match the exact parameter signature
        _searchService.GetIndexDetails(Arg.Is(serviceName), Arg.Is(indexName), Arg.Any<RetryPolicyOptions?>())
            .Returns([expectedDefinition]);

        var command = new IndexGetCommand(_logger);

        var args = command.GetCommand().Parse($"--service {serviceName} --index {indexName}");
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, SearchJsonContext.Default.IndexGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result?.Indexes);
        Assert.Single(result.Indexes);
        Assert.Equal(expectedDefinition.Name, result.Indexes[0].Name);
        Assert.Equal(expectedDefinition.Fields?.Count, result.Indexes[0].Fields?.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenDefinitionIsNull()
    {
        // Arrange
        var serviceName = "service123";
        var indexName = "index1";

        _searchService.GetIndexDetails(Arg.Is(serviceName), Arg.Is(indexName), Arg.Any<RetryPolicyOptions?>())
            .Returns([]);

        var command = new IndexGetCommand(_logger);

        var args = command.GetCommand().Parse($"--service {serviceName} --index {indexName}");
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, SearchJsonContext.Default.IndexGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Indexes);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceException()
    {
        // Arrange
        var expectedError = "Test error";
        var serviceName = "service123";
        var indexName = "index1";

        _searchService.GetIndexDetails(
            Arg.Is(serviceName),
            Arg.Is(indexName),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Exception(expectedError));

        var command = new IndexGetCommand(_logger);

        var args = command.GetCommand().Parse($"--service {serviceName} --index {indexName}");
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Contains(expectedError, response.Message ?? string.Empty);
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesRequiredOptions()
    {
        // Arrange
        var command = new IndexGetCommand(_logger);

        var args = command.GetCommand().Parse(""); // Missing required options
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.NotNull(response.Message);
        Assert.Contains("service", response.Message);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange & Act
        var command = new IndexGetCommand(_logger);
        var cmd = command.GetCommand();

        // Assert
        Assert.Equal("describe", cmd.Name);
        Assert.NotNull(cmd.Description!);
        Assert.NotEmpty(cmd.Description!);

        // Verify options
        var serviceOption = cmd.Options.FirstOrDefault(o => o.Name == "--service");
        var indexOption = cmd.Options.FirstOrDefault(o => o.Name == "--index");

        Assert.NotNull(serviceOption);
        Assert.NotNull(indexOption);
    }

    private static IndexInfo CreateMockIndexDefinition()
        => new("sampleIndex", null, [
            new("id", "Edm.String", true, null, null, null, null, null),
            new("title", "Edm.String", null, true, null, null, null, null),
            new("content", "Edm.String", null, true, true, null, null, null)
        ]);
}
