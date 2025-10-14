// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
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

public class KnowledgeBaseRetrieveCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISearchService _searchService;
    private readonly ILogger<KnowledgeBaseRetrieveCommand> _logger;

    public KnowledgeBaseRetrieveCommandTests()
    {
        _searchService = Substitute.For<ISearchService>();
        _logger = Substitute.For<ILogger<KnowledgeBaseRetrieveCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_searchService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsResult_WhenSuccessful_WithQuery()
    {
        var json = "{\"answer\":\"42\"}";
        _searchService.RetrieveFromKnowledgeBase(
            Arg.Is("svc"),
            Arg.Is("base1"),
            Arg.Is("life"),
            Arg.Is<List<(string role, string message)>?>(m => m == null),
            Arg.Any<RetryPolicyOptions>())
            .Returns(json);

        var command = new KnowledgeBaseRetrieveCommand(_logger);
        var args = command.GetCommand().Parse("--service svc --knowledge-base base1 --query life");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsResult_WhenSuccessful_WithMessages()
    {
        var json = "{\"conversation\":true}";
        _searchService.RetrieveFromKnowledgeBase(
            Arg.Is("svc"),
            Arg.Is("base1"),
            Arg.Is<string?>(q => q == null),
            Arg.Is<List<(string role, string message)>?>(m => m != null && m.Count == 1 && m[0].role == "user"),
            Arg.Any<RetryPolicyOptions>())
            .Returns(json);

        var command = new KnowledgeBaseRetrieveCommand(_logger);
        var args = command.GetCommand().Parse("--service svc --knowledge-base base1 --messages user:Hello");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenMissingQueryAndMessages()
    {
        var command = new KnowledgeBaseRetrieveCommand(_logger);
        var args = command.GetCommand().Parse("--service svc --knowledge-base base1");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Either --query or at least one --messages", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenHasBothQueryAndMessages()
    {
        var command = new KnowledgeBaseRetrieveCommand(_logger);
        var args = command.GetCommand().Parse("--service svc --knowledge-base base1 --query life --messages user:Hello");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Specifying both --query and --messages is not allowed.", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenMessageFormatInvalid()
    {
        var command = new KnowledgeBaseRetrieveCommand(_logger);
        var args = command.GetCommand().Parse("--service svc --knowledge-base base1 --messages bad-format");
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid message format", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        _searchService.RetrieveFromKnowledgeBase(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<List<(string role, string message)>?>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception("Test failure"));

        var command = new KnowledgeBaseRetrieveCommand(_logger);
        var args = command.GetCommand().Parse("--service svc --knowledge-base base1 --query hi");
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test failure", response.Message);
    }
}
