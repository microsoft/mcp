// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class ItemJobRunRequestException(HttpStatusCode statusCode, RetryConditionHeaderValue? retryAfter)
    : HttpRequestException("Fabric job submission was not confirmed.", null, statusCode)
{
    internal RetryConditionHeaderValue? RetryAfter { get; } = retryAfter;
}
