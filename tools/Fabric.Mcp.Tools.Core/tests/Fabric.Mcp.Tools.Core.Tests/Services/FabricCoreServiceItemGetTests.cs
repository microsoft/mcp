// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Net;
using System.Text;
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

public class FabricCoreServiceItemGetTests()
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
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(ItemJson));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        using var handler = new FabricCoreHttpMessageHandler((request, cancellationToken) =>
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
        Assert.False(stream.CanRead);
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
        json["data"] = new JsonObject { ["rows"] = "excluded-data" };
        json["workloadSpecificProperty"] = "excluded-workload-property";
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
    public async Task GetItemAsync_PreservesOptionalDescriptionSemantics(string? description)
    {
        var json = JsonNode.Parse(ItemJson)!.AsObject();
        json["description"] = description;
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json.ToJsonString()) };
        using var client = CreateClient(response);
        var service = new FabricCoreService(client, CreateCredential());

        var item = await service.GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken);

        Assert.Equal(description, item.Description);
        var output = JsonSerializer.SerializeToElement(item, CoreJsonContext.Default.FabricItemMetadata);
        Assert.Equal(description is not null, output.TryGetProperty("description", out var property));
        if (description is not null)
        {
            Assert.Equal("", property.GetString());
        }
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
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not be called."));
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
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
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
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task GetItemAsync_PreservesFailureStatusWithoutExposingResponseBody(HttpStatusCode statusCode)
    {
        using var response = new HttpResponseMessage(statusCode) { Content = new StringContent(FabricCoreErrorTestData.PrivateDetails) };
        using var client = CreateClient(response);
        var service = new FabricCoreService(client, CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => service.GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken));

        Assert.Equal(statusCode, exception.StatusCode);
        Assert.DoesNotContain("private", exception.Message);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.MovedPermanently)]
    [InlineData(HttpStatusCode.Found)]
    [InlineData(HttpStatusCode.TemporaryRedirect)]
    [InlineData(HttpStatusCode.PermanentRedirect)]
    public async Task GetItemAsync_DoesNotReadFailureBodiesOrFollowLocations(HttpStatusCode statusCode)
    {
        using var content = new UnreadableHttpContent();
        using var response = new HttpResponseMessage(statusCode) { Content = content };
        response.Headers.Location = new Uri("https://example.com/must-not-follow");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken));

        Assert.Equal(statusCode, exception.StatusCode);
        Assert.True(content.IsDisposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.PartialContent)]
    public async Task GetItemAsync_RejectsUnexpectedSuccessStatus(HttpStatusCode statusCode)
    {
        using var content = new UnreadableHttpContent();
        using var response = new HttpResponseMessage(statusCode) { Content = content };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => service.GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
        Assert.True(content.IsDisposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("120", "Wait at least 120 seconds before retrying")]
    [InlineData("0", "Wait at least 0 seconds before retrying")]
    [InlineData("Tue, 01 Jan 2030 00:00:00 GMT", "Retry after 2030-01-01 00:00:00 UTC")]
    public async Task GetItemAsync_PreservesRetryAfterWithoutExposingResponseBody(string retryAfter, string expectedGuidance)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent(FabricCoreErrorTestData.PrivateDetails)
        };
        Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", retryAfter));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CreateCredential());

        var exception = await Assert.ThrowsAsync<FabricThrottledException>(() =>
            service.GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Contains(expectedGuidance, exception.Message);
        Assert.DoesNotContain("private", exception.Message);
        Assert.Equal(1, handler.CallCount);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("")]
    [InlineData("-1")]
    [InlineData("1.5")]
    [InlineData("9223372036854775807")]
    [InlineData("private-header-detail")]
    [InlineData("60, 120")]
    public async Task GetItemAsync_IgnoresInvalidRetryAfter(string retryAfter)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent(FabricCoreErrorTestData.PrivateDetails)
        };
        Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", retryAfter));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Equal("Unable to retrieve Fabric item metadata.", exception.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetItemAsync_IgnoresMultipleRetryAfterHeaders()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", ["60", "120"]));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Equal("Unable to retrieve Fabric item metadata.", exception.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    public async Task GetItemAsync_RejectsMissingOrMalformedMetadata(string payload)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        using var client = CreateClient(response);
        var service = new FabricCoreService(client, CreateCredential());

        await Assert.ThrowsAsync<JsonException>(() => service.GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken));

        Assert.False(stream.CanRead);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("id", "cccccccc-cccc-4ccc-8ccc-cccccccccccc")]
    [InlineData("workspaceId", "cccccccc-cccc-4ccc-8ccc-cccccccccccc")]
    [InlineData("id", "00000000-0000-0000-0000-000000000000")]
    [InlineData("workspaceId", "00000000-0000-0000-0000-000000000000")]
    [InlineData("id", "not-a-guid")]
    [InlineData("workspaceId", "not-a-guid")]
    [InlineData("id", null)]
    [InlineData("workspaceId", null)]
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
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not be called."));
        using var client = new HttpClient(handler);
        var credential = CreateCredential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<AccessToken>(new AuthenticationFailedException(FabricCoreErrorTestData.PrivateDetails)));
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => service.GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken));

        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetItemAsync_RejectsCancellationBeforeHttp(bool cancelDuringAuthentication)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var credential = CreateCredential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                cancellation.Cancel();
                return new ValueTask<AccessToken>(new AccessToken("test-token", DateTimeOffset.MaxValue));
            });
        if (!cancelDuringAuthentication)
        {
            await cancellation.CancelAsync();
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not be called."));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetItemAsync(WorkspaceId, ItemId, cancellation.Token));

        Assert.Equal(0, handler.CallCount);
        await credential.Received(cancelDuringAuthentication ? 1 : 0).GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellation.Token);
    }

    [Fact]
    public async Task GetItemAsync_PropagatesCancellationToHttp()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = new FabricCoreHttpMessageHandler((_, cancellationToken) =>
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
    public async Task GetItemAsync_PropagatesStreamCancellationAndDisposesResponse()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var stream = new ItemUpdateCancellationStream(cancellation);
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        using var client = CreateClient(response);
        var service = new FabricCoreService(client, CreateCredential());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetItemAsync(WorkspaceId, ItemId, cancellation.Token));

        Assert.True(stream.IsDisposed);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetItemAsync_KeepsConcurrentRequestsAndCredentialsIndependent()
    {
        var tokenCount = 0;
        var credential = CreateCredential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new ValueTask<AccessToken>(new AccessToken($"test-token-{Interlocked.Increment(ref tokenCount)}", DateTimeOffset.MaxValue)));
        var tokens = new ConcurrentBag<string>();
        using var handler = new FabricCoreHttpMessageHandler(async (request, _) =>
        {
            tokens.Add(Assert.IsType<string>(request.Headers.Authorization?.Parameter));
            await Task.Yield();
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ItemJson) };
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        var items = await Task.WhenAll(Enumerable.Range(0, 4)
            .Select(_ => service.GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken)));

        Assert.Equal(4, handler.CallCount);
        Assert.Equal(4, tokens.Distinct().Count());
        Assert.All(items, item => Assert.Equal(Guid.Parse(ItemId), item.Id));
        Assert.Null(client.DefaultRequestHeaders.Authorization);
        await credential.Received(4).GetTokenAsync(Arg.Any<TokenRequestContext>(), TestContext.Current.CancellationToken);
    }

    private static HttpClient CreateClient(HttpResponseMessage response) =>
        new(new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response)));

    private static TokenCredential CreateCredential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("test-token", DateTimeOffset.MaxValue)));
        return credential;
    }
}
