// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class WorkspaceCreateRequestException(HttpStatusCode statusCode, RetryConditionHeaderValue? retryAfter)
    : HttpRequestException("Fabric did not return a successful workspace creation response.", null, statusCode)
{
    public string? RetryAfter { get; } = retryAfter?.ToString();
}
