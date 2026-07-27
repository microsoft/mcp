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

public class ItemGetCommandTests : SubscriptionCommandUnitTestsBase<ItemGetCommand, ICosmosService>
{
    [Fact]
    public void Name_IsCorrect() => Assert.Equal("get", Command.Name);

    [Fact]
    public async Task ExecuteAsync_ReturnsItem_OnSuccess()
    {
        var item = JsonDocument.Parse("{\"id\":\"abc\",\"value\":42}").RootElement.Clone();

        Service.GetItem(
            Arg.Is("acct"), Arg.Is("db"), Arg.Is("c"), Arg.Is("abc"),
            Arg.Is("pk1"),
            Arg.Is("sub"), Arg.Any<AuthMethod>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(item);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--account", "acct",
            "--database", "db",
            "--container", "c",
            "--id", "abc",
            "--partition-key", "pk1");

        var result = ValidateAndDeserializeResponse(response, CosmosJsonContext.Default.ItemGetCommandResult);
        Assert.NotNull(result.Item);
        Assert.Equal("abc", result.Item.Value.GetProperty("id").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNullItem_WhenNotFound()
    {
        Service.GetItem(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(), Arg.Any<AuthMethod>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns((JsonElement?)null);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--account", "acct",
            "--database", "db",
            "--container", "c",
            "--id", "missing");

        var result = ValidateAndDeserializeResponse(response, CosmosJsonContext.Default.ItemGetCommandResult);
        Assert.Null(result.Item);
    }

    [Fact]
    public async Task ExecuteAsync_FailsWithoutId()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--account", "acct",
            "--database", "db",
            "--container", "c");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }
}
