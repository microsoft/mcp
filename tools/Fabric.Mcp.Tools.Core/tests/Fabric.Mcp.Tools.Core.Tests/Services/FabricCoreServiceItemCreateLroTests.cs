// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class FabricCoreServiceItemCreateLroTests()
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task CreateItem_UsesOnePost_ThenOnlyRequestedOperationReads(bool sync, bool accepted)
    {
        var clock = new OperationTestTimeProvider();
        var methods = new List<HttpMethod>();
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            methods.Add(request.Method);
            Assert.Equal("operation-test-token", request.Headers.Authorization?.Parameter);
            if (request.Method == HttpMethod.Post)
            {
                Assert.Equal($"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{FabricOperationTestData.WorkspaceId}/items", request.RequestUri?.AbsoluteUri);
                using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.Equal(["displayName", "type"], body.RootElement.EnumerateObject().Select(property => property.Name).Order());
                return accepted ? FabricOperationTestData.Accepted() : FabricOperationTestData.ItemResponse();
            }
            if (request.RequestUri!.AbsolutePath.EndsWith("/result", StringComparison.Ordinal))
            {
                Assert.Equal($"/v1/operations/{FabricOperationTestData.OperationId}/result", request.RequestUri.AbsolutePath);
                var response = FabricOperationTestData.ItemResponse();
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            Assert.Equal($"/v1/operations/{FabricOperationTestData.OperationId}", request.RequestUri.AbsolutePath);
            return FabricOperationTestData.JsonResponse("""{"status":"Succeeded"}""");
        });
        using var client = new HttpClient(handler);
        var credential = FabricOperationTestData.Credential();
        var service = new FabricCoreService(client, credential, clock);

        var task = CreateAsync(service, new(Sync: sync), TestContext.Current.CancellationToken);
        if (sync && accepted)
        {
            await clock.WaitForTimerAsync(3, TestContext.Current.CancellationToken);
            clock.Advance(3);
        }
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, methods.Count(method => method == HttpMethod.Post));
        Assert.Equal(sync && accepted ? 3 : 1, handler.CallCount);
        await credential.Received(handler.CallCount).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)), Arg.Any<CancellationToken>());
        if (accepted && !sync)
        {
            Assert.Null(result.Item);
            Assert.Equal(FabricOperationStatus.Accepted, result.Operation?.Status);
            Assert.Null(result.Operation?.LastState);
            Assert.Equal(20, result.Operation?.ServerRetryAfterSeconds);
            Assert.Equal(3, result.Operation?.RecommendedPollAfterSeconds);
        }
        else
        {
            Assert.Equal(FabricOperationTestData.ItemId, result.Item?.Id);
            Assert.Equal(accepted ? FabricOperationStatus.Succeeded : null, result.Operation?.Status);
        }
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateItem_RejectsInvalidBudgetBeforeMutationEvenWithoutWaiting(double seconds)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = FabricOperationTestData.Credential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAsync<ArgumentException>(() => CreateAsync(service, new(MaxWaitSeconds: seconds), TestContext.Current.CancellationToken));

        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(600)]
    [InlineData(double.MaxValue)]
    public async Task CreateItem_AllowsFinitePositiveBudgetsWithoutArbitraryPublicCap(double seconds)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(FabricOperationTestData.Accepted()));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var result = await CreateAsync(service, new(MaxWaitSeconds: seconds), TestContext.Current.CancellationToken);

        Assert.Equal(FabricOperationStatus.Accepted, result.Operation?.Status);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{")]
    [InlineData("""{"id":"invalid","displayName":"Item","type":"Lakehouse"}""")]
    [InlineData("""{"id":"5b218778-e7a5-4d73-8187-f10824047715","workspaceId":"5b218778-e7a5-4d73-8187-f10824047715","displayName":"Item","type":"Lakehouse"}""")]
    [InlineData("""{"id":"5b218778-e7a5-4d73-8187-f10824047715","workspaceId":"cfafbeb1-8037-4d0c-896e-a46fb27ff229","displayName":"","type":"Lakehouse"}""")]
    public async Task CreateItem_RejectsMalformedCompletionWithoutFabricatingItem(string json)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
            Task.FromResult(FabricOperationTestData.JsonResponse(json, HttpStatusCode.Created)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var result = await CreateAsync(service, new(Sync: true), TestContext.Current.CancellationToken);

        Assert.Null(result.Item);
        Assert.Equal(FabricOperationStatus.ResultUnavailable, result.Operation?.Status);
        Assert.Equal((int)HttpStatusCode.BadGateway, result.Operation?.Issue?.HttpStatus);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.PartialContent)]
    public async Task CreateItem_OnlyAcceptsDocumentedSynchronousStatus(HttpStatusCode status)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
        {
            var response = FabricOperationTestData.ItemResponse();
            response.StatusCode = status;
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var error = await Assert.ThrowsAsync<FabricOperationException>(() => CreateAsync(service, new(), TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
        Assert.Contains("whether creation occurred", error.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task BudgetShorterThanInitialDelay_ReturnsAcceptedReferenceWithoutStartingGet()
    {
        var clock = new OperationTestTimeProvider();
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(FabricOperationTestData.Accepted()));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential(), clock);

        var task = CreateAsync(service, new(Sync: true, MaxWaitSeconds: 1), TestContext.Current.CancellationToken);
        clock.Advance(1);
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(FabricOperationStatus.Pending, result.Operation?.Status);
        Assert.Equal(Guid.Parse(FabricOperationTestData.OperationId), result.Operation?.OperationId);
        Assert.Null(result.Operation?.LastState);
        Assert.Equal("WaitBudgetExpired", result.Operation?.Issue?.Code);
        Assert.Equal(1, handler.CallCount);
        Assert.Empty(clock.Timers);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task BudgetBoundsInFlightHeadersAndBody_WithoutLosingKnownCompletion(bool blockBody, bool resultPhase)
    {
        var clock = new OperationTestTimeProvider();
        var blocked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var posts = 0;
        OperationTestStream? stream = null;
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            if (request.Method == HttpMethod.Post)
            {
                posts++;
                return FabricOperationTestData.Accepted();
            }
            if (resultPhase && !request.RequestUri!.AbsolutePath.EndsWith("/result", StringComparison.Ordinal))
            {
                return FabricOperationTestData.JsonResponse("""{"status":"Succeeded"}""");
            }
            if (blockBody)
            {
                stream = new OperationTestStream(Encoding.UTF8.GetBytes("{}"), async (_, readToken) =>
                {
                    blocked.TrySetResult();
                    await Task.Delay(Timeout.InfiniteTimeSpan, readToken);
                });
                var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
                response.Content.Headers.ContentType = new("application/json");
                return response;
            }
            blocked.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            throw new InvalidOperationException("Canceled requests must not continue.");
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential(), clock);

        var task = CreateAsync(service, new(Sync: true), TestContext.Current.CancellationToken);
        clock.Advance(3);
        await blocked.Task.WaitAsync(TestContext.Current.CancellationToken);
        clock.Advance(117);
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(resultPhase ? FabricOperationStatus.ResultUnavailable : FabricOperationStatus.Pending, result.Operation?.Status);
        Assert.Equal(resultPhase ? "Succeeded" : null, result.Operation?.LastState?.Status);
        Assert.Equal("WaitBudgetExpired", result.Operation?.Issue?.Code);
        Assert.Equal(1, posts);
        Assert.Equal(resultPhase ? 3 : 2, handler.CallCount);
        if (blockBody)
        {
            Assert.True(stream?.Disposed);
        }
        Assert.Empty(clock.Timers);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CallerCancellationPropagatesWithReceipt_AndDoesNotCancelFabric(bool afterSucceeded)
    {
        var clock = new OperationTestTimeProvider();
        using var caller = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var readingResult = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            if (request.Method == HttpMethod.Post)
            {
                return FabricOperationTestData.Accepted();
            }
            if (request.RequestUri!.AbsolutePath.EndsWith("/result", StringComparison.Ordinal))
            {
                readingResult.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                throw new InvalidOperationException("Canceled requests must not continue.");
            }
            return FabricOperationTestData.JsonResponse("""{"status":"Succeeded"}""");
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential(), clock);
        var task = CreateAsync(service, new(Sync: true), caller.Token);
        if (afterSucceeded)
        {
            clock.Advance(3);
            await readingResult.Task.WaitAsync(TestContext.Current.CancellationToken);
        }
        await caller.CancelAsync();

        var error = await Assert.ThrowsAsync<FabricOperationCanceledException>(() => task);

        Assert.Equal(caller.Token, error.CancellationToken);
        Assert.Equal("CallerCanceled", error.Operation.Issue?.Code);
        Assert.Equal(Guid.Parse(FabricOperationTestData.OperationId), error.Operation.OperationId);
        Assert.Equal(afterSucceeded ? FabricOperationStatus.ResultUnavailable : FabricOperationStatus.TrackingStopped, error.Operation.Status);
        Assert.Equal(afterSucceeded ? "Succeeded" : null, error.Operation.LastState?.Status);
        Assert.Equal(afterSucceeded ? 3 : 1, handler.CallCount);
        Assert.Empty(clock.Timers);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.OK)]
    public async Task ResultFailure_DoesNotTurnSucceededOperationIntoFailedOrNoResult(HttpStatusCode resultStatus)
    {
        var clock = new OperationTestTimeProvider();
        using var handler = new FabricCoreHttpMessageHandler((request, _) => Task.FromResult(
            request.Method == HttpMethod.Post ? FabricOperationTestData.Accepted() :
            request.RequestUri!.AbsolutePath.EndsWith("/result", StringComparison.Ordinal)
                ? FabricOperationTestData.JsonResponse("private-backend-detail", resultStatus)
                : FabricOperationTestData.JsonResponse("""{"status":"Succeeded"}""")));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential(), clock);

        var task = CreateAsync(service, new(Sync: true), TestContext.Current.CancellationToken);
        clock.Advance(3);
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(FabricOperationStatus.ResultUnavailable, result.Operation?.Status);
        Assert.Equal("Succeeded", result.Operation?.LastState?.Status);
        Assert.Equal(Guid.Parse(FabricOperationTestData.OperationId), result.Operation?.OperationId);
        Assert.Equal((int)(resultStatus == HttpStatusCode.OK ? HttpStatusCode.BadGateway : resultStatus), result.Operation?.Issue?.HttpStatus);
        Assert.DoesNotContain("private-backend-detail", result.Operation?.Issue?.Message);
        Assert.Null(result.Item);
        Assert.Equal(3, handler.CallCount);
    }

    [Fact]
    public async Task StateReceivedAtDeadline_IsNotDowngradedFromSucceeded()
    {
        var clock = new OperationTestTimeProvider();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            if (request.Method == HttpMethod.Post)
            {
                return Task.FromResult(FabricOperationTestData.Accepted());
            }
            var stream = new OperationTestStream(Encoding.UTF8.GetBytes("""{"status":"Succeeded"}"""), (read, _) =>
            {
                if (read == 0)
                {
                    clock.Advance(117);
                }
                return ValueTask.CompletedTask;
            });
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
            response.Content.Headers.ContentType = new("application/json");
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential(), clock);

        var task = CreateAsync(service, new(Sync: true), TestContext.Current.CancellationToken);
        clock.Advance(3);
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(FabricOperationStatus.ResultUnavailable, result.Operation?.Status);
        Assert.Equal("Succeeded", result.Operation?.LastState?.Status);
        Assert.Equal("WaitBudgetExpired", result.Operation?.Issue?.Code);
        Assert.Equal(2, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CompletedItem_IsPreservedWhenCancellationArrivesAfterResultRead(bool accepted)
    {
        var clock = new OperationTestTimeProvider();
        using var caller = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            if (accepted && request.Method == HttpMethod.Post)
            {
                return FabricOperationTestData.Accepted();
            }
            if (accepted && !request.RequestUri!.AbsolutePath.EndsWith("/result", StringComparison.Ordinal))
            {
                return FabricOperationTestData.JsonResponse("""{"status":"Succeeded"}""");
            }
            using var item = FabricOperationTestData.ItemResponse();
            var bytes = await item.Content.ReadAsByteArrayAsync(token);
            var stream = new OperationTestStream(bytes, (read, _) =>
            {
                if (read == 0)
                {
                    caller.Cancel();
                }
                return ValueTask.CompletedTask;
            });
            var response = new HttpResponseMessage(accepted ? HttpStatusCode.OK : HttpStatusCode.Created) { Content = new StreamContent(stream) };
            response.Content.Headers.ContentType = new("application/json");
            return response;
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential(), clock);

        var task = CreateAsync(service, new(Sync: true), caller.Token);
        if (accepted)
        {
            clock.Advance(3);
        }
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.True(caller.IsCancellationRequested);
        Assert.Equal(FabricOperationTestData.ItemId, result.Item?.Id);
        Assert.Equal(accepted ? FabricOperationStatus.Succeeded : null, result.Operation?.Status);
        Assert.Equal(accepted ? 3 : 1, handler.CallCount);
    }

    [Fact]
    public async Task ConcurrentOperations_ResolveCredentialsForEveryReadWithoutCrossUserTokens()
    {
        var identity = new AsyncLocal<string>();
        var calls = new ConcurrentDictionary<string, int>();
        var observed = new ConcurrentBag<string>();
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(_ =>
        {
            var user = identity.Value!;
            return new AccessToken($"{user}-{calls.AddOrUpdate(user, 1, static (_, count) => count + 1)}", DateTimeOffset.MaxValue);
        });
        var clock = new OperationTestTimeProvider();
        using var handler = new FabricCoreHttpMessageHandler(async (request, _) =>
        {
            await Task.Yield();
            var user = identity.Value!;
            var id = user == "first" ? FabricOperationTestData.OperationId : FabricOperationTestData.ItemId;
            Assert.StartsWith(user + "-", request.Headers.Authorization?.Parameter);
            observed.Add(request.Headers.Authorization!.Parameter!);
            if (request.Method == HttpMethod.Post)
            {
                return FabricOperationTestData.Accepted(id);
            }
            Assert.StartsWith($"/v1/operations/{id}", request.RequestUri?.AbsolutePath);
            if (request.RequestUri!.AbsolutePath.EndsWith("/result", StringComparison.Ordinal))
            {
                var response = FabricOperationTestData.ItemResponse();
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            return FabricOperationTestData.JsonResponse("""{"status":"Succeeded"}""");
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential, clock);

        var first = CreateAsAsync("first");
        var second = CreateAsAsync("second");
        await clock.WaitForTimerAsync(3, TestContext.Current.CancellationToken);
        await clock.WaitForTimerAsync(3, TestContext.Current.CancellationToken);
        clock.Advance(3);
        var results = await Task.WhenAll(first, second);

        Assert.All(results, result => Assert.Equal(FabricOperationTestData.ItemId, result.Item?.Id));
        Assert.Equal(["first-1", "first-2", "first-3", "second-1", "second-2", "second-3"], observed.Order());
        Assert.Equal(6, handler.CallCount);
        Assert.Null(client.DefaultRequestHeaders.Authorization);

        async Task<ItemCreateCommandResult> CreateAsAsync(string user)
        {
            identity.Value = user;
            return await CreateAsync(service, new(Sync: true), TestContext.Current.CancellationToken);
        }
    }

    private static Task<ItemCreateCommandResult> CreateAsync(
        FabricCoreService service, FabricOperationWaitOptions options, CancellationToken cancellationToken) =>
        service.CreateItemAsync(FabricOperationTestData.WorkspaceId,
            new() { DisplayName = "Created", Type = "Lakehouse" }, options, cancellationToken);

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests, false)]
    [InlineData(HttpStatusCode.ServiceUnavailable, false)]
    [InlineData(HttpStatusCode.TooManyRequests, true)]
    [InlineData(HttpStatusCode.ServiceUnavailable, true)]
    public async Task SafeGetRetries_UseValidatedHttpDateWithoutRepeatingPost(HttpStatusCode status, bool resultPhase)
    {
        var clock = new OperationTestTimeProvider();
        var attempts = 0;
        var posts = 0;
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            if (request.Method == HttpMethod.Post)
            {
                posts++;
                return Task.FromResult(FabricOperationTestData.Accepted());
            }
            var isResult = request.RequestUri!.AbsolutePath.EndsWith("/result", StringComparison.Ordinal);
            if (isResult == resultPhase && ++attempts == 1)
            {
                return Task.FromResult(FabricOperationTestData.JsonResponse("private-backend-detail", status, "Tue, 01 Jan 2030 00:00:23 GMT"));
            }
            if (isResult)
            {
                var response = FabricOperationTestData.ItemResponse();
                response.StatusCode = HttpStatusCode.OK;
                return Task.FromResult(response);
            }
            return Task.FromResult(FabricOperationTestData.JsonResponse("""{"status":"Succeeded"}"""));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential(), clock);

        var task = CreateAsync(service, new(Sync: true), TestContext.Current.CancellationToken);
        clock.Advance(3);
        await clock.WaitForTimerAsync(20, TestContext.Current.CancellationToken);
        Assert.Equal(1, attempts);
        clock.Advance(20);
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(FabricOperationTestData.ItemId, result.Item?.Id);
        Assert.Equal(2, attempts);
        Assert.Equal(1, posts);
        Assert.Equal(4, handler.CallCount);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task TransportFailuresAndPerRequestTimeouts_AreNotRetriedOrCalledOperationFailures(bool resultPhase, bool timeout)
    {
        var clock = new OperationTestTimeProvider();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            if (request.Method == HttpMethod.Post)
            {
                return Task.FromResult(FabricOperationTestData.Accepted());
            }
            if (resultPhase && !request.RequestUri!.AbsolutePath.EndsWith("/result", StringComparison.Ordinal))
            {
                return Task.FromResult(FabricOperationTestData.JsonResponse("""{"status":"Succeeded"}"""));
            }
            return Task.FromException<HttpResponseMessage>(timeout
                ? new TaskCanceledException("private-timeout-detail")
                : new HttpRequestException("private-transport-detail", null, HttpStatusCode.ServiceUnavailable));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential(), clock);

        var task = CreateAsync(service, new(Sync: true), TestContext.Current.CancellationToken);
        clock.Advance(3);
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(resultPhase ? FabricOperationStatus.ResultUnavailable : FabricOperationStatus.TrackingStopped, result.Operation?.Status);
        Assert.Equal((int)(timeout ? HttpStatusCode.GatewayTimeout : HttpStatusCode.ServiceUnavailable), result.Operation?.Issue?.HttpStatus);
        Assert.DoesNotContain("private-", result.Operation?.Issue?.Message);
        Assert.Equal(resultPhase ? 3 : 2, handler.CallCount);
        Assert.Empty(clock.Timers);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WaitBudgetBoundsAuthentication_AndNeverSendsGetAfterDeadline(bool returnsTokenLate)
    {
        var clock = new OperationTestTimeProvider();
        var credentialStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var tokenCalls = 0;
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            if (++tokenCalls == 1)
            {
                return ValueTask.FromResult(new AccessToken("initial-token", DateTimeOffset.MaxValue));
            }
            credentialStarted.TrySetResult();
            if (returnsTokenLate)
            {
                clock.Advance(117);
                return ValueTask.FromResult(new AccessToken("too-late-token", DateTimeOffset.MaxValue));
            }
            return WaitForCredentialAsync(call.Arg<CancellationToken>());
        });
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            return Task.FromResult(FabricOperationTestData.Accepted());
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential, clock);

        var task = CreateAsync(service, new(Sync: true), TestContext.Current.CancellationToken);
        clock.Advance(3);
        await credentialStarted.Task.WaitAsync(TestContext.Current.CancellationToken);
        if (!returnsTokenLate)
        {
            clock.Advance(117);
        }
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(FabricOperationStatus.Pending, result.Operation?.Status);
        Assert.Equal("WaitBudgetExpired", result.Operation?.Issue?.Code);
        Assert.Equal(2, tokenCalls);
        Assert.Equal(1, handler.CallCount);
        Assert.Empty(clock.Timers);

        static async ValueTask<AccessToken> WaitForCredentialAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException("Canceled credential requests must not continue.");
        }
    }
}
