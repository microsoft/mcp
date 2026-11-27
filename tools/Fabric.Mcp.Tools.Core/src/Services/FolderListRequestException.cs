// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class FolderListRequestException(HttpStatusCode statusCode, RetryConditionHeaderValue? retryAfter)
    : HttpRequestException("Unable to list Fabric folders.", null, statusCode)
{
    internal RetryConditionHeaderValue? RetryAfter { get; } = retryAfter;
}
