// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;

namespace Azure.Mcp.Tools.ResourceHealth.Services;

public sealed class ResourceHealthUnsupportedResourceException(
    string resourceId,
    string resourceType,
    string? responseContent = null)
    : Exception($"Resource Health availability status is not supported for resource type '{resourceType}'.")
{
    public string ResourceId { get; } = resourceId;

    public string ResourceType { get; } = resourceType;

    public string? ResponseContent { get; } = responseContent;

    public HttpStatusCode StatusCode => HttpStatusCode.UnprocessableEntity;
}