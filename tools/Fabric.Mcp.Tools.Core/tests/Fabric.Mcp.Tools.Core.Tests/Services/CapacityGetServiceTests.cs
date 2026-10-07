// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public class CapacityGetServiceTests()
{
    [Theory]
    [InlineData(CapacityGetTestData.CapacityId)]
    [InlineData("96f3f0ff4fe24712b61b05a456ba9357")]
    [InlineData("{96F3F0FF-4FE2-4712-B61B-05A456BA9357}")]
    public async Task GetCapacityAsync_SendsCanonicalGetAndDisposesResponse(string capacityId)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(CapacityGetTestData.FullJson));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        using var handler = new FabricCoreHttpMessageHandler((request, token) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{FabricEndpoints.FabricApiBaseUrl}/capacities/{CapacityGetTestData.CapacityId}", request.RequestUri?.AbsoluteUri);
            Assert.Null(request.Content);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.True(token.CanBeCanceled);
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var credential = CapacityGetTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        var capacity = await service.GetCapacityAsync(capacityId, TestContext.Current.CancellationToken);

        Assert.Equal(Guid.Parse(CapacityGetTestData.CapacityId), capacity.Id);
        Assert.Equal("F4 Capacity", capacity.DisplayName);
        Assert.Equal("F4", capacity.Sku);
        Assert.Equal("West Central US", capacity.Region);
        Assert.Equal("Active", capacity.State);
        Assert.False(stream.CanRead);
        Assert.Equal(1, handler.CallCount);
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
        var json = JsonSerializer.Serialize(capacity, CoreJsonContext.Default.FabricCapacityMetadata);
        Assert.DoesNotContain("excluded-backend-property", json);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Finance")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("https://example.com")]
    [InlineData(CapacityGetTestData.CapacityId + "/items")]
    [InlineData(CapacityGetTestData.CapacityId + "?unexpected=true")]
    public async Task GetCapacityAsync_RejectsInvalidIdBeforeAuthentication(string? capacityId)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not be called"));
        using var client = new HttpClient(handler);
        var credential = CapacityGetTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetCapacityAsync(capacityId!, TestContext.Current.CancellationToken));

        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task GetCapacityAsync_AcceptsFutureStringValues()
    {
        var payload = JsonNode.Parse(CapacityGetTestData.FullJson)!.AsObject();
        payload["sku"] = "Future SKU";
        payload["region"] = "Future region";
        payload["state"] = "FutureState";
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(payload.ToJsonString()) };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityGetTestData.CreateCredential());

        var capacity = await service.GetCapacityAsync(CapacityGetTestData.CapacityId, TestContext.Current.CancellationToken);

        Assert.Equal("Future SKU", capacity.Sku);
        Assert.Equal("Future region", capacity.Region);
        Assert.Equal("FutureState", capacity.State);
    }

    [Theory]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("{")]
    [InlineData("\"private-backend-detail\"")]
    public async Task GetCapacityAsync_RejectsInvalidBodiesAndDisposesResponse(string body)
    {
        await AssertInvalidMetadataAsync(body);
    }

    [Theory]
    [InlineData("id")]
    [InlineData("displayName")]
    [InlineData("sku")]
    [InlineData("region")]
    [InlineData("state")]
    public async Task GetCapacityAsync_RejectsMissingRequiredFields(string property)
    {
        var payload = JsonNode.Parse(CapacityGetTestData.FullJson)!.AsObject();
        Assert.True(payload.Remove(property));

        await AssertInvalidMetadataAsync(payload.ToJsonString());
    }

    [Theory]
    [InlineData("id", "null")]
    [InlineData("id", "\"invalid\"")]
    [InlineData("id", "\"00000000-0000-0000-0000-000000000000\"")]
    [InlineData("id", "\"aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa\"")]
    [InlineData("displayName", "null")]
    [InlineData("displayName", "\" \"")]
    [InlineData("displayName", "42")]
    [InlineData("sku", "null")]
    [InlineData("sku", "\"\"")]
    [InlineData("sku", "42")]
    [InlineData("region", "null")]
    [InlineData("region", "\" \"")]
    [InlineData("region", "42")]
    [InlineData("state", "null")]
    [InlineData("state", "\"\"")]
    [InlineData("state", "42")]
    public async Task GetCapacityAsync_RejectsInvalidRequiredFields(string property, string value)
    {
        var payload = JsonNode.Parse(CapacityGetTestData.FullJson)!.AsObject();
        payload[property] = JsonNode.Parse(value);

        await AssertInvalidMetadataAsync(payload.ToJsonString());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.PartialContent)]
    public async Task GetCapacityAsync_RejectsNon200WithoutLeakingDetailsOrPolling(HttpStatusCode status)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("private-backend-detail"));
        using var response = new HttpResponseMessage(status) { Content = new StreamContent(stream) };
        response.Headers.Location = new Uri("https://example.com/operation");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityGetTestData.CreateCredential());

        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(
            () => service.GetCapacityAsync(CapacityGetTestData.CapacityId, TestContext.Current.CancellationToken));

        Assert.Equal(response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : status, exception.StatusCode);
        Assert.DoesNotContain("private-backend-detail", exception.Message);
        Assert.False(stream.CanRead);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("0", "Wait at least 0 seconds")]
    [InlineData("120", "Wait at least 120 seconds")]
    [InlineData("Tue, 01 Jan 2030 00:00:00 GMT", "Retry after 2030-01-01 00:00:00 UTC")]
    [InlineData("-1", "Unable to retrieve")]
    [InlineData("private-header-detail", "Unable to retrieve")]
    [InlineData("1, 2", "Unable to retrieve")]
    [InlineData("999999999999999999999", "Unable to retrieve")]
    public async Task GetCapacityAsync_PreservesOnlyValidatedRetryAfter(string retryAfter, string expectedMessage)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Content = new StringContent("private-backend-detail") };
        Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", retryAfter));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityGetTestData.CreateCredential());

        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(
            () => service.GetCapacityAsync(CapacityGetTestData.CapacityId, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Contains(expectedMessage, exception.Message);
        Assert.DoesNotContain("private-backend-detail", exception.Message);
        Assert.DoesNotContain("private-header-detail", exception.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetCapacityAsync_IgnoresDuplicateRetryAfterHeaders()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", ["10", "20"]));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityGetTestData.CreateCredential());

        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(
            () => service.GetCapacityAsync(CapacityGetTestData.CapacityId, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Equal("Unable to retrieve Fabric capacity metadata.", exception.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetCapacityAsync_DoesNotAuthenticateWhenAlreadyCanceled()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        await cancellation.CancelAsync();
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not be called"));
        using var client = new HttpClient(handler);
        var credential = CapacityGetTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetCapacityAsync(CapacityGetTestData.CapacityId, cancellation.Token));

        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task GetCapacityAsync_PropagatesCancellationDuringAuthentication()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            var token = call.ArgAt<CancellationToken>(1);
            Assert.Equal(cancellation.Token, token);
            cancellation.Cancel();
            return ValueTask.FromCanceled<AccessToken>(token);
        });
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not be called"));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetCapacityAsync(CapacityGetTestData.CapacityId, cancellation.Token));

        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task GetCapacityAsync_PropagatesCancellationDuringHttp()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = new FabricCoreHttpMessageHandler((_, token) =>
        {
            cancellation.Cancel();
            Assert.True(token.IsCancellationRequested);
            return Task.FromCanceled<HttpResponseMessage>(token);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityGetTestData.CreateCredential());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetCapacityAsync(CapacityGetTestData.CapacityId, cancellation.Token));

        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetCapacityAsync_PropagatesCancellationDuringDeserializationAndDisposesStream()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var stream = Substitute.For<Stream>();
        stream.CanRead.Returns(true);
        stream.ReadAsync(Arg.Any<Memory<byte>>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            var token = call.ArgAt<CancellationToken>(1);
            Assert.Equal(cancellation.Token, token);
            cancellation.Cancel();
            return ValueTask.FromCanceled<int>(token);
        });
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityGetTestData.CreateCredential());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetCapacityAsync(CapacityGetTestData.CapacityId, cancellation.Token));

        await stream.Received().DisposeAsync();
    }

    [Fact]
    public async Task GetCapacityAsync_ResolvesTokensPerRequest()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(
            new AccessToken("first-token", DateTimeOffset.MaxValue),
            new AccessToken("second-token", DateTimeOffset.MaxValue));
        var tokens = new List<string?>();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            tokens.Add(request.Headers.Authorization?.Parameter);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(CapacityGetTestData.FullJson) });
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        await service.GetCapacityAsync(CapacityGetTestData.CapacityId, TestContext.Current.CancellationToken);
        await service.GetCapacityAsync(CapacityGetTestData.CapacityId, TestContext.Current.CancellationToken);

        Assert.Equal(["first-token", "second-token"], tokens);
        Assert.Null(client.DefaultRequestHeaders.Authorization);
        Assert.Equal(2, handler.CallCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExistingOperations_PreservePostRequestsAndResults(bool createItem)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(createItem ? """{"id":"existing-item"}""" : """{"value":[]}""")
        };
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(createItem
                ? $"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{CapacityGetTestData.CapacityId}/items"
                : $"{FabricEndpoints.FabricApiBaseUrl}/catalog/search", request.RequestUri?.AbsoluteUri);
            Assert.NotNull(request.Content);
            using var body = JsonDocument.Parse(await request.Content.ReadAsStringAsync(token));
            Assert.Equal(createItem ? "New item" : "sales",
                body.RootElement.GetProperty(createItem ? "displayName" : "search").GetString());
            return response;
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityGetTestData.CreateCredential());

        if (createItem)
        {
            var item = await service.CreateItemAsync(CapacityGetTestData.CapacityId,
                new CreateItemRequest { DisplayName = "New item", Type = "Lakehouse" }, TestContext.Current.CancellationToken);
            Assert.Equal("existing-item", item.Id);
        }
        else
        {
            var results = await service.SearchCatalogAsync(new CatalogSearchRequest { Search = "sales" }, TestContext.Current.CancellationToken);
            Assert.Empty(results.Value);
        }

        Assert.Equal(1, handler.CallCount);
    }

    private static async Task AssertInvalidMetadataAsync(string body)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CapacityGetTestData.CreateCredential());

        await Assert.ThrowsAsync<JsonException>(() => service.GetCapacityAsync(CapacityGetTestData.CapacityId, TestContext.Current.CancellationToken));

        Assert.False(stream.CanRead);
        Assert.Equal(1, handler.CallCount);
    }
}
