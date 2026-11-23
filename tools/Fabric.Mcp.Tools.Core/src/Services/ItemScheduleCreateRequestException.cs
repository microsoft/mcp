// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class ItemScheduleCreateRequestException(
    HttpStatusCode statusCode, RetryConditionHeaderValue? retryAfter, string? errorCode = null,
    bool errorMetadataUnavailable = false, bool creationConfirmed = false)
    : HttpRequestException("Fabric schedule creation did not return usable schedule metadata.", null, statusCode)
{
    internal RetryConditionHeaderValue? RetryAfter { get; } = retryAfter;
    internal string? ErrorCode { get; } = errorCode;
    internal bool ErrorMetadataUnavailable { get; } = errorMetadataUnavailable;
    internal bool CreationConfirmed { get; } = creationConfirmed;
}
