// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class FabricCoreServiceBaselineTests()
{
    private const string WorkspaceId = "cfafbeb1-8037-4d0c-896e-a46fb27ff229";

    [Fact]
    public async Task CreateItemAsync_PreservesAuthenticatedRequestAndResponse()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent($$"""
                {"id":"item-id","displayName":"Sales","type":"Lakehouse","workspaceId":"{{WorkspaceId}}"}
                """)
        };
        using var handler = new FabricCoreHttpMessageHandler(async (request, cancellationToken) =>
        {
            AssertRequest(request, $"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{WorkspaceId}/items");
            Assert.Equal("application/json", request.Content?.Headers.ContentType?.MediaType);
            Assert.Equal("utf-8", request.Content?.Headers.ContentType?.CharSet);
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            Assert.Equal("Sales", body.RootElement.GetProperty("displayName").GetString());
            Assert.Equal("Lakehouse", body.RootElement.GetProperty("type").GetString());
            Assert.False(body.RootElement.TryGetProperty("description", out _));
            Assert.False(body.RootElement.TryGetProperty("definition", out _));
            return response;
        });
        using var client = new HttpClient(handler);
        var credential = CreateCredential();
        var service = new FabricCoreService(client, credential);

        var item = await service.CreateItemAsync(WorkspaceId,
            new CreateItemRequest { DisplayName = "Sales", Type = "Lakehouse" }, TestContext.Current.CancellationToken);

        Assert.Equal("item-id", item.Id);
        Assert.Equal("Sales", item.DisplayName);
        Assert.Equal("Lakehouse", item.Type);
        Assert.Equal(WorkspaceId, item.WorkspaceId);
        Assert.Equal(1, handler.CallCount);
        await AssertCredentialAsync(credential, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("raw+/%3D%G1")]
    [InlineData("%41%7e%2D%5f%2E")]
    public async Task SearchCatalogAsync_PreservesBodyTokensAndDoesNotFollowMetadataUrls(string continuationToken)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($$"""
                {
                  "value":[{"id":"entry-id","displayName":"Sales report","type":"Report","catalogEntryType":"FabricItem"}],
                  "continuationToken":"{{continuationToken}}",
                  "continuationUri":"https://example.invalid/next",
                  "apiEndpoint":"https://example.invalid/api"
                }
                """)
        };
        using var handler = new FabricCoreHttpMessageHandler(async (request, cancellationToken) =>
        {
            AssertRequest(request, $"{FabricEndpoints.FabricApiBaseUrl}/catalog/search");
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            Assert.Equal("Sales", body.RootElement.GetProperty("search").GetString());
            Assert.Equal("Type eq 'Report'", body.RootElement.GetProperty("filter").GetString());
            Assert.Equal(25, body.RootElement.GetProperty("pageSize").GetInt32());
            Assert.Equal(continuationToken, body.RootElement.GetProperty("continuationToken").GetString());
            return response;
        });
        using var client = new HttpClient(handler);
        var credential = CreateCredential();
        var service = new FabricCoreService(client, credential);

        var page = await service.SearchCatalogAsync(new CatalogSearchRequest
        {
            Search = "Sales",
            Filter = "Type eq 'Report'",
            PageSize = 25,
            ContinuationToken = continuationToken
        }, TestContext.Current.CancellationToken);

        Assert.Equal("entry-id", Assert.Single(page.Value).Id);
        Assert.Equal(continuationToken, page.ContinuationToken);
        Assert.Equal(1, handler.CallCount);
        await AssertCredentialAsync(credential, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LegacyOperations_PreserveNullResponseDefaults(bool search)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("null") };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CreateCredential());

        if (search)
        {
            var page = await service.SearchCatalogAsync(new CatalogSearchRequest(), TestContext.Current.CancellationToken);
            Assert.Empty(page.Value);
            Assert.Null(page.ContinuationToken);
        }
        else
        {
            var item = await service.CreateItemAsync(WorkspaceId, new CreateItemRequest(), TestContext.Current.CancellationToken);
            Assert.Empty(item.Id);
            Assert.Empty(item.DisplayName);
            Assert.Empty(item.Type);
            Assert.Empty(item.WorkspaceId);
        }

        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false, HttpStatusCode.BadRequest)]
    [InlineData(true, HttpStatusCode.BadRequest)]
    [InlineData(false, HttpStatusCode.Forbidden)]
    [InlineData(true, HttpStatusCode.Forbidden)]
    [InlineData(false, HttpStatusCode.TooManyRequests)]
    [InlineData(true, HttpStatusCode.TooManyRequests)]
    [InlineData(false, HttpStatusCode.InternalServerError)]
    [InlineData(true, HttpStatusCode.InternalServerError)]
    public async Task LegacyOperations_PreserveErrorMessageAndDoNotRetry(bool search, HttpStatusCode status)
    {
        const string backendError = """{"errorCode":"OriginalError","message":"original-backend-text"}""";
        using var response = new HttpResponseMessage(status) { Content = new StringContent(backendError) };
        response.Headers.RetryAfter = new(TimeSpan.FromSeconds(45));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => InvokeAsync(service, search, TestContext.Current.CancellationToken));

        Assert.Equal($"Fabric API request failed with status {(int)status} ({status}): {backendError}", exception.Message);
        Assert.Null(exception.StatusCode);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LegacyOperations_PreserveDeserializationFailures(bool search)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{") };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CreateCredential());

        await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(service, search, TestContext.Current.CancellationToken));

        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LegacyOperations_PropagateCredentialFailureBeforeHttp(bool search)
    {
        var failure = new AuthenticationFailedException("credential-failure");
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<AccessToken>(failure));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not be called."));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        var exception = await Assert.ThrowsAsync<AuthenticationFailedException>(() => InvokeAsync(service, search, TestContext.Current.CancellationToken));

        Assert.Same(failure, exception);
        Assert.Equal(0, handler.CallCount);
        await AssertCredentialAsync(credential, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LegacyOperations_PropagateTransportFailureWithoutRetry(bool search)
    {
        var failure = new HttpRequestException("transport-failure", null, HttpStatusCode.ServiceUnavailable);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromException<HttpResponseMessage>(failure));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => InvokeAsync(service, search, TestContext.Current.CancellationToken));

        Assert.Same(failure, exception);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LegacyOperations_ForwardCancellationToAuthenticationAndHttp(bool search)
    {
        using var source = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = new FabricCoreHttpMessageHandler((_, cancellationToken) =>
        {
            source.Cancel();
            Assert.True(cancellationToken.IsCancellationRequested);
            return Task.FromCanceled<HttpResponseMessage>(cancellationToken);
        });
        using var client = new HttpClient(handler);
        var credential = CreateCredential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => InvokeAsync(service, search, source.Token));

        Assert.Equal(1, handler.CallCount);
        await AssertCredentialAsync(credential, source.Token);
    }

    private static void AssertRequest(HttpRequestMessage request, string expectedUrl)
    {
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal(expectedUrl, request.RequestUri?.AbsoluteUri);
        Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
        Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
        Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
    }

    private static TokenCredential CreateCredential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.MaxValue));
        return credential;
    }

    private static async Task AssertCredentialAsync(TokenCredential credential, CancellationToken cancellationToken)
    {
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            cancellationToken);
    }

    private static async Task InvokeAsync(FabricCoreService service, bool search, CancellationToken cancellationToken)
    {
        if (search)
        {
            await service.SearchCatalogAsync(new CatalogSearchRequest(), cancellationToken);
        }
        else
        {
            await service.CreateItemAsync(WorkspaceId, new CreateItemRequest(), cancellationToken);
        }
    }
}
