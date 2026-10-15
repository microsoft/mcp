// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;
using Microsoft.Identity.Client;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class FabricOperationRunner(TimeProvider timeProvider)
{
    private readonly TimeProvider _timeProvider = timeProvider;
    private static readonly TimeSpan MaximumTimerDelay = TimeSpan.FromMilliseconds(uint.MaxValue - 1);

    internal async Task<FabricOperationOutcome<T>> HandleAsync<T>(
        HttpResponseMessage response,
        FabricOperationContract<T> contract,
        FabricOperationWaitOptions options,
        Func<Guid, CancellationToken, Task<OperationStateResult>> getState,
        Func<Guid, CancellationToken, Task<OperationResult>> getResult,
        CancellationToken cancellationToken) where T : class
    {
        if (response.StatusCode != HttpStatusCode.Accepted)
        {
            if (!contract.CompletedStatusCodes.Contains(response.StatusCode))
            {
                FabricOperationHttp.ThrowUnexpectedResponse(response);
            }

            var completed = new FabricOperationReceipt(FabricOperationStatus.Succeeded, FabricOperationHttp.GetOperationId(response));
            try
            {
                var body = await FabricOperationHttp.ReadJsonAsync(response, completed.OperationId, cancellationToken);
                return new(ReadResult(body, contract), null);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw Canceled(completed, cancellationToken);
            }
            catch (Exception ex) when (IsReadFailure(ex) || ex is OperationCanceledException)
            {
                return new(null, ReadStopped(completed, FabricOperationErrors.ToIssue(ex)));
            }
        }

        var started = _timeProvider.GetTimestamp();
        var operationId = FabricOperationHttp.GetOperationId(response);
        var serverDelay = FabricOperationHttp.GetRetryAfterSeconds(response, _timeProvider);
        var nextDelay = FabricOperationHttp.GetPollDelay(serverDelay, options.EarlyPoll);
        var receipt = new FabricOperationReceipt(FabricOperationStatus.Accepted, operationId,
            ServerRetryAfterSeconds: serverDelay,
            RecommendedPollAfterSeconds: nextDelay);

        if (operationId is null)
        {
            return new(null, receipt with
            {
                Status = FabricOperationStatus.AcceptedWithoutOperationId,
                RecommendedPollAfterSeconds = null,
                Issue = new("MissingOperationId",
                    "Fabric accepted the mutation but returned no usable operation ID. Completion is unconfirmed. Check whether creation occurred; do not automatically resubmit.")
            });
        }
        if (!options.Sync)
        {
            return new(null, receipt);
        }

        var fetchResult = false;
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!await DelayWithinBudgetAsync(nextDelay, started, options.MaxWaitSeconds, cancellationToken))
                {
                    return new(null, BudgetExpired(receipt));
                }

                var remaining = RemainingSeconds(started, options.MaxWaitSeconds);
                if (remaining <= 0)
                {
                    return new(null, BudgetExpired(receipt));
                }
                using var budget = new CancellationTokenSource(ToTimerDelay(remaining), _timeProvider);
                using var request = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, budget.Token);
                try
                {
                    request.Token.ThrowIfCancellationRequested();
                    if (fetchResult)
                    {
                        var result = await getResult(operationId.Value, request.Token);
                        return new(ReadResult(result, contract), receipt with
                        {
                            Status = FabricOperationStatus.Succeeded,
                            RecommendedPollAfterSeconds = null,
                            Issue = null
                        });
                    }

                    var state = await getState(operationId.Value, request.Token);
                    receipt = receipt with
                    {
                        LastState = state.State,
                        ServerRetryAfterSeconds = state.ServerRetryAfterSeconds,
                        RecommendedPollAfterSeconds = state.RecommendedPollAfterSeconds,
                        Issue = null
                    };
                    if (state.State.Status == "Failed")
                    {
                        return new(null, receipt with
                        {
                            Status = FabricOperationStatus.Failed,
                            RecommendedPollAfterSeconds = null,
                            Issue = new("OperationFailed",
                                "Fabric reports that the operation failed. Inspect the safe error code and request ID; the mutation was not resubmitted.",
                                (int)HttpStatusCode.BadGateway)
                        });
                    }
                    if (state.State.Status == "Succeeded")
                    {
                        receipt = receipt with { Status = FabricOperationStatus.Succeeded };
                        if (contract.ReadResult is null)
                        {
                            return new(null, receipt with { RecommendedPollAfterSeconds = null });
                        }
                        fetchResult = true;
                        nextDelay = state.ServerRetryAfterSeconds is { } delay ? FabricOperationHttp.GetPollDelay(delay) : 0;
                    }
                    else
                    {
                        // Undefined and future statuses are not evidence of completion.
                        receipt = receipt with { Status = FabricOperationStatus.Pending };
                        nextDelay = state.RecommendedPollAfterSeconds;
                    }
                }
                catch (Exception ex) when (IsRetryableReadResponse(ex))
                {
                    var retryAfter = ex is FabricThrottledException throttled
                        ? throttled.RetryAfter
                        : ((FabricOperationException)ex).RetryAfter;
                    serverDelay = FabricOperationHttp.GetRetryAfterSeconds(retryAfter, _timeProvider);
                    nextDelay = FabricOperationHttp.GetPollDelay(serverDelay);
                    receipt = receipt with
                    {
                        ServerRetryAfterSeconds = serverDelay,
                        RecommendedPollAfterSeconds = nextDelay,
                        Issue = FabricOperationErrors.ToIssue(ex)
                    };
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    if (RemainingSeconds(started, options.MaxWaitSeconds) <= 0)
                    {
                        return new(null, BudgetExpired(receipt));
                    }
                    return new(null, ReadStopped(receipt, new("ReadTimedOut",
                        "The operation read timed out before the wait budget expired. Resume using the operation ID; the Fabric operation was not canceled.",
                        (int)HttpStatusCode.GatewayTimeout)));
                }
                catch (Exception ex) when (IsReadFailure(ex))
                {
                    return new(null, ReadStopped(receipt, FabricOperationErrors.ToIssue(ex)));
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw Canceled(receipt, cancellationToken);
        }
    }

    private static T? ReadResult<T>(OperationResult result, FabricOperationContract<T> contract) where T : class
    {
        if (contract.ReadResult is { } reader)
        {
            if (!result.HasBody || result.Value.ValueKind == JsonValueKind.Undefined)
            {
                throw new FabricOperationException("MissingResult", "The operation completed but Fabric did not return its required JSON result.");
            }
            return reader(result.Value);
        }
        if (result.HasBody)
        {
            throw new FabricOperationException("UnexpectedResult", "Fabric returned a body for an operation whose documented synchronous result is empty.");
        }
        return null;
    }

    private static bool IsRetryableReadResponse(Exception exception) =>
        exception is FabricThrottledException or FabricOperationException
        {
            StatusCode: HttpStatusCode.TooManyRequests or HttpStatusCode.ServiceUnavailable
        };

    private static bool IsReadFailure(Exception exception) =>
        exception is HttpRequestException or IOException or JsonException or AuthenticationFailedException or
            RequestFailedException or MsalException or TimeoutException or InvalidOperationException;

    private static FabricOperationReceipt ReadStopped(FabricOperationReceipt receipt, FabricOperationIssue issue) =>
        receipt with
        {
            Status = receipt.Status == FabricOperationStatus.Succeeded
                ? FabricOperationStatus.ResultUnavailable
                : FabricOperationStatus.TrackingStopped,
            Issue = receipt.Status == FabricOperationStatus.Succeeded
                ? issue with { Message = $"The operation completed, but its result was not retrieved. {issue.Message.TrimEnd('.')}. Do not resubmit the original mutation." }
                : issue
        };

    private static FabricOperationReceipt BudgetExpired(FabricOperationReceipt receipt) =>
        receipt with
        {
            Status = receipt.Status == FabricOperationStatus.Succeeded
                ? FabricOperationStatus.ResultUnavailable
                : FabricOperationStatus.Pending,
            Issue = new("WaitBudgetExpired", receipt.Status == FabricOperationStatus.Succeeded
                ? "The operation succeeded, but its result was not retrieved within the local wait budget. Resume with get-operation-result; do not recreate the item."
                : "The local wait budget expired. Completion remains unconfirmed. Resume with get-operation-state; stopping this wait did not cancel Fabric's operation.")
        };

    private static FabricOperationCanceledException Canceled(FabricOperationReceipt receipt, CancellationToken cancellationToken) =>
        new(ReadStopped(receipt, new("CallerCanceled",
            "The caller canceled the local wait, not the Fabric operation. Resume using the operation ID; do not resubmit the mutation.",
            (int)HttpStatusCode.RequestTimeout)), cancellationToken);

    private double RemainingSeconds(long started, double budget) =>
        budget - _timeProvider.GetElapsedTime(started).TotalSeconds;

    private async Task<bool> DelayWithinBudgetAsync(double delay, long started, double budget, CancellationToken cancellationToken)
    {
        while (delay > 0)
        {
            var remaining = RemainingSeconds(started, budget);
            if (remaining <= 0)
            {
                return false;
            }
            var duration = ToTimerDelay(Math.Min(delay, remaining));
            await Task.Delay(duration, _timeProvider, cancellationToken);
            delay -= duration.TotalSeconds;
        }
        return RemainingSeconds(started, budget) > 0;
    }

    private static TimeSpan ToTimerDelay(double seconds) =>
        TimeSpan.FromTicks(Math.Max(1, (long)(Math.Min(seconds, MaximumTimerDelay.TotalSeconds) * TimeSpan.TicksPerSecond)));
}
