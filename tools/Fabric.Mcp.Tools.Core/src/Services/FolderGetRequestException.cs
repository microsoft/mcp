// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class FolderGetRequestException(
    HttpStatusCode statusCode, RetryConditionHeaderValue? retryAfter = null,
    string? errorCode = null, bool errorMetadataUnavailable = false)
    : HttpRequestException("Unable to retrieve Fabric folder metadata.", null, statusCode)
{
    internal RetryConditionHeaderValue? RetryAfter { get; } = retryAfter;
    internal string? ErrorCode { get; } = errorCode;
    internal bool ErrorMetadataUnavailable { get; } = errorMetadataUnavailable;
}
