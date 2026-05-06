// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;

namespace Azure.Mcp.Tools.ResourceHealth.Services;

public sealed class ResourceHealthUnprocessableEntityException(
    string resourceId,
    string resourceType,
    string? errorCode,
    string? errorMessage,
    string? responseContent = null)
    : Exception(CreateMessage(resourceType, errorCode, errorMessage))
{
    public string ResourceId { get; } = resourceId;

    public string ResourceType { get; } = resourceType;

    public string? ErrorCode { get; } = errorCode;

    public string? ErrorDetails { get; } = errorMessage;

    public string? ResponseContent { get; } = responseContent;

    public HttpStatusCode StatusCode => HttpStatusCode.UnprocessableEntity;

    private static string CreateMessage(string resourceType, string? errorCode, string? errorMessage)
    {
        var code = string.IsNullOrWhiteSpace(errorCode) ? "UnprocessableEntity" : errorCode;
        var details = string.IsNullOrWhiteSpace(errorMessage)
            ? $"Azure Resource Health could not process availability status for resource type '{resourceType}'"
            : errorMessage;

        return $"Azure Resource Health returned {code}: {details}";
    }
}
