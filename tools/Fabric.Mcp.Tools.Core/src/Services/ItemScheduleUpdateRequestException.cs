// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class ItemScheduleUpdateRequestException(
    HttpStatusCode statusCode, RetryConditionHeaderValue? retryAfter, string? errorCode = null,
    bool errorMetadataUnavailable = false, bool updateConfirmed = false)
    : HttpRequestException("Fabric schedule update did not return usable schedule metadata.", null, statusCode)
{
    internal RetryConditionHeaderValue? RetryAfter { get; } = retryAfter;
    internal string? ErrorCode { get; } = errorCode;
    internal bool ErrorMetadataUnavailable { get; } = errorMetadataUnavailable;
    internal bool UpdateConfirmed { get; } = updateConfirmed;
}
