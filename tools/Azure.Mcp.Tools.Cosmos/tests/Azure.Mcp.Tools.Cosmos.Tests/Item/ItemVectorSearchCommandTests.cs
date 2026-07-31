// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Cosmos.Commands;
using Azure.Mcp.Tools.Cosmos.Commands.Item;
using Azure.Mcp.Tools.Cosmos.Models;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Cosmos.Tests.Item;

public class ItemVectorSearchCommandTests : SubscriptionCommandUnitTestsBase<ItemVectorSearchCommand, ICosmosService>
{
    [Fact]
    public void Name_IsCorrect() => Assert.Equal("vector-search", Command.Name);

    [Fact]
    public async Task ExecuteAsync_GeneratesEmbedding_AndReturnsItems()
    {
        var items = new List<JsonElement>
        {
            JsonDocument.Parse("{\"id\":\"x\",\"_score\":0.1}").RootElement.Clone(),
        };

        Service.GenerateEmbedding(
            Arg.Is("hello"),
            Arg.Is<EmbeddingRequest>(r => r.Endpoint == "https://aoai.openai.azure.com/" && r.DeploymentName == "my-deployment"),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new[] { 0.5f, 0.25f });

        Service.VectorSearch(
            Arg.Is("acct"), Arg.Is("db"), Arg.Is("c"),
            Arg.Is("embedding"),
            Arg.Is<IReadOnlyList<string>?>(p => p != null && p.Count == 2 && p[0] == "id" && p[1] == "title"),
            Arg.Is<IReadOnlyList<float>>(v => v.Count == 2 && v[0] == 0.5f),
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
            "--properties-to-select", "id,title",
            "--count", "3",
            "--search-text", "hello",
            "--openai-endpoint", "https://aoai.openai.azure.com/",
            "--embedding-deployment", "my-deployment");

        var result = ValidateAndDeserializeResponse(response, CosmosJsonContext.Default.ItemVectorSearchCommandResult);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task ExecuteAsync_RequiresSearchTextAndOpenAIArguments()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--account", "acct",
            "--database", "db",
            "--container", "c",
            "--vector-property", "embedding",
            "--properties-to-select", "id");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("search-text", response.Message, StringComparison.OrdinalIgnoreCase);
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
            "--properties-to-select", "*",
            "--search-text", "hi",
            "--openai-endpoint", "https://aoai.openai.azure.com/",
            "--embedding-deployment", "my-deployment");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("wildcard", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("https://other-server.com/")]
    [InlineData("https://aoai.openai.azure.com.other.com/")]
    [InlineData("http://aoai.openai.azure.com/")]
    [InlineData("https://attacker.com#.openai.azure.com")]
    [InlineData("https://attacker.com#openai.azure.com")]
    [InlineData("https://attacker.com/#.openai.azure.com")]
    [InlineData("https://attacker.com?x=.openai.azure.com")]
    public async Task ExecuteAsync_RejectsUntrustedOpenAIEndpoint(string endpoint)
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--account", "acct",
            "--database", "db",
            "--container", "c",
            "--vector-property", "embedding",
            "--search-text", "hi",
            "--openai-endpoint", endpoint,
            "--embedding-deployment", "my-deployment");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Azure OpenAI endpoint", response.Message, StringComparison.OrdinalIgnoreCase);

        await Service.DidNotReceive().GenerateEmbedding(
            Arg.Any<string>(),
            Arg.Any<EmbeddingRequest>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }
}
