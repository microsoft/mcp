// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class FolderDeleteRequestException(
    HttpStatusCode statusCode, RetryConditionHeaderValue? retryAfter = null, string? errorCode = null,
    bool errorMetadataUnavailable = false, bool requestNotSent = false, HttpStatusCode? responseStatusCode = null)
    : HttpRequestException("Fabric folder deletion could not return a receipt.", null, statusCode)
{
    internal RetryConditionHeaderValue? RetryAfter { get; } = retryAfter;
    internal string? ErrorCode { get; } = errorCode;
    internal bool ErrorMetadataUnavailable { get; } = errorMetadataUnavailable;
    internal bool RequestNotSent { get; } = requestNotSent;
    internal HttpStatusCode? ResponseStatusCode { get; } = responseStatusCode;
    internal bool DeletionConfirmed => ResponseStatusCode == HttpStatusCode.OK;
}
