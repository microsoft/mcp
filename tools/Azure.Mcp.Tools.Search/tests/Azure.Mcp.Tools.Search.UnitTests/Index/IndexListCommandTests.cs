// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Search.Commands.Index;
using Azure.Mcp.Tools.Search.Models;
using Azure.Mcp.Tools.Search.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Search.UnitTests.Index;

public class IndexListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISearchService _searchService;
    private readonly ILogger<IndexListCommand> _logger;

    public IndexListCommandTests()
    {
        _searchService = Substitute.For<ISearchService>();
        _logger = Substitute.For<ILogger<IndexListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_searchService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsIndexes_WhenIndexesExist()
    {
        var expectedIndexes = new List<IndexInfo> { new("index1", null), new("index2", "This is the second index") };
        _searchService.ListIndexes(Arg.Is("service123"), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedIndexes);

        var command = new IndexListCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse("--service service123");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var result = JsonSerializer.Deserialize<IndexListResult>(json, options);

        Assert.NotNull(result);
        Assert.Equal(expectedIndexes, result.Indexes);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoIndexes()
    {
        _searchService.ListIndexes(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(new List<IndexInfo>());

        var command = new IndexListCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse("--service service123");
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

        _searchService.ListIndexes(Arg.Is(serviceName), Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var command = new IndexListCommand(_logger);
        var parser = new Parser(command.GetCommand());
        var args = parser.Parse($"--service {serviceName}");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    private class IndexListResult
    {
        [JsonPropertyName("indexes")]
        public List<IndexInfo> Indexes { get; set; } = [];
    }
}
