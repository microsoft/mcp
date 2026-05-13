// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Cosmos.Commands;
using Azure.Mcp.Tools.Cosmos.Commands.Item;
using Azure.Mcp.Tools.Cosmos.Models;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Cosmos.UnitTests.Item;

public class ItemVectorSearchCommandTests
    : CommandUnitTestsBase<Azure.Mcp.Tools.Cosmos.Commands.Item.ItemVectorSearchCommand, ICosmosService>
{
    [Fact]
    public void Name_IsCorrect() => Assert.Equal("vector-search", Command.Name);

    [Fact]
    public async Task ExecuteAsync_ReturnsItems_WhenEmbeddingProvided()
    {
        var items = new List<JsonElement>
        {
            JsonDocument.Parse("{\"id\":\"x\",\"_score\":0.1}").RootElement.Clone(),
        };

        Service.VectorSearch(
            Arg.Is("acct"), Arg.Is("db"), Arg.Is("c"),
            Arg.Is("embedding"),
            Arg.Is<IReadOnlyList<string>>(p => p.Count == 2 && p[0] == "id" && p[1] == "title"),
            Arg.Is<IReadOnlyList<float>>(v => v.Count == 3),
            Arg.Is(3),
            Arg.Is("sub"), Arg.Any<AuthMethod>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(items);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--account", "acct",
            "--database", "db",
            "--container", "c",
            "--vector-property", "embedding",
            "--select-properties", "id,title",
            "--count", "3",
            "--embedding", "0.1,0.2,0.3");

        var result = ValidateAndDeserializeResponse(response, CosmosJsonContext.Default.ItemVectorSearchCommandResult);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task ExecuteAsync_GeneratesEmbedding_WhenSearchTextProvided()
    {
        Service.GenerateEmbedding(
            Arg.Is("hello"),
            Arg.Is<EmbeddingRequest>(r => r.Endpoint == "https://aoai.example/" && r.DeploymentName == "ada"),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new[] { 0.5f, 0.25f });

        Service.VectorSearch(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Is<IReadOnlyList<float>>(v => v.Count == 2 && v[0] == 0.5f),
            Arg.Any<int>(),
            Arg.Any<string>(), Arg.Any<AuthMethod>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--account", "acct",
            "--database", "db",
            "--container", "c",
            "--vector-property", "embedding",
            "--select-properties", "id",
            "--search-text", "hello",
            "--openai-endpoint", "https://aoai.example/",
            "--embedding-deployment", "ada");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_RequiresEmbeddingOrSearchText()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--account", "acct",
            "--database", "db",
            "--container", "c",
            "--vector-property", "embedding",
            "--select-properties", "id");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("embedding", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsBothEmbeddingAndSearchText()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--account", "acct",
            "--database", "db",
            "--container", "c",
            "--vector-property", "embedding",
            "--select-properties", "id",
            "--embedding", "0.1,0.2",
            "--search-text", "hi",
            "--openai-endpoint", "https://aoai.example/",
            "--embedding-deployment", "ada");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("mutually exclusive", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsWildcardSelectProperties()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--account", "acct",
            "--database", "db",
            "--container", "c",
            "--vector-property", "embedding",
            "--select-properties", "*",
            "--embedding", "0.1,0.2");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("wildcard", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
