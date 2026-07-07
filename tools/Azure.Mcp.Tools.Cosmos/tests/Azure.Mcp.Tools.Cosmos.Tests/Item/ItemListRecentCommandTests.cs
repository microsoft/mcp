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

public class ItemListRecentCommandTests : SubscriptionCommandUnitTestsBase<ItemListRecentCommand, ICosmosService>
{
    [Fact]
    public void Name_IsCorrect() => Assert.Equal("list-recent", Command.Name);

    [Fact]
    public async Task ExecuteAsync_ReturnsItems_OnSuccess()
    {
        var items = new List<JsonElement>
        {
            JsonDocument.Parse("{\"id\":\"a\"}").RootElement.Clone(),
            JsonDocument.Parse("{\"id\":\"b\"}").RootElement.Clone(),
        };

        Service.GetRecentItems(
            Arg.Is("acct"), Arg.Is("db"), Arg.Is("c"), Arg.Is(2),
            Arg.Is("sub"), Arg.Any<AuthMethod>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(items);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--account", "acct",
            "--database", "db",
            "--container", "c",
            "--count", "2");

        var result = ValidateAndDeserializeResponse(response, CosmosJsonContext.Default.ItemListRecentCommandResult);
        Assert.Equal(2, result.Items.Count);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("21")]
    public async Task ExecuteAsync_RejectsOutOfRangeCount(string count)
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--account", "acct",
            "--database", "db",
            "--container", "c",
            "--count", count);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("count", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
