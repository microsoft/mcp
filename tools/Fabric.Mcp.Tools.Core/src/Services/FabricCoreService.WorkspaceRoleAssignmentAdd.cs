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
    public async Task<WorkspaceRoleAssignmentAddCommandResult> AddWorkspaceRoleAssignmentAsync(
        string workspaceId, string principal, string role, CancellationToken cancellationToken = default)
    {
        var request = WorkspaceRoleAssignmentAddInput.CreateRequest(workspaceId, principal, role, cancellationToken);
        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{request.WorkspaceId:D}/roleAssignments";
        var requestStarted = false;
        HttpStatusCode? responseStatus = null;
        HttpStatusCode? failureStatus = null;
        RetryConditionHeaderValue? retryAfter = null;
        try
        {
            using var response = await SendFabricHttpRequestAsync(
                HttpMethod.Post, url, request.Body, completionOption: HttpCompletionOption.ResponseHeadersRead,
                onSending: () => requestStarted = true, cancellationToken: cancellationToken);
            responseStatus = response.StatusCode;
            retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
            if (responseStatus != HttpStatusCode.Created)
            {
                failureStatus = response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode;
                var (code, unavailable) = response.IsSuccessStatusCode
                    ? (null, false)
                    : await FabricCoreHttpHelpers.ReadErrorCodeAsync(response, cancellationToken);
                throw new WorkspaceRoleAssignmentAddRequestException(failureStatus.Value, retryAfter, code, unavailable,
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
            var result = ReadAddedWorkspaceRoleAssignment(bytes, request.PrincipalId, request.PrincipalType, role);
            cancellationToken.ThrowIfCancellationRequested();
            return result with
            {
                RetryAfterSeconds = retryAfter?.Delta is { } delay ? (long)delay.TotalSeconds : null,
                RetryAfterUtc = retryAfter?.Date
            };
        }
        catch (Exception ex) when (ex is not WorkspaceRoleAssignmentAddRequestException &&
            (ex is JsonException or IOException or HttpRequestException or OperationCanceledException or TimeoutException or AuthenticationFailedException ||
            (responseStatus.HasValue && ex is InvalidOperationException or NotSupportedException)))
        {
            var status = failureStatus ?? (ex switch
            {
                OperationCanceledException => HttpStatusCode.RequestTimeout,
                TimeoutException => HttpStatusCode.GatewayTimeout,
                _ when responseStatus == HttpStatusCode.Created => HttpStatusCode.BadGateway,
                AuthenticationFailedException => HttpStatusCode.Unauthorized,
                HttpRequestException { StatusCode: { } httpStatus } => httpStatus,
                _ => HttpStatusCode.ServiceUnavailable
            });
            throw new WorkspaceRoleAssignmentAddRequestException(status, retryAfter,
                errorMetadataUnavailable: failureStatus.HasValue, requestNotSent: !requestStarted, responseStatusCode: responseStatus);
        }
    }

    private static WorkspaceRoleAssignmentAddCommandResult ReadAddedWorkspaceRoleAssignment(
        ReadOnlyMemory<byte> body, Guid principalId, string principalType, string role)
    {
        try
        {
            using var document = JsonDocument.Parse(body, new JsonDocumentOptions { MaxDepth = 64, AllowDuplicateProperties = false });
            FabricCoreHttpHelpers.ValidateMetadataJson(document.RootElement);
            var assignment = ReadWorkspaceRoleAssignment(document.RootElement);
            if (assignment.Principal.Id != principalId || assignment.Principal.Type != principalType || assignment.Role != role)
            {
                throw new JsonException(InvalidWorkspaceRoleAssignmentMessage);
            }
            var result = new WorkspaceRoleAssignmentAddCommandResult(assignment);
            // Validate the actual result inside the confirmed-creation boundary, reserving
            // namespace, tool-response, and JSON-RPC envelope depth without truncating parents.
            using var writer = new Utf8JsonWriter(Stream.Null, new JsonWriterOptions { MaxDepth = MaxRetainedRoleAssignmentDepth });
            JsonSerializer.Serialize(writer, result, CoreJsonContext.Default.WorkspaceRoleAssignmentAddCommandResult);
            return result;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            throw new JsonException(InvalidWorkspaceRoleAssignmentMessage);
        }
    }
}
