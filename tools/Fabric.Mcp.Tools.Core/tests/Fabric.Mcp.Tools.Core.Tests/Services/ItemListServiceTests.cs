// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public class ItemListServiceTests
{
    [Fact]
    public async Task ListItemsAsync_ReturnsWhitelistedMetadataAndContinuation()
    {
        using var content = new StringContent(ItemListTestData.PageJson);
        using var handler = new FabricCoreHttpMessageHandler((request, token) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(ItemListTestData.ItemsUrl + "?recursive=true", request.RequestUri?.AbsoluteUri);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
            Assert.Contains("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.Null(request.Content);
            Assert.True(token.CanBeCanceled);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        });
        using var client = new HttpClient(handler);
        var credential = ItemListTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        var page = await service.ListItemsAsync(ItemListTestData.WorkspaceId, cancellationToken: TestContext.Current.CancellationToken);

        var item = Assert.Single(page.Value);
        Assert.Equal(Guid.Parse(ItemListTestData.ItemId), item.Id);
        Assert.Equal(Guid.Parse(ItemListTestData.WorkspaceId), item.WorkspaceId);
        Assert.Equal(Guid.Parse(ItemListTestData.FolderId), item.FolderId);
        Assert.NotNull(item.LogicalId);
        Assert.NotNull(item.SensitivityLabel);
        Assert.Equal("Sales", Assert.Single(item.Tags!).DisplayName);
        Assert.Equal("FutureItemType", item.Type);
        Assert.Equal(ItemListTestData.Token, page.ContinuationToken);
        Assert.Equal(ItemListTestData.ContinuationUri, page.ContinuationUri);
        var json = JsonSerializer.Serialize(page, CoreJsonContext.Default.ItemListResponse);
        Assert.DoesNotContain("excluded-", json);
        Assert.DoesNotContain("defaultIdentity", json);
        Assert.DoesNotContain("definition", json);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => content.ReadAsStream(TestContext.Current.CancellationToken));
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(null, true, null, "?recursive=true")]
    [InlineData(null, false, null, "?recursive=false")]
    [InlineData("FutureItemType", true, null, "?type=FutureItemType&recursive=true")]
    [InlineData(null, false, ItemListTestData.FolderId, "?recursive=false&rootFolderId=" + ItemListTestData.FolderId)]
    [InlineData("Lakehouse", true, ItemListTestData.FolderId, "?type=Lakehouse&recursive=true&rootFolderId=" + ItemListTestData.FolderId)]
    [InlineData("a+b/c=d&e?f#g h%26", false, null, "?type=a%2Bb%2Fc%3Dd%26e%3Ff%23g%20h%2526&recursive=false")]
    public async Task ListItemsAsync_EncodesFilters(string? type, bool recursive, string? folder, string expectedQuery)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(ItemListTestData.ItemsUrl + expectedQuery, request.RequestUri?.AbsoluteUri);
            Assert.Empty(request.RequestUri!.Fragment);
            return Task.FromResult(ItemListTestData.Response());
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, ItemListTestData.CreateCredential());

        var page = await service.ListItemsAsync(ItemListTestData.WorkspaceId, type, recursive, folder,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(page.Value);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(ItemListTestData.Token, ItemListTestData.Token)]
    [InlineData("a+b/c=d&e?f#g h", "a%2Bb%2Fc%3Dd%26e%3Ff%23g%20h")]
    [InlineData("a%2Bb%2Fc%3Dd%26e%3Ff%23g%20h", "a%2Bb%2Fc%3Dd%26e%3Ff%23g%20h")]
    [InlineData("a%2Bb+c%26d&e", "a%2Bb%2Bc%26d%26e")]
    [InlineData("bad%", "bad%25")]
    [InlineData("bad%2", "bad%252")]
    [InlineData("bad%GG", "bad%25GG")]
    [InlineData("%2b%3d", "%2b%3d")]
    [InlineData("%41%7e", "A~")]
    [InlineData("%2526", "%2526")]
    [InlineData("caf\u00e9+\u96ea", "caf%C3%A9%2B%E9%9B%AA")]
    [InlineData("x&include=DefaultIdentity#fragment", "x%26include%3DDefaultIdentity%23fragment")]
    [InlineData("x%26include%3DDefaultIdentity%23fragment", "x%26include%3DDefaultIdentity%23fragment")]
    [InlineData("%0D%0A&recursive=false", "%0D%0A%26recursive%3Dfalse")] // cspell:ignore Dfalse
    public async Task ListItemsAsync_PreservesTokenWireSemanticsWithoutInjection(string token, string encoded)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(ItemListTestData.ItemsUrl + "?recursive=true&continuationToken=" + encoded, request.RequestUri?.AbsoluteUri);
            Assert.Empty(request.RequestUri!.Fragment);
            Assert.Equal(2, request.RequestUri.Query.Split('&').Length);
            return Task.FromResult(ItemListTestData.Response());
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, ItemListTestData.CreateCredential());

        await service.ListItemsAsync(ItemListTestData.WorkspaceId, continuationToken: token,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task ListItemsAsync_RequestsEachPageOnlyWhenCalledAndKeepsFilters()
    {
        var requests = new List<string>();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            requests.Add(request.RequestUri!.AbsoluteUri);
            return Task.FromResult(requests.Count == 1
                ? ItemListTestData.Response(ItemListTestData.PageJson)
                : ItemListTestData.Response());
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, ItemListTestData.CreateCredential());
        var workspace = $"{{{ItemListTestData.WorkspaceId.ToUpperInvariant()}}}";
        var folder = Guid.Parse(ItemListTestData.FolderId).ToString("N").ToUpperInvariant();
        var first = await service.ListItemsAsync(workspace, "Lakehouse", false, folder, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Single(requests);
        Assert.Equal(ItemListTestData.Token, first.ContinuationToken);
        Assert.Equal(ItemListTestData.ContinuationUri, first.ContinuationUri);

        var last = await service.ListItemsAsync(workspace, "Lakehouse", false, folder, first.ContinuationToken, TestContext.Current.CancellationToken);

        var initialUrl = ItemListTestData.ItemsUrl + "?type=Lakehouse&recursive=false&rootFolderId=" + ItemListTestData.FolderId;
        Assert.Equal([initialUrl, initialUrl + "&continuationToken=" + ItemListTestData.Token], requests);
        Assert.Empty(last.Value);
        Assert.Null(last.ContinuationToken);
        Assert.Null(last.ContinuationUri);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(ItemListTestData.Token, null)]
    [InlineData(null, "https://untrusted.invalid/next")]
    [InlineData(ItemListTestData.Token, ItemListTestData.ContinuationUri)]
    public async Task ListItemsAsync_PreservesContinuationOnEmptyPages(string? token, string? uri)
    {
        var json = JsonSerializer.Serialize(new ItemListResponse([], token, uri), CoreJsonContext.Default.ItemListResponse);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(ItemListTestData.Response(json)));
        using var client = new HttpClient(handler);

        var page = await new FabricCoreService(client, ItemListTestData.CreateCredential())
            .ListItemsAsync(ItemListTestData.WorkspaceId, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(page.Value);
        Assert.Equal(token, page.ContinuationToken);
        Assert.Equal(uri, page.ContinuationUri);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("", null, null, null)]
    [InlineData("not-a-uuid", null, null, null)]
    [InlineData("00000000-0000-0000-0000-000000000000", null, null, null)]
    [InlineData(ItemListTestData.WorkspaceId + "/items", null, null, null)]
    [InlineData(ItemListTestData.WorkspaceId, "", null, null)]
    [InlineData(ItemListTestData.WorkspaceId, "00000000-0000-0000-0000-000000000000", null, null)]
    [InlineData(ItemListTestData.WorkspaceId, "?include=DefaultIdentity", null, null)]
    [InlineData(ItemListTestData.WorkspaceId, null, " ", null)]
    [InlineData(ItemListTestData.WorkspaceId, null, null, " ")]
    public async Task ListItemsAsync_ValidatesBeforeAuthentication(string workspace, string? folder, string? type, string? token)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not run."));
        using var client = new HttpClient(handler);
        var credential = ItemListTestData.CreateCredential();

        await Assert.ThrowsAnyAsync<ArgumentException>(() => new FabricCoreService(client, credential)
            .ListItemsAsync(workspace, type, true, folder, token, TestContext.Current.CancellationToken));

        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-json")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("""{"value":null}""")]
    [InlineData("""{"value":{}}""")]
    [InlineData("""{"value":[null]}""")]
    [InlineData("""{"value":[{}]}""")]
    public async Task ListItemsAsync_RejectsInvalidPages(string json)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(ItemListTestData.Response(json)));
        using var client = new HttpClient(handler);

        await Assert.ThrowsAsync<JsonException>(() => new FabricCoreService(client, ItemListTestData.CreateCredential())
            .ListItemsAsync(ItemListTestData.WorkspaceId, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("id", "not-a-uuid")]
    [InlineData("id", "00000000-0000-0000-0000-000000000000")]
    [InlineData("workspaceId", ItemListTestData.FolderId)]
    [InlineData("workspaceId", "00000000-0000-0000-0000-000000000000")]
    [InlineData("displayName", " ")]
    [InlineData("type", "")]
    [InlineData("type", null)]
    public async Task ListItemsAsync_RejectsInvalidItemIdentity(string property, string? value)
    {
        var json = JsonNode.Parse(ItemListTestData.PageJson)!;
        json["value"]![0]![property] = value;
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(ItemListTestData.Response(json.ToJsonString())));
        using var client = new HttpClient(handler);

        await Assert.ThrowsAsync<JsonException>(() => new FabricCoreService(client, ItemListTestData.CreateCredential())
            .ListItemsAsync(ItemListTestData.WorkspaceId, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden, HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound, HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests, HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable, HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.Accepted, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.NoContent, HttpStatusCode.BadGateway)]
    public async Task ListItemsAsync_PreservesFailureStatusAndSanitizesBody(HttpStatusCode status, HttpStatusCode expected)
    {
        using var content = new StringContent("private-backend-detail");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(status)
        {
            Content = content,
            ReasonPhrase = "private-reason"
        }));
        using var client = new HttpClient(handler);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => new FabricCoreService(client, ItemListTestData.CreateCredential())
            .ListItemsAsync(ItemListTestData.WorkspaceId, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(expected, exception.StatusCode);
        Assert.DoesNotContain("private-", exception.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("0", "Wait at least 0 seconds")]
    [InlineData("120", "Wait at least 120 seconds")]
    [InlineData("Tue, 01 Jan 2030 00:00:00 GMT", "Retry after 2030-01-01 00:00:00 UTC")]
    [InlineData(null, "Unable to list")]
    [InlineData("private-header-detail", "Unable to list")]
    [InlineData("-1", "Unable to list")]
    [InlineData("999999999999999999999999999", "Unable to list")]
    [InlineData("10, 20", "Unable to list")]
    public async Task ListItemsAsync_UsesOnlyValidatedRetryAfter(string? header, string expected)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
        {
            var response = ItemListTestData.Response("private-backend-detail", HttpStatusCode.TooManyRequests);
            if (header is not null)
            {
                response.Headers.TryAddWithoutValidation("Retry-After", header);
            }
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);

        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(() => new FabricCoreService(client, ItemListTestData.CreateCredential())
            .ListItemsAsync(ItemListTestData.WorkspaceId, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Contains(expected, exception.Message);
        Assert.DoesNotContain("private-", exception.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task ListItemsAsync_IgnoresMultipleRetryAfterValues()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
        {
            var response = ItemListTestData.Response("private-backend-detail", HttpStatusCode.TooManyRequests);
            response.Headers.TryAddWithoutValidation("Retry-After", ["10", "20"]);
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => new FabricCoreService(client, ItemListTestData.CreateCredential())
            .ListItemsAsync(ItemListTestData.WorkspaceId, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.DoesNotContain("10", exception.Message);
        Assert.DoesNotContain("20", exception.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("before")]
    [InlineData("credential")]
    [InlineData("send")]
    [InlineData("body")]
    public async Task ListItemsAsync_PropagatesCancellation(string stage)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var content = new CancellationHttpContent(cancellation);
        var credential = ItemListTestData.CreateCredential();
        if (stage == "before")
        {
            cancellation.Cancel();
        }
        else if (stage == "credential")
        {
            credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    cancellation.Cancel();
                    return new ValueTask<AccessToken>(Task.FromCanceled<AccessToken>(call.Arg<CancellationToken>()));
                });
        }
        using var handler = new FabricCoreHttpMessageHandler((_, token) =>
        {
            if (stage == "send")
            {
                cancellation.Cancel();
                return Task.FromCanceled<HttpResponseMessage>(token);
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        });
        using var client = new HttpClient(handler);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new FabricCoreService(client, credential)
            .ListItemsAsync(ItemListTestData.WorkspaceId, cancellationToken: cancellation.Token));

        Assert.True(cancellation.IsCancellationRequested);
        Assert.Equal(stage is "before" or "credential" ? 0 : 1, handler.CallCount);
        if (stage == "before")
        {
            Assert.Empty(credential.ReceivedCalls());
        }
        if (stage == "body")
        {
            Assert.True(content.ObservedCancellation);
        }
    }

    [Fact]
    public async Task ExistingOperations_PreservePostRequestsAndResponses()
    {
        var requests = new List<string>();
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
            Assert.Contains("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.NotNull(request.Content);
            Assert.Equal("application/json", request.Content.Headers.ContentType?.MediaType);
            requests.Add(await request.Content.ReadAsStringAsync(token));
            return request.RequestUri!.AbsolutePath.EndsWith("/items")
                ? FabricOperationTestData.ItemResponse(ItemListTestData.WorkspaceId)
                : ItemListTestData.Response("""{"value":[],"continuationToken":"catalog-token"}""");
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, ItemListTestData.CreateCredential());

        var item = await service.CreateItemAsync(ItemListTestData.WorkspaceId,
            new CreateItemRequest { DisplayName = "Created", Type = "Lakehouse" }, cancellationToken: TestContext.Current.CancellationToken);
        var catalog = await service.SearchCatalogAsync(new CatalogSearchRequest { Search = "Sales", PageSize = 25 },
            TestContext.Current.CancellationToken);

        Assert.Equal(FabricOperationTestData.ItemId, item.Item?.Id);
        Assert.Equal("catalog-token", catalog.ContinuationToken);
        Assert.Equal(2, requests.Count);
        Assert.Contains("\"displayName\":\"Created\"", requests[0]);
        Assert.Contains("\"search\":\"Sales\"", requests[1]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExistingOperations_PreserveFailureHandling(bool create)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
            Task.FromResult(ItemListTestData.Response("existing-error", HttpStatusCode.BadRequest)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, ItemListTestData.CreateCredential());

        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(async () =>
        {
            if (create)
            {
                await service.CreateItemAsync(ItemListTestData.WorkspaceId, new(), cancellationToken: TestContext.Current.CancellationToken);
            }
            else
            {
                await service.SearchCatalogAsync(new(), TestContext.Current.CancellationToken);
            }
        });

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        if (create)
        {
            Assert.DoesNotContain("existing-error", exception.Message);
        }
        else
        {
            Assert.Contains("existing-error", exception.Message);
        }
        Assert.Equal(1, handler.CallCount);
    }
}
