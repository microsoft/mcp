// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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

public sealed class ItemDefinitionGetServiceTests()
{
    private CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Initial_UsesOnePostAndOnlyExplicitlyRequestedOperationReads(bool sync, bool accepted)
    {
        var clock = new OperationTestTimeProvider();
        List<HttpMethod> methods = [];
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            methods.Add(request.Method);
            Assert.Equal("operation-test-token", request.Headers.Authorization?.Parameter);
            Assert.Null(request.Content);
            HttpResponseMessage response;
            if (request.Method == HttpMethod.Post)
            {
                Assert.Equal($"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{FabricOperationTestData.WorkspaceId}/items/{FabricOperationTestData.ItemId}/getDefinition",
                    request.RequestUri?.AbsoluteUri);
                response = accepted ? FabricOperationTestData.Accepted() : ItemDefinitionTestData.Response();
            }
            else if (request.RequestUri!.AbsolutePath.EndsWith("/result", StringComparison.Ordinal))
            {
                Assert.Equal($"/v1/operations/{FabricOperationTestData.OperationId}/result", request.RequestUri.AbsolutePath);
                response = ItemDefinitionTestData.Response();
            }
            else
            {
                Assert.Equal($"/v1/operations/{FabricOperationTestData.OperationId}", request.RequestUri.AbsolutePath);
                response = FabricOperationTestData.JsonResponse("""{"status":"Succeeded"}""");
            }
            response.Headers.Location = new Uri("https://example.invalid/do-not-follow");
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var credential = FabricOperationTestData.Credential();
        var service = new FabricCoreService(client, credential, clock);

        var task = service.GetItemDefinitionAsync($"{{{FabricOperationTestData.WorkspaceId.ToUpperInvariant()}}}",
            Guid.Parse(FabricOperationTestData.ItemId).ToString("N"), waitOptions: new(Sync: sync), cancellationToken: CancellationToken);
        if (sync && accepted)
        {
            await clock.WaitForTimerAsync(3, CancellationToken);
            clock.Advance(3);
        }
        var result = await task.WaitAsync(CancellationToken);

        Assert.Equal(1, methods.Count(method => method == HttpMethod.Post));
        Assert.Equal(sync && accepted ? 3 : 1, handler.CallCount);
        await credential.Received(handler.CallCount).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)), Arg.Any<CancellationToken>());
        if (accepted && !sync)
        {
            Assert.Null(result.Definition);
            Assert.Equal(FabricOperationStatus.Accepted, result.Operation?.Status);
            Assert.Equal(Guid.Parse(FabricOperationTestData.OperationId), result.Operation?.OperationId);
        }
        else
        {
            Assert.Equal(ItemDefinitionTestData.Definition.Parts, result.Definition?.Parts);
            Assert.Equal("FutureFormat", result.Definition?.Format);
            Assert.Equal(accepted ? FabricOperationStatus.Succeeded : null, result.Operation?.Status);
        }
        Assert.Null(result.OutputDirectory);
        Assert.Empty(clock.Timers);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("ipynb")]
    [InlineData("future &mode=x/#%+ value")]
    public async Task Format_IsOmittedOrEncodedAsOneQueryValue_AndNeverInventedInOutput(string? format)
    {
        var definition = ItemDefinitionTestData.Definition with { Format = null };
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(format is null ? "" : $"?format={Uri.EscapeDataString(format)}", request.RequestUri?.Query);
            Assert.Equal("", request.RequestUri?.Fragment);
            return Task.FromResult(ItemDefinitionTestData.Response(definition));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var result = await InitialAsync(service, format: format);

        Assert.NotNull(result.Definition);
        Assert.Null(result.Definition.Format);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("immediate")]
    [InlineData("poll")]
    [InlineData("resume")]
    public async Task DefinitionReads_AllowMoreThanOneMiB_WithoutChangingDecodedParts(string mode)
    {
        var definition = new FabricItemDefinition([ItemDefinitionTestData.Part("large.bin", new byte[1024 * 1024])]);
        var json = ItemDefinitionTestData.Json(definition);
        Assert.True(Encoding.UTF8.GetByteCount(json) > FabricOperationHttp.MaxJsonBytes);
        var clock = new OperationTestTimeProvider();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            if (mode == "resume")
            {
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.Equal($"{FabricEndpoints.FabricApiBaseUrl}/operations/{FabricOperationTestData.OperationId}/result", request.RequestUri?.AbsoluteUri);
            }
            if (mode == "poll" && request.Method == HttpMethod.Post)
            {
                return Task.FromResult(FabricOperationTestData.Accepted());
            }
            return Task.FromResult(mode == "poll" && !request.RequestUri!.AbsolutePath.EndsWith("/result", StringComparison.Ordinal)
                ? FabricOperationTestData.JsonResponse("""{"status":"Succeeded"}""")
                : FabricOperationTestData.JsonResponse(json));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential(), clock);

        var task = mode == "resume"
            ? service.GetItemDefinitionAsync(operationId: FabricOperationTestData.OperationId, cancellationToken: CancellationToken)
            : InitialAsync(service, new(Sync: mode == "poll"));
        if (mode == "poll")
        {
            await clock.WaitForTimerAsync(3, CancellationToken);
            clock.Advance(3);
        }
        var result = await task.WaitAsync(CancellationToken);

        Assert.Equal(definition.Parts, result.Definition?.Parts);
        Assert.Null(result.Definition?.Format);
        Assert.Equal(mode == "poll" ? 3 : 1, handler.CallCount);
        Assert.Equal(mode == "immediate" ? null : Guid.Parse(FabricOperationTestData.OperationId), result.Operation?.OperationId);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public async Task DefinitionReads_EnforceExact64MiBWireLimit_WithOrWithoutContentLength(bool contentLength, int excess)
    {
        using var stream = new PaddedJsonTestStream(Encoding.UTF8.GetBytes(ItemDefinitionTestData.Json()),
            FabricCoreService.MaxDefinitionJsonBytes + excess);
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        if (contentLength)
        {
            response.Content.Headers.ContentLength = stream.Length;
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var result = await InitialAsync(service);

        if (excess == 0)
        {
            Assert.Equal(ItemDefinitionTestData.Definition.Parts, result.Definition?.Parts);
            Assert.Equal(FabricCoreService.MaxDefinitionJsonBytes, stream.BytesRead);
        }
        else
        {
            Assert.Null(result.Definition);
            Assert.Equal("ResultTooLarge", result.Operation?.Issue?.Code);
            Assert.Equal((int)HttpStatusCode.BadGateway, result.Operation?.Issue?.HttpStatus);
            Assert.Equal(contentLength ? 0 : FabricCoreService.MaxDefinitionJsonBytes + 1, stream.BytesRead);
        }
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExistingOperations_RetainOneMiBDefault(bool create)
    {
        using var stream = new PaddedJsonTestStream(Encoding.UTF8.GetBytes("{}"), FabricOperationHttp.MaxJsonBytes + 1);
        using var response = new HttpResponseMessage(create ? HttpStatusCode.Created : HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        if (create)
        {
            var result = await service.CreateItemAsync(FabricOperationTestData.WorkspaceId,
                new() { DisplayName = "Created", Type = "Notebook" }, cancellationToken: CancellationToken);
            Assert.Null(result.Item);
            Assert.Equal("ResultTooLarge", result.Operation?.Issue?.Code);
            Assert.Contains("1 MiB", result.Operation?.Issue?.Message);
        }
        else
        {
            var error = await Assert.ThrowsAsync<FabricOperationException>(() =>
                service.GetOperationResultAsync(FabricOperationTestData.OperationId, CancellationToken));
            Assert.Equal("ResultTooLarge", error.Code);
            Assert.Contains("1 MiB", error.Message);
        }
        Assert.Equal(1024 * 1024 + 1, stream.BytesRead);
        Assert.True(stream.Disposed);
    }

    [Theory]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{private")]
    [InlineData("""{"definition":null}""")]
    [InlineData("""{"definition":{}}""")]
    [InlineData("""{"definition":{"parts":null}}""")]
    [InlineData("""{"definition":{"parts":[]}}""")]
    [InlineData("""{"definition":{"parts":[null]}}""")]
    [InlineData("""{"definition":{"parts":[{}]}}""")]
    [InlineData("""{"definition":{"parts":[{"path":"data","payload":"private-invalid","payloadType":"InlineBase64"}]}}""")]
    [InlineData("""{"definition":{"parts":[{"path":"data","payload":"","payloadType":"FutureType"}]}}""")]
    [InlineData("""{"definition":{"parts":[{"path":"../private","payload":"","payloadType":"InlineBase64"}]}}""")]
    [InlineData("""{"definition":{"parts":[{"path":"/private","payload":"","payloadType":"InlineBase64"}]}}""")]
    [InlineData("""{"definition":{"parts":[{"path":"C:/private","payload":"","payloadType":"InlineBase64"}]}}""")]
    [InlineData("""{"definition":{"parts":[{"path":"data","payload":null,"payloadType":"InlineBase64"}]}}""")]
    [InlineData("""{"definition":{"parts":[{"path":"data","payload":"","payloadType":"InlineBase64"}],"format":" "}}""")]
    [InlineData("""{"definition":{"parts":[{"path":"data","payload":"","payloadType":"InlineBase64"},{"path":"DATA","payload":"","payloadType":"InlineBase64"}]}}""")]
    [InlineData("""{"definition":{"parts":[{"path":"data","payload":"","payloadType":"InlineBase64"},{"path":"data/child","payload":"","payloadType":"InlineBase64"}]}}""")]
    [InlineData("""{"definition":{"parts":[{"path":"data","path":"other","payload":"","payloadType":"InlineBase64"}]}}""")]
    [InlineData("""{"definition":{"parts":[]},"definition":{"parts":[{"path":"data","payload":"","payloadType":"InlineBase64"}]}}""")]
    public async Task InvalidCompletedDefinitions_AreNotReturnedOrTreatedAsSuccess(string json)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(FabricOperationTestData.JsonResponse(json)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var result = await InitialAsync(service);

        Assert.Null(result.Definition);
        Assert.Null(result.OutputDirectory);
        Assert.Equal(FabricOperationStatus.ResultUnavailable, result.Operation?.Status);
        Assert.Equal((int)HttpStatusCode.BadGateway, result.Operation?.Issue?.HttpStatus);
        Assert.DoesNotContain("private", result.Operation?.Issue?.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("exact", true)]
    [InlineData("part", false)]
    [InlineData("total", false)]
    [InlineData("count", false)]
    public async Task DefinitionReads_ApplySharedDecodedBudgets(string scenario, bool succeeds)
    {
        var parts = scenario switch
        {
            "part" => new[] { ItemDefinitionTestData.Part("a", new byte[4]) },
            "total" => [ItemDefinitionTestData.Part("a", new byte[3]), ItemDefinitionTestData.Part("b", new byte[3])],
            "count" => [ItemDefinitionTestData.Part("a", []), ItemDefinitionTestData.Part("b", []), ItemDefinitionTestData.Part("c", [])],
            _ => [ItemDefinitionTestData.Part("a", new byte[3]), ItemDefinitionTestData.Part("b", new byte[2])]
        };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(ItemDefinitionTestData.Response(new(parts))));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential(), definitionLimits: new(2, 3, 5));

        var result = await InitialAsync(service);

        Assert.Equal(succeeds, result.Definition is not null);
        Assert.Equal(succeeds ? null : "InvalidItemDefinition", result.Operation?.Issue?.Code);
    }

    [Theory]
    [InlineData(null, null, null, null, false, 120)]
    [InlineData("", FabricOperationTestData.ItemId, null, null, false, 120)]
    [InlineData(FabricOperationTestData.WorkspaceId, "00000000-0000-0000-0000-000000000000", null, null, false, 120)]
    [InlineData("private-name", FabricOperationTestData.ItemId, null, null, false, 120)]
    [InlineData(FabricOperationTestData.WorkspaceId, FabricOperationTestData.ItemId, "", null, false, 120)]
    [InlineData(FabricOperationTestData.WorkspaceId, FabricOperationTestData.ItemId, "\n", null, false, 120)]
    [InlineData(FabricOperationTestData.WorkspaceId, FabricOperationTestData.ItemId, null, null, false, 0)]
    [InlineData(FabricOperationTestData.WorkspaceId, FabricOperationTestData.ItemId, null, null, true, 600.1)]
    [InlineData(FabricOperationTestData.WorkspaceId, FabricOperationTestData.ItemId, null, null, false, double.NaN)]
    [InlineData(FabricOperationTestData.WorkspaceId, FabricOperationTestData.ItemId, null, null, true, double.PositiveInfinity)]
    [InlineData(null, null, null, "private-operation", false, 120)]
    [InlineData(null, null, null, "", false, 120)]
    [InlineData(null, null, null, "00000000-0000-0000-0000-000000000000", false, 120)]
    [InlineData(FabricOperationTestData.WorkspaceId, null, null, FabricOperationTestData.OperationId, false, 120)]
    [InlineData(null, FabricOperationTestData.ItemId, null, FabricOperationTestData.OperationId, false, 120)]
    [InlineData(null, null, "ipynb", FabricOperationTestData.OperationId, false, 120)]
    [InlineData(null, null, null, FabricOperationTestData.OperationId, true, 120)]
    public async Task InvalidInput_FailsBeforeAuthenticationOrHttp(
        string? workspace, string? item, string? format, string? operation, bool sync, double wait)
    {
        using var handler = NoHttp();
        using var client = new HttpClient(handler);
        var credential = FabricOperationTestData.Credential();
        var service = new FabricCoreService(client, credential);

        var error = await Assert.ThrowsAsync<ArgumentException>(() => service.GetItemDefinitionAsync(
            workspace, item, format, operation, new(sync, wait), CancellationToken));

        Assert.DoesNotContain("private", error.Message);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(128, true)]
    [InlineData(129, false)]
    public async Task Format_HasExactCharacterBound(int length, bool valid)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(ItemDefinitionTestData.Response()));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());
        if (valid)
        {
            Assert.NotNull((await InitialAsync(service, format: new string('x', length))).Definition);
        }
        else
        {
            await Assert.ThrowsAsync<ArgumentException>(() => InitialAsync(service, format: new string('x', length)));
        }
        Assert.Equal(valid ? 1 : 0, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.PartialContent)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TemporaryRedirect)]
    public async Task Initial_RejectsUndocumentedStatusesWithoutReadingErrorBodyOrFollowingLocation(HttpStatusCode status)
    {
        using var content = new UnreadableHttpContent();
        using var response = new HttpResponseMessage(status) { Content = content };
        response.Headers.Location = new Uri("https://example.invalid/private");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var error = await Assert.ThrowsAsync<FabricOperationException>(() => InitialAsync(service));

        Assert.Equal(response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : status, error.StatusCode);
        Assert.Equal(1, handler.CallCount);
        Assert.True(content.IsDisposed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AcceptedWithoutId_NeverReadsReceiptBodyOrInventsCompletion(bool sync)
    {
        using var content = new UnreadableHttpContent();
        using var response = FabricOperationTestData.Accepted(operationId: null);
        response.Content = content;
        response.Headers.Location = new Uri("https://example.invalid/not-an-operation-reference");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var result = await InitialAsync(service, new(Sync: sync));

        Assert.Null(result.Definition);
        Assert.Null(result.Operation?.OperationId);
        Assert.Equal(FabricOperationStatus.AcceptedWithoutOperationId, result.Operation?.Status);
        Assert.Contains("definition retrieval", result.Operation?.Issue?.Message);
        Assert.DoesNotContain("creation", result.Operation?.Issue?.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.True(content.IsDisposed);
    }

    [Theory]
    [InlineData("pending")]
    [InlineData("failed")]
    [InlineData("result-forbidden")]
    [InlineData("invalid-result")]
    public async Task Waiting_PreservesRealOperationStateAndSafeFailures(string scenario)
    {
        var clock = new OperationTestTimeProvider();
        using var handler = new FabricCoreHttpMessageHandler((request, _) => Task.FromResult(
            request.Method == HttpMethod.Post ? FabricOperationTestData.Accepted() :
            request.RequestUri!.AbsolutePath.EndsWith("/result", StringComparison.Ordinal)
                ? FabricOperationTestData.JsonResponse("private-invalid", scenario == "result-forbidden" ? HttpStatusCode.Forbidden : HttpStatusCode.OK)
                : FabricOperationTestData.JsonResponse(scenario switch
                {
                    "pending" => """{"status":"Running","percentComplete":5}""",
                    "failed" => """{"status":"Failed","error":{"errorCode":"Denied","message":"private-backend"}}""",
                    _ => """{"status":"Succeeded"}"""
                })));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential(), clock);

        var task = InitialAsync(service, new(Sync: true, MaxWaitSeconds: 4));
        await clock.WaitForTimerAsync(3, CancellationToken);
        clock.Advance(3);
        if (scenario == "pending")
        {
            await clock.WaitForTimerAsync(1, CancellationToken);
            clock.Advance(1);
        }
        var result = await task.WaitAsync(CancellationToken);

        Assert.Null(result.Definition);
        Assert.Equal(Guid.Parse(FabricOperationTestData.OperationId), result.Operation?.OperationId);
        Assert.Equal(scenario switch
        {
            "pending" => FabricOperationStatus.Pending,
            "failed" => FabricOperationStatus.Failed,
            _ => FabricOperationStatus.ResultUnavailable
        }, result.Operation?.Status);
        Assert.Contains("No definition was returned or saved", result.Operation?.Issue?.Message);
        Assert.Contains("--operation-id", result.Operation?.Issue?.Message);
        Assert.DoesNotContain("private", result.Operation?.Issue?.Message);
        Assert.Equal(scenario is "pending" or "failed" ? 2 : 3, handler.CallCount);
        Assert.Empty(clock.Timers);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Cancellation_StopsWaitingAndDisposesInFlightResultWithOperationId(bool readingResult)
    {
        var clock = new OperationTestTimeProvider();
        using var caller = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
        var blocked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(ItemDefinitionTestData.Json()), async (_, token) =>
        {
            blocked.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
        });
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            if (request.Method == HttpMethod.Post)
            {
                return Task.FromResult(FabricOperationTestData.Accepted());
            }
            if (!request.RequestUri!.AbsolutePath.EndsWith("/result", StringComparison.Ordinal))
            {
                return Task.FromResult(FabricOperationTestData.JsonResponse("""{"status":"Succeeded"}"""));
            }
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
            response.Content.Headers.ContentType = new("application/json");
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential(), clock);

        var task = service.GetItemDefinitionAsync(FabricOperationTestData.WorkspaceId, FabricOperationTestData.ItemId,
            waitOptions: new(Sync: true), cancellationToken: caller.Token);
        await clock.WaitForTimerAsync(3, CancellationToken);
        if (readingResult)
        {
            clock.Advance(3);
            await blocked.Task.WaitAsync(CancellationToken);
        }
        await caller.CancelAsync();
        var error = await Assert.ThrowsAsync<FabricOperationCanceledException>(() => task);

        Assert.Equal(Guid.Parse(FabricOperationTestData.OperationId), error.Operation.OperationId);
        Assert.Equal(caller.Token, error.CancellationToken);
        Assert.Contains("--operation-id", error.Operation.Issue?.Message);
        Assert.Equal(readingResult ? 3 : 1, handler.CallCount);
        Assert.Equal(readingResult, stream.Disposed);
        Assert.Empty(clock.Timers);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WaitBudget_CancelsResultHeadersOrBodyAndPreservesConfirmedCompletion(bool blockBody)
    {
        var clock = new OperationTestTimeProvider();
        var blocked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(ItemDefinitionTestData.Json()), async (_, token) =>
        {
            blocked.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
        });
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            if (request.Method == HttpMethod.Post)
            {
                return FabricOperationTestData.Accepted();
            }
            if (!request.RequestUri!.AbsolutePath.EndsWith("/result", StringComparison.Ordinal))
            {
                return FabricOperationTestData.JsonResponse("""{"status":"Succeeded"}""");
            }
            if (blockBody)
            {
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

        var task = InitialAsync(service, new(Sync: true));
        await clock.WaitForTimerAsync(3, CancellationToken);
        clock.Advance(3);
        await blocked.Task.WaitAsync(CancellationToken);
        clock.Advance(117);
        var result = await task.WaitAsync(CancellationToken);

        Assert.Null(result.Definition);
        Assert.Equal(FabricOperationStatus.ResultUnavailable, result.Operation?.Status);
        Assert.Equal("Succeeded", result.Operation?.LastState?.Status);
        Assert.Equal("WaitBudgetExpired", result.Operation?.Issue?.Code);
        Assert.Contains("operation succeeded", result.Operation?.Issue?.Message);
        Assert.Equal(Guid.Parse(FabricOperationTestData.OperationId), result.Operation?.OperationId);
        Assert.Equal(blockBody, stream.Disposed);
        Assert.Equal(3, handler.CallCount);
        Assert.Empty(clock.Timers);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PreCancellation_SkipsAuthenticationAndHttp(bool resume)
    {
        using var caller = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
        await caller.CancelAsync();
        using var handler = NoHttp();
        using var client = new HttpClient(handler);
        var credential = FabricOperationTestData.Credential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetItemDefinitionAsync(
            resume ? null : FabricOperationTestData.WorkspaceId, resume ? null : FabricOperationTestData.ItemId,
            operationId: resume ? FabricOperationTestData.OperationId : null, cancellationToken: caller.Token));

        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task Resume_IsOneResultRead_EvenWhenResultIsNotAvailable(HttpStatusCode status)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.EndsWith($"/operations/{FabricOperationTestData.OperationId}/result", request.RequestUri?.AbsoluteUri);
            return Task.FromResult(FabricOperationTestData.JsonResponse("private", status));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        await Assert.ThrowsAsync<FabricOperationException>(() => service.GetItemDefinitionAsync(
            operationId: FabricOperationTestData.OperationId, cancellationToken: CancellationToken));

        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task Resume_RejectsAResponseThatChangesTheOperationId()
    {
        using var response = ItemDefinitionTestData.Response();
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", FabricOperationTestData.ItemId);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var error = await Assert.ThrowsAsync<FabricOperationException>(() =>
            service.GetItemDefinitionAsync(operationId: FabricOperationTestData.OperationId, cancellationToken: CancellationToken));

        Assert.Equal("InvalidOperationId", error.Code);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task Waiting_RetainsTheOneMiBStateLimitAndReportsTheCorrectBoundary()
    {
        var clock = new OperationTestTimeProvider();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            var response = FabricOperationTestData.Accepted();
            if (request.Method != HttpMethod.Post)
            {
                response.StatusCode = HttpStatusCode.OK;
                response.Content = new UnreadableHttpContent();
                response.Content.Headers.ContentType = new("application/json");
                response.Content.Headers.ContentLength = FabricOperationHttp.MaxJsonBytes + 1;
            }
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential(), clock);

        var task = InitialAsync(service, new(Sync: true));
        await clock.WaitForTimerAsync(3, CancellationToken);
        clock.Advance(3);
        var result = await task.WaitAsync(CancellationToken);

        Assert.Null(result.Definition);
        Assert.Equal(FabricOperationStatus.TrackingStopped, result.Operation?.Status);
        Assert.Equal("ResultTooLarge", result.Operation?.Issue?.Code);
        Assert.Contains("1 MiB", result.Operation?.Issue?.Message);
        Assert.DoesNotContain("64 MiB", result.Operation?.Issue?.Message);
        Assert.Equal(2, handler.CallCount);
        Assert.Empty(clock.Timers);
    }

    private Task<ItemDefinitionGetCommandResult> InitialAsync(
        FabricCoreService service, FabricOperationWaitOptions? waitOptions = null, string? format = null) =>
        service.GetItemDefinitionAsync(FabricOperationTestData.WorkspaceId, FabricOperationTestData.ItemId,
            format, waitOptions: waitOptions, cancellationToken: CancellationToken);

    private static FabricCoreHttpMessageHandler NoHttp() =>
        new((_, _) => throw new InvalidOperationException("No HTTP expected."));
}
