// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class FabricItemDeleteThrottledException(RetryConditionHeaderValue? retryAfter)
    : HttpRequestException(CreateMessage(retryAfter), null, HttpStatusCode.TooManyRequests)
{
    private static string CreateMessage(RetryConditionHeaderValue? retryAfter) => retryAfter switch
    {
        { Delta: { } delay } when delay >= TimeSpan.Zero =>
            FormattableString.Invariant($"Fabric throttled the deletion request. Wait at least {delay.TotalSeconds:0} seconds before retrying"),
        { Date: { } date } =>
            FormattableString.Invariant($"Fabric throttled the deletion request. Retry after {date.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC"),
        _ => "Fabric throttled the deletion request. Wait before retrying; the request was not automatically retried"
    };
}
