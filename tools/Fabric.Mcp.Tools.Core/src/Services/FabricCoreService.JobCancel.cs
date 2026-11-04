// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    /// <inheritdoc />
    public async Task<ItemJobInstanceCancelCommandResult> CancelItemJobInstanceAsync(
        string workspaceId, string itemId, string jobInstanceId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(workspaceId, out var workspace) || workspace == Guid.Empty)
        {
            throw new ArgumentException("Workspace ID must be a nonempty UUID.", nameof(workspaceId));
        }
        if (!Guid.TryParse(itemId, out var item) || item == Guid.Empty)
        {
            throw new ArgumentException("Item ID must be a nonempty UUID.", nameof(itemId));
        }
        if (!Guid.TryParse(jobInstanceId, out var job) || job == Guid.Empty)
        {
            throw new ArgumentException("Job instance ID must be a nonempty UUID.", nameof(jobInstanceId));
        }

        cancellationToken.ThrowIfCancellationRequested();
        using var response = await SendFabricHttpRequestAsync(
            HttpMethod.Post,
            $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspace:D}/items/{item:D}/jobs/instances/{job:D}/cancel",
            completionOption: HttpCompletionOption.ResponseHeadersRead, cancellationToken: cancellationToken);
        var retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
        if (response.StatusCode == HttpStatusCode.Accepted)
        {
            // The caller already supplied the target. Ignore Location and the body entirely,
            // and never let late request cancellation erase confirmed acceptance.
            return new(workspace, item, job,
                retryAfter?.Delta is { } delay ? (long)delay.TotalSeconds : null, retryAfter?.Date,
                retryAfter is null ? "MissingOrInvalidRetryAfter" : null);
        }
        if (response.IsSuccessStatusCode)
        {
            throw new ItemJobInstanceCancelRequestException(HttpStatusCode.BadGateway, retryAfter);
        }

        var (errorCode, unavailable) = await ReadJobCancellationErrorCodeAsync(response, cancellationToken);
        throw new ItemJobInstanceCancelRequestException(response.StatusCode, retryAfter, errorCode, unavailable);
    }

    private static async Task<(string? Code, bool Unavailable)> ReadJobCancellationErrorCodeAsync(
        HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            if (!string.Equals(response.Content.Headers.ContentType?.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                return (null, true);
            }
            var body = await FabricResponseBodyReader.ReadAsync(
                response, FabricResponseBodyReader.DefaultMaxJsonBytes,
                static () => new JsonException("Fabric cancellation error metadata exceeds the 1 MiB limit."),
                cancellationToken);
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                try
                {
                    // Ambiguous duplicate codes must not turn a generic error into JobAlreadyCompleted.
                    var codes = document.RootElement.EnumerateObject().Where(static property => property.NameEquals("errorCode")).ToArray();
                    if (codes is [{ Value.ValueKind: JsonValueKind.String }] &&
                        codes[0].Value.GetString() is { } code && FabricCoreHttpHelpers.IsSafeErrorCode(code))
                    {
                        return (code, false);
                    }
                }
                catch (InvalidOperationException)
                {
                    // JsonDocument decodes escaped UTF-16 lazily in names and string values.
                    // Limit this catch to JSON inspection, not unrelated response-reader failures.
                    return (null, true);
                }
            }
            return (null, true);
        }
        catch (Exception ex) when (ex is JsonException or IOException or HttpRequestException or OperationCanceledException)
        {
            // Preserve the known HTTP failure and retry guidance even if optional metadata cannot
            // be read. The command explicitly reports its absence without exposing backend content.
            return (null, true);
        }
    }
}
