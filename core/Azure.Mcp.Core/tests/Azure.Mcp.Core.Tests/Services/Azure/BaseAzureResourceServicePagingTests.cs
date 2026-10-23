// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers.Text;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Azure;
using Azure.Mcp.Core.Services.Azure;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Models.Paging;
using Xunit;

namespace Azure.Mcp.Core.Tests.Services.Azure;

public sealed class BaseAzureResourceServicePagingTests()
{
    [Theory]
    [InlineData(null, 50)]
    [InlineData(1, 1)]
    [InlineData(1000, 1000)]
    public async Task QueryAsync_UsesBoundedOrderedResourceGraphPage(int? requestedSize, int expectedSize)
    {
        using var handler = new ResourceGraphPagingHttpMessageHandler(_ => Response("""{"data":[],"resultTruncated":false}"""));
        var service = new ResourceGraphPagingTestService(ResourceGraphPagingTestFactory.CreateAzureService(handler));

        var result = await service.QueryAsync(new(requestedSize), cancellationToken: TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);
        var query = request.GetProperty("query").GetString();
        Assert.EndsWith(" | order by id asc", query);
        Assert.DoesNotContain("| limit", query);
        Assert.Equal(expectedSize, request.GetProperty("options").GetProperty("$top").GetInt32());
        Assert.Equal("objectArray", request.GetProperty("options").GetProperty("resultFormat").GetString());
        Assert.False(request.GetProperty("options").TryGetProperty("$skipToken", out _));
        Assert.Equal(ResourceGraphPagingTestFactory.Subscription, request.GetProperty("subscriptions")[0].GetString());
        Assert.Equal(expectedSize, Assert.IsType<ResourceQueryPage>(result.Page).PageSize);
        Assert.Null(result.Page.ContinuationToken);
        Assert.False(result.AreResultsTruncated);
    }

