// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class ItemScheduleRequestException(HttpStatusCode statusCode, RetryConditionHeaderValue? retryAfter)
    : HttpRequestException("Unable to retrieve the Fabric item schedules.", null, statusCode)
{
    internal RetryConditionHeaderValue? RetryAfter { get; } = retryAfter;
}
