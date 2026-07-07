// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Cosmos.Commands;
using Azure.Mcp.Tools.Cosmos.Commands.Item;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Cosmos.Tests.Item;

public class ItemTextSearchCommandTests : SubscriptionCommandUnitTestsBase<ItemTextSearchCommand, ICosmosService>
{
    [Fact]
    public void Name_IsCorrect() => Assert.Equal("text-search", Command.Name);

    [Fact]
    public async Task ExecuteAsync_ReturnsItems_OnSuccess()
    {
        var items = new List<JsonElement>
        {
            JsonDocument.Parse("{\"id\":\"hit\"}").RootElement.Clone(),
        };

        Service.TextSearch(
            Arg.Is("acct"), Arg.Is("db"), Arg.Is("c"),
            Arg.Is("name"), Arg.Is("azure"),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Is(5), Arg.Is("sub"),
            Arg.Any<AuthMethod>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(items);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--account", "acct",
            "--database", "db",
            "--container", "c",
            "--search-property", "name",
            "--search-phrase", "azure",
            "--count", "5");

        var result = ValidateAndDeserializeResponse(response, CosmosJsonContext.Default.ItemTextSearchCommandResult);
        Assert.Single(result.Items);
    }

    [Theory]
    [InlineData("123name")]
    [InlineData("name;drop")]
    [InlineData("a..b")]
    public async Task ExecuteAsync_RejectsInvalidProperty(string property)
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--account", "acct",
            "--database", "db",
            "--container", "c",
            "--search-property", property,
            "--search-phrase", "azure");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("property", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
