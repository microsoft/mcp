// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class FabricOperationException(
    string code,
    string message,
    HttpStatusCode statusCode = HttpStatusCode.BadGateway,
    RetryConditionHeaderValue? retryAfter = null) : HttpRequestException(message, null, statusCode)
{
    internal string Code { get; } = code;
    internal RetryConditionHeaderValue? RetryAfter { get; } = retryAfter;
}
