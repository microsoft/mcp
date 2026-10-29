// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using System.Text;

namespace Fabric.Mcp.Tools.Core.Services;

internal static class FabricCoreHttpHelpers
{
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
