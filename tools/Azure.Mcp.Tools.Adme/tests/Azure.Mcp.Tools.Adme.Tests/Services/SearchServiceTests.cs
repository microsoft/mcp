// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Tools.Adme.Models.Search;
using Azure.Mcp.Tools.Adme.Services;
using Azure.Mcp.Tools.Adme.Tests.TestSupport;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests.Services;

public sealed class SearchServiceTests
{
    [Fact]
    public async Task QueryAsync_PostsExactOsduPayloadAndDeserializesResponse()
    {
        var handler = JsonHandler(HttpStatusCode.OK,
            """{"results":[],"aggregations":[{"key":"kind","count":2}],"totalCount":2}""");
        var service = CreateService(handler);
        var spatialFilter = JsonDocument.Parse("""{"field":"data.Wgs84Coordinates"}""").RootElement.Clone();

        var response = await service.QueryAsync(
            TestConstants.Endpoint, TestConstants.DataPartition,
            new SearchQueryRequest
            {
                Kind = ["*:*:*:*"],
                Query = "data.FieldID:*Volve*",
                SuggestPhrase = "welll",
                Limit = 5,
                Offset = 20,
                ReturnedFields = ["id"],
                AggregateBy = "kind",
                TrackTotalCount = true,
                Sort = new SearchSort
                {
                    Field = ["data.Name.keyword"],
                    Order = ["DESC"],
                    Filter = ["data.Nested.Id:1"],
                },
                SpatialFilter = spatialFilter,
                QueryAsOwner = true,
                ExcludedFields = ["data.Big"],
                HighlightedFields = ["data.FieldID"],
            },
            TestConstants.Tenant, TestContext.Current.CancellationToken);

        Assert.Equal(2, response.TotalCount);
        Assert.Equal(2, Assert.Single(response.Aggregations!).Count);
        Assert.Equal("/api/search/v2/query", handler.LastRequest!.RequestUri!.PathAndQuery);
        Assert.Equal(
            """{"kind":["*:*:*:*"],"query":"data.FieldID:*Volve*","suggestPhrase":"welll","limit":5,"offset":20,"returnedFields":["id"],"aggregateBy":"kind","trackTotalCount":true,"sort":{"field":["data.Name.keyword"],"order":["DESC"],"filter":["data.Nested.Id:1"]},"spatialFilter":{"field":"data.Wgs84Coordinates"},"queryAsOwner":true,"excludedFields":["data.Big"],"highlightedFields":["data.FieldID"]} """.TrimEnd(),
            handler.LastRequestBody);
    }

    [Fact]
    public async Task QueryAsync_OmitsNullOptionalProperties()
    {
        var handler = JsonHandler(HttpStatusCode.OK, """{"results":[],"totalCount":0}""");
        var service = CreateService(handler);

        await service.QueryAsync(
            TestConstants.Endpoint, TestConstants.DataPartition,
            new SearchQueryRequest { Kind = ["*:*:*:*"] }, null,
            TestContext.Current.CancellationToken);

        Assert.Equal("""{"kind":["*:*:*:*"]}""", handler.LastRequestBody);
    }

    [Fact]
    public async Task QueryWithCursorAsync_PostsCursorPayloadAndDeserializesCursor()
    {
        var handler = JsonHandler(HttpStatusCode.OK,
            """{"results":[{"id":"record-1"}],"cursor":"NEXT","totalCount":42}""");
        var httpClientFactory = new FakeHttpClientFactory(handler);
        var service = CreateService(httpClientFactory);

        var response = await service.QueryWithCursorAsync(
            TestConstants.Endpoint, TestConstants.DataPartition,
            new SearchCursorRequest
            {
                Kind = [TestConstants.WellKind],
                Limit = 1,
                Cursor = "PREV",
                ReturnedFields = ["id"],
            },
            TestConstants.Tenant, TestContext.Current.CancellationToken);

        Assert.Equal("NEXT", response.Cursor);
        Assert.Equal(42, response.TotalCount);
        Assert.Equal(AdmeServiceHelper.NonRetryingHttpClientName, httpClientFactory.LastClientName);
        Assert.Equal("/api/search/v2/query_with_cursor", handler.LastRequest!.RequestUri!.PathAndQuery);
        Assert.Equal(
            $$"""{"kind":["{{TestConstants.WellKind}}"],"limit":1,"returnedFields":["id"],"cursor":"PREV"}""",
            handler.LastRequestBody);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(new object[] { new string[0] })]
    [InlineData(new object[] { new[] { "" } })]
    [InlineData(new object[] { new[] { "   " } })]
    public async Task QueryAsync_RejectsMissingKind(string[]? kinds)
    {
        var handler = JsonHandler(HttpStatusCode.OK, """{"results":[]}""");
        var service = CreateService(handler);

        await Assert.ThrowsAnyAsync<ArgumentException>(() => service.QueryAsync(
            TestConstants.Endpoint, TestConstants.DataPartition,
            new SearchQueryRequest { Kind = kinds! }, null,
            TestContext.Current.CancellationToken));

        Assert.Null(handler.LastRequest);
    }

    private static SearchService CreateService(StubHttpMessageHandler handler) =>
        new(CreateCredentialProvider(), new FakeHttpClientFactory(handler));

    private static SearchService CreateService(IHttpClientFactory httpClientFactory) =>
        new(CreateCredentialProvider(), httpClientFactory);

    private static IAzureTokenCredentialProvider CreateCredentialProvider()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("fake-token", DateTimeOffset.UtcNow.AddHours(1)));
        var provider = Substitute.For<IAzureTokenCredentialProvider>();
        provider.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
        return provider;
    }

    private static StubHttpMessageHandler JsonHandler(HttpStatusCode status, string content) =>
        new(_ => new HttpResponseMessage(status)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json"),
        });
}