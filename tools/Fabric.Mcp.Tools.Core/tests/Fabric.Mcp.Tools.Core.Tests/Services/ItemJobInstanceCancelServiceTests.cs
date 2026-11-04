// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using NSubstitute;
using Xunit;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceCancelTestData;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class ItemJobInstanceCancelServiceTests()
{
    [Fact]
    public static async Task Cancel_SendsOneNormalizedAuthenticatedPostWithoutBodyAndDisposesResponse()
    {
        using var content = new UnreadableHttpContent();
        using var handler = new FabricCoreHttpMessageHandler((request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(Location + "/cancel", request.RequestUri?.AbsoluteUri);
            Assert.Equal("", request.RequestUri?.Query);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("job-test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.True(token.CanBeCanceled);
            Assert.Null(request.Content);
            return Task.FromResult(Accepted(content: content));
        });
        using var client = new HttpClient(handler);
        var credential = Credential();

        var receipt = await new FabricCoreService(client, credential).CancelItemJobInstanceAsync(
            $"{{{WorkspaceId.ToUpperInvariant()}}}", Guid.Parse(ItemId).ToString("N").ToUpperInvariant(),
            JobInstanceId.ToUpperInvariant(), TestContext.Current.CancellationToken);

        Assert.Equal(Receipt(), receipt);
        Assert.Equal("Requested", receipt.CancellationStatus);
        var json = JsonSerializer.Serialize(receipt, CoreJsonContext.Default.ItemJobInstanceCancelCommandResult);
        using var output = JsonDocument.Parse(json);
        Assert.Equal(["cancellationStatus", "guidance", "itemId", "jobInstanceId", "retryAfterSeconds", "workspaceId"],
            output.RootElement.EnumerateObject().Select(property => property.Name).Order());
        Assert.Equal(1, handler.CallCount);
        Assert.True(content.IsDisposed);
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("workspace")]
    [InlineData("item")]
    [InlineData("job")]
    public static async Task Cancel_RejectsInvalidIdsBeforeAuthenticationOrHttp(string option)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);
        foreach (var value in new[] { null, "", " ", Guid.Empty.ToString(), "../private", "https://example.invalid/private", "%2f", new string('x', 4096) })
        {
            var error = await Assert.ThrowsAsync<ArgumentException>(() => service.CancelItemJobInstanceAsync(
                option == "workspace" ? value! : WorkspaceId, option == "item" ? value! : ItemId,
                option == "job" ? value! : JobInstanceId, TestContext.Current.CancellationToken));
            Assert.DoesNotContain("private", error.Message);
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    public static TheoryData<string?> IgnoredLocations => new()
    {
        (string?)null, "", Location, "private-invalid", "https://example.invalid/private", Location + "/result",
        Location + "?private", Location + "#private", "//api.fabric.microsoft.com/private",
        Location.Replace(WorkspaceId, ActivityId, StringComparison.Ordinal),
        Location.Replace(ItemId, ActivityId, StringComparison.Ordinal),
        Location.Replace(JobInstanceId, ActivityId, StringComparison.Ordinal),
        Location.Replace("https://", "http://", StringComparison.Ordinal),
        Location.Replace("https://", "https://private@", StringComparison.Ordinal),
        Location.Replace("/v1/", "/v1/../v1/", StringComparison.Ordinal),
        Location.Replace("/jobs/", "/jobs%2F", StringComparison.Ordinal),
        Location.Replace("/jobs/", "\\jobs\\", StringComparison.Ordinal),
        " " + Location, Location + "\r\nprivate", new string('x', 4097),
        $"{FabricEndpoints.FabricApiBaseUrl}/operations/{ActivityId}"
    };

    [Theory]
    [MemberData(nameof(IgnoredLocations))]
    public static async Task Cancel_IgnoresLocationWithoutLosingAcceptanceOrChangingTarget(string? location)
    {
        using var content = new UnreadableHttpContent();
        using var response = Accepted(content: content);
        if (location is not null)
        {
            response.Headers.TryAddWithoutValidation("Location", location);
        }
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", ActivityId);
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(Location + "/cancel", request.RequestUri?.AbsoluteUri);
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);

        var receipt = await CancelAsync(new FabricCoreService(client, Credential()));

        Assert.Equal(Receipt(), receipt);
        var json = JsonSerializer.Serialize(receipt, CoreJsonContext.Default.ItemJobInstanceCancelCommandResult);
        Assert.DoesNotContain("private", json);
        Assert.DoesNotContain("https://", json);
        Assert.DoesNotContain(ActivityId, json);
        Assert.DoesNotContain("operationId", json);
        Assert.Equal(1, handler.CallCount);
        Assert.True(content.IsDisposed);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("0", 0L)]
    [InlineData("60", 60L)]
    [InlineData("2147483647", 2147483647L)]
    [InlineData("2147483648", null)]
    [InlineData("-1", null)]
    [InlineData("0.5", null)]
    [InlineData("private-invalid", null)]
    [InlineData("60, 120", null)]
    public static async Task Cancel_ValidatesRetryGuidanceWithoutInventingADelay(string? retry, long? expected)
    {
        using var response = Accepted(retry);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var receipt = await CancelAsync(new FabricCoreService(client, Credential()));
        Assert.Equal("Requested", receipt.CancellationStatus);
        Assert.Equal(expected, receipt.RetryAfterSeconds);
        Assert.Null(receipt.RetryAfterUtc);
        Assert.Equal(expected is null ? "MissingOrInvalidRetryAfter" : null, receipt.RetryAfterIssue);
        Assert.Contains(expected is null ? "first-read delay is unknown" : $"Wait at least {expected} seconds", receipt.Guidance);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Cancel_PreservesHttpDatesAndRejectsMultipleRetryValuesWithoutUsingLocations(bool multiple)
    {
        using var response = Accepted(retryAfter: null);
        response.Headers.TryAddWithoutValidation("Location", [Location, "https://example.invalid/private"]);
        response.Headers.TryAddWithoutValidation("Retry-After", multiple ? ["60", "60"] : ["Tue, 01 Jan 2030 00:00:00 GMT"]);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var receipt = await CancelAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(Guid.Parse(JobInstanceId), receipt.JobInstanceId);
        Assert.Null(receipt.RetryAfterSeconds);
        Assert.Equal(multiple ? null : new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero), receipt.RetryAfterUtc);
        Assert.Equal(multiple ? "MissingOrInvalidRetryAfter" : null, receipt.RetryAfterIssue);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.Created, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.NoContent, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.Found, HttpStatusCode.Found)]
    [InlineData(HttpStatusCode.BadRequest, HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden, HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound, HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Conflict, HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.TooManyRequests, HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable, HttpStatusCode.ServiceUnavailable)]
    public static async Task Cancel_PreservesHttpFailuresWithoutFollowingUrlsOrRetrying(HttpStatusCode status, HttpStatusCode expected)
    {
        using var content = new UnreadableHttpContent();
        using var response = new HttpResponseMessage(status) { Content = content };
        response.Headers.TryAddWithoutValidation("Location", "https://example.invalid/private");
        response.Headers.TryAddWithoutValidation("Retry-After", "90");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemJobInstanceCancelRequestException>(() => CancelAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(expected, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(90), error.RetryAfter?.Delta);
        Assert.Null(error.ErrorCode);
        Assert.DoesNotContain("private", error.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.True(content.IsDisposed);
    }

    [Theory]
    [InlineData("JobAlreadyCompleted")]
    [InlineData("InsufficientPrivileges")]
    [InlineData("ItemNotFound")]
    [InlineData("Future.Error_1-2")]
    public static async Task Cancel_ReadsOnlyABoundedSafeErrorCode(string code)
    {
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(AlreadyCompleted.Replace("JobAlreadyCompleted", code, StringComparison.Ordinal)));
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        response.Headers.TryAddWithoutValidation("Retry-After", "0");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemJobInstanceCancelRequestException>(() => CancelAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.BadRequest, error.StatusCode);
        Assert.Equal(code, error.ErrorCode);
        Assert.False(error.ErrorMetadataUnavailable);
        Assert.Equal(TimeSpan.Zero, error.RetryAfter?.Delta);
        Assert.DoesNotContain("private", error.Message);
        Assert.Null(error.InnerException);
        Assert.Equal(1, handler.CallCount);
        Assert.True(stream.Disposed);
    }

    [Theory]
    [InlineData("")]
    [InlineData("private-not-json")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("""{"errorCode":null}""")]
    [InlineData("""{"errorCode":42}""")]
    [InlineData("""{"errorCode":""}""")]
    [InlineData("""{"errorCode":"private\nsecret"}""")]
    [InlineData("""{"errorCode":"\uD800"}""")]
    [InlineData("""{"errorCode":"\uDC00"}""")]
    [InlineData("""{"error\uD800Code":"JobAlreadyCompleted"}""")]
    [InlineData("""{"error\uDC00Code":"JobAlreadyCompleted"}""")]
    [InlineData("""{"errorCode":"https://example.invalid/private"}""")]
    [InlineData("""{"errorCode":"JobAlreadyCompleted","errorCode":"JobAlreadyCompleted"}""")]
    [InlineData("""{"errorCode":"FutureCode","errorCode":"JobAlreadyCompleted"}""")]
    [InlineData("""{"moreDetails":[{"errorCode":"JobAlreadyCompleted"}]}""")]
    public static async Task Cancel_InvalidErrorMetadataDoesNotReplaceTheKnownHttpFailure(string json)
    {
        using var response = Response(json, HttpStatusCode.TooManyRequests, "60");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemJobInstanceCancelRequestException>(() => CancelAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.TooManyRequests, error.StatusCode);
        Assert.Null(error.ErrorCode);
        Assert.True(error.ErrorMetadataUnavailable);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Cancel_DoesNotSwallowUnrelatedMetadataReaderFailures()
    {
        using var content = new UnreadableHttpContent();
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Content = content };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        await Assert.ThrowsAsync<InvalidOperationException>(() => CancelAsync(new FabricCoreService(client, Credential())));

        Assert.True(content.IsDisposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    public static async Task Cancel_BoundsErrorMetadataIncludingUnknownLengthStreams(int overLimit, bool contentLength)
    {
        const int limit = FabricResponseBodyReader.DefaultMaxJsonBytes;
        using var stream = new PaddedJsonTestStream(Encoding.UTF8.GetBytes(AlreadyCompleted), limit + overLimit);
        using var response = new HttpResponseMessage(HttpStatusCode.Conflict) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        if (contentLength)
        {
            response.Content.Headers.ContentLength = limit + overLimit;
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemJobInstanceCancelRequestException>(() => CancelAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.Conflict, error.StatusCode);
        Assert.Equal(overLimit == 0 ? "JobAlreadyCompleted" : null, error.ErrorCode);
        Assert.Equal(overLimit > 0, error.ErrorMetadataUnavailable);
        Assert.Equal(contentLength ? 0 : limit + overLimit, stream.BytesRead);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("code-length")]
    [InlineData("depth")]
    [InlineData("content-type")]
    public static async Task Cancel_RejectsOversizedCodesDeepJsonAndNonJsonContent(string failure)
    {
        var json = failure == "code-length" ? $$"""{"errorCode":"{{new string('x', 129)}}"}""" :
            failure == "depth" ? new string('[', 65) + AlreadyCompleted + new string(']', 65) : AlreadyCompleted;
        using var response = Response(json, HttpStatusCode.BadRequest);
        if (failure == "content-type")
        {
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemJobInstanceCancelRequestException>(() => CancelAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.BadRequest, error.StatusCode);
        Assert.Null(error.ErrorCode);
        Assert.True(error.ErrorMetadataUnavailable);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("io")]
    [InlineData("network")]
    [InlineData("cancellation")]
    public static async Task Cancel_ErrorBodyReadFailureKeepsTheOriginalFailureAndDisposes(string failure)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(AlreadyCompleted), (_, token) =>
        {
            if (failure == "cancellation")
            {
                cancellation.Cancel();
                return ValueTask.FromCanceled(token);
            }
            return ValueTask.FromException(failure == "io" ? new IOException("private") : new HttpRequestException("private"));
        });
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        response.Headers.TryAddWithoutValidation("Retry-After", "60");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemJobInstanceCancelRequestException>(() => CancelAsync(new FabricCoreService(client, Credential()), cancellation.Token));
        Assert.Equal(HttpStatusCode.TooManyRequests, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.True(error.ErrorMetadataUnavailable);
        Assert.Null(error.ErrorCode);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Cancel_PreSendCancellationStopsBeforeHttp(bool duringAuth)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(_ =>
        {
            cancellation.Cancel();
            return new AccessToken("job-test-token", DateTimeOffset.MaxValue);
        });
        if (!duringAuth)
        {
            await cancellation.CancelAsync();
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CancelAsync(new FabricCoreService(client, credential), cancellation.Token));
        Assert.Equal(0, handler.CallCount);
        await credential.Received(duringAuth ? 1 : 0).GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellation.Token);
    }

    [Fact]
    public static async Task Cancel_AuthenticationFailureStopsBeforeHttp()
    {
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<AccessToken>(new AuthenticationFailedException("private-auth")));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => CancelAsync(new FabricCoreService(client, credential)));
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("network")]
    [InlineData("timeout")]
    [InlineData("cancellation")]
    public static async Task Cancel_UncertainSendNeverRetries(string failure)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = new FabricCoreHttpMessageHandler((_, token) =>
        {
            if (failure == "cancellation")
            {
                cancellation.Cancel();
                return Task.FromCanceled<HttpResponseMessage>(token);
            }
            throw failure == "network" ? new HttpRequestException("private-network") : new TimeoutException("private-timeout");
        });
        using var client = new HttpClient(handler);
        var error = await Record.ExceptionAsync(() => CancelAsync(new FabricCoreService(client, Credential()), cancellation.Token));
        Assert.NotNull(error);
        Assert.IsAssignableFrom(failure == "network" ? typeof(HttpRequestException) :
            failure == "timeout" ? typeof(TimeoutException) : typeof(OperationCanceledException), error);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Cancel_LateCancellationPreservesConfirmedAcceptanceAndDisposesUnreadBody()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var content = new UnreadableHttpContent();
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
        {
            cancellation.Cancel();
            return Task.FromResult(Accepted(content: content));
        });
        using var client = new HttpClient(handler);
        var receipt = await CancelAsync(new FabricCoreService(client, Credential()), cancellation.Token);
        Assert.True(cancellation.IsCancellationRequested);
        Assert.Equal(Receipt(), receipt);
        Assert.True(content.IsDisposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Cancel_IsolatesConcurrentIdentitiesAndKnownTargets()
    {
        var identity = new AsyncLocal<string>();
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new AccessToken(identity.Value!, DateTimeOffset.MaxValue));
        var observed = new ConcurrentBag<string>();
        using var handler = new FabricCoreHttpMessageHandler(async (request, _) =>
        {
            var current = request.Headers.Authorization!.Parameter!;
            observed.Add(current);
            await Task.Yield();
            Assert.Equal($"/v1/workspaces/{WorkspaceId}/items/{ItemId}/jobs/instances/{current}/cancel", request.RequestUri?.AbsolutePath);
            Assert.Null(request.Content);
            return Accepted();
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);
        await Task.WhenAll(Enumerable.Range(0, 8).Select(async _ =>
        {
            var id = Guid.NewGuid();
            identity.Value = id.ToString();
            var receipt = await service.CancelItemJobInstanceAsync(WorkspaceId, ItemId, id.ToString(), TestContext.Current.CancellationToken);
            Assert.Equal(id, receipt.JobInstanceId);
            Assert.Equal("Requested", receipt.CancellationStatus);
        }));
        Assert.Equal(8, handler.CallCount);
        Assert.Equal(8, observed.Distinct().Count());
        await credential.Received(8).GetTokenAsync(Arg.Any<TokenRequestContext>(), TestContext.Current.CancellationToken);
    }
}
