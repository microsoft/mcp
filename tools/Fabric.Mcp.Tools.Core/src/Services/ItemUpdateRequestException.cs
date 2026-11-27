// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class ItemUpdateRequestException(HttpStatusCode statusCode, int? retryAfterSeconds)
    : HttpRequestException($"Fabric Update Item returned HTTP {(int)statusCode}.", null, statusCode)
{
    internal int? RetryAfterSeconds { get; } = retryAfterSeconds;
}
