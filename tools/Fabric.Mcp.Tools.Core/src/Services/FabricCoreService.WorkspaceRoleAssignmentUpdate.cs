// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Validation;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    /// <inheritdoc />
    public async Task<WorkspaceRoleAssignmentUpdateCommandResult> UpdateWorkspaceRoleAssignmentAsync(
        string workspaceId, string workspaceRoleAssignmentId, string role, CancellationToken cancellationToken = default)
    {
        var request = WorkspaceRoleAssignmentUpdateInput.CreateRequest(workspaceId, workspaceRoleAssignmentId, role, cancellationToken);
        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{request.WorkspaceId:D}/roleAssignments/{request.AssignmentId:D}";
        var requestStarted = false;
        HttpStatusCode? responseStatus = null;
        HttpStatusCode? failureStatus = null;
        RetryConditionHeaderValue? retryAfter = null;
        try
        {
            using var response = await SendFabricHttpRequestAsync(
                HttpMethod.Patch, url, request.Body, completionOption: HttpCompletionOption.ResponseHeadersRead,
                onSending: () => requestStarted = true, cancellationToken: cancellationToken);
            responseStatus = response.StatusCode;
            retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
            if (responseStatus != HttpStatusCode.OK)
            {
                failureStatus = response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode;
                var (code, unavailable) = response.IsSuccessStatusCode
                    ? (null, false)
                    : await FabricCoreHttpHelpers.ReadErrorCodeAsync(response, cancellationToken);
                throw new WorkspaceRoleAssignmentUpdateRequestException(failureStatus.Value, retryAfter, code, unavailable,
                    responseStatusCode: responseStatus);
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (!string.Equals(response.Content.Headers.ContentType?.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                throw new JsonException(InvalidWorkspaceRoleAssignmentMessage);
            }
            var bytes = await FabricResponseBodyReader.ReadAsync(
                response, FabricResponseBodyReader.DefaultMaxJsonBytes,
                static () => new JsonException(InvalidWorkspaceRoleAssignmentMessage), cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            var result = ReadUpdatedWorkspaceRoleAssignment(bytes, request.AssignmentId, role);
            cancellationToken.ThrowIfCancellationRequested();
            return result with
            {
                RetryAfterSeconds = retryAfter?.Delta is { } delay ? (long)delay.TotalSeconds : null,
                RetryAfterUtc = retryAfter?.Date
            };
        }
        catch (Exception ex) when (ex is not WorkspaceRoleAssignmentUpdateRequestException &&
            (ex is JsonException or IOException or HttpRequestException or OperationCanceledException or TimeoutException or AuthenticationFailedException ||
            (responseStatus.HasValue && ex is InvalidOperationException or NotSupportedException)))
        {
            var status = failureStatus ?? (ex switch
            {
                OperationCanceledException => HttpStatusCode.RequestTimeout,
                TimeoutException => HttpStatusCode.GatewayTimeout,
                _ when responseStatus == HttpStatusCode.OK => HttpStatusCode.BadGateway,
                AuthenticationFailedException => HttpStatusCode.Unauthorized,
                HttpRequestException { StatusCode: { } httpStatus } => httpStatus,
                _ => HttpStatusCode.ServiceUnavailable
            });
            throw new WorkspaceRoleAssignmentUpdateRequestException(status, retryAfter,
                errorMetadataUnavailable: failureStatus.HasValue, requestNotSent: !requestStarted, responseStatusCode: responseStatus);
        }
    }

    private static WorkspaceRoleAssignmentUpdateCommandResult ReadUpdatedWorkspaceRoleAssignment(
        ReadOnlyMemory<byte> body, Guid assignmentId, string role)
    {
        try
        {
            var assignment = ReadWorkspaceRoleAssignment(body);
            if (assignment.Id != assignmentId || assignment.Role != role)
            {
                throw new JsonException(InvalidWorkspaceRoleAssignmentMessage);
            }
            var result = new WorkspaceRoleAssignmentUpdateCommandResult(assignment);
            // Validate before leaving the confirmed-update boundary, reserving the real
            // namespace, tool-response, and JSON-RPC envelopes without truncating parents.
            using var writer = new Utf8JsonWriter(Stream.Null, new JsonWriterOptions { MaxDepth = MaxRetainedRoleAssignmentDepth });
            JsonSerializer.Serialize(writer, result, CoreJsonContext.Default.WorkspaceRoleAssignmentUpdateCommandResult);
            return result;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            throw new JsonException(InvalidWorkspaceRoleAssignmentMessage);
        }
    }
}
