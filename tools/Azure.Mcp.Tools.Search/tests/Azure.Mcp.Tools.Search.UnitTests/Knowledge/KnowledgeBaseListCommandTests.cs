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

public class KnowledgeBaseListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISearchService _searchService;
    private readonly ILogger<KnowledgeBaseListCommand> _logger;

    public KnowledgeBaseListCommandTests()
    {
        _searchService = Substitute.For<ISearchService>();
        _logger = Substitute.For<ILogger<KnowledgeBaseListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_searchService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsKnowledgeBases_WhenBasesExist()
    {
        var expectedBases = new List<KnowledgeBaseInfo>
        {
            new("base1", "First base", new List<string> { "source1" }),
            new("base2", "Second base", new List<string> { "source2", "source3" })
        };

        _searchService.ListKnowledgeBases(Arg.Is("service123"), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedBases);

        var command = new KnowledgeBaseListCommand(_logger);

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

        var result = JsonSerializer.Deserialize<KnowledgeBaseListResult>(json, options);
        Assert.NotNull(result);
        Assert.Equal(expectedBases.Count, result.KnowledgeBases.Count);
        for (int i = 0; i < expectedBases.Count; i++)
        {
            Assert.Equal(expectedBases[i].Name, result.KnowledgeBases[i].Name);
            Assert.Equal(expectedBases[i].Description, result.KnowledgeBases[i].Description);
            Assert.Equal(expectedBases[i].KnowledgeSources, result.KnowledgeBases[i].KnowledgeSources);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoBases()
    {
        _searchService.ListKnowledgeBases(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(new List<KnowledgeBaseInfo>());

        var command = new KnowledgeBaseListCommand(_logger);

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

        _searchService.ListKnowledgeBases(Arg.Is(serviceName), Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var command = new KnowledgeBaseListCommand(_logger);

        var args = command.GetCommand().Parse($"--service {serviceName}");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    private class KnowledgeBaseListResult
    {
        [JsonPropertyName("knowledgeBases")]
        public List<KnowledgeBaseInfo> KnowledgeBases { get; set; } = [];
    }
}
