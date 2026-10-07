// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public class FabricCoreServiceWorkspaceDeleteTests()
{
    [Theory]
    [InlineData(WorkspaceDeleteTestData.WorkspaceId)]
    [InlineData("CFAFBEB1-8037-4D0C-896E-A46FB27FF222")]
    [InlineData("{cfafbeb1-8037-4d0c-896e-a46fb27ff222}")]
    [InlineData("cfafbeb180374d0c896ea46fb27ff222")]
    public async Task DeleteWorkspaceAsync_SendsOneDeleteWithoutBodyToExactNormalizedEndpoint(string workspaceId)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal($"https://api.fabric.microsoft.com/v1/workspaces/{WorkspaceDeleteTestData.WorkspaceId}", request.RequestUri?.AbsoluteUri);
            Assert.Null(request.Content);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal(WorkspaceDeleteTestData.Token, request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });
        using var client = new HttpClient(handler);
        var credential = WorkspaceDeleteTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        await service.DeleteWorkspaceAsync(workspaceId, TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.CallCount);
        Assert.Null(client.DefaultRequestHeaders.Authorization);
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.Length == 1 && context.Scopes[0] == FabricEndpoints.FabricScope),
            Arg.Is<CancellationToken>(token => token == TestContext.Current.CancellationToken));
    }

    [Theory]
    [MemberData(nameof(WorkspaceDeleteTestData.InvalidWorkspaceIds), MemberType = typeof(WorkspaceDeleteTestData))]
    public async Task DeleteWorkspaceAsync_RejectsInvalidUuidBeforeCredentialsOrHttp(string? workspaceId)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var client = new HttpClient(handler);
        var credential = WorkspaceDeleteTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteWorkspaceAsync(workspaceId!, TestContext.Current.CancellationToken));

        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task DeleteWorkspaceAsync_AcceptsEmpty200WithoutDeserializingWorkspace(string? body)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = body is null ? null : new StringContent(body)
        };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceDeleteTestData.CreateCredential());

        await service.DeleteWorkspaceAsync(WorkspaceDeleteTestData.WorkspaceId, TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.Accepted)]
    public async Task DeleteWorkspaceAsync_DisposesRequestAndResponseWithoutReadingBody(HttpStatusCode status)
    {
        using var content = new WorkspaceDeleteResponseContent();
        using var response = new HttpResponseMessage(status) { Content = content };
        HttpRequestMessage? sentRequest = null;
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            sentRequest = request;
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceDeleteTestData.CreateCredential());

        if (status == HttpStatusCode.OK)
        {
            await service.DeleteWorkspaceAsync(WorkspaceDeleteTestData.WorkspaceId, TestContext.Current.CancellationToken);
        }
        else
        {
            await Assert.ThrowsAnyAsync<HttpRequestException>(() =>
                service.DeleteWorkspaceAsync(WorkspaceDeleteTestData.WorkspaceId, TestContext.Current.CancellationToken));
        }

        Assert.False(content.ReadAttempted);
        Assert.True(content.IsDisposed);
        Assert.NotNull(sentRequest);
        using var unusedContent = new StringContent("");
        Assert.Throws<ObjectDisposedException>(() => sentRequest.Content = unusedContent);
        Assert.Equal(1, handler.CallCount);
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
    [InlineData(HttpStatusCode.GatewayTimeout)]
    [InlineData(HttpStatusCode.Redirect)]
    public async Task DeleteWorkspaceAsync_PreservesFailureStatusWithoutRetryingOrEchoingBody(HttpStatusCode status)
    {
        using var response = new HttpResponseMessage(status) { Content = new StringContent("private-backend-detail") };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceDeleteTestData.CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.DeleteWorkspaceAsync(WorkspaceDeleteTestData.WorkspaceId, TestContext.Current.CancellationToken));

        Assert.Equal(status, exception.StatusCode);
        Assert.DoesNotContain("private-", exception.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.PartialContent)]
    public async Task DeleteWorkspaceAsync_RejectsUndocumentedSuccessWithoutPolling(HttpStatusCode status)
    {
        using var response = new HttpResponseMessage(status);
        response.Headers.Location = new Uri("https://untrusted.example/operations/private-operation");
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(1));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceDeleteTestData.CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.DeleteWorkspaceAsync(WorkspaceDeleteTestData.WorkspaceId, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
        Assert.DoesNotContain("private-", exception.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("0", "Wait at least 0 seconds")]
    [InlineData("120", "Wait at least 120 seconds")]
    [InlineData("Tue, 01 Jan 2030 00:00:00 GMT", "Retry after 2030-01-01 00:00:00 UTC")]
    public async Task DeleteWorkspaceAsync_PreservesOnlyValidatedRetryAfterGuidance(string retryAfter, string expected)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Content = new StringContent("private-body") };
        Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", retryAfter));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceDeleteTestData.CreateCredential());

        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(() =>
            service.DeleteWorkspaceAsync(WorkspaceDeleteTestData.WorkspaceId, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Contains(expected, exception.Message);
        Assert.DoesNotContain("private-", exception.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("-1")]
    [InlineData("1.5")]
    [InlineData("private-header-detail")]
    [InlineData("120, 240")]
    [InlineData("99999999999999999999999999999999999999999")]
    public async Task DeleteWorkspaceAsync_DoesNotEchoInvalidRetryAfter(string? retryAfter)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        if (retryAfter is not null)
        {
            Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", retryAfter));
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceDeleteTestData.CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.DeleteWorkspaceAsync(WorkspaceDeleteTestData.WorkspaceId, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Equal("Fabric workspace deletion was not confirmed.", exception.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task DeleteWorkspaceAsync_RejectsAmbiguousRetryAfterHeaders()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", ["120", "240"]));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceDeleteTestData.CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.DeleteWorkspaceAsync(WorkspaceDeleteTestData.WorkspaceId, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Equal("Fabric workspace deletion was not confirmed.", exception.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task DeleteWorkspaceAsync_RepeatedRequestDoesNotTurn404IntoSuccess()
    {
        var calls = 0;
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(++calls == 1 ? HttpStatusCode.OK : HttpStatusCode.NotFound)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceDeleteTestData.CreateCredential());

        await service.DeleteWorkspaceAsync(WorkspaceDeleteTestData.WorkspaceId, TestContext.Current.CancellationToken);
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.DeleteWorkspaceAsync(WorkspaceDeleteTestData.WorkspaceId, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task DeleteWorkspaceAsync_PreCancellationSkipsCredentialsAndHttp()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        await cancellation.CancelAsync();
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var client = new HttpClient(handler);
        var credential = WorkspaceDeleteTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.DeleteWorkspaceAsync(WorkspaceDeleteTestData.WorkspaceId, cancellation.Token));

        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DeleteWorkspaceAsync_CancellationDuringCredentialsSkipsHttp(bool tokenReturned)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var client = new HttpClient(handler);
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                Assert.Equal(cancellation.Token, call.Arg<CancellationToken>());
                cancellation.Cancel();
                return tokenReturned
                    ? ValueTask.FromResult(new AccessToken(WorkspaceDeleteTestData.Token, DateTimeOffset.MaxValue))
                    : ValueTask.FromCanceled<AccessToken>(cancellation.Token);
            });
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.DeleteWorkspaceAsync(WorkspaceDeleteTestData.WorkspaceId, cancellation.Token));

        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DeleteWorkspaceAsync_CancellationDuringHttpNeverConfirmsDeletion(bool responseReturned)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var content = new WorkspaceDeleteResponseContent();
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        using var handler = new FabricCoreHttpMessageHandler((_, token) =>
        {
            Assert.True(token.CanBeCanceled);
            cancellation.Cancel();
            Assert.True(token.IsCancellationRequested);
            return responseReturned ? Task.FromResult(response) : Task.FromCanceled<HttpResponseMessage>(token);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceDeleteTestData.CreateCredential());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.DeleteWorkspaceAsync(WorkspaceDeleteTestData.WorkspaceId, cancellation.Token));

        Assert.Equal(1, handler.CallCount);
        if (responseReturned)
        {
            Assert.True(content.IsDisposed);
        }
        Assert.False(content.ReadAttempted);
    }

    [Fact]
    public async Task DeleteWorkspaceAsync_ConcurrentRequestsKeepTargetsAndCredentialsSeparate()
    {
        const string secondWorkspace = "11111111-2222-3333-4444-555555555555";
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.CancelAfter(TimeSpan.FromSeconds(10));
        var bothSent = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var requests = new ConcurrentDictionary<string, string>();
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.True(requests.TryAdd(request.RequestUri!.AbsolutePath, request.Headers.Authorization!.Parameter!));
            if (requests.Count == 2)
            {
                bothSent.TrySetResult();
            }
            await bothSent.Task.WaitAsync(token);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var client = new HttpClient(handler);
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("first-token", DateTimeOffset.MaxValue), new AccessToken("second-token", DateTimeOffset.MaxValue));
        var service = new FabricCoreService(client, credential);

        await Task.WhenAll(
            service.DeleteWorkspaceAsync(WorkspaceDeleteTestData.WorkspaceId, cancellation.Token),
            service.DeleteWorkspaceAsync(secondWorkspace, cancellation.Token));

        Assert.Equal("first-token", requests[$"/v1/workspaces/{WorkspaceDeleteTestData.WorkspaceId}"]);
        Assert.Equal("second-token", requests[$"/v1/workspaces/{secondWorkspace}"]);
        Assert.Equal(2, handler.CallCount);
        Assert.Null(client.DefaultRequestHeaders.Authorization);
        await credential.Received(2).GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateItemAsync_PreservesExistingAuthenticatedJsonPost()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent("""{"id":"item-id","displayName":"Example","type":"Lakehouse"}""")
        };
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal($"https://api.fabric.microsoft.com/v1/workspaces/{WorkspaceDeleteTestData.WorkspaceId}/items", request.RequestUri?.AbsoluteUri);
            Assert.Equal(WorkspaceDeleteTestData.Token, request.Headers.Authorization?.Parameter);
            Assert.NotNull(request.Content);
            Assert.Equal("application/json", request.Content.Headers.ContentType?.MediaType);
            using var body = JsonDocument.Parse(await request.Content.ReadAsStringAsync(token));
            Assert.Equal("Example", body.RootElement.GetProperty("displayName").GetString());
            Assert.Equal("Lakehouse", body.RootElement.GetProperty("type").GetString());
            return response;
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceDeleteTestData.CreateCredential());

        var item = await service.CreateItemAsync(
            WorkspaceDeleteTestData.WorkspaceId, new CreateItemRequest { DisplayName = "Example", Type = "Lakehouse" },
            TestContext.Current.CancellationToken);

        Assert.Equal("item-id", item.Id);
        Assert.Equal("Example", item.DisplayName);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task SearchCatalogAsync_PreservesExistingAuthenticatedJsonPost()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"value":[{"id":"item-id","displayName":"Example"}],"continuationToken":"next-page"}""")
        };
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("https://api.fabric.microsoft.com/v1/catalog/search", request.RequestUri?.AbsoluteUri);
            Assert.Equal(WorkspaceDeleteTestData.Token, request.Headers.Authorization?.Parameter);
            Assert.NotNull(request.Content);
            using var body = JsonDocument.Parse(await request.Content.ReadAsStringAsync(token));
            Assert.Equal("Example", body.RootElement.GetProperty("search").GetString());
            Assert.Equal(5, body.RootElement.GetProperty("pageSize").GetInt32());
            return response;
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceDeleteTestData.CreateCredential());

        var page = await service.SearchCatalogAsync(
            new CatalogSearchRequest { Search = "Example", PageSize = 5 }, TestContext.Current.CancellationToken);

        Assert.Equal("item-id", Assert.Single(page.Value).Id);
        Assert.Equal("next-page", page.ContinuationToken);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExistingPostOperations_PreserveErrorHandling(bool createItem)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new StringContent("legacy-error") };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceDeleteTestData.CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            if (createItem)
            {
                await service.CreateItemAsync(WorkspaceDeleteTestData.WorkspaceId, new CreateItemRequest(), TestContext.Current.CancellationToken);
            }
            else
            {
                await service.SearchCatalogAsync(new CatalogSearchRequest(), TestContext.Current.CancellationToken);
            }
        });

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
        Assert.Contains("403", exception.Message);
        Assert.Contains("legacy-error", exception.Message);
        Assert.Equal(1, handler.CallCount);
    }
}
