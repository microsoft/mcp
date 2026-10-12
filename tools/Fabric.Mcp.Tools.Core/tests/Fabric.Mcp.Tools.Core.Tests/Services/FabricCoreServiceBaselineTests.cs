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

    [Theory]
    [InlineData(WorkspaceId)]
    [InlineData("CFAFBEB180374D0C896EA46FB27FF229")]
    [InlineData("{CFAFBEB1-8037-4D0C-896E-A46FB27FF229}")]
    public async Task CreateItemAsync_PreservesAuthenticatedRequestAndResponse(string workspaceId)
    {
        using var response = FabricOperationTestData.ItemResponse(WorkspaceId, "Sales");
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

        var result = await service.CreateItemAsync(workspaceId,
            new CreateItemRequest { DisplayName = "Sales", Type = "Lakehouse" }, cancellationToken: TestContext.Current.CancellationToken);

        var item = Assert.IsType<FabricItem>(result.Item);
        Assert.Equal(FabricOperationTestData.ItemId, item.Id);
        Assert.Equal("Sales", item.DisplayName);
        Assert.Equal("Lakehouse", item.Type);
        Assert.Equal(WorkspaceId, item.WorkspaceId);
        Assert.Equal(1, handler.CallCount);
        await AssertCredentialAsync(credential, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("workspace-name")]
    [InlineData("not-a-uuid")]
    [InlineData("../other?token=value")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task CreateItemAsync_RejectsInvalidWorkspaceBeforeAuthenticationOrHttp(string? workspaceId)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not be called."));
        using var client = new HttpClient(handler);
        var credential = CreateCredential();
        var service = new FabricCoreService(client, credential);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateItemAsync(
            workspaceId!, new CreateItemRequest { DisplayName = "Sales", Type = "Lakehouse" }, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("nonempty UUID", exception.Message);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("raw+/%3D%G1", null)]
    [InlineData("%41%7e%2D%5f%2E", 25)]
    public async Task SearchCatalogAsync_PreservesBodyTokensAndDoesNotFollowMetadataUrls(string continuationToken, int? pageSize)
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
            Assert.False(body.RootElement.TryGetProperty("search", out _));
            Assert.False(body.RootElement.TryGetProperty("filter", out _));
            if (pageSize is { } size)
            {
                Assert.Equal(size, body.RootElement.GetProperty("pageSize").GetInt32());
            }
            else
            {
                Assert.False(body.RootElement.TryGetProperty("pageSize", out _));
            }
            Assert.Equal(continuationToken, body.RootElement.GetProperty("continuationToken").GetString());
            return response;
        });
        using var client = new HttpClient(handler);
        var credential = CreateCredential();
        var service = new FabricCoreService(client, credential);

        var page = await service.SearchCatalogAsync(new CatalogSearchRequest
        {
            PageSize = pageSize,
            ContinuationToken = continuationToken
        }, TestContext.Current.CancellationToken);

        Assert.Equal("entry-id", Assert.Single(page.Value).Id);
        Assert.Equal(continuationToken, page.ContinuationToken);
        Assert.Equal(1, handler.CallCount);
        await AssertCredentialAsync(credential, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("Sales", null, null)]
    [InlineData(null, "Type eq 'Report'", null)]
    [InlineData("", null, null)]
    [InlineData(null, " ", null)]
    [InlineData(null, null, 0)]
    [InlineData(null, null, 1001)]
    public async Task SearchCatalogAsync_RejectsInvalidContinuationBeforeAuthenticationOrHttp(string? search, string? filter, int? pageSize)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not be called."));
        using var client = new HttpClient(handler);
        var credential = CreateCredential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAsync<ArgumentException>(() => service.SearchCatalogAsync(new CatalogSearchRequest
        {
            Search = search,
            Filter = filter,
            PageSize = pageSize,
            ContinuationToken = "next-page"
        }, TestContext.Current.CancellationToken));

        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PostOperations_KeepCatalogNullDefaultButRejectMissingCreatedItem(bool search)
    {
        using var response = FabricOperationTestData.JsonResponse("null", search ? HttpStatusCode.OK : HttpStatusCode.Created);
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
            var result = await service.CreateItemAsync(WorkspaceId, new CreateItemRequest(), cancellationToken: TestContext.Current.CancellationToken);
            Assert.Null(result.Item);
            Assert.Equal(FabricOperationStatus.ResultUnavailable, result.Operation?.Status);
            Assert.Equal("InvalidItemMetadata", result.Operation?.Issue?.Code);
        }

        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false, HttpStatusCode.BadRequest)]
    [InlineData(true, HttpStatusCode.BadRequest)]
    [InlineData(false, HttpStatusCode.Unauthorized)]
    [InlineData(true, HttpStatusCode.Unauthorized)]
    [InlineData(false, HttpStatusCode.Forbidden)]
    [InlineData(true, HttpStatusCode.Forbidden)]
    [InlineData(false, HttpStatusCode.NotFound)]
    [InlineData(true, HttpStatusCode.NotFound)]
    [InlineData(false, HttpStatusCode.Conflict)]
    [InlineData(true, HttpStatusCode.Conflict)]
    [InlineData(false, HttpStatusCode.TooManyRequests)]
    [InlineData(true, HttpStatusCode.TooManyRequests)]
    [InlineData(false, HttpStatusCode.InternalServerError)]
    [InlineData(true, HttpStatusCode.InternalServerError)]
    [InlineData(false, HttpStatusCode.ServiceUnavailable)]
    [InlineData(true, HttpStatusCode.ServiceUnavailable)]
    [InlineData(false, HttpStatusCode.TemporaryRedirect)]
    [InlineData(true, HttpStatusCode.TemporaryRedirect)]
    [InlineData(false, HttpStatusCode.PermanentRedirect)]
    [InlineData(true, HttpStatusCode.PermanentRedirect)]
    public async Task LegacyOperations_PreserveErrorStatusAndMessageAndDoNotRetry(bool search, HttpStatusCode status)
    {
        var backendError = status switch
        {
            HttpStatusCode.Forbidden => """{"errorCode":"FeatureNotAvailable","message":"Feature is not available"}""",
            HttpStatusCode.TooManyRequests => """{"errorCode":"CapacityLimitExceeded","message":"Capacity limit exceeded","isRetriable":true}""", // cspell:ignore Retriable
            _ => """{"errorCode":"OriginalError","message":"original-backend-text"}"""
        };
        using var response = new HttpResponseMessage(status) { Content = new StringContent(backendError) };
        response.Headers.Location = new Uri("https://example.invalid/must-not-follow");
        response.Headers.RetryAfter = new(TimeSpan.FromSeconds(45));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CreateCredential());

        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(() => InvokeAsync(service, search, TestContext.Current.CancellationToken));

        if (search)
        {
            Assert.Equal($"Fabric API request failed with status {(int)status} ({status}): {backendError}", exception.Message);
        }
        else
        {
            Assert.DoesNotContain(backendError, exception.Message);
        }
        Assert.Equal(status, exception.StatusCode);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PostOperations_RejectMalformedJsonWithoutFabricatingCreatedItems(bool search)
    {
        using var response = FabricOperationTestData.JsonResponse("{", search ? HttpStatusCode.OK : HttpStatusCode.Created);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CreateCredential());

        if (search)
        {
            await Assert.ThrowsAsync<JsonException>(() => InvokeAsync(service, search, TestContext.Current.CancellationToken));
        }
        else
        {
            var result = await service.CreateItemAsync(WorkspaceId, new CreateItemRequest(), cancellationToken: TestContext.Current.CancellationToken);
            Assert.Null(result.Item);
            Assert.Equal("InvalidJson", result.Operation?.Issue?.Code);
        }

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
    [InlineData(false, null)]
    [InlineData(true, null)]
    [InlineData(false, HttpStatusCode.ServiceUnavailable)]
    [InlineData(true, HttpStatusCode.ServiceUnavailable)]
    public async Task LegacyOperations_PropagateTransportFailureWithoutRetry(bool search, HttpStatusCode? status)
    {
        var failure = new HttpRequestException("transport-failure", null, status);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromException<HttpResponseMessage>(failure));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => InvokeAsync(service, search, TestContext.Current.CancellationToken));

        Assert.Same(failure, exception);
        Assert.Equal(status, exception.StatusCode);
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
            await service.CreateItemAsync(WorkspaceId, new CreateItemRequest(), cancellationToken: cancellationToken);
        }
    }
}
