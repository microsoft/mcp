// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
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

        FabricApiResponse apiResponse;
        try
        {
            apiResponse = await FabricApiResponseReader.ReadAsync(target, response, limit, _timeProvider, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Fabric already accepted the request; keep the operation ID so it can be resumed instead of resubmitted.
            throw new FabricOperationCanceledException(new(FabricOperationStatus.TrackingStopped, FabricOperationHttp.GetOperationId(response),
                Issue: new("CallerCanceled",
                    "The caller canceled after Fabric accepted the request. Resume using the operation ID; do not resubmit the request.",
                    (int)HttpStatusCode.RequestTimeout)), cancellationToken);
        }

        // Only Fabric 202 responses are tracked through the shared operation runner; this response is always 202 here.
        var contract = new FabricOperationContract<OperationResult>(
            new HashSet<HttpStatusCode>(),
            operationResult ? static value => new OperationResult(null, true, value) : null);
        var outcome = await new FabricOperationRunner(_timeProvider).HandleAsync(
            response, contract, waitOptions, GetOperationStateAsync,
            (id, token) => GetOperationResultAsync(id, limit, token), cancellationToken);
        return new(apiResponse, outcome.Operation,
            outcome.Result is { } result ? result with { OperationId = outcome.Operation?.OperationId } : null);
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
