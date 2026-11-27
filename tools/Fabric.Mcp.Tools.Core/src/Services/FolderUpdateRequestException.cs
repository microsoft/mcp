// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class FolderUpdateRequestException(
    HttpStatusCode statusCode, RetryConditionHeaderValue? retryAfter = null, string? errorCode = null,
    bool errorMetadataUnavailable = false, bool requestNotSent = false, HttpStatusCode? responseStatusCode = null)
    : HttpRequestException("Fabric folder update did not return usable folder metadata.", null, statusCode)
{
    internal RetryConditionHeaderValue? RetryAfter { get; } = retryAfter;
    internal string? ErrorCode { get; } = errorCode;
    internal bool ErrorMetadataUnavailable { get; } = errorMetadataUnavailable;
    internal bool RequestNotSent { get; } = requestNotSent;
    internal HttpStatusCode? ResponseStatusCode { get; } = responseStatusCode;
    internal bool UpdateConfirmed => ResponseStatusCode == HttpStatusCode.OK;
}
