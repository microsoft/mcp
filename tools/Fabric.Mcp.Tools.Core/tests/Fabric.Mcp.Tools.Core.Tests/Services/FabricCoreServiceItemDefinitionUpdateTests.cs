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
using Fabric.Mcp.Tools.Core.Validation;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class FabricCoreServiceItemDefinitionUpdateTests()
{
    [Theory]
    [InlineData(false, false, null)]
    [InlineData(true, false, null)]
    [InlineData(false, true, null)]
    [InlineData(true, true, null)]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public async Task Update_UsesOnePostAndOnlyRequestedStateReads(bool sync, bool accepted, bool? updateMetadata)
    {
        var clock = new OperationTestTimeProvider();
        var methods = new List<HttpMethod>();
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            methods.Add(request.Method);
            Assert.Equal("operation-test-token", request.Headers.Authorization?.Parameter);
            if (request.Method == HttpMethod.Post)
            {
                var query = updateMetadata switch { true => "?updateMetadata=true", false => "?updateMetadata=false", _ => "" };
                Assert.Equal(ItemDefinitionUpdateTestData.Url + query, request.RequestUri?.AbsoluteUri);
                using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.Equal(["definition"], body.RootElement.EnumerateObject().Select(static property => property.Name));
                using var expected = JsonDocument.Parse(ItemDefinitionUpdateTestData.Definition);
                Assert.True(JsonElement.DeepEquals(expected.RootElement, body.RootElement.GetProperty("definition")));
                return accepted ? FabricOperationTestData.Accepted() : new(HttpStatusCode.OK);
            }
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{FabricEndpoints.FabricApiBaseUrl}/operations/{FabricOperationTestData.OperationId}", request.RequestUri?.AbsoluteUri);
            return FabricOperationTestData.JsonResponse("""{"status":"Succeeded"}""");
        });
        using var client = new HttpClient(handler);
        var credential = FabricOperationTestData.Credential();
        var service = new FabricCoreService(client, credential, clock);

        var task = service.UpdateItemDefinitionAsync(
            $"{{{FabricOperationTestData.WorkspaceId.ToUpperInvariant()}}}", Guid.Parse(FabricOperationTestData.ItemId).ToString("N"),
            Request(), updateMetadata, new(Sync: sync), TestContext.Current.CancellationToken);
        if (sync && accepted)
        {
            await clock.WaitForTimerAsync(3, TestContext.Current.CancellationToken);
            clock.Advance(3);
        }
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(accepted && !sync ? FabricOperationStatus.Accepted : FabricOperationStatus.Succeeded, result.Operation.Status);
        Assert.Equal(accepted ? Guid.Parse(FabricOperationTestData.OperationId) : null, result.Operation.OperationId);
        Assert.Equal(accepted && sync ? "Succeeded" : null, result.Operation.LastState?.Status);
        Assert.Equal(accepted && !sync ? 3d : null, result.Operation.RecommendedPollAfterSeconds);
        Assert.Equal(sync && accepted ? 2 : 1, handler.CallCount);
        Assert.Equal(1, methods.Count(static method => method == HttpMethod.Post));
        Assert.Empty(clock.Timers);
        await credential.Received(handler.CallCount).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)), Arg.Any<CancellationToken>());
        using var output = JsonDocument.Parse(JsonSerializer.Serialize(result, CoreJsonContext.Default.ItemDefinitionUpdateCommandResult));
        Assert.Equal(["operation"], output.RootElement.EnumerateObject().Select(static property => property.Name));
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(null, true)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task PlatformAndOpaqueOptions_ArePreservedWithoutMetadataInference(bool? updateMetadata, bool platform)
    {
        using var options = JsonDocument.Parse("""{"future":{"enabled":false,"empty":"","missing":null,"values":[9007199254740993,2.50]},"sync":true}""");
        var request = Request() with
        {
            Definition = new(platform
                ? [new("content", "e30=", "InlineBase64"), new(".platform", Convert.ToBase64String(Encoding.UTF8.GetBytes("""{"metadata":{"type":"Notebook","displayName":"Changed"}}""")), "InlineBase64")]
                : [new("content", "e30=", "InlineBase64")], "FutureFormat"),
            Options = options.RootElement.Clone()
        };
        using var handler = new FabricCoreHttpMessageHandler(async (message, token) =>
        {
            using var body = JsonDocument.Parse(await message.Content!.ReadAsStringAsync(token));
            Assert.Equal(["definition", "options"], body.RootElement.EnumerateObject().Select(static property => property.Name).Order());
            using var expected = JsonDocument.Parse(JsonSerializer.Serialize(request, CoreJsonContext.Default.UpdateItemDefinitionRequest));
            Assert.True(JsonElement.DeepEquals(expected.RootElement, body.RootElement));
            return new(HttpStatusCode.OK);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var result = await service.UpdateItemDefinitionAsync(FabricOperationTestData.WorkspaceId, FabricOperationTestData.ItemId,
            request, updateMetadata, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(FabricOperationStatus.Succeeded, result.Operation.Status);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("00000000-0000-0000-0000-000000000000", true)]
    [InlineData("../private-path?x=1", true)]
    [InlineData("", false)]
    [InlineData("00000000-0000-0000-0000-000000000000", false)]
    [InlineData("https://private-host/item", false)]
    public async Task InvalidIds_FailBeforeAuthentication(string id, bool workspace)
    {
        using var handler = ItemDefinitionUpdateTestData.NoHttp();
        using var client = new HttpClient(handler);
        var credential = FabricOperationTestData.Credential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateItemDefinitionAsync(
            workspace ? id : FabricOperationTestData.WorkspaceId, workspace ? FabricOperationTestData.ItemId : id,
            Request(), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData(false, double.NaN)]
    [InlineData(true, double.PositiveInfinity)]
    [InlineData(false, 0)]
    [InlineData(true, -1)]
    [InlineData(false, 600.001)]
    [InlineData(true, 600.001)]
    public async Task InvalidWaitBudget_FailsBeforeAuthentication(bool sync, double seconds)
    {
        using var handler = ItemDefinitionUpdateTestData.NoHttp();
        using var client = new HttpClient(handler);
        var credential = FabricOperationTestData.Credential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAsync<ArgumentException>(() => UpdateAsync(service, new(sync, seconds), TestContext.Current.CancellationToken));

        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("""{"private-duplicate":1,"private-duplicate":2}""")]
    [InlineData("""{"data":{"private-duplicate":1,"private-duplicate":2}}""")]
    public async Task DirectService_ValidatesTypedOptionsBeforeAuthentication(string json)
    {
        using var handler = ItemDefinitionUpdateTestData.NoHttp();
        using var client = new HttpClient(handler);
        var credential = FabricOperationTestData.Credential();
        var service = new FabricCoreService(client, credential);
        using var options = JsonDocument.Parse(json);

        var error = await Assert.ThrowsAsync<FabricItemInputException>(() => service.UpdateItemDefinitionAsync(
            FabricOperationTestData.WorkspaceId, FabricOperationTestData.ItemId, Request() with { Options = options.RootElement.Clone() },
            cancellationToken: TestContext.Current.CancellationToken));

        Assert.DoesNotContain("private-", error.Message);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task DirectService_ValidatesDefinitionWithSharedLimitsBeforeAuthentication()
    {
        using var handler = ItemDefinitionUpdateTestData.NoHttp();
        using var client = new HttpClient(handler);
        var credential = FabricOperationTestData.Credential();
        var service = new FabricCoreService(client, credential, definitionLimits: new(MaxParts: 1, MaxPartBytes: 1, MaxTotalBytes: 1));

        await Assert.ThrowsAsync<InvalidDataException>(() => UpdateAsync(service, new(), TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateItemDefinitionAsync(
            FabricOperationTestData.WorkspaceId, FabricOperationTestData.ItemId, new(new([])),
            cancellationToken: TestContext.Current.CancellationToken));

        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.Created, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.NoContent, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.Redirect, HttpStatusCode.Redirect)]
    [InlineData(HttpStatusCode.BadRequest, HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Forbidden, HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound, HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests, HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable, HttpStatusCode.ServiceUnavailable)]
    public async Task UnexpectedPostStatus_NeverRetriesReadsBackendContentOrFollowsLocation(HttpStatusCode status, HttpStatusCode expected)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(status) { Content = new UnreadableHttpContent() };
            response.Headers.Location = new("https://private-host/operation");
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var error = await Assert.ThrowsAsync<FabricOperationException>(() => UpdateAsync(service, new(Sync: true), TestContext.Current.CancellationToken));

        Assert.Equal(expected, error.StatusCode);
        Assert.DoesNotContain("private-", error.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("""{"id":"private-backend-data"}""")]
    [InlineData("{")]
    public async Task UnexpectedSynchronousBody_IsReportedWithoutReturningItOrReadingAResult(string body)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(FabricOperationTestData.JsonResponse(body)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var result = await UpdateAsync(service, new(Sync: true), TestContext.Current.CancellationToken);

        Assert.Equal(FabricOperationStatus.ResultUnavailable, result.Operation.Status);
        Assert.Equal((int)HttpStatusCode.BadGateway, result.Operation.Issue?.HttpStatus);
        Assert.DoesNotContain("private-", JsonSerializer.Serialize(result, CoreJsonContext.Default.ItemDefinitionUpdateCommandResult));
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task AcceptedResponse_DisposesContentWithoutReadingIt()
    {
        using var content = new UnreadableHttpContent();
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
        {
            var response = FabricOperationTestData.Accepted();
            response.Content = content;
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var result = await UpdateAsync(service, new(), TestContext.Current.CancellationToken);

        Assert.Equal(FabricOperationStatus.Accepted, result.Operation.Status);
        Assert.True(content.IsDisposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task ThrottledInitiatingPost_DoesNotEnterTheReadRetryLoop()
    {
        using var content = new UnreadableHttpContent();
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Content = content };
            response.Headers.RetryAfter = new(TimeSpan.FromSeconds(9));
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var error = await Assert.ThrowsAsync<FabricThrottledException>(() =>
            UpdateAsync(service, new(Sync: true), TestContext.Current.CancellationToken));

        Assert.Equal(TimeSpan.FromSeconds(9), error.RetryAfter.Delta);
        Assert.True(content.IsDisposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task AcceptedWithoutOperationId_DoesNotRecoverFromLocationOrResubmit()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
        {
            var response = FabricOperationTestData.Accepted(operationId: null);
            response.Headers.Location = new($"{FabricEndpoints.FabricApiBaseUrl}/operations/{FabricOperationTestData.OperationId}");
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var result = await UpdateAsync(service, new(Sync: true), TestContext.Current.CancellationToken);

        Assert.Equal(FabricOperationStatus.AcceptedWithoutOperationId, result.Operation.Status);
        Assert.Null(result.Operation.OperationId);
        Assert.Equal("MissingOperationId", result.Operation.Issue?.Code);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WaitDeadline_PreservesUnknownStateWithoutAnotherPostOrRead(bool early)
    {
        var clock = new OperationTestTimeProvider();
        var requests = new List<HttpMethod>();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            requests.Add(request.Method);
            return Task.FromResult(request.Method == HttpMethod.Post
                ? FabricOperationTestData.Accepted()
                : FabricOperationTestData.JsonResponse("""{"status":"FutureStatus","percentComplete":60}""", retryAfter: "600"));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential(), clock);

        var task = UpdateAsync(service, new(Sync: true, MaxWaitSeconds: 600, EarlyPoll: early), TestContext.Current.CancellationToken);
        var firstDelay = early ? 3 : 20;
        await clock.WaitForTimerAsync(firstDelay, TestContext.Current.CancellationToken);
        clock.Advance(firstDelay);
        await clock.WaitForTimerAsync(600 - firstDelay, TestContext.Current.CancellationToken);
        clock.Advance(600 - firstDelay);
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(FabricOperationStatus.Pending, result.Operation.Status);
        Assert.Equal("FutureStatus", result.Operation.LastState?.Status);
        Assert.Equal(60, result.Operation.LastState?.PercentComplete);
        Assert.Equal("WaitBudgetExpired", result.Operation.Issue?.Code);
        Assert.Equal([HttpMethod.Post, HttpMethod.Get], requests);
        Assert.Empty(clock.Timers);
        clock.Advance(600);
        Assert.Equal(2, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task RetryableStateFailure_OnlyRetriesStateAndHonorsTheHint(HttpStatusCode status)
    {
        var clock = new OperationTestTimeProvider();
        var reads = 0;
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            if (request.Method == HttpMethod.Post)
            {
                return Task.FromResult(FabricOperationTestData.Accepted());
            }
            Assert.Equal($"/v1/operations/{FabricOperationTestData.OperationId}", request.RequestUri?.AbsolutePath);
            reads++;
            return Task.FromResult(reads == 1
                ? FabricOperationTestData.JsonResponse("""{"message":"private-error"}""", status, "9")
                : FabricOperationTestData.JsonResponse("""{"status":"Succeeded"}"""));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential(), clock);

        var task = UpdateAsync(service, new(Sync: true), TestContext.Current.CancellationToken);
        await clock.WaitForTimerAsync(3, TestContext.Current.CancellationToken);
        clock.Advance(3);
        await clock.WaitForTimerAsync(9, TestContext.Current.CancellationToken);
        clock.Advance(9);
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(FabricOperationStatus.Succeeded, result.Operation.Status);
        Assert.Null(result.Operation.Issue);
        Assert.Equal(3, handler.CallCount);
        Assert.Equal(2, reads);
    }

    [Theory]
    [InlineData("failed", FabricOperationStatus.Failed)]
    [InlineData("forbidden", FabricOperationStatus.TrackingStopped)]
    [InlineData("timeout", FabricOperationStatus.TrackingStopped)]
    public async Task StateFailures_PreserveReceiptAndNeverReadAResult(string failure, FabricOperationStatus expected)
    {
        var clock = new OperationTestTimeProvider();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            if (request.Method == HttpMethod.Post)
            {
                return Task.FromResult(FabricOperationTestData.Accepted());
            }
            Assert.Equal($"/v1/operations/{FabricOperationTestData.OperationId}", request.RequestUri?.AbsolutePath);
            return failure switch
            {
                "failed" => Task.FromResult(FabricOperationTestData.JsonResponse("""{"status":"Failed","error":{"errorCode":"InvalidDefinition","message":"private-content"}}""")),
                "forbidden" => Task.FromResult(FabricOperationTestData.JsonResponse("""{"message":"private-content"}""", HttpStatusCode.Forbidden)),
                _ => throw new TimeoutException("private-content")
            };
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential(), clock);

        var task = UpdateAsync(service, new(Sync: true), TestContext.Current.CancellationToken);
        await clock.WaitForTimerAsync(3, TestContext.Current.CancellationToken);
        clock.Advance(3);
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(expected, result.Operation.Status);
        Assert.Equal(Guid.Parse(FabricOperationTestData.OperationId), result.Operation.OperationId);
        Assert.DoesNotContain("private-content", JsonSerializer.Serialize(result, CoreJsonContext.Default.ItemDefinitionUpdateCommandResult));
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task CallerCancellation_StopsOnlyLocalWaitingAndRetainsAcceptance()
    {
        using var caller = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var clock = new OperationTestTimeProvider();
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(FabricOperationTestData.Accepted()));
        using var client = new HttpClient(handler);
        var credential = FabricOperationTestData.Credential();
        var service = new FabricCoreService(client, credential, clock);

        var task = UpdateAsync(service, new(Sync: true), caller.Token);
        await clock.WaitForTimerAsync(3, TestContext.Current.CancellationToken);
        await caller.CancelAsync();
        var error = await Assert.ThrowsAsync<FabricOperationCanceledException>(() => task);

        Assert.Equal(caller.Token, error.CancellationToken);
        Assert.Equal(Guid.Parse(FabricOperationTestData.OperationId), error.Operation.OperationId);
        Assert.Equal(FabricOperationStatus.TrackingStopped, error.Operation.Status);
        Assert.Equal("CallerCanceled", error.Operation.Issue?.Code);
        Assert.Equal(1, handler.CallCount);
        credential.ClearReceivedCalls();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => UpdateAsync(service, new(), caller.Token));
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task WaitBudget_CancelsAnInFlightStateBodyAndDisposesIt()
    {
        var clock = new OperationTestTimeProvider();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes("""{"status":"Running"}"""), async (_, token) =>
        {
            started.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
        });
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            if (request.Method == HttpMethod.Post)
            {
                return Task.FromResult(FabricOperationTestData.Accepted());
            }
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
            response.Content.Headers.ContentType = new("application/json");
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential(), clock);

        var task = UpdateAsync(service, new(Sync: true, MaxWaitSeconds: 4), TestContext.Current.CancellationToken);
        await clock.WaitForTimerAsync(3, TestContext.Current.CancellationToken);
        clock.Advance(3);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        clock.Advance(1);
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(FabricOperationStatus.Pending, result.Operation.Status);
        Assert.Equal("WaitBudgetExpired", result.Operation.Issue?.Code);
        Assert.True(stream.Disposed);
        Assert.Equal(2, handler.CallCount);
        Assert.Empty(clock.Timers);
    }

    [Fact]
    public async Task ConcurrentRequests_KeepCredentialsBodiesAndTargetsIndependent()
    {
        var identity = new AsyncLocal<string>();
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new AccessToken(identity.Value!, DateTimeOffset.MaxValue));
        var seen = new ConcurrentBag<string>();
        var bothStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var arrivals = 0;
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            var caller = request.Headers.Authorization!.Parameter!;
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            Assert.Equal(caller, body.RootElement.GetProperty("options").GetProperty("caller").GetString());
            Assert.Equal(caller, Encoding.UTF8.GetString(Convert.FromBase64String(
                Assert.Single(body.RootElement.GetProperty("definition").GetProperty("parts").EnumerateArray()).GetProperty("payload").GetString()!)));
            Assert.Equal($"/v1/workspaces/{FabricOperationTestData.WorkspaceId}/items/{caller}/updateDefinition", request.RequestUri!.AbsolutePath);
            seen.Add(caller);
            if (Interlocked.Increment(ref arrivals) == 2)
            {
                bothStarted.TrySetResult();
            }
            await bothStarted.Task.WaitAsync(TimeSpan.FromSeconds(10), token);
            return new(HttpStatusCode.OK);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        var results = await Task.WhenAll(new[] { FabricOperationTestData.ItemId, FabricOperationTestData.WorkspaceId }.Select(async id =>
        {
            identity.Value = id;
            using var options = JsonDocument.Parse($$"""{"caller":"{{id}}"}""");
            var definition = new FabricItemDefinition([new("content", Convert.ToBase64String(Encoding.UTF8.GetBytes(id)), "InlineBase64")]);
            return await service.UpdateItemDefinitionAsync(FabricOperationTestData.WorkspaceId, id,
                new(definition, options.RootElement.Clone()), cancellationToken: TestContext.Current.CancellationToken);
        }));

        Assert.All(results, static result => Assert.Equal(FabricOperationStatus.Succeeded, result.Operation.Status));
        Assert.Equal(2, seen.Distinct().Count());
        Assert.Equal(2, handler.CallCount);
        await credential.Received(2).GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
        Assert.Null(client.DefaultRequestHeaders.Authorization);
    }

    private static UpdateItemDefinitionRequest Request() => new(new([new("content", "e30=", "InlineBase64")]));

    private static Task<ItemDefinitionUpdateCommandResult> UpdateAsync(
        FabricCoreService service, FabricOperationWaitOptions options, CancellationToken cancellationToken) =>
        service.UpdateItemDefinitionAsync(FabricOperationTestData.WorkspaceId, FabricOperationTestData.ItemId,
            Request(), waitOptions: options, cancellationToken: cancellationToken);
}
