// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class FolderCreateRequestException(
    HttpStatusCode statusCode, RetryConditionHeaderValue? retryAfter = null, string? errorCode = null,
    bool errorMetadataUnavailable = false, bool requestNotSent = false, bool creationConfirmed = false)
    : HttpRequestException("Fabric folder creation did not return usable folder metadata.", null, statusCode)
{
    internal RetryConditionHeaderValue? RetryAfter { get; } = retryAfter;
    internal string? ErrorCode { get; } = errorCode;
    internal bool ErrorMetadataUnavailable { get; } = errorMetadataUnavailable;
    internal bool RequestNotSent { get; } = requestNotSent;
    internal bool CreationConfirmed { get; } = creationConfirmed;
}
