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

    public string? ErrorMessage { get; } = errorMessage;

    public string? ResponseContent { get; } = responseContent;

    private static string CreateMessage(HttpStatusCode statusCode, string? errorCode, string? errorMessage)
    {
        var message = $"Azure Resource Health request failed with status {(int)statusCode} ({statusCode})";
        if (!string.IsNullOrWhiteSpace(errorCode))
        {
            message += $". Error code: {errorCode.Trim()}";
        }

        if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            message += $". Details: {errorMessage.Trim()}";
        }

        return message;
    }
}
