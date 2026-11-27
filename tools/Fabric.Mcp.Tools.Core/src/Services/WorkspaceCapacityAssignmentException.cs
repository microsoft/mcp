// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class WorkspaceCapacityAssignmentException(HttpStatusCode statusCode, long? retryAfterSeconds = null)
    : HttpRequestException(CreateMessage(statusCode, retryAfterSeconds), null, statusCode)
{
    public long? RetryAfterSeconds { get; } = retryAfterSeconds;

    private static string CreateMessage(HttpStatusCode statusCode, long? retryAfterSeconds)
    {
        var message = statusCode switch
        {
            HttpStatusCode.BadRequest =>
                "Invalid capacity assignment request. Check the workspace and capacity IDs and the documented region and capacity restrictions.",
            HttpStatusCode.Unauthorized =>
                "Authentication failed. Use an identity authorized for Fabric with Capacity.ReadWrite.All and Workspace.ReadWrite.All scopes.",
            HttpStatusCode.Forbidden =>
                "Access denied. Workspace Admin and capacity Contributor or Admin permissions are required, along with Capacity.ReadWrite.All and Workspace.ReadWrite.All scopes.",
            HttpStatusCode.NotFound =>
                "The workspace or capacity was not found or is not accessible. Verify both IDs and the caller's permissions.",
            HttpStatusCode.Conflict =>
                "The capacity assignment conflicts with the current workspace state. Inspect the workspace before submitting again.",
            HttpStatusCode.TooManyRequests =>
                "Fabric rate limited the capacity assignment request. Follow Retry-After guidance before making further requests.",
            HttpStatusCode.BadGateway =>
                "Fabric returned an unexpected response instead of 202 Accepted. Assignment acceptance could not be confirmed; inspect the workspace before submitting again.",
            HttpStatusCode.GatewayTimeout =>
                "The capacity assignment request timed out. Acceptance could not be confirmed; inspect the workspace before submitting again.",
            _ =>
                "The capacity assignment request failed. Acceptance could not be confirmed; inspect the workspace before submitting again."
        };

        return retryAfterSeconds is >= 0
            ? $"{message} Retry-After: wait at least {retryAfterSeconds} seconds before making further requests."
            : message;
    }
}
