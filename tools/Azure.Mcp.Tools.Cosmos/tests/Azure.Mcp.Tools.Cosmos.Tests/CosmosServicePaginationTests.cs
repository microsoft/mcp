// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Services.Caching;
using NSubstitute;
using Xunit;
using CosmosContainer = Microsoft.Azure.Cosmos.Container;

namespace Azure.Mcp.Tools.Cosmos.Tests;

public sealed class CosmosServicePaginationTests()
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Queries_ReturnOnlyOneFixedSizePageAndResume(bool recent, bool emptyFirstPage)
    {
        const string cursor = " opaque+/=% token ";
        var pageSize = recent ? 10 : 100;
        var firstIterator = Substitute.For<FeedIterator>();
        var lastIterator = Substitute.For<FeedIterator>();
        firstIterator.HasMoreResults.Returns(true);
        lastIterator.HasMoreResults.Returns(false);
        firstIterator.ReadNextAsync(Arg.Any<CancellationToken>())
            .Returns(CreateResponse(emptyFirstPage ? 0 : pageSize, cursor));
        lastIterator.ReadNextAsync(Arg.Any<CancellationToken>()).Returns(CreateResponse(1, null));
        var container = Substitute.For<CosmosContainer>();
        container.GetItemQueryStreamIterator(
                Arg.Any<QueryDefinition>(), Arg.Any<string?>(), Arg.Any<QueryRequestOptions>())
            .Returns(firstIterator, lastIterator);
        await using var service = CreateService(container);

        var first = await GetPageAsync(service, recent, null, TestContext.Current.CancellationToken);

        Assert.Equal(cursor, first.NextCursor);
        Assert.Equal(emptyFirstPage ? 0 : pageSize, GetDocumentCount(first.Items, recent));
        await firstIterator.Received(1).ReadNextAsync(TestContext.Current.CancellationToken);
        container.Received(1).GetItemQueryStreamIterator(
            Arg.Is<QueryDefinition>(query => query.QueryText == (recent
                ? "SELECT * FROM c ORDER BY c._ts DESC"
                : "SELECT * FROM c WHERE c.type = @p0")),
            null, Arg.Is<QueryRequestOptions>(options => options.MaxItemCount == pageSize));

        var last = await GetPageAsync(service, recent, first.NextCursor, TestContext.Current.CancellationToken);

        Assert.Null(last.NextCursor);
        Assert.Equal(1, GetDocumentCount(last.Items, recent));
        await lastIterator.Received(1).ReadNextAsync(TestContext.Current.CancellationToken);
        container.Received(1).GetItemQueryStreamIterator(
            Arg.Any<QueryDefinition>(), cursor, Arg.Is<QueryRequestOptions>(options => options.MaxItemCount == pageSize));
        container.Received(2).GetItemQueryStreamIterator(
            Arg.Any<QueryDefinition>(), Arg.Any<string?>(), Arg.Any<QueryRequestOptions>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Queries_PropagateErrorsWithoutRestarting(bool recent)
    {
        var iterator = Substitute.For<FeedIterator>();
        iterator.ReadNextAsync(Arg.Any<CancellationToken>()).Returns(new ResponseMessage(HttpStatusCode.BadRequest));
        var container = Substitute.For<CosmosContainer>();
        container.GetItemQueryStreamIterator(
                Arg.Any<QueryDefinition>(), Arg.Any<string?>(), Arg.Any<QueryRequestOptions>())
            .Returns(iterator);
        await using var service = CreateService(container);

        await Assert.ThrowsAsync<CosmosException>(() =>
            GetPageAsync(service, recent, "invalid-cursor", TestContext.Current.CancellationToken));

        await iterator.Received(1).ReadNextAsync(TestContext.Current.CancellationToken);
        container.Received(1).GetItemQueryStreamIterator(
            Arg.Any<QueryDefinition>(), "invalid-cursor", Arg.Any<QueryRequestOptions>());
    }

    private static CosmosService CreateService(CosmosContainer container)
    {
        var client = Substitute.For<CosmosClient>();
        client.GetContainer("database", "container").Returns(container);
        var cache = Substitute.For<ICacheService>();
        cache.GetAsync<CosmosClient>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(client);
        cache.GetGroupKeysAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<IEnumerable<string>>([]));
        return new CosmosService(AzureServiceTestHelpers.CreateAzureService(), cache, NullLogger<CosmosService>.Instance);
    }

    private static Task<(List<JsonElement> Items, string? NextCursor)> GetPageAsync(
        CosmosService service, bool recent, string? cursor, CancellationToken cancellationToken) =>
        recent
            ? service.GetRecentItems("account", "database", "container", "sub", cursor, cancellationToken: cancellationToken)
            : service.QueryItems("account", "database", "container", "SELECT * FROM c WHERE c.type = 'test'", "sub",
                cursor, cancellationToken: cancellationToken);

    private static int GetDocumentCount(List<JsonElement> items, bool recent) =>
        recent ? items.Count : Assert.Single(items).GetProperty("Documents").GetArrayLength();

    private static ResponseMessage CreateResponse(int count, string? cursor)
    {
        var documents = string.Join(",", Enumerable.Range(0, count).Select(index => $"{{\"id\":\"item{index}\"}}"));
        var response = new ResponseMessage(HttpStatusCode.OK)
        {
            Content = new MemoryStream(Encoding.UTF8.GetBytes($"{{\"Documents\":[{documents}]}}"))
        };
        if (cursor is not null)
        {
            response.Headers.Add("x-ms-continuation", cursor);
        }
        return response;
    }
}
