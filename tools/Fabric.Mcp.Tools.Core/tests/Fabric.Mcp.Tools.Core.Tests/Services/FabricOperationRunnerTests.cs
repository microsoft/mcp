// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class FabricOperationRunnerTests()
{
    [Theory]
    [InlineData(HttpStatusCode.OK, false, false)]
    [InlineData(HttpStatusCode.OK, false, true)]
    [InlineData(HttpStatusCode.OK, true, false)]
    [InlineData(HttpStatusCode.OK, true, true)]
    [InlineData(HttpStatusCode.Created, false, false)]
    [InlineData(HttpStatusCode.Created, false, true)]
    [InlineData(HttpStatusCode.Created, true, false)]
    [InlineData(HttpStatusCode.Created, true, true)]
    public async Task HybridDispatcher_CompletesDocumentedSynchronousResultsWithoutPolling(
        HttpStatusCode status, bool sync, bool hasResult)
    {
        using var response = hasResult
            ? FabricOperationTestData.JsonResponse("\"created\"", status)
            : new HttpResponseMessage(status);
        var clock = new OperationTestTimeProvider();
        var contract = new FabricOperationContract<string>(new HashSet<HttpStatusCode> { status },
            hasResult ? static value => value.GetString()! : null);

        var outcome = await new FabricOperationRunner(clock).HandleAsync(
            response, contract, new(Sync: sync), NoStateRead, NoResultRead, TestContext.Current.CancellationToken);

        Assert.Equal(hasResult ? "created" : null, outcome.Result);
        Assert.Null(outcome.Operation);
        Assert.Equal(FabricOperationTestData.Epoch, clock.GetUtcNow());
        Assert.Empty(clock.Timers);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.PartialContent)]
    public async Task HybridDispatcher_RejectsSuccessOutsideAdapterContract(HttpStatusCode status)
    {
        using var response = FabricOperationTestData.JsonResponse("{}", status);

        var error = await Assert.ThrowsAsync<FabricOperationException>(() => new FabricOperationRunner(new OperationTestTimeProvider())
            .HandleAsync(response, NoResultContract(), new(Sync: true), NoStateRead, NoResultRead, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
        Assert.Contains("whether creation occurred", error.Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HybridDispatcher_ValidatesSynchronousPayloadContract(bool hasResult)
    {
        using var response = hasResult
            ? new HttpResponseMessage(HttpStatusCode.Created)
            : FabricOperationTestData.JsonResponse("{}", HttpStatusCode.Created);
        var contract = new FabricOperationContract<string>(new HashSet<HttpStatusCode> { HttpStatusCode.Created },
            hasResult ? static _ => "created" : null);

        var outcome = await new FabricOperationRunner(new OperationTestTimeProvider()).HandleAsync(
            response, contract, new(), NoStateRead, NoResultRead, TestContext.Current.CancellationToken);

        Assert.Null(outcome.Result);
        Assert.Equal(FabricOperationStatus.ResultUnavailable, outcome.Operation?.Status);
        Assert.Equal(hasResult ? "MissingResult" : "UnexpectedResult", outcome.Operation?.Issue?.Code);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Poller_HandlesNotStartedRunningSucceeded_WithAndWithoutResult(bool hasResult)
    {
        using var response = FabricOperationTestData.Accepted();
        var clock = new OperationTestTimeProvider();
        string[] statuses = ["NotStarted", "Running", "Succeeded"];
        var states = 0;
        var results = 0;
        var contract = new FabricOperationContract<string>(new HashSet<HttpStatusCode> { HttpStatusCode.Created },
            hasResult ? static value => value.GetString()! : null);
        var runner = new FabricOperationRunner(clock);

        var task = runner.HandleAsync(response, contract, new(Sync: true),
            (id, _) => Task.FromResult(new OperationStateResult(id, new(statuses[states++]), null, 20)),
            (id, _) =>
            {
                results++;
                return Task.FromResult(new OperationResult(id, true, JsonSerializer.SerializeToElement("created", CoreJsonContext.Default.String)));
            }, TestContext.Current.CancellationToken);
        await clock.WaitForTimerAsync(3, TestContext.Current.CancellationToken);
        clock.Advance(3);
        await clock.WaitForTimerAsync(20, TestContext.Current.CancellationToken);
        clock.Advance(20);
        await clock.WaitForTimerAsync(20, TestContext.Current.CancellationToken);
        clock.Advance(20);
        var outcome = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(hasResult ? "created" : null, outcome.Result);
        Assert.Equal(3, states);
        Assert.Equal(hasResult ? 1 : 0, results);
        Assert.Equal(FabricOperationStatus.Succeeded, outcome.Operation?.Status);
        Assert.Equal("Succeeded", outcome.Operation?.LastState?.Status);
        Assert.Equal(43, (clock.GetUtcNow() - FabricOperationTestData.Epoch).TotalSeconds);
        Assert.Empty(clock.Timers);
    }

    [Theory]
    [InlineData(true, 3)]
    [InlineData(false, 20)]
    public async Task Poller_MakesOnlyOneEarlyProbe_ThenHonorsStateRetryAfter(bool early, double firstDelay)
    {
        using var response = FabricOperationTestData.Accepted();
        var clock = new OperationTestTimeProvider();
        var states = 0;
        var observed = new List<double>();
        var task = new FabricOperationRunner(clock).HandleAsync(response, NoResultContract(), new(Sync: true, EarlyPoll: early),
            (id, _) =>
            {
                observed.Add((clock.GetUtcNow() - FabricOperationTestData.Epoch).TotalSeconds);
                return Task.FromResult(new OperationStateResult(id, new(++states == 1 ? "Running" : "Succeeded"), 20, 20));
            }, NoResultRead, TestContext.Current.CancellationToken);

        await clock.WaitForTimerAsync(firstDelay, TestContext.Current.CancellationToken);
        clock.Advance(firstDelay);
        await clock.WaitForTimerAsync(20, TestContext.Current.CancellationToken);
        Assert.Equal(1, states);
        clock.Advance(20);
        var outcome = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal([firstDelay, firstDelay + 20], observed);
        Assert.Equal(FabricOperationStatus.Succeeded, outcome.Operation?.Status);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests, false)]
    [InlineData(HttpStatusCode.ServiceUnavailable, false)]
    [InlineData(HttpStatusCode.TooManyRequests, true)]
    [InlineData(HttpStatusCode.ServiceUnavailable, true)]
    public async Task Poller_HonorsBackoffOnFirstStateOrResultRead(HttpStatusCode status, bool throttleResult)
    {
        using var response = FabricOperationTestData.Accepted();
        var clock = new OperationTestTimeProvider();
        var attempts = 0;
        var successfulStates = 0;
        var contract = new FabricOperationContract<string>(new HashSet<HttpStatusCode> { HttpStatusCode.Created },
            throttleResult ? static value => value.GetString()! : null);
        var task = new FabricOperationRunner(clock).HandleAsync(response, contract, new(Sync: true),
            (id, _) =>
            {
                if (!throttleResult && ++attempts == 1)
                {
                    throw RetryableFailure(status);
                }
                successfulStates++;
                return Task.FromResult(new OperationStateResult(id, new("Succeeded"), null, 20));
            },
            (id, _) =>
            {
                if (++attempts == 1)
                {
                    throw RetryableFailure(status);
                }
                return Task.FromResult(new OperationResult(id, true, JsonSerializer.SerializeToElement("created", CoreJsonContext.Default.String)));
            }, TestContext.Current.CancellationToken);

        await clock.WaitForTimerAsync(3, TestContext.Current.CancellationToken);
        clock.Advance(3);
        await clock.WaitForTimerAsync(20, TestContext.Current.CancellationToken);
        Assert.Equal(1, attempts);
        clock.Advance(19);
        Assert.Equal(1, attempts);
        clock.Advance(1);
        var outcome = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, attempts);
        Assert.Equal(1, successfulStates);
        Assert.Equal(FabricOperationStatus.Succeeded, outcome.Operation?.Status);
        Assert.Equal(23, (clock.GetUtcNow() - FabricOperationTestData.Epoch).TotalSeconds);
    }

    [Theory]
    [InlineData(null, null, 20)]
    [InlineData("-1", null, 20)]
    [InlineData("private-header-detail", null, 20)]
    [InlineData("1.5", null, 20)]
    [InlineData("20,40", null, 20)]
    [InlineData("2147483648", null, 20)]
    [InlineData("999999999999999999999999999999999", null, 20)]
    [InlineData("2147483647", 2147483647d, 2147483647d)]
    [InlineData("0", 0d, 1d)]
    [InlineData("1", 1d, 1d)]
    [InlineData("Tue, 01 Jan 2030 00:00:20 GMT", 20d, 20d)]
    [InlineData("Mon, 01 Jan 2029 00:00:00 GMT", 0d, 1d)]
    public async Task AcceptedReceipt_SeparatesServerHintFromSafePollingFallback(string? header, double? serverDelay, double delay)
    {
        using var response = FabricOperationTestData.Accepted(retryAfter: header);
        var clock = new OperationTestTimeProvider();

        var outcome = await new FabricOperationRunner(clock).HandleAsync(
            response, NoResultContract(), new(EarlyPoll: false), NoStateRead, NoResultRead, TestContext.Current.CancellationToken);

        Assert.Equal(FabricOperationStatus.Accepted, outcome.Operation?.Status);
        Assert.Null(outcome.Operation?.LastState);
        Assert.Equal(serverDelay, outcome.Operation?.ServerRetryAfterSeconds);
        Assert.Equal(delay, outcome.Operation?.RecommendedPollAfterSeconds);
        Assert.Empty(clock.Timers);
    }

    [Theory]
    [InlineData("1", 1)]
    [InlineData("2", 2)]
    [InlineData("20", 3)]
    public async Task EarlyHint_DoesNotLengthenShortServerDelay(string header, double delay)
    {
        using var response = FabricOperationTestData.Accepted(retryAfter: header);
        var result = await new FabricOperationRunner(new OperationTestTimeProvider()).HandleAsync(
            response, NoResultContract(), new(), NoStateRead, NoResultRead, TestContext.Current.CancellationToken);

        Assert.Equal(delay, result.Operation?.RecommendedPollAfterSeconds);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("invalid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task AcceptedWithoutUsableId_IsNotMisreportedAsCompletedOrRejected(string? id)
    {
        using var response = FabricOperationTestData.Accepted(id);
        response.Headers.Location = new Uri("https://example.invalid/operations/private");
        var result = await new FabricOperationRunner(new OperationTestTimeProvider()).HandleAsync(
            response, NoResultContract(), new(Sync: true), NoStateRead, NoResultRead, TestContext.Current.CancellationToken);

        Assert.Null(result.Result);
        Assert.Null(result.Operation?.OperationId);
        Assert.Null(result.Operation?.LastState);
        Assert.Equal(FabricOperationStatus.AcceptedWithoutOperationId, result.Operation?.Status);
        Assert.Contains("accepted", result.Operation?.Issue?.Message);
        Assert.DoesNotContain("private", result.Operation?.Issue?.Message);
    }

    [Fact]
    public async Task AmbiguousHeaders_DoNotChooseAnOperationOrRetryHint()
    {
        using var response = FabricOperationTestData.Accepted();
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", FabricOperationTestData.ItemId);
        response.Headers.TryAddWithoutValidation("Retry-After", "30");

        var outcome = await new FabricOperationRunner(new OperationTestTimeProvider()).HandleAsync(
            response, NoResultContract(), new(Sync: true), NoStateRead, NoResultRead, TestContext.Current.CancellationToken);

        Assert.Equal(FabricOperationStatus.AcceptedWithoutOperationId, outcome.Operation?.Status);
        Assert.Null(outcome.Operation?.ServerRetryAfterSeconds);
    }

    [Theory]
    [InlineData("Running")]
    [InlineData("Undefined")]
    [InlineData("FutureState")]
    public async Task BudgetExpiry_ReturnsLastKnownStateWithoutClaimingMutationFailure(string state)
    {
        using var response = FabricOperationTestData.Accepted();
        var clock = new OperationTestTimeProvider();
        var states = 0;
        var task = new FabricOperationRunner(clock).HandleAsync(response, NoResultContract(), new(Sync: true, MaxWaitSeconds: 10),
            (id, _) =>
            {
                states++;
                return Task.FromResult(new OperationStateResult(id, new(state), 20, 20));
            }, NoResultRead, TestContext.Current.CancellationToken);

        clock.Advance(3);
        await clock.WaitForTimerAsync(7, TestContext.Current.CancellationToken);
        clock.Advance(7);
        var outcome = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(FabricOperationStatus.Pending, outcome.Operation?.Status);
        Assert.Equal(state, outcome.Operation?.LastState?.Status);
        Assert.Equal("WaitBudgetExpired", outcome.Operation?.Issue?.Code);
        Assert.Equal(1, states);
        Assert.Empty(clock.Timers);
    }

    [Fact]
    public async Task FailedOperation_StopsWithoutResultFetch()
    {
        using var response = FabricOperationTestData.Accepted();
        var clock = new OperationTestTimeProvider();
        var states = 0;
        var task = new FabricOperationRunner(clock).HandleAsync(response,
            new FabricOperationContract<string>(new HashSet<HttpStatusCode> { HttpStatusCode.Created }, static _ => "never"),
            new(Sync: true), (id, _) =>
            {
                states++;
                return Task.FromResult(new OperationStateResult(id,
                    new("Failed", Error: new("CapacityUnavailable", Guid.Parse(FabricOperationTestData.ItemId))), null, 20));
            }, NoResultRead, TestContext.Current.CancellationToken);
        clock.Advance(3);
        var outcome = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(FabricOperationStatus.Failed, outcome.Operation?.Status);
        Assert.Equal("CapacityUnavailable", outcome.Operation?.LastState?.Error?.ErrorCode);
        Assert.Equal("OperationFailed", outcome.Operation?.Issue?.Code);
        Assert.Equal(1, states);
    }

    private static Exception RetryableFailure(HttpStatusCode status) => status == HttpStatusCode.TooManyRequests
        ? new FabricThrottledException(new(TimeSpan.FromSeconds(20)))
        : new FabricOperationException("Unavailable", "Read unavailable.", status, new(TimeSpan.FromSeconds(20)));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NoResultCompletion_IsPreservedAtDeadlineOrLateCallerCancellation(bool cancelCaller)
    {
        using var response = FabricOperationTestData.Accepted();
        using var caller = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var clock = new OperationTestTimeProvider();
        var task = new FabricOperationRunner(clock).HandleAsync(response, NoResultContract(), new(Sync: true),
            (id, _) =>
            {
                if (cancelCaller)
                {
                    caller.Cancel();
                }
                else
                {
                    clock.Advance(117);
                }
                return Task.FromResult(new OperationStateResult(id, new("Succeeded"), null, 20));
            }, NoResultRead, caller.Token);

        clock.Advance(3);
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(FabricOperationStatus.Succeeded, result.Operation?.Status);
        Assert.Null(result.Operation?.Issue);
        Assert.Empty(clock.Timers);
    }

    [Fact]
    public async Task NoResultAdapter_DoesNotInferCompletionFromNotFoundOrMissingLocation()
    {
        using var response = FabricOperationTestData.Accepted();
        var clock = new OperationTestTimeProvider();
        var task = new FabricOperationRunner(clock).HandleAsync(response, NoResultContract(), new(Sync: true),
            (_, _) => throw new FabricOperationException("NotFound", "State not found.", HttpStatusCode.NotFound),
            NoResultRead, TestContext.Current.CancellationToken);

        clock.Advance(3);
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(FabricOperationStatus.TrackingStopped, result.Operation?.Status);
        Assert.Equal((int)HttpStatusCode.NotFound, result.Operation?.Issue?.HttpStatus);
        Assert.Null(result.Operation?.LastState);
    }

    private static FabricOperationContract<string> NoResultContract() =>
        new(new HashSet<HttpStatusCode> { HttpStatusCode.Created }, null);

    private static Task<OperationStateResult> NoStateRead(Guid id, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("No state read expected.");

    private static Task<OperationResult> NoResultRead(Guid id, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("No result read expected.");
}
