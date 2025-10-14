// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Search.Commands;
using Azure.Mcp.Tools.Search.Commands.Knowledge;
using Azure.Mcp.Tools.Search.Models;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Search.UnitTests.Knowledge;

public class KnowledgeSourceGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISearchService _searchService;
    private readonly ILogger<KnowledgeSourceGetCommand> _logger;

    public KnowledgeSourceGetCommandTests()
    {
        _searchService = Substitute.For<ISearchService>();
        _logger = Substitute.For<ILogger<KnowledgeSourceGetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_searchService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsKnowledgeSources_WhenSourcesExist()
    {
        var expectedSources = new List<KnowledgeSourceInfo>
        {
            new("source1", "BlobSource", "First source"),
            new("source2", "IndexSource", "Second source")
        };

        _searchService.ListKnowledgeSources(Arg.Is("service123"), Arg.Is((string?)null), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedSources);

        var command = new KnowledgeSourceGetCommand(_logger);

        var args = command.GetCommand().Parse("--service service123");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, SearchJsonContext.Default.KnowledgeSourceGetCommandResult);
        Assert.NotNull(result);
        Assert.Equal(expectedSources, result.KnowledgeSources);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSingleKnowledgeSource_WhenNameProvided()
    {
        var expectedSource = new KnowledgeSourceInfo("source1", "BlobSource", "First source");

        _searchService.ListKnowledgeSources(Arg.Is("service123"), Arg.Is("source1"), Arg.Any<RetryPolicyOptions>())
            .Returns([expectedSource]);

        var command = new KnowledgeSourceGetCommand(_logger);

        var args = command.GetCommand().Parse("--service service123 --knowledge-source source1");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, SearchJsonContext.Default.KnowledgeSourceGetCommandResult);
        Assert.NotNull(result);
        Assert.Single(result.KnowledgeSources);
        Assert.Equal(expectedSource, result.KnowledgeSources[0]);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoSources()
    {
        _searchService.ListKnowledgeSources(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>()).Returns([]);

        var command = new KnowledgeSourceGetCommand(_logger);

        var args = command.GetCommand().Parse("--service service123");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response?.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, SearchJsonContext.Default.KnowledgeSourceGetCommandResult);
        Assert.NotNull(result);
        Assert.Empty(result.KnowledgeSources);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        var serviceName = "service123";

        _searchService.ListKnowledgeSources(Arg.Is(serviceName), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var command = new KnowledgeSourceGetCommand(_logger);

        var args = command.GetCommand().Parse($"--service {serviceName}");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
