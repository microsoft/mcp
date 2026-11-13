// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Validation;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    /// <inheritdoc />
    public async Task<ApiResourceGetCommandResult> GetApiResourceAsync(
        string? api,
        string path,
        int? maxResponseBytes = null,
        CancellationToken cancellationToken = default)
    {
        var target = ApiRequestInput.ParseApi(api);
        var uri = ApiRequestInput.BuildUri(target, path);
        var limit = ApiRequestInput.ParseMaxResponseBytes(maxResponseBytes);
        cancellationToken.ThrowIfCancellationRequested();

        using var response = await SendApiRequestAsync(target, HttpMethod.Get, uri, null, cancellationToken);
        return new(await FabricApiResponseReader.ReadAsync(target, response, limit, _timeProvider, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<ApiRequestInvokeCommandResult> InvokeApiRequestAsync(
        string? api,
        string path,
        string method,
        string? body = null,
        int? maxResponseBytes = null,
        FabricOperationWaitOptions? waitOptions = null,
        bool operationResult = false,
        CancellationToken cancellationToken = default)
    {
        var target = ApiRequestInput.ParseApi(api);
        var uri = ApiRequestInput.BuildUri(target, path);
        var httpMethod = ApiRequestInput.ParseWriteMethod(method);
        var jsonBody = ApiRequestInput.ParseBody(body);
        var limit = ApiRequestInput.ParseMaxResponseBytes(maxResponseBytes);
        waitOptions ??= new();
        if (ApiRequestInput.GetWaitOptionsError(target, waitOptions.Sync, waitOptions.MaxWaitSeconds, operationResult) is { } error)
        {
            throw new ApiRequestInputException(error);
        }
        cancellationToken.ThrowIfCancellationRequested();

        // Exactly one request is sent; it is never retried or resubmitted.
        using var response = await SendApiRequestAsync(target, httpMethod, uri, jsonBody, cancellationToken);
        if (target != ApiRequestTarget.Fabric || response.StatusCode != HttpStatusCode.Accepted)
        {
            return new(await FabricApiResponseReader.ReadAsync(target, response, limit, _timeProvider, cancellationToken));
        }

        // Fabric already accepted the request, so capture its tracking metadata before reading the body. Every
        // body read failure keeps this receipt so the operation can be resumed instead of resubmitted.
        var accepted = new FabricOperationReceipt(
            FabricOperationStatus.Accepted,
            FabricOperationHttp.GetOperationId(response),
            ServerRetryAfterSeconds: FabricOperationHttp.GetRetryAfterSeconds(response, _timeProvider));
        FabricApiResponse apiResponse;
        try
        {
            apiResponse = await FabricApiResponseReader.ReadAsync(target, response, limit, _timeProvider, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw new FabricOperationCanceledException(accepted with
            {
                Status = FabricOperationStatus.TrackingStopped,
                Issue = new("CallerCanceled",
                    "The caller canceled after Fabric accepted the request. Resume using the operation ID; do not resubmit the request.",
                    (int)HttpStatusCode.RequestTimeout)
            }, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or JsonException or InvalidOperationException or
            TimeoutException or OperationCanceledException)
        {
            throw new FabricOperationReceiptException(UnreadableAcceptedResponse(accepted, ex));
        }

        // Only Fabric 202 responses are tracked through the shared operation runner; this response is always 202 here.
        var contract = new FabricOperationContract<OperationResult>(
            new HashSet<HttpStatusCode>(),
            operationResult ? static value => new OperationResult(null, true, FabricApiResponseReader.EnsureRetainedDepth(value)) : null);
        var outcome = await new FabricOperationRunner(_timeProvider).HandleAsync(
            response, contract, waitOptions, GetOperationStateAsync,
            (id, token) => GetOperationResultAsync(id, limit, token), cancellationToken);
        return new(apiResponse, outcome.Operation,
            outcome.Result is { } result ? result with { OperationId = outcome.Operation?.OperationId } : null);
    }

    private static FabricOperationReceipt UnreadableAcceptedResponse(FabricOperationReceipt accepted, Exception exception)
    {
        var code = exception is ApiRequestException { Failure: var failure } ? failure.ToString() : "ResponseReadFailed";
        return accepted.OperationId is null
            ? accepted with
            {
                Status = FabricOperationStatus.AcceptedWithoutOperationId,
                Issue = new(code,
                    "Fabric accepted the request, but its response could not be read and it returned no usable operation ID. Completion is unconfirmed. Check whether the request completed; do not resubmit it automatically.",
                    (int)HttpStatusCode.BadGateway)
            }
            : accepted with
            {
                Status = FabricOperationStatus.TrackingStopped,
                Issue = new(code,
                    "Fabric accepted the request, but its response could not be read. Resume with get-operation-state using the operation ID; do not resubmit the request.",
                    (int)HttpStatusCode.BadGateway)
            };
    }

    private Task<HttpResponseMessage> SendApiRequestAsync(
        ApiRequestTarget target, HttpMethod method, Uri uri, string? jsonBody, CancellationToken cancellationToken) =>
        SendFabricHttpRequestAsync(
            method,
            uri.OriginalString,
            jsonBody,
            new() { ["Accept"] = "application/json" },
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken: cancellationToken,
            scopes: target.Scopes);
}
