// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;

namespace Azure.Mcp.Tools.ResourceHealth.Services;

public sealed class ResourceHealthRequestFailedException(
    HttpStatusCode statusCode,
    string? errorCode,
    string? errorMessage,
    string? responseContent = null)
    : Exception(CreateMessage(statusCode, errorCode, errorMessage))
{
    public HttpStatusCode StatusCode { get; } = statusCode;

    public string? ErrorCode { get; } = errorCode;

    public string? ErrorDetails { get; } = errorMessage;

    public string? ResponseContent { get; } = responseContent;

    private static string CreateMessage(HttpStatusCode statusCode, string? errorCode, string? errorMessage)
    {
        var code = string.IsNullOrWhiteSpace(errorCode) ? statusCode.ToString() : errorCode;
        var details = string.IsNullOrWhiteSpace(errorMessage)
            ? $"Azure Resource Health returned status {(int)statusCode} ({statusCode})"
            : errorMessage;

        return $"Azure Resource Health returned {code}: {details}";
    }
}