// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class WorkspaceRoleAssignmentListRequestException(HttpStatusCode statusCode, RetryConditionHeaderValue? retryAfter)
    : HttpRequestException("Unable to list Fabric workspace role assignments.", null, statusCode)
{
    internal RetryConditionHeaderValue? RetryAfter { get; } = retryAfter;
}
