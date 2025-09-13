// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Search.Commands.Knowledge;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Search.UnitTests.Knowledge;

public class KnowledgeAgentRunRetrievalCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISearchService _searchService;
    private readonly ILogger<KnowledgeAgentRunRetrievalCommand> _logger;

    public KnowledgeAgentRunRetrievalCommandTests()
    {
        _searchService = Substitute.For<ISearchService>();
        _logger = Substitute.For<ILogger<KnowledgeAgentRunRetrievalCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_searchService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsResult_WhenSuccessful_WithQuery()
    {
        var json = "{\"answer\":\"42\"}";
        _searchService.RetrieveFromKnowledgeAgent(
            Arg.Is("svc"),
            Arg.Is("agent1"),
            Arg.Is("life"),
            Arg.Is<List<(string role, string message)>?>(m => m == null),
            Arg.Any<RetryPolicyOptions>())
            .Returns(json);

        var command = new KnowledgeAgentRunRetrievalCommand(_logger);
        var args = command.GetCommand().Parse("--service svc --agent agent1 --query life");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsResult_WhenSuccessful_WithMessages()
    {
        var json = "{\"conversation\":true}";
        _searchService.RetrieveFromKnowledgeAgent(
            Arg.Is("svc"),
            Arg.Is("agent1"),
            Arg.Is<string?>(q => q == null),
            Arg.Is<List<(string role, string message)>?>(m => m != null && m.Count == 1 && m[0].role == "user"),
            Arg.Any<RetryPolicyOptions>())
            .Returns(json);

        var command = new KnowledgeAgentRunRetrievalCommand(_logger);
        var args = command.GetCommand().Parse("--service svc --agent agent1 --messages user:Hello");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenMissingQueryAndMessages()
    {
        var command = new KnowledgeAgentRunRetrievalCommand(_logger);
        var args = command.GetCommand().Parse("--service svc --agent agent1");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);
        Assert.Equal(400, response.Status);
        Assert.Contains("Either --query or at least one --messages", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenMessageFormatInvalid()
    {
        var command = new KnowledgeAgentRunRetrievalCommand(_logger);
        var args = command.GetCommand().Parse("--service svc --agent agent1 --messages badformat");
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);
        Assert.Equal(400, response.Status);
        Assert.Contains("Invalid message format", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        _searchService.RetrieveFromKnowledgeAgent(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<List<(string role, string message)>?>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception("Test failure"));

        var command = new KnowledgeAgentRunRetrievalCommand(_logger);
        var args = command.GetCommand().Parse("--service svc --agent agent1 --query hi");
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);
        Assert.Equal(500, response.Status);
        Assert.Contains("Test failure", response.Message);
    }
}
