using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public class FabricCoreServiceTests
{
    private const string WorkspaceId = "aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa";
    private const string ItemId = "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb";
    private const string ItemJson = """
        {
          "id": "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb",
          "workspaceId": "aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa",
          "displayName": "Sales Lakehouse",
          "description": "Test metadata",
          "type": "Lakehouse"
        }
        """;

    [Fact]
    public async Task GetItemAsync_SendsOneAuthenticatedGetAndDisposesResponse()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ItemJson) };
        using var handler = new StubHttpMessageHandler((request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{WorkspaceId}/items/{ItemId}", request.RequestUri?.AbsoluteUri);
            Assert.Null(request.Content);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.True(cancellationToken.CanBeCanceled);
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var credential = CreateCredential();
        var service = new FabricCoreService(client, credential);

        var item = await service.GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken);

        Assert.Equal(Guid.Parse(ItemId), item.Id);
        Assert.Equal(Guid.Parse(WorkspaceId), item.WorkspaceId);
        Assert.Equal("Sales Lakehouse", item.DisplayName);
        Assert.Equal("Lakehouse", item.Type);
        Assert.Equal("Test metadata", item.Description);
        Assert.Equal(1, handler.CallCount);
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetItemAsync_ProjectsOnlyGenericMetadataAndAcceptsNewItemTypes()
    {
        var json = JsonNode.Parse(ItemJson);
        Assert.NotNull(json);
        json["type"] = "FutureFabricItemType";
        json.AsObject().Remove("description");
        json["definition"] = new JsonObject { ["payload"] = "excluded-definition" };
        json["defaultIdentity"] = new JsonObject { ["displayName"] = "excluded-identity" };
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json.ToJsonString()) };
        using var client = CreateClient(response);
        var service = new FabricCoreService(client, CreateCredential());

        var item = await service.GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken);

        Assert.Equal("FutureFabricItemType", item.Type);
        Assert.Null(item.Description);
        var output = JsonSerializer.SerializeToElement(item, CoreJsonContext.Default.FabricItemMetadata);
        Assert.Equal(["id", "displayName", "type", "workspaceId"], output.EnumerateObject().Select(property => property.Name));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("../workspaces")]
    [InlineData("https://example.com")]
    [InlineData(ItemId + "/getDefinition")]
    [InlineData(ItemId + "?include=DefaultIdentity")]
    public async Task GetItemAsync_RejectsInvalidIdsBeforeAuthentication(string? invalidId)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ItemJson) };
        using var handler = new StubHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var credential = CreateCredential();
        var service = new FabricCoreService(client, credential);

        var workspaceException = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetItemAsync(invalidId!, ItemId, TestContext.Current.CancellationToken));
        var itemException = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetItemAsync(WorkspaceId, invalidId!, TestContext.Current.CancellationToken));

        Assert.Equal("workspaceId", workspaceException.ParamName);
        Assert.Equal("itemId", itemException.ParamName);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("D")]
    [InlineData("N")]
    [InlineData("B")]
    [InlineData("P")]
    public async Task GetItemAsync_NormalizesStringIdsBeforeBuildingRequest(string format)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ItemJson) };
        using var handler = new StubHttpMessageHandler((request, _) =>
        {
            Assert.Equal($"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{WorkspaceId}/items/{ItemId}", request.RequestUri?.AbsoluteUri);
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CreateCredential());

        var item = await service.GetItemAsync(
            Guid.Parse(WorkspaceId).ToString(format).ToUpperInvariant(),
            Guid.Parse(ItemId).ToString(format).ToUpperInvariant(),
            TestContext.Current.CancellationToken);

        Assert.Equal(Guid.Parse(ItemId), item.Id);
        Assert.Equal(Guid.Parse(WorkspaceId), item.WorkspaceId);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task GetItemAsync_PreservesFailureStatusWithoutExposingResponseBody(HttpStatusCode statusCode)
    {
        using var response = new HttpResponseMessage(statusCode) { Content = new StringContent("private-backend-detail") };
        using var client = CreateClient(response);
        var service = new FabricCoreService(client, CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => service.GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken));

        Assert.Equal(statusCode, exception.StatusCode);
        Assert.DoesNotContain("private-backend-detail", exception.Message);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NoContent)]
    public async Task GetItemAsync_RejectsUnexpectedSuccessStatus(HttpStatusCode statusCode)
    {
        using var response = new HttpResponseMessage(statusCode) { Content = new StringContent(ItemJson) };
        using var client = CreateClient(response);
        var service = new FabricCoreService(client, CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => service.GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
    }

    [Theory]
    [InlineData("120", "Wait at least 120 seconds before retrying")]
    [InlineData("0", "Wait at least 0 seconds before retrying")]
    [InlineData("Tue, 01 Jan 2030 00:00:00 GMT", "Retry after 2030-01-01 00:00:00 UTC")]
    public async Task GetItemAsync_PreservesRetryAfterWithoutExposingResponseBody(string retryAfter, string expectedGuidance)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent("private-backend-detail")
        };
        Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", retryAfter));
        using var handler = new StubHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CreateCredential());

        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(() =>
            service.GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Contains(expectedGuidance, exception.Message);
        Assert.DoesNotContain("private-backend-detail", exception.Message);
        Assert.Equal(1, handler.CallCount);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("")]
    [InlineData("-1")]
    [InlineData("1.5")]
    [InlineData("9223372036854775807")]
    [InlineData("private-header-detail")]
    public async Task GetItemAsync_IgnoresInvalidRetryAfter(string retryAfter)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent("private-backend-detail")
        };
        Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", retryAfter));
        using var client = CreateClient(response);
        var service = new FabricCoreService(client, CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Equal("Unable to retrieve Fabric item metadata.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    public async Task GetItemAsync_RejectsMissingOrMalformedMetadata(string payload)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(payload) };
        using var client = CreateClient(response);
        var service = new FabricCoreService(client, CreateCredential());

        await Assert.ThrowsAsync<JsonException>(() => service.GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("id", "cccccccc-cccc-4ccc-8ccc-cccccccccccc")]
    [InlineData("workspaceId", "cccccccc-cccc-4ccc-8ccc-cccccccccccc")]
    [InlineData("id", "00000000-0000-0000-0000-000000000000")]
    [InlineData("id", "not-a-guid")]
    [InlineData("displayName", "")]
    [InlineData("displayName", null)]
    [InlineData("type", " ")]
    [InlineData("type", null)]
    public async Task GetItemAsync_RejectsInvalidMetadataFields(string field, string? value)
    {
        var json = JsonNode.Parse(ItemJson);
        Assert.NotNull(json);
        json[field] = value;
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json.ToJsonString()) };
        using var client = CreateClient(response);
        var service = new FabricCoreService(client, CreateCredential());

        await Assert.ThrowsAsync<JsonException>(() => service.GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("id")]
    [InlineData("workspaceId")]
    [InlineData("displayName")]
    [InlineData("type")]
    public async Task GetItemAsync_RejectsMissingRequiredFields(string field)
    {
        var json = JsonNode.Parse(ItemJson);
        Assert.NotNull(json);
        json.AsObject().Remove(field);
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json.ToJsonString()) };
        using var client = CreateClient(response);
        var service = new FabricCoreService(client, CreateCredential());

        await Assert.ThrowsAsync<JsonException>(() => service.GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetItemAsync_PropagatesAuthenticationFailureWithoutSendingHttp()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ItemJson) };
        using var handler = new StubHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var credential = CreateCredential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<AccessToken>(new AuthenticationFailedException("Test authentication failure")));
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => service.GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken));

        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task GetItemAsync_PropagatesCancellationToHttp()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = new StubHttpMessageHandler((_, cancellationToken) =>
        {
            cancellation.Cancel();
            return Task.FromCanceled<HttpResponseMessage>(cancellationToken);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CreateCredential());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetItemAsync(WorkspaceId, ItemId, cancellation.Token));

        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task CreateItemAsync_PreservesExistingRequestAndResponse()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.Created) { Content = new StringContent(ItemJson) };
        using var handler = new StubHttpMessageHandler(async (request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal($"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{WorkspaceId}/items", request.RequestUri?.AbsoluteUri);
            Assert.NotNull(request.Content);
            var body = JsonNode.Parse(await request.Content.ReadAsStringAsync(cancellationToken));
            Assert.NotNull(body);
            Assert.Equal("Sales Lakehouse", body["displayName"]?.GetValue<string>());
            Assert.Equal("Lakehouse", body["type"]?.GetValue<string>());
            return response;
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CreateCredential());

        var item = await service.CreateItemAsync(WorkspaceId, new CreateItemRequest
        {
            DisplayName = "Sales Lakehouse",
            Type = "Lakehouse"
        }, TestContext.Current.CancellationToken);

        Assert.Equal(ItemId, item.Id);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task SearchCatalogAsync_PreservesExistingRequestAndResponse()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"value":[],"continuationToken":"next-page"}""")
        };
        using var handler = new StubHttpMessageHandler(async (request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal($"{FabricEndpoints.GetFabricApiBaseUrl()}/catalog/search", request.RequestUri?.AbsoluteUri);
            Assert.NotNull(request.Content);
            var body = JsonNode.Parse(await request.Content.ReadAsStringAsync(cancellationToken));
            Assert.NotNull(body);
            Assert.Equal("Sales", body["search"]?.GetValue<string>());
            return response;
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CreateCredential());

        var result = await service.SearchCatalogAsync(new CatalogSearchRequest { Search = "Sales" }, TestContext.Current.CancellationToken);

        Assert.Empty(result.Value);
        Assert.Equal("next-page", result.ContinuationToken);
        Assert.Equal(1, handler.CallCount);
    }

    private static HttpClient CreateClient(HttpResponseMessage response) =>
        new(new StubHttpMessageHandler((_, _) => Task.FromResult(response)));

    private static TokenCredential CreateCredential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("test-token", DateTimeOffset.MaxValue)));
        return credential;
    }
}
