// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;

namespace Fabric.Mcp.Tools.Core.Services;

/// <summary>A sanitized REST failure. It never carries the backend error message or response body.</summary>
internal sealed class ApiRequestException(
    ApiRequestFailure failure,
    HttpStatusCode statusCode,
    int? responseStatus = null,
    RetryConditionHeaderValue? retryAfter = null,
    string? errorCode = null,
    string? requestId = null,
    bool errorMetadataUnavailable = false)
    : HttpRequestException("The Fabric or Power BI REST request failed.", null, statusCode)
{
    internal ApiRequestFailure Failure { get; } = failure;
    internal int? ResponseStatus { get; } = responseStatus;
    internal RetryConditionHeaderValue? RetryAfter { get; } = retryAfter;
    internal string? ErrorCode { get; } = errorCode;
    internal string? RequestId { get; } = requestId;
    internal bool ErrorMetadataUnavailable { get; } = errorMetadataUnavailable;
}
