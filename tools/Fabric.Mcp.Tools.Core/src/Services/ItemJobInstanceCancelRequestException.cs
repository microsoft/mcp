// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class ItemJobInstanceCancelRequestException(
    HttpStatusCode statusCode, RetryConditionHeaderValue? retryAfter, string? errorCode = null, bool errorMetadataUnavailable = false)
    : HttpRequestException("Fabric job cancellation was not confirmed.", null, statusCode)
{
    internal RetryConditionHeaderValue? RetryAfter { get; } = retryAfter;
    internal string? ErrorCode { get; } = errorCode;
    internal bool ErrorMetadataUnavailable { get; } = errorMetadataUnavailable;
}
