// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// cspell:ignore LDEs Fexample Froles

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

public class FabricCoreServiceWorkspaceListTests()
{
    [Fact]
    public async Task ListWorkspacesAsync_UsesFixedEndpointAndRequestScopedAuthentication()
    {
        using var response = WorkspaceListTestData.CreateResponse(WorkspaceListTestData.MinimalPage);
        using var handler = new FabricCoreHttpMessageHandler((request, token) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("https://api.fabric.microsoft.com/v1/workspaces", request.RequestUri?.AbsoluteUri);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.Null(request.Content);
            Assert.True(token.CanBeCanceled);
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var credential = WorkspaceListTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        var page = await service.ListWorkspacesAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("My workspace", Assert.Single(page.Value).DisplayName);
        Assert.Equal(1, handler.CallCount);
        Assert.Null(client.DefaultRequestHeaders.Authorization);
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ListWorkspacesAsync_ComposesAllQueryOptions(bool? preference)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            var expected = "?roles=Admin%2CViewer&continuationToken=LDEsMTAwMDAwLDA%3D";
            if (preference is { } value)
            {
                expected += $"&preferWorkspaceSpecificEndpoints={(value ? "true" : "false")}";
            }
            Assert.Equal(expected, request.RequestUri?.Query);
            return Task.FromResult(WorkspaceListTestData.CreateResponse());
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceListTestData.CreateCredential());

        var page = await service.ListWorkspacesAsync(
            " admin, VIEWER,Admin ", WorkspaceListTestData.ContinuationToken, preference, TestContext.Current.CancellationToken);

        Assert.Empty(page.Value);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("LDEsMTAwMDAwLDA%3D", "LDEsMTAwMDAwLDA%3D")]
    [InlineData("+/=", "%2B%2F%3D")]
    [InlineData("%2B%2F%3D", "%2B%2F%3D")]
    [InlineData("a%25b", "a%25b")]
    [InlineData("%2526", "%2526")]
    [InlineData("%41%7e%2D%5f%2E", "A~-_.")]
    [InlineData("a&roles=Admin#?/%GG", "a%26roles%3DAdmin%23%3F%2F%25GG")]
    [InlineData("%26roles%3DAdmin", "%26roles%3DAdmin")]
    [InlineData(" spaced ", "%20spaced%20")]
    [InlineData("https://example.test/?roles=Admin", "https%3A%2F%2Fexample.test%2F%3Froles%3DAdmin")]
    public async Task ListWorkspacesAsync_EncodesOpaqueCursorWithoutDoubleEncoding(string token, string encoded)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal("https://api.fabric.microsoft.com/v1/workspaces", request.RequestUri?.GetLeftPart(UriPartial.Path));
            Assert.Equal($"?continuationToken={encoded}", request.RequestUri?.Query);
            Assert.Equal("", request.RequestUri?.Fragment);
            return Task.FromResult(WorkspaceListTestData.CreateResponse());
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceListTestData.CreateCredential());

        await service.ListWorkspacesAsync(continuationToken: token, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task ListWorkspacesAsync_ReturnsOnePageAndNeverFollowsReturnedUrls()
    {
        List<Uri> requests = [];
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.NotNull(request.RequestUri);
            requests.Add(request.RequestUri);
            return Task.FromResult(WorkspaceListTestData.CreateResponse(
                requests.Count == 1 ? WorkspaceListTestData.FullPage : WorkspaceListTestData.EmptyPage));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceListTestData.CreateCredential());

        var first = await service.ListWorkspacesAsync("Admin", preferWorkspaceSpecificEndpoints: true, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.CallCount);
        Assert.Single(first.Value);
        Assert.Equal("https://workspace.example.test", first.Value[0].ApiEndpoint);
        Assert.Equal(WorkspaceListTestData.ContinuationToken, first.ContinuationToken);
        Assert.StartsWith("https://continuation.example.test/", first.ContinuationUri);

        var last = await service.ListWorkspacesAsync("Admin", first.ContinuationToken, true, TestContext.Current.CancellationToken);

        Assert.Empty(last.Value);
        Assert.Null(last.ContinuationToken);
        Assert.Null(last.ContinuationUri);
        Assert.Equal(2, handler.CallCount);
        Assert.Equal("?roles=Admin&preferWorkspaceSpecificEndpoints=true", requests[0].Query);
        Assert.Equal("?roles=Admin&continuationToken=LDEsMTAwMDAwLDA%3D&preferWorkspaceSpecificEndpoints=true", requests[1].Query);
        Assert.All(requests, static uri => Assert.Equal("api.fabric.microsoft.com", uri.Host));
    }

    [Theory]
    [InlineData("""{"value":[],"continuationToken":"next"}""", "next", null)]
    [InlineData("""{"value":[],"continuationUri":"https://example.test/next"}""", null, "https://example.test/next")]
    [InlineData("""{"value":[],"continuationToken":"next","continuationUri":"https://example.test/next"}""", "next", "https://example.test/next")]
    [InlineData("""{"value":[],"continuationToken":null,"continuationUri":null}""", null, null)]
    public async Task ListWorkspacesAsync_PreservesContinuationOnEmptyPages(string body, string? token, string? uri)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(WorkspaceListTestData.CreateResponse(body)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceListTestData.CreateCredential());

        var page = await service.ListWorkspacesAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(page.Value);
        Assert.Equal(token, page.ContinuationToken);
        Assert.Equal(uri, page.ContinuationUri);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("Owner", null)]
    [InlineData("Admin,Owner", null)]
    [InlineData("Admin,,Viewer", null)]
    [InlineData("", null)]
    [InlineData(" ", null)]
    [InlineData(null, "")]
    [InlineData(null, " ")]
    public async Task ListWorkspacesAsync_RejectsInvalidInputBeforeAuthentication(string? roles, string? token)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(WorkspaceListTestData.CreateResponse()));
        using var client = new HttpClient(handler);
        var credential = WorkspaceListTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ListWorkspacesAsync(roles, token, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("not-json")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("""{"value":null}""")]
    [InlineData("""{"value":{}}""")]
    [InlineData("""{"value":[null]}""")]
    [InlineData("""{"value":[{}]}""")]
    [InlineData("""{"value":[{"id":"aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa","displayName":"Workspace","type":42}]}""")]
    [InlineData("""{"value":[],"continuationToken":42}""")]
    public async Task ListWorkspacesAsync_RejectsInvalidPagesAndDisposesResponse(string body)
    {
        using var response = WorkspaceListTestData.CreateResponse(body);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceListTestData.CreateCredential());

        await Assert.ThrowsAsync<JsonException>(() => service.ListWorkspacesAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("id", null)]
    [InlineData("id", "not-a-guid")]
    [InlineData("id", "00000000-0000-0000-0000-000000000000")]
    [InlineData("displayName", null)]
    [InlineData("displayName", " ")]
    [InlineData("type", " ")]
    [InlineData("type", "")]
    public async Task ListWorkspacesAsync_RejectsInvalidWorkspaceIdentity(string property, string? value)
    {
        var body = JsonNode.Parse(WorkspaceListTestData.MinimalPage)!;
        body["value"]![0]![property] = value;
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(WorkspaceListTestData.CreateResponse(body.ToJsonString())));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceListTestData.CreateCredential());

        await Assert.ThrowsAsync<JsonException>(() => service.ListWorkspacesAsync(cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("[null]")]
    [InlineData("[{}]")]
    [InlineData("""[{"id":"00000000-0000-0000-0000-000000000000","displayName":"Finance"}]""")]
    [InlineData("""[{"id":"dddddddd-dddd-4ddd-8ddd-dddddddddddd","displayName":null}]""")]
    public async Task ListWorkspacesAsync_RejectsMalformedAppliedTags(string tags)
    {
        var body = JsonNode.Parse(WorkspaceListTestData.MinimalPage)!;
        body["value"]![0]!["tags"] = JsonNode.Parse(tags);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(WorkspaceListTestData.CreateResponse(body.ToJsonString())));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceListTestData.CreateCredential());

        await Assert.ThrowsAsync<JsonException>(() => service.ListWorkspacesAsync(cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden, HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound, HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable, HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.Accepted, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.NoContent, HttpStatusCode.BadGateway)]
    public async Task ListWorkspacesAsync_PreservesFailuresAndRejectsUnexpectedSuccess(HttpStatusCode status, HttpStatusCode expected)
    {
        using var response = WorkspaceListTestData.CreateResponse("private-backend-detail", status);
        response.Headers.Location = new Uri("https://example.test/operations/not-followed");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceListTestData.CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.ListWorkspacesAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(expected, exception.StatusCode);
        Assert.DoesNotContain("private-backend-detail", exception.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("120", "Wait at least 120 seconds before retrying")]
    [InlineData("Tue, 01 Jan 2030 00:00:00 GMT", "Retry after 2030-01-01 00:00:00 UTC")]
    [InlineData(null, "Unable to list Fabric workspaces")]
    [InlineData("private-header-detail", "Unable to list Fabric workspaces")]
    [InlineData("-1", "Unable to list Fabric workspaces")]
    [InlineData("120, 60", "Unable to list Fabric workspaces")]
    public async Task ListWorkspacesAsync_SurfacesSanitizedRetryAfterWithoutRetrying(string? retryAfter, string expected)
    {
        using var response = WorkspaceListTestData.CreateResponse("private-backend-detail", HttpStatusCode.TooManyRequests);
        if (retryAfter is not null)
        {
            Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", retryAfter));
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceListTestData.CreateCredential());

        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(() =>
            service.ListWorkspacesAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Contains(expected, exception.Message);
        Assert.DoesNotContain("private-", exception.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("120", "60")]
    [InlineData("120", "120")]
    [InlineData("Tue, 01 Jan 2030 00:00:00 GMT", "private-header-detail")]
    public async Task ListWorkspacesAsync_RejectsMultipleRetryAfterValues(string first, string second)
    {
        using var response = WorkspaceListTestData.CreateResponse("private-backend-detail", HttpStatusCode.TooManyRequests);
        Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", [first, second]));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceListTestData.CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.ListWorkspacesAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Equal("Unable to list Fabric workspaces.", exception.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ListWorkspacesAsync_CancelsBeforeAuthentication()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        await cancellation.CancelAsync();
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(WorkspaceListTestData.CreateResponse()));
        using var client = new HttpClient(handler);
        var credential = WorkspaceListTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ListWorkspacesAsync(cancellationToken: cancellation.Token));

        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ListWorkspacesAsync_PropagatesCancellationToCredentialsAndHttp(bool duringAuthentication)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = new FabricCoreHttpMessageHandler((_, token) =>
        {
            cancellation.Cancel();
            return Task.FromCanceled<HttpResponseMessage>(token);
        });
        using var client = new HttpClient(handler);
        var credential = WorkspaceListTestData.CreateCredential();
        if (duringAuthentication)
        {
            credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    cancellation.Cancel();
                    return ValueTask.FromCanceled<AccessToken>(call.Arg<CancellationToken>());
                });
        }
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ListWorkspacesAsync(cancellationToken: cancellation.Token));

        Assert.Equal(duringAuthentication ? 0 : 1, handler.CallCount);
    }

    [Fact]
    public async Task ListWorkspacesAsync_PropagatesCancellationDuringDeserializationAndDisposesStream()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var stream = new WorkspaceListCancellationStream(cancellation);
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceListTestData.CreateCredential());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ListWorkspacesAsync(cancellationToken: cancellation.Token));

        Assert.True(stream.ReadStarted);
        Assert.False(stream.CanRead);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task ListWorkspacesAsync_CancelingOneCallDoesNotCancelAnother()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var handler = new FabricCoreHttpMessageHandler(async (request, cancellationToken) =>
        {
            if (request.RequestUri?.Query == "?roles=Admin")
            {
                firstStarted.SetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            return WorkspaceListTestData.CreateResponse();
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceListTestData.CreateCredential());
        var canceledCall = service.ListWorkspacesAsync("Admin", cancellationToken: cancellation.Token);
        await firstStarted.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);

        var otherPage = await service.ListWorkspacesAsync("Viewer", cancellationToken: TestContext.Current.CancellationToken);
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => canceledCall);
        Assert.Empty(otherPage.Value);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task ListWorkspacesAsync_ConcurrentCallsKeepTokensAndResultsIndependent()
    {
        var sends = 0;
        var tokenRequests = 0;
        var bothStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var handler = new FabricCoreHttpMessageHandler(async (request, cancellationToken) =>
        {
            var token = request.Headers.Authorization?.Parameter;
            var role = request.RequestUri?.Query == "?roles=Admin" ? "Admin" : "Viewer";
            if (Interlocked.Increment(ref sends) == 2)
            {
                bothStarted.SetResult();
            }
            await bothStarted.Task.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);
            Assert.Equal(token, request.Headers.Authorization?.Parameter);
            return WorkspaceListTestData.CreateResponse($$"""
                {"value":[{"id":"{{WorkspaceListTestData.WorkspaceId}}","displayName":"{{role}}:{{token}}","type":"Workspace"}]}
                """);
        });
        using var client = new HttpClient(handler);
        var credential = WorkspaceListTestData.CreateCredential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new AccessToken($"token-{Interlocked.Increment(ref tokenRequests)}", DateTimeOffset.MaxValue));
        var service = new FabricCoreService(client, credential);

        var pages = await Task.WhenAll(
            service.ListWorkspacesAsync("Admin", cancellationToken: TestContext.Current.CancellationToken),
            service.ListWorkspacesAsync("Viewer", cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(["Admin:token-1", "Viewer:token-2"], pages.Select(static page => Assert.Single(page.Value).DisplayName));
        Assert.Equal(2, tokenRequests);
        Assert.Equal(2, handler.CallCount);
        Assert.Null(client.DefaultRequestHeaders.Authorization);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SharedRequestHelper_PreservesExistingPostOperations(bool createItem)
    {
        using var response = WorkspaceListTestData.CreateResponse(createItem
            ? $$"""{"id":"item","displayName":"Lakehouse","type":"Lakehouse","workspaceId":"{{WorkspaceListTestData.WorkspaceId}}"}"""
            : """{"value":[]}""");
        using var handler = new FabricCoreHttpMessageHandler(async (request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(createItem ? $"/v1/workspaces/{WorkspaceListTestData.WorkspaceId}/items" : "/v1/catalog/search", request.RequestUri?.AbsolutePath);
            Assert.NotNull(request.Content);
            using var body = JsonDocument.Parse(await request.Content.ReadAsStringAsync(cancellationToken));
            Assert.Equal(createItem ? "Lakehouse" : "finance", body.RootElement.GetProperty(createItem ? "displayName" : "search").GetString());
            return response;
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceListTestData.CreateCredential());

        if (createItem)
        {
            var item = await service.CreateItemAsync(WorkspaceListTestData.WorkspaceId, new() { DisplayName = "Lakehouse", Type = "Lakehouse" }, TestContext.Current.CancellationToken);
            Assert.Equal("item", item.Id);
        }
        else
        {
            var results = await service.SearchCatalogAsync(new() { Search = "finance" }, TestContext.Current.CancellationToken);
            Assert.Empty(results.Value);
        }
        Assert.Equal(1, handler.CallCount);
    }
}