    [Fact]
    public async Task QueryAsync_ResumesOnFreshInstanceWithFreshAuthorization()
    {
        string continuation;
        using (var firstHandler = new ResourceGraphPagingHttpMessageHandler(_ =>
            Response("""{"data":[{"id":"a"}],"resultTruncated":"true","$skipToken":"native-position"}""")))
        {
            var first = new ResourceGraphPagingTestService(ResourceGraphPagingTestFactory.CreateAzureService(firstHandler, "first-credential"));
            var result = await first.QueryAsync(new(1), cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal(["a"], result.Results);
            Assert.True(result.AreResultsTruncated);
            continuation = Assert.IsType<string>(result.Page?.ContinuationToken);
            Assert.Equal("Bearer first-credential", Assert.Single(firstHandler.AuthorizationHeaders));
        }

        using var secondHandler = new ResourceGraphPagingHttpMessageHandler(request =>
        {
            Assert.Equal("native-position", request.GetProperty("options").GetProperty("$skipToken").GetString());
            return Response("""{"data":[{"id":"b"}],"resultTruncated":false}""");
        });
        var second = new ResourceGraphPagingTestService(ResourceGraphPagingTestFactory.CreateAzureService(secondHandler, "second-credential"));

        var next = await second.QueryAsync(new(ContinuationToken: continuation), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["b"], next.Results);
        Assert.Equal(1, next.Page?.PageSize);
        Assert.Null(next.Page?.ContinuationToken);
        Assert.False(next.AreResultsTruncated);
        Assert.Equal("Bearer second-credential", Assert.Single(secondHandler.AuthorizationHeaders));
    }

    [Fact]
    public async Task QueryAsync_EmptyPageRetainsContinuation()
    {
        using var handler = new ResourceGraphPagingHttpMessageHandler(_ =>
            Response("""{"data":[],"resultTruncated":true,"$skipToken":"next"}"""));
        var service = new ResourceGraphPagingTestService(ResourceGraphPagingTestFactory.CreateAzureService(handler));

        var result = await service.QueryAsync(new(), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(result.Results);
        Assert.NotNull(result.Page?.ContinuationToken);
        Assert.True(result.AreResultsTruncated);
    }

    [Theory]
    [InlineData("page-size")]
    [InlineData("resource-type")]
    [InlineData("resource-group")]
    [InlineData("subscription")]
    [InlineData("tenant")]
    [InlineData("cloud")]
    public async Task QueryAsync_RejectsChangedContinuationContextBeforeResourceGraphRequest(string change)
    {
        string continuation;
        using (var firstHandler = new ResourceGraphPagingHttpMessageHandler(_ =>
            Response("""{"data":[],"resultTruncated":true,"$skipToken":"native-position"}""")))
        {
            var first = new ResourceGraphPagingTestService(ResourceGraphPagingTestFactory.CreateAzureService(firstHandler));
            var result = await first.QueryAsync(new(2), cancellationToken: TestContext.Current.CancellationToken);
            continuation = Assert.IsType<string>(result.Page?.ContinuationToken);
        }

        using var secondHandler = new ResourceGraphPagingHttpMessageHandler(_ => throw new InvalidOperationException("No request should be sent."));
        var azureService = ResourceGraphPagingTestFactory.CreateAzureService(
            secondHandler,
            subscription: change == "subscription" ? "00000000-0000-0000-0000-000000000003" : ResourceGraphPagingTestFactory.Subscription,
            tenant: change == "tenant" ? "00000000-0000-0000-0000-000000000004" : ResourceGraphPagingTestFactory.Tenant,
            environment: change == "cloud" ? ArmEnvironment.AzureChina : ArmEnvironment.AzurePublicCloud);
        var second = new ResourceGraphPagingTestService(azureService);

        await Assert.ThrowsAsync<ArgumentException>(() => second.QueryAsync(
            new(change == "page-size" ? 3 : null, continuation),
            resourceType: change == "resource-type" ? "Microsoft.App/containerApps" : "Microsoft.Storage/storageAccounts",
            resourceGroup: change == "resource-group" ? "different-group" : null,
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Empty(secondHandler.Requests);
    }

    [Theory]
    [InlineData("not a cursor")]
    [InlineData("bnVsbA")]
    [InlineData("e30")]
    public async Task QueryAsync_RejectsMalformedContinuationBeforeNetwork(string continuation)
    {
        using var handler = new ResourceGraphPagingHttpMessageHandler(_ => throw new InvalidOperationException("No request should be sent."));
        var service = new ResourceGraphPagingTestService(ResourceGraphPagingTestFactory.CreateAzureService(handler));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.QueryAsync(new(ContinuationToken: continuation), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task QueryAsync_RejectsUnsupportedTokenVersion()
    {
        string continuation;
        using (var firstHandler = new ResourceGraphPagingHttpMessageHandler(_ =>
            Response("""{"data":[],"resultTruncated":true,"$skipToken":"next"}""")))
        {
            var service = new ResourceGraphPagingTestService(ResourceGraphPagingTestFactory.CreateAzureService(firstHandler));
            var result = await service.QueryAsync(new(), cancellationToken: TestContext.Current.CancellationToken);
            continuation = Assert.IsType<string>(result.Page?.ContinuationToken);
        }
        var cursor = JsonNode.Parse(Base64Url.DecodeFromChars(continuation))!.AsObject();
        cursor["version"] = 2;
        var unsupported = Base64Url.EncodeToString(Encoding.UTF8.GetBytes(cursor.ToJsonString()));
        using var handler = new ResourceGraphPagingHttpMessageHandler(_ => throw new InvalidOperationException("No request should be sent."));
        var nextService = new ResourceGraphPagingTestService(ResourceGraphPagingTestFactory.CreateAzureService(handler));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            nextService.QueryAsync(new(ContinuationToken: unsupported), cancellationToken: TestContext.Current.CancellationToken));
        Assert.Empty(handler.Requests);
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("""{"data":[]}""")]
    [InlineData("""{"data":[],"resultTruncated":"invalid"}""")]
    [InlineData("""{"data":[],"resultTruncated":1}""")]
    [InlineData("""{"data":{},"resultTruncated":false}""")]
    [InlineData("""{"data":[{"id":"a"},{"id":"b"},{"id":"c"}],"resultTruncated":false}""")]
    [InlineData("""{"data":[],"resultTruncated":true}""")]
    [InlineData("""{"data":[],"resultTruncated":" true "}""")]
    [InlineData("""{"data":[],"resultTruncated":"true","$skipToken":""}""")]
    [InlineData("""{"data":[],"resultTruncated":false,"$skipToken":123}""")]
    [InlineData("""{"data":[],"resultTruncated":false,"$skipToken":" "}""")]
    public async Task QueryAsync_RejectsInvalidIncompleteOrOversizedBackendPages(string body)
    {
        using var handler = new ResourceGraphPagingHttpMessageHandler(_ => Response(body));
        var service = new ResourceGraphPagingTestService(ResourceGraphPagingTestFactory.CreateAzureService(handler));

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.QueryAsync(new(2), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
    }

    [Fact]
    public async Task QueryAsync_DoesNotHideBackendAuthorizationFailure()
    {
        using var handler = new ResourceGraphPagingHttpMessageHandler(_ =>
            new(HttpStatusCode.Forbidden) { Content = new StringContent("""{"error":{"code":"AuthorizationFailed","message":"Denied"}}""") });
        var service = new ResourceGraphPagingTestService(ResourceGraphPagingTestFactory.CreateAzureService(handler));

        var exception = await Assert.ThrowsAsync<RequestFailedException>(() =>
            service.QueryAsync(new(), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(403, exception.Status);
    }

    [Fact]
    public async Task QueryAsync_PropagatesCancellation()
    {
        using var handler = new ResourceGraphPagingHttpMessageHandler(_ => Response("""{"data":[],"resultTruncated":false}"""));
        var service = new ResourceGraphPagingTestService(ResourceGraphPagingTestFactory.CreateAzureService(handler));
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.QueryAsync(new(), cancellationToken: cancellation.Token));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task QueryLegacyAsync_PreservesExistingLimitAndResponse()
    {
        using var handler = new ResourceGraphPagingHttpMessageHandler(_ => Response("""{"data":[],"resultTruncated":false}"""));
        var service = new ResourceGraphPagingTestService(ResourceGraphPagingTestFactory.CreateAzureService(handler));

        var result = await service.QueryLegacyAsync(TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);
        Assert.EndsWith(" | limit 50", request.GetProperty("query").GetString());
        Assert.False(request.TryGetProperty("options", out _));
        Assert.Null(result.Page);
    }

    private static HttpResponseMessage Response(string body) =>
        new(HttpStatusCode.OK) { Content = new StringContent(body) };
}
