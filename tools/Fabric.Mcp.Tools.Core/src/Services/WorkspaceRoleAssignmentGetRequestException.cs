// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class WorkspaceRoleAssignmentGetRequestException(HttpStatusCode statusCode, RetryConditionHeaderValue? retryAfter)
    : HttpRequestException("Unable to retrieve Fabric workspace role-assignment metadata.", null, statusCode)
{
    internal RetryConditionHeaderValue? RetryAfter { get; } = retryAfter;
}
