// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Fabric.Mcp.Tools.Core.Services;

internal static class FabricCoreHttpHelpers
{
    internal static bool IsSafeErrorCode(string? code) =>
        code is { Length: > 0 and <= 128 } && code.All(static ch => char.IsAsciiLetterOrDigit(ch) || ch is '_' or '-' or '.');

    internal static RetryConditionHeaderValue? GetRetryAfter(HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return response.Headers.NonValidated.TryGetValues("Retry-After", out var values) &&
            values.Count == 1 &&
            RetryConditionHeaderValue.TryParse(values.Single(), out var retryAfter) &&
            (retryAfter.Delta is null || retryAfter.Delta >= TimeSpan.Zero)
                ? retryAfter
                : null;
    }

    internal static async Task<(string? Code, bool Unavailable)> ReadErrorCodeAsync(
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
                static () => new JsonException("Fabric error metadata exceeds the 1 MiB limit."),
                cancellationToken);
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                try
                {
                    // Duplicate codes cannot safely identify a specific service failure.
                    var codes = document.RootElement.EnumerateObject().Where(static property => property.NameEquals("errorCode")).ToArray();
                    if (codes is [{ Value.ValueKind: JsonValueKind.String }] &&
                        codes[0].Value.GetString() is { } code && IsSafeErrorCode(code))
                    {
                        return (code, false);
                    }
                }
                catch (InvalidOperationException)
                {
                    // JSON decoding is lazy. Keep this boundary separate from reader failures.
                    return (null, true);
                }
            }
            return (null, true);
        }
        catch (Exception ex) when (ex is JsonException or IOException or HttpRequestException or OperationCanceledException)
        {
            // Optional metadata must not replace a known HTTP status or Retry-After.
            // Callers explicitly report its unavailability instead of exposing backend content.
            return (null, true);
        }
    }

    internal static string EncodeContinuationToken(string token)
    {
        ArgumentNullException.ThrowIfNull(token);

        // Fabric can return percent-escaped tokens; encode only their raw spans.
        var encoded = new StringBuilder();
        var segmentStart = 0;
        for (var index = 0; index + 2 < token.Length; index++)
        {
            if (token[index] == '%' && char.IsAsciiHexDigit(token[index + 1]) && char.IsAsciiHexDigit(token[index + 2]))
            {
                encoded.Append(Uri.EscapeDataString(token.AsSpan(segmentStart, index - segmentStart)));
                encoded.Append(token.AsSpan(index, 3));
                index += 2;
                segmentStart = index + 1;
            }
        }

        encoded.Append(Uri.EscapeDataString(token.AsSpan(segmentStart)));
        return encoded.ToString();
    }
}
