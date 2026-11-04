// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    /// <inheritdoc />
    public async Task<ItemJobInstanceGetCommandResult> GetItemJobInstanceAsync(
        string workspaceId, string itemId, string jobInstanceId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(workspaceId, out var parsedWorkspaceId) || parsedWorkspaceId == Guid.Empty)
        {
            throw new ArgumentException("Workspace ID must be a nonempty UUID.", nameof(workspaceId));
        }
        if (!Guid.TryParse(itemId, out var parsedItemId) || parsedItemId == Guid.Empty)
        {
            throw new ArgumentException("Item ID must be a nonempty UUID.", nameof(itemId));
        }
        if (!Guid.TryParse(jobInstanceId, out var parsedJobInstanceId) || parsedJobInstanceId == Guid.Empty)
        {
            throw new ArgumentException("Job instance ID must be a nonempty UUID.", nameof(jobInstanceId));
        }

        cancellationToken.ThrowIfCancellationRequested();
        using var response = await SendFabricHttpRequestAsync(
            HttpMethod.Get,
            $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{parsedWorkspaceId:D}/items/{parsedItemId:D}/jobs/instances/{parsedJobInstanceId:D}",
            completionOption: HttpCompletionOption.ResponseHeadersRead, cancellationToken: cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new ItemJobInstanceRequestException(
                response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode, retryAfter);
        }

        if (!string.Equals(response.Content.Headers.ContentType?.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
        {
            throw new JsonException("Fabric returned a non-JSON job-instance response.");
        }

        var body = await FabricResponseBodyReader.ReadAsync(
            response, FabricResponseBodyReader.DefaultMaxJsonBytes,
            static () => new JsonException("The Fabric job-instance response exceeds the 1 MiB (1,048,576 byte) limit."),
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var instance = ReadItemJobInstance(body.Span, parsedItemId, parsedJobInstanceId);
        cancellationToken.ThrowIfCancellationRequested();
        return new(instance, retryAfter?.Delta is { } delay ? (long)delay.TotalSeconds : null, retryAfter?.Date);
    }

    private static FabricItemJobInstance ReadItemJobInstance(ReadOnlySpan<byte> body, Guid itemId, Guid jobInstanceId)
    {
        FabricItemJobInstance? instance;
        try
        {
            instance = JsonSerializer.Deserialize(body, CoreJsonContext.Default.FabricItemJobInstance);
        }
        catch (JsonException)
        {
            throw new JsonException("Fabric returned invalid job-instance metadata.");
        }

        return ValidateItemJobInstance(instance, itemId, jobInstanceId);
    }

    private static FabricItemJobInstance ValidateItemJobInstance(
        FabricItemJobInstance? instance, Guid itemId, Guid? jobInstanceId = null)
    {
        if (instance is null || instance.Id == Guid.Empty ||
            (jobInstanceId is { } requestedId && instance.Id != requestedId) || instance.ItemId != itemId ||
            string.IsNullOrWhiteSpace(instance.JobType) || string.IsNullOrWhiteSpace(instance.InvokeType) ||
            string.IsNullOrWhiteSpace(instance.Status))
        {
            throw new JsonException("Fabric returned invalid job-instance metadata.");
        }

        if (instance.FailureReason is { } failure)
        {
            instance = instance with
            {
                FailureReason = failure with
                {
                    ErrorCode = FabricCoreHttpHelpers.IsSafeErrorCode(failure.ErrorCode) ? failure.ErrorCode : null,
                    RequestId = failure.RequestId == Guid.Empty ? null : failure.RequestId
                }
            };
        }
        return instance;
    }
}
