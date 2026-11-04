// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Cosmos.Commands;
using Azure.Mcp.Tools.Cosmos.Commands.Item;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Mcp.Core.Models;
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
            Arg.Is("acct"), Arg.Is("db"), Arg.Is("c"),
            Arg.Is("sub"), null, Arg.Any<AuthMethod>(), Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns((items, null));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--account", "acct",
            "--database", "db",
            "--container", "c");

        var result = ValidateAndDeserializeResponse(response, CosmosJsonContext.Default.ItemListRecentCommandResult);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsRemovedCountOption()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--account", "acct",
            "--database", "db",
            "--container", "c",
            "--count", "2");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("count", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(null, " next+/=% token ")]
    [InlineData(" next+/=% token ", " last+/=% token ")]
    [InlineData(" last+/=% token ", null)]
    [InlineData(null, null)]
    public async Task ExecuteAsync_PreservesOpaqueCursorsOnEmptyPages(string? cursor, string? nextCursor)
    {
        Service.GetRecentItems("account", "database", "container", "sub", cursor,
                AuthMethod.Credential, "tenant", Arg.Any<CancellationToken>())
            .Returns(([], nextCursor));
        var args = new List<string>
        {
            "--account", "account", "--database", "database", "--container", "container",
            "--subscription", "sub", "--tenant", "tenant"
        };
        if (cursor is not null)
        {
            args.AddRange(["--cursor", cursor]);
        }

        var response = await ExecuteCommandAsync(args.ToArray());

        var result = ValidateAndDeserializeResponse(response, CosmosJsonContext.Default.ItemListRecentCommandResult);
        Assert.Empty(result.Items);
        Assert.Equal(nextCursor, result.NextCursor);
        var payload = JsonSerializer.SerializeToElement(result, CosmosJsonContext.Default.ItemListRecentCommandResult);
        Assert.Equal(nextCursor is not null, payload.TryGetProperty("nextCursor", out _));
        await Service.Received(1).GetRecentItems("account", "database", "container", "sub", cursor,
            AuthMethod.Credential, "tenant", Arg.Any<CancellationToken>());
    }
}
