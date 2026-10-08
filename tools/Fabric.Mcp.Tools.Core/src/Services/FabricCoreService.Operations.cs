// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    public async Task<OperationStateResult> GetOperationStateAsync(string operationId, CancellationToken cancellationToken) =>
        await GetOperationStateAsync(ParseOperationId(operationId), cancellationToken);

    public async Task<OperationResult> GetOperationResultAsync(string operationId, CancellationToken cancellationToken) =>
        await GetOperationResultAsync(ParseOperationId(operationId), cancellationToken);

    private async Task<OperationStateResult> GetOperationStateAsync(Guid operationId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var response = await SendFabricHttpRequestAsync(
            HttpMethod.Get, $"{FabricEndpoints.GetFabricApiBaseUrl()}/operations/{operationId:D}",
            completionOption: HttpCompletionOption.ResponseHeadersRead, cancellationToken: cancellationToken);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            FabricOperationHttp.ThrowUnexpectedResponse(response);
        }
        FabricOperationHttp.ValidateOperationId(response, operationId);
        var body = await FabricOperationHttp.ReadJsonAsync(response, operationId, cancellationToken);
        if (body.Value.ValueKind != JsonValueKind.Object)
        {
            throw InvalidState();
        }

        FabricOperationState? state;
        try
        {
            state = body.Value.Deserialize(CoreJsonContext.Default.FabricOperationState);
        }
        catch (JsonException)
        {
            throw InvalidState();
        }
        if (state is null || string.IsNullOrWhiteSpace(state.Status) || state.PercentComplete is < 0 or > 100)
        {
            throw InvalidState();
        }
        if (state.Error is { } error)
        {
            state = state with
            {
                Error = error with
                {
                    ErrorCode = IsSafeErrorCode(error.ErrorCode) ? error.ErrorCode : null,
                    RequestId = error.RequestId == Guid.Empty ? null : error.RequestId
                }
            };
        }

        var retryAfter = FabricOperationHttp.GetRetryAfterSeconds(response, _timeProvider);
        return new(operationId, state, retryAfter, FabricOperationHttp.GetPollDelay(retryAfter));
    }

    private async Task<OperationResult> GetOperationResultAsync(Guid operationId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var response = await SendFabricHttpRequestAsync(
            HttpMethod.Get, $"{FabricEndpoints.GetFabricApiBaseUrl()}/operations/{operationId:D}/result",
            completionOption: HttpCompletionOption.ResponseHeadersRead, cancellationToken: cancellationToken);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            FabricOperationHttp.ThrowUnexpectedResponse(response);
        }
        FabricOperationHttp.ValidateOperationId(response, operationId);
        return await FabricOperationHttp.ReadJsonAsync(response, operationId, cancellationToken);
    }

    private static Guid ParseOperationId(string operationId) =>
        Guid.TryParse(operationId, out var id) && id != Guid.Empty
            ? id
            : throw new ArgumentException("Operation ID must be a nonempty UUID.", nameof(operationId));

    private static bool IsSafeErrorCode(string? code) =>
        code is { Length: > 0 and <= 128 } && code.All(static ch => char.IsAsciiLetterOrDigit(ch) || ch is '_' or '-' or '.');

    private static FabricOperationException InvalidState() =>
        new("InvalidOperationState", "Fabric returned an invalid operation state. Completion could not be determined from this response.");
}
