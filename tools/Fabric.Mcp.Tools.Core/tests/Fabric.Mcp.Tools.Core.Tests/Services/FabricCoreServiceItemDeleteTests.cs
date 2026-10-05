// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public class FabricCoreServiceItemDeleteTests()
{
    [Theory]
    [InlineData(null, "")]
    [InlineData(false, "?hardDelete=false")]
    [InlineData(true, "?hardDelete=true")]
    public async Task DeleteItemAsync_SendsOneDeleteAndDisposesEmptySuccess(bool? hardDelete, string query)
    {
        using var response = ItemDeleteTestData.CreateResponse();
        var content = Assert.IsType<ItemDeleteTrackingContent>(response.Content);
        HttpRequestMessage? capturedRequest = null;
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            capturedRequest = request;
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal(ItemDeleteTestData.ItemUrl + query, request.RequestUri?.AbsoluteUri);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.Null(request.Content);
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var credential = ItemDeleteTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        await service.DeleteItemAsync($"{{{ItemDeleteTestData.WorkspaceId.ToUpperInvariant()}}}",
            Guid.Parse(ItemDeleteTestData.ItemId).ToString("N"), hardDelete, TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.CallCount);
        Assert.True(content.IsDisposed);
        Assert.NotNull(capturedRequest);
        Assert.Throws<ObjectDisposedException>(() => capturedRequest.RequestUri = new Uri(ItemDeleteTestData.ItemUrl));
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("", ItemDeleteTestData.ItemId)]
    [InlineData("not-a-uuid", ItemDeleteTestData.ItemId)]
    [InlineData("00000000-0000-0000-0000-000000000000", ItemDeleteTestData.ItemId)]
    [InlineData(ItemDeleteTestData.WorkspaceId, " ")]
    [InlineData(ItemDeleteTestData.WorkspaceId, "../items")]
    [InlineData(ItemDeleteTestData.WorkspaceId, "00000000-0000-0000-0000-000000000000")]
    [InlineData(ItemDeleteTestData.WorkspaceId + "/items", ItemDeleteTestData.ItemId)]
    [InlineData(ItemDeleteTestData.WorkspaceId, ItemDeleteTestData.ItemId + "?hardDelete=true")]
    public async Task DeleteItemAsync_RejectsInvalidIdsBeforeAuthentication(string workspaceId, string itemId)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not run."));
        using var client = new HttpClient(handler);
        var credential = ItemDeleteTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.DeleteItemAsync(workspaceId, itemId, true, TestContext.Current.CancellationToken));

        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.UnprocessableEntity)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task DeleteItemAsync_PreservesFailureStatusWithoutRetryOrHardDeleteFallback(HttpStatusCode status)
    {
        using var response = ItemDeleteTestData.CreateResponse(status, "private-backend-detail");
        var content = Assert.IsType<ItemDeleteTrackingContent>(response.Content);
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(ItemDeleteTestData.ItemUrl + "?hardDelete=false", request.RequestUri?.AbsoluteUri);
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, ItemDeleteTestData.CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.DeleteItemAsync(ItemDeleteTestData.WorkspaceId, ItemDeleteTestData.ItemId, false, TestContext.Current.CancellationToken));

        Assert.Equal(status, exception.StatusCode);
        Assert.DoesNotContain("private-", exception.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.True(content.IsDisposed);
    }

    [Theory]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NoContent)]
    public async Task DeleteItemAsync_RejectsUnexpectedSuccessfulStatusWithoutPolling(HttpStatusCode status)
    {
        using var response = ItemDeleteTestData.CreateResponse(status);
        response.Headers.Location = new Uri("https://example.invalid/poll");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, ItemDeleteTestData.CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.DeleteItemAsync(ItemDeleteTestData.WorkspaceId, ItemDeleteTestData.ItemId, null, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
        Assert.Equal(1, handler.CallCount);
        Assert.True(Assert.IsType<ItemDeleteTrackingContent>(response.Content).IsDisposed);
    }

    [Theory]
    [InlineData(null, "Wait before retrying")]
    [InlineData("120", "Wait at least 120 seconds")]
    [InlineData("0", "Wait at least 0 seconds")]
    [InlineData("Tue, 01 Jan 2030 00:00:00 GMT", "Retry after 2030-01-01 00:00:00 UTC")]
    [InlineData("-1", "Wait before retrying")]
    [InlineData("999999999999999999999999999999999999999", "Wait before retrying")]
    [InlineData("private-header-detail", "Wait before retrying")]
    [InlineData("120, 240", "Wait before retrying")]
    public async Task DeleteItemAsync_ValidatesRetryAfterAndDoesNotRetry(string? retryAfter, string expectedMessage)
    {
        using var response = ItemDeleteTestData.CreateResponse(HttpStatusCode.TooManyRequests, "private-backend-detail");
        if (retryAfter is not null)
        {
            Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", retryAfter));
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, ItemDeleteTestData.CreateCredential());

        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(() =>
            service.DeleteItemAsync(ItemDeleteTestData.WorkspaceId, ItemDeleteTestData.ItemId, null, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Contains(expectedMessage, exception.Message);
        Assert.DoesNotContain("private-", exception.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.True(Assert.IsType<ItemDeleteTrackingContent>(response.Content).IsDisposed);
    }

    [Fact]
    public async Task DeleteItemAsync_DoesNotAuthenticateWhenAlreadyCanceled()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        await cancellation.CancelAsync();
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not run."));
        using var client = new HttpClient(handler);
        var credential = ItemDeleteTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.DeleteItemAsync(ItemDeleteTestData.WorkspaceId, ItemDeleteTestData.ItemId, true, cancellation.Token));

        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task DeleteItemAsync_PropagatesCancellationDuringAuthentication()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellation.Token).Returns(_ =>
        {
            cancellation.Cancel();
            return ValueTask.FromCanceled<AccessToken>(cancellation.Token);
        });
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not run."));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.DeleteItemAsync(ItemDeleteTestData.WorkspaceId, ItemDeleteTestData.ItemId, true, cancellation.Token));

        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task DeleteItemAsync_PropagatesCancellationDuringHttpWithoutRetry()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        HttpRequestMessage? capturedRequest = null;
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            capturedRequest = request;
            await cancellation.CancelAsync();
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            throw new InvalidOperationException("The canceled request must not complete.");
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, ItemDeleteTestData.CreateCredential());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.DeleteItemAsync(ItemDeleteTestData.WorkspaceId, ItemDeleteTestData.ItemId, true, cancellation.Token));

        Assert.Equal(1, handler.CallCount);
        Assert.NotNull(capturedRequest);
        Assert.Throws<ObjectDisposedException>(() => capturedRequest.RequestUri = new Uri(ItemDeleteTestData.ItemUrl));
    }

    [Fact]
    public async Task DeleteItemAsync_DoesNotConvertNetworkFailureToSuccessOrRetry()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new HttpRequestException("Network failed."));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, ItemDeleteTestData.CreateCredential());

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.DeleteItemAsync(ItemDeleteTestData.WorkspaceId, ItemDeleteTestData.ItemId, true, TestContext.Current.CancellationToken));

        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExistingOperations_KeepTheirRequestAndResponseBehavior(bool createItem)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
            Assert.NotNull(request.Content);
            Assert.Equal(createItem
                ? $"https://api.fabric.microsoft.com/v1/workspaces/{ItemDeleteTestData.WorkspaceId}/items"
                : "https://api.fabric.microsoft.com/v1/catalog/search", request.RequestUri?.AbsoluteUri);
            return Task.FromResult(ItemDeleteTestData.CreateResponse(body: createItem
                ? $$"""{"id":"{{ItemDeleteTestData.ItemId}}","displayName":"Notebook","type":"Notebook"}"""
                : """{"value":[]}"""));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, ItemDeleteTestData.CreateCredential());

        if (createItem)
        {
            var item = await service.CreateItemAsync(ItemDeleteTestData.WorkspaceId,
                new CreateItemRequest { DisplayName = "Notebook", Type = "Notebook" }, TestContext.Current.CancellationToken);
            Assert.Equal(ItemDeleteTestData.ItemId, item.Id);
        }
        else
        {
            var result = await service.SearchCatalogAsync(new CatalogSearchRequest(), TestContext.Current.CancellationToken);
            Assert.Empty(result.Value);
        }

        Assert.Equal(1, handler.CallCount);
    }
}
