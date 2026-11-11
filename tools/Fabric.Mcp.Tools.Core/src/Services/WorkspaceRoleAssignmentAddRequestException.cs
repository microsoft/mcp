// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class WorkspaceRoleAssignmentAddRequestException(
    HttpStatusCode statusCode, RetryConditionHeaderValue? retryAfter = null, string? errorCode = null,
    bool errorMetadataUnavailable = false, bool requestNotSent = false, HttpStatusCode? responseStatusCode = null)
    : HttpRequestException("Fabric role-assignment creation did not return usable metadata.", null, statusCode)
{
    internal RetryConditionHeaderValue? RetryAfter { get; } = retryAfter;
    internal string? ErrorCode { get; } = errorCode;
    internal bool ErrorMetadataUnavailable { get; } = errorMetadataUnavailable;
    internal bool RequestNotSent { get; } = requestNotSent;
    internal HttpStatusCode? ResponseStatusCode { get; } = responseStatusCode;
    internal bool CreationConfirmed => ResponseStatusCode == HttpStatusCode.Created;
}
