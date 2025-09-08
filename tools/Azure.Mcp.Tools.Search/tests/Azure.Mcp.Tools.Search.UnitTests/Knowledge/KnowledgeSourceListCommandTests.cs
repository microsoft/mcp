// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Search.Commands.Knowledge;
using Azure.Mcp.Tools.Search.Models;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Search.UnitTests.Knowledge;

public class KnowledgeSourceListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISearchService _searchService;
    private readonly ILogger<KnowledgeSourceListCommand> _logger;

    public KnowledgeSourceListCommandTests()
    {
        _searchService = Substitute.For<ISearchService>();
        _logger = Substitute.For<ILogger<KnowledgeSourceListCommand>>();

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

        _searchService.ListKnowledgeSources(Arg.Is("service123"), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedSources);

        var command = new KnowledgeSourceListCommand(_logger);

        var args = command.GetCommand().Parse("--service service123");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var result = JsonSerializer.Deserialize<KnowledgeSourceListResult>(json, options);
        Assert.NotNull(result);
        Assert.Equal(expectedSources, result.KnowledgeSources);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoSources()
    {
        _searchService.ListKnowledgeSources(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(new List<KnowledgeSourceInfo>());

        var command = new KnowledgeSourceListCommand(_logger);

        var args = command.GetCommand().Parse("--service service123");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        var serviceName = "service123";

        _searchService.ListKnowledgeSources(Arg.Is(serviceName), Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var command = new KnowledgeSourceListCommand(_logger);

        var args = command.GetCommand().Parse($"--service {serviceName}");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    private class KnowledgeSourceListResult
    {
        [JsonPropertyName("knowledgeSources")]
        public List<KnowledgeSourceInfo> KnowledgeSources { get; set; } = [];
    }
}
