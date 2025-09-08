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

public class KnowledgeAgentListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISearchService _searchService;
    private readonly ILogger<KnowledgeAgentListCommand> _logger;

    public KnowledgeAgentListCommandTests()
    {
        _searchService = Substitute.For<ISearchService>();
        _logger = Substitute.For<ILogger<KnowledgeAgentListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_searchService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsKnowledgeAgents_WhenAgentsExist()
    {
        var expectedAgents = new List<KnowledgeAgentInfo>
        {
            new("agent1", "First agent", new List<string> { "source1" }),
            new("agent2", "Second agent", new List<string> { "source2", "source3" })
        };

        _searchService.ListKnowledgeAgents(Arg.Is("service123"), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedAgents);

        var command = new KnowledgeAgentListCommand(_logger);

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

        var result = JsonSerializer.Deserialize<KnowledgeAgentListResult>(json, options);
        Assert.NotNull(result);
        Assert.Equal(expectedAgents.Count, result.KnowledgeAgents.Count);
        for (int i = 0; i < expectedAgents.Count; i++)
        {
            Assert.Equal(expectedAgents[i].Name, result.KnowledgeAgents[i].Name);
            Assert.Equal(expectedAgents[i].Description, result.KnowledgeAgents[i].Description);
            Assert.Equal(expectedAgents[i].KnowledgeSources, result.KnowledgeAgents[i].KnowledgeSources);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoAgents()
    {
        _searchService.ListKnowledgeAgents(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(new List<KnowledgeAgentInfo>());

        var command = new KnowledgeAgentListCommand(_logger);

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

        _searchService.ListKnowledgeAgents(Arg.Is(serviceName), Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var command = new KnowledgeAgentListCommand(_logger);

        var args = command.GetCommand().Parse($"--service {serviceName}");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    private class KnowledgeAgentListResult
    {
        [JsonPropertyName("knowledgeAgents")]
        public List<KnowledgeAgentInfo> KnowledgeAgents { get; set; } = [];
    }
}
