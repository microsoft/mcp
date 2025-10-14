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

public class KnowledgeBaseGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISearchService _searchService;
    private readonly ILogger<KnowledgeBaseGetCommand> _logger;

    public KnowledgeBaseGetCommandTests()
    {
        _searchService = Substitute.For<ISearchService>();
        _logger = Substitute.For<ILogger<KnowledgeBaseGetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_searchService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsKnowledgeBases_WhenBasesExist()
    {
        var expectedBases = new List<KnowledgeBaseInfo>
        {
            new("base1", "First base", ["source1"]),
            new("base2", "Second base", ["source2", "source3"])
        };

        _searchService.ListKnowledgeBases(Arg.Is("service123"), Arg.Is((string?)null), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedBases);

        var command = new KnowledgeBaseGetCommand(_logger);

        var args = command.GetCommand().Parse("--service service123");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, SearchJsonContext.Default.KnowledgeBaseGetCommandResult);
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
    public async Task ExecuteAsync_ReturnsSingleKnowledgeBase_WhenNameProvided()
    {
        var expectedBase = new KnowledgeBaseInfo("base1", "First base", ["source1"]);

        _searchService.ListKnowledgeBases(Arg.Is("service123"), Arg.Is("base1"), Arg.Any<RetryPolicyOptions>())
            .Returns([expectedBase]);

        var command = new KnowledgeBaseGetCommand(_logger);

        var args = command.GetCommand().Parse("--service service123 --knowledge-base base1");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, SearchJsonContext.Default.KnowledgeBaseGetCommandResult);
        Assert.NotNull(result);
        Assert.Single(result.KnowledgeBases);
        Assert.Equal(expectedBase.Name, result.KnowledgeBases[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoBases()
    {
        _searchService.ListKnowledgeBases(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>()).Returns([]);

        var command = new KnowledgeBaseGetCommand(_logger);

        var args = command.GetCommand().Parse("--service service123");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response?.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, SearchJsonContext.Default.KnowledgeBaseGetCommandResult);
        Assert.NotNull(result);
        Assert.Empty(result.KnowledgeBases);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        var serviceName = "service123";

        _searchService.ListKnowledgeBases(Arg.Is(serviceName), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var command = new KnowledgeBaseGetCommand(_logger);

        var args = command.GetCommand().Parse($"--service {serviceName}");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
