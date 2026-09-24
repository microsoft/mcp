using System.Net;
using System.Net.Http.Headers;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class FabricItemThrottledException(RetryConditionHeaderValue retryAfter)
    : HttpRequestException(CreateMessage(retryAfter), null, HttpStatusCode.TooManyRequests)
{
    private static string CreateMessage(RetryConditionHeaderValue retryAfter) =>
        retryAfter.Delta is { } delay
            ? FormattableString.Invariant($"Fabric throttled the request. Wait at least {delay.TotalSeconds:0} seconds before retrying")
            : FormattableString.Invariant($"Fabric throttled the request. Retry after {retryAfter.Date?.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC");
}
