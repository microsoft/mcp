// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

internal static class FabricOperationHttp
{
    internal const int MaxJsonBytes = 1024 * 1024;
    internal const double FallbackPollSeconds = 20;
    internal const double EarlyPollSeconds = 3;

    internal static double? GetRetryAfterSeconds(HttpResponseMessage response, TimeProvider timeProvider) =>
        GetRetryAfterSeconds(FabricCoreHttpHelpers.GetRetryAfter(response), timeProvider);

    internal static double? GetRetryAfterSeconds(RetryConditionHeaderValue? retryAfter, TimeProvider timeProvider) =>
        retryAfter?.Delta?.TotalSeconds ??
        (retryAfter?.Date is { } date ? Math.Max(0, (date - timeProvider.GetUtcNow()).TotalSeconds) : null);

    internal static double GetPollDelay(double? serverDelay, bool early = false)
    {
        var delay = Math.Max(1, serverDelay ?? FallbackPollSeconds);
        return early ? Math.Min(EarlyPollSeconds, delay) : delay;
    }

    internal static Guid? GetOperationId(HttpResponseMessage response)
    {
        if (response.Headers.NonValidated.TryGetValues("x-ms-operation-id", out var values) &&
            values.Count == 1 && Guid.TryParse(values.Single(), out var id) && id != Guid.Empty)
        {
            return id;
        }
        return null;
    }

    internal static void ValidateOperationId(HttpResponseMessage response, Guid requestedId)
    {
        if (response.Headers.Contains("x-ms-operation-id") && GetOperationId(response) != requestedId)
        {
            throw new FabricOperationException("InvalidOperationId", "Fabric returned inconsistent operation identification. Resume only with the original operation ID.");
        }
    }

    internal static void ThrowUnexpectedResponse(HttpResponseMessage response)
    {
        var retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
        if (response.StatusCode == HttpStatusCode.TooManyRequests && retryAfter is not null)
        {
            throw new FabricThrottledException(retryAfter);
        }

        throw new FabricOperationException(
            "UnexpectedResponse",
            "Fabric did not return the documented response. Check the operation or whether creation occurred before submitting another mutation.",
            response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode,
            retryAfter);
    }

    internal static async Task<OperationResult> ReadJsonAsync(
        HttpResponseMessage response, Guid? operationId, CancellationToken cancellationToken)
    {
        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (mediaType is not null && !string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase))
        {
            throw new FabricOperationException("UnsupportedResultMediaType",
                "Only JSON and empty Fabric operation results are supported. Binary and other media types are not downloaded.");
        }

        if (response.Content.Headers.ContentLength > MaxJsonBytes)
        {
            throw ResultTooLarge();
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var buffer = new MemoryStream();
        var chunk = new byte[8192];
        while (true)
        {
            // Read at most one byte beyond the limit, including streams without Content-Length.
            var count = (int)Math.Min(chunk.Length, MaxJsonBytes + 1 - buffer.Length);
            var read = await stream.ReadAsync(chunk.AsMemory(0, count), cancellationToken);
            if (read == 0)
            {
                break;
            }
            if (buffer.Length + read > MaxJsonBytes)
            {
                throw ResultTooLarge();
            }
            buffer.Write(chunk, 0, read);
        }

        if (buffer.Length == 0)
        {
            return new(operationId, false, default);
        }
        if (mediaType is null)
        {
            throw new FabricOperationException("UnsupportedResultMediaType", "A nonempty Fabric operation result must have application/json content type.");
        }

        try
        {
            var value = JsonSerializer.Deserialize(buffer.GetBuffer().AsSpan(0, (int)buffer.Length), CoreJsonContext.Default.JsonElement);
            return new(operationId, true, value);
        }
        catch (JsonException)
        {
            throw new FabricOperationException("InvalidJson", "Fabric returned invalid JSON for the operation. No result was fabricated or truncated.");
        }
    }

    private static FabricOperationException ResultTooLarge() =>
        new("ResultTooLarge", "The Fabric operation JSON response exceeds the 1 MiB (1,048,576 byte) limit. No result was truncated.");
}
