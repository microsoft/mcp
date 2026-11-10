// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    /// <inheritdoc />
    public async Task<WorkspaceRoleAssignmentDeleteCommandResult> DeleteWorkspaceRoleAssignmentAsync(
        string workspaceId, string workspaceRoleAssignmentId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(workspaceId, out var workspace) || workspace == Guid.Empty)
        {
            throw new ArgumentException("Workspace ID must be a nonempty UUID.", nameof(workspaceId));
        }
        if (!Guid.TryParse(workspaceRoleAssignmentId, out var assignment) || assignment == Guid.Empty)
        {
            throw new ArgumentException("Workspace role assignment ID must be a nonempty UUID.", nameof(workspaceRoleAssignmentId));
        }

        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspace:D}/roleAssignments/{assignment:D}";
        var requestStarted = false;
        HttpStatusCode? responseStatus = null;
        HttpStatusCode? failureStatus = null;
        RetryConditionHeaderValue? retryAfter = null;
        string? errorCode = null;
        var errorMetadataUnavailable = false;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var response = await SendFabricHttpRequestAsync(
                HttpMethod.Delete, url, completionOption: HttpCompletionOption.ResponseHeadersRead,
                onSending: () => requestStarted = true, cancellationToken: cancellationToken);
            responseStatus = response.StatusCode;
            if (responseStatus == HttpStatusCode.OK)
            {
                // This endpoint has no success body contract; headers confirm deletion even after late cancellation.
                return new(workspace, assignment);
            }

            failureStatus = response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode;
            retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
            if (!response.IsSuccessStatusCode)
            {
                errorMetadataUnavailable = true;
                (errorCode, errorMetadataUnavailable) = await FabricCoreHttpHelpers.ReadErrorCodeAsync(response, cancellationToken);
            }
            throw new WorkspaceRoleAssignmentDeleteRequestException(failureStatus.Value, retryAfter, errorCode, errorMetadataUnavailable,
                responseStatusCode: responseStatus);
        }
        catch (Exception ex) when (ex is not WorkspaceRoleAssignmentDeleteRequestException &&
            (ex is HttpRequestException or IOException or OperationCanceledException or TimeoutException or AuthenticationFailedException ||
            (responseStatus.HasValue && ex is InvalidOperationException or NotSupportedException)))
        {
            var status = responseStatus == HttpStatusCode.OK ? HttpStatusCode.BadGateway : failureStatus ?? (ex switch
            {
                OperationCanceledException => HttpStatusCode.RequestTimeout,
                TimeoutException => HttpStatusCode.GatewayTimeout,
                AuthenticationFailedException => HttpStatusCode.Unauthorized,
                HttpRequestException { StatusCode: { } httpStatus } => httpStatus,
                _ => HttpStatusCode.ServiceUnavailable
            });
            throw new WorkspaceRoleAssignmentDeleteRequestException(status, retryAfter, errorCode, errorMetadataUnavailable,
                requestNotSent: !requestStarted, responseStatusCode: responseStatus);
        }
    }
}
